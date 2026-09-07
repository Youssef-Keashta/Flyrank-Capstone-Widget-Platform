# Design Doc — Embeddable Widget & Lead-Capture Platform

## Problem
Let a customer (tenant) create a widget, embed it on any external site via one `<script>` tag,
and safely accept public submissions from that widget — validated, rate-limited, spam-filtered,
geo-enriched, and visible on a dashboard.

## Non-goal
No visual widget-builder UI. Widgets are configured via authenticated API calls; the visitor-facing
render is a minimal auto-generated form. The product being tested here is the backend contract,
not a drag-and-drop editor.

## Tech stack
- ASP.NET Core Web API (.NET 8)
- EF Core + PostgreSQL (Docker)
- ASP.NET Core Identity + JWT (owner auth)
- Built-in `Microsoft.AspNetCore.RateLimiting`
- `IHttpClientFactory` (named clients) for the geo provider fallback chain
- Console/Mailpit for the email side effect

## Data model

**ApplicationUser** (Identity) — the tenant. No separate Organization entity.

**Widget**
- Id (Guid)
- OwnerId (FK -> ApplicationUser) — tenant isolation boundary
- Type (enum: SignupForm, Cta, Popover)
- Title, Description
- FieldsJson (jsonb) — field defs: name, label, type, required
- ButtonText
- DisplayOptionsJson (jsonb)
- Version (int) — bumped on config-affecting changes, drives cache-busting
- CreatedAt

**Submission**
- Id (Guid)
- WidgetId (FK -> Widget)
- OwnerId (denormalized FK -> ApplicationUser) — makes tenant-scoped queries a single WHERE, no join required for isolation checks
- DataJson (jsonb) — submitted field values, already validated against Widget.FieldsJson
- IpAddress
- Country, City, GeoProvider (nullable — enrichment may fail entirely)
- CreatedAt

Indexes: Widget(OwnerId), Submission(WidgetId), Submission(OwnerId, CreatedAt) for dashboard queries.

## The three request paths

**1. Owner (authenticated)**
```
POST   /api/widgets                  create
GET    /api/widgets                  list own widgets
GET    /api/widgets/{id}             get own widget (404 if not owner, never 403 — don't leak existence)
PUT    /api/widgets/{id}             update
DELETE /api/widgets/{id}             delete
GET    /api/dashboard/widgets/{id}   stats: count over time, geo breakdown
```
Every handler filters by `OwnerId == CurrentUserId` at the query level — never "fetch then check ownership in code."

**2. Customer site (public, cached)**
```
GET /widget.js                       versioned static bundle, Cache-Control: max-age=31536000, immutable
GET /api/widgets/{id}/config         public config, Cache-Control: max-age=60, keyed on Widget.Version
```

**3. Visitor (public, CORS, protected)**
```
POST /api/submissions
```
Pipeline inside the handler, in order — each stage can short-circuit the request but must never crash it:
1. CORS + preflight (handled by middleware, not app code)
2. Payload validation (FluentValidation) — malformed/oversized -> 4xx + JSON error body
3. Honeypot check -> silently 2xx-and-drop if tripped (don't tell bots why)
4. Rate limit (per IP, per WidgetId) -> 429 on burst
5. Store submission (row must exist even if steps 6-7 fail)
6. Geo enrichment: try Provider A -> Provider B -> null (never blocks storage)
7. Side effect (email/webhook) — fire-and-forget or try/catch that swallows failure; logged, never surfaced to caller

## Embed flow
```
Customer site: <script src="https://api/widget.js?id={widgetId}">
  -> widget.js reads its own ?id, fetches GET /api/widgets/{id}/config
  -> renders form from FieldsJson
  -> on submit: POST /api/submissions (cross-origin, CORS)
```

## Explicit assumption to revisit
"Fake" email failure and provider-down states must be triggerable deterministically for EVIDENCE.md
(e.g. an appsettings flag or a `?forceProviderDown=true` dev-only override), not just hoped-for via
flaky real APIs.
