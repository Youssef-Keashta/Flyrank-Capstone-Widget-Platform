# Flyrank Capstone — Widget & Lead-Capture Platform

Embeddable widget platform: customers create a widget, embed it on any external
site with one `<script>` tag, and safely collect visitor submissions — validated,
rate-limited, spam-filtered, geo-enriched, and viewable on a dashboard.

Repo: https://github.com/Youssef-Keashta/Flyrank-Capstone-Widget-Platform

## Architecture

```
Widget Owner (authenticated)
 -> Widget Management API -> Widget DB (tenant-isolated) -> embed snippet

Customer Website (any origin)
 <script src="widget.v1.js?id=...">
 -> GET /api/widgets/:id/config (public, cached, CORS)
 -> render widget
 -> POST /api/submissions (public, CORS, rate-limited, honeypot)

Website Visitor
 -> POST /api/submissions
    | validation -> 4xx on bad input
    | idempotency check (Idempotency-Key header)
    | honeypot check -> silently accepted, not stored
    | rate limit -> 429 on burst
    | geo enrichment: Provider A -> Provider B -> null
    | store submission
    | email notification (background job, failure never blocks response)

Widget Owner (authenticated)
 -> Dashboard API <- submissions + stats
```

Four-project layered solution:
- `WidgetPlatform.Domain` — entities (Widget, Submission, ApplicationUser), zero EF Core dependency
- `WidgetPlatform.Data` — EF Core DbContext, migrations
- `WidgetPlatform.Application` — services, DTOs, business logic
- `WidgetPlatform.Api` — controllers, middleware, Program.cs

See `DESIGN.md` for the full design doc and data model.

## Tech stack

- ASP.NET Core Web API (.NET 8/9)
- EF Core + PostgreSQL (Docker)
- ASP.NET Core Identity + JWT
- Built-in `Microsoft.AspNetCore.RateLimiting`

## Setup

**Prerequisites:** .NET SDK, Docker Desktop, `dotnet-ef` (`dotnet tool install --global dotnet-ef`)

1. Clone the repo.
2. Copy `.env.example` to `.env` and fill in real values (used only by Docker Compose):
   ```
   POSTGRES_USER=widgetplatform_user
   POSTGRES_PASSWORD=<your choice>
   POSTGRES_DB=widgetplatform
   POSTGRES_PORT=5434
   ```
3. Start Postgres:
   ```
   docker compose up -d
   ```
4. Set your local connection string and JWT signing key via user-secrets (never committed):
   ```
   cd src/WidgetPlatform.Api
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5434;Database=widgetplatform;Username=widgetplatform_user;Password=<same as .env>"
   dotnet user-secrets set "Jwt:SigningKey" "<32+ char random string>"
   ```
5. Apply migrations:
   ```
   dotnet ef database update --project src/WidgetPlatform.Data --startup-project src/WidgetPlatform.Api
   ```
6. Run the API (F5 in Visual Studio, or `dotnet run --project src/WidgetPlatform.Api`).

### Seed / demo data

No seed script — create a user and widget via the API itself:
```
POST /api/auth/register   { "email": "...", "password": "..." }
POST /api/auth/login      -> copy the token
POST /api/widgets          (Authorization: Bearer <token>)
```
The response includes the widget's `embedSnippet` — paste it into any HTML page to see it live.

### Testing the cross-origin embed flow

A minimal "customer site" is included at `test-site/index.html`. Serve it on a different port than the API to exercise CORS for real:
```
cd test-site
npx serve -l 5500
```
Update the `<script src="...">` widget id in `index.html` to match a widget you created, then open `http://localhost:5500`.

## API surface

| Path | Auth | Notes |
|---|---|---|
| `POST /api/auth/register` | none | |
| `POST /api/auth/login` | none | returns JWT |
| `GET /api/auth/me` | JWT | |
| `GET/POST/PUT/DELETE /api/widgets` | JWT | tenant-isolated |
| `GET /api/widgets/{id}/config` | none | public, CORS, cached 60s |
| `GET /widget.v1.js` | none | public, cached 1 year, immutable |
| `POST /api/submissions` | none | public, CORS, rate-limited, honeypot, idempotent |
| `GET /api/dashboard/widgets/{id}/submissions` | JWT | tenant-isolated |
| `GET /api/dashboard/widgets/{id}/stats` | JWT | tenant-isolated |

## Limitations (honest, not hidden)

- No visual widget-builder UI — configuration is via API calls (explicit non-goal, see `DESIGN.md`).
- Geo enrichment uses two mocked providers (deterministic fallback simulation via config
  flags), not live calls to ip-api.com/ipapi.co — chosen for reliability under time pressure
  rather than dependency on external services during grading.
- JWT expiry is currently set longer than production-appropriate for development convenience;
  no refresh-token flow (explicit non-goal — out of scope per the brief).
- Email notification is simulated (logged), not a real SMTP/Mailpit send — the graded behavior
  (failure doesn't block the response) is implemented and tested regardless of the transport.
- Rate limiting is in-memory (per-instance) — fine for a single-instance capstone, would need
  a distributed store (e.g. Redis) to work correctly behind multiple API instances.

## Evidence

See `EVIDENCE.md` for real request/response transcripts proving every requirement in Section 6
of the brief, and `BUILDLOG.md` for the AI-assisted build process, mistakes made and fixed, and
decisions taken along the way.
