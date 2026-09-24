# Evidence

Proof for each Section 6 requirement, added as each is built and verified — not
reconstructed after the fact.

## Widget management

- [x] Authenticated CRUD endpoints for widgets; requests without valid auth are rejected.
- [x] Multi-tenant isolation proven: tenant A cannot read or modify tenant B's widgets or submissions.

**Requests without valid auth are rejected — no token on a widget endpoint:**
```
POST https://localhost:7111/api/widgets
Content-Type: application/json

{
  "type": "SignupForm",
  "title": "Test",
  "fieldsJson": "[]",
  "buttonText": "Go"
}
-------------------------------------------------------------------------
401 Unauthorized
WWW-Authenticate: Bearer
(empty body)
```

**Tenant A creates a widget:**
```
POST https://localhost:7111/api/widgets
Content-Type: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJjYWEwNzE2OS04ZGMxLTRiYzgtYWJjYS04NGY4M2E0ZmQ1MzMiLCJlbWFpbCI6InRlc3RAZXhhbXBsZS5jb20iLCJqdGkiOiIyMzk2MDQyNy0yNjE1LTQ2NDQtYjUzOC00OGQzNzZjMDllNGIiLCJleHAiOjE3ODkwNTcwMTMsImlzcyI6IldpZGdldFBsYXRmb3JtQXBpIiwiYXVkIjoiV2lkZ2V0UGxhdGZvcm1DbGllbnQifQ.ZpxRG3Y_lalv5bvNmCmvlpb7GBpkUL6V_-xbzyBuxLU

{
  "type": "SignupForm",
  "title": "Newsletter Signup",
  "description": "Join our list",
  "fieldsJson": "[{\"name\":\"email\",\"label\":\"Email\",\"type\":\"email\",\"required\":true}]",
  "buttonText": "Subscribe",
  "displayOptionsJson": null
}
-------------------------------------------------------------------------
201 Created
{
  "id": "07298df4-ee73-4b9f-834c-19987cfe630a",
  "type": "SignupForm",
  "title": "Newsletter Signup",
  "description": "Join our list",
  "fieldsJson": "[{\"name\":\"email\",\"label\":\"Email\",\"type\":\"email\",\"required\":true}]",
  "buttonText": "Subscribe",
  "displayOptionsJson": null,
  "version": 1,
  "createdAt": "2026-09-10T12:38:32.4231312Z"
}
```

**Tenant A can read their own widget:**
```
GET https://localhost:7111/api/widgets/07298df4-ee73-4b9f-834c-19987cfe630a
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJjYWEwNzE2OS04ZGMxLTRiYzgtYWJjYS04NGY4M2E0ZmQ1MzMiLCJlbWFpbCI6InRlc3RAZXhhbXBsZS5jb20iLCJqdGkiOiIyMzk2MDQyNy0yNjE1LTQ2NDQtYjUzOC00OGQzNzZjMDllNGIiLCJleHAiOjE3ODkwNTcwMTMsImlzcyI6IldpZGdldFBsYXRmb3JtQXBpIiwiYXVkIjoiV2lkZ2V0UGxhdGZvcm1DbGllbnQifQ.ZpxRG3Y_lalv5bvNmCmvlpb7GBpkUL6V_-xbzyBuxLU
-------------------------------------------------------------------------
200 OK
{
  "id": "07298df4-ee73-4b9f-834c-19987cfe630a",
  "type": "SignupForm",
  "title": "Newsletter Signup",
  "description": "Join our list",
  "fieldsJson": "[{\"name\":\"email\",\"label\":\"Email\",\"type\":\"email\",\"required\":true}]",
  "buttonText": "Subscribe",
  "displayOptionsJson": null,
  "version": 1,
  "createdAt": "2026-09-10T12:38:32.423131Z"
}
```

**Tenant B CANNOT read tenant A's widget — 404, not 403 (doesn't leak existence):**
```
GET https://localhost:7111/api/widgets/07298df4-ee73-4b9f-834c-19987cfe630a
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhYzU4YWI5ZS0yMTMzLTQ3NDAtYTE3NS1jYmUzNjhlM2Q4ODciLCJlbWFpbCI6InRlc3QzQGV4YW1wbGUuY29tIiwianRpIjoiZDUyMDg2YTItNGQ5Zi00MzlmLWE0NGUtODFkYWIyOWNiMThiIiwiZXhwIjoxNzg5MDU3MDYzLCJpc3MiOiJXaWRnZXRQbGF0Zm9ybUFwaSIsImF1ZCI6IldpZGdldFBsYXRmb3JtQ2xpZW50In0.ZnTb1ZzciglOaBDckME917g3ersgjYeTAPB_iEbnljg
-------------------------------------------------------------------------
404 Not Found
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "traceId": "00-ca9f9991b22844048d1651305b6ac230-d4da1a31997d2025-00"
}
```

**Tenant B's widget list is correctly scoped (does not include tenant A's widget):**
```
GET https://localhost:7111/api/widgets
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhYzU4YWI5ZS0yMTMzLTQ3NDAtYTE3NS1jYmUzNjhlM2Q4ODciLCJlbWFpbCI6InRlc3QzQGV4YW1wbGUuY29tIiwianRpIjoiZDUyMDg2YTItNGQ5Zi00MzlmLWE0NGUtODFkYWIyOWNiMThiIiwiZXhwIjoxNzg5MDU3MDYzLCJpc3MiOiJXaWRnZXRQbGF0Zm9ybUFwaSIsImF1ZCI6IldpZGdldFBsYXRmb3JtQ2xpZW50In0.ZnTb1ZzciglOaBDckME917g3ersgjYeTAPB_iEbnljg
-------------------------------------------------------------------------
200 OK
[]
```

**Tenant B CANNOT delete tenant A's widget — 404, no side effect:**
```
DELETE https://localhost:7111/api/widgets/07298df4-ee73-4b9f-834c-19987cfe630a
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhYzU4YWI5ZS0yMTMzLTQ3NDAtYTE3NS1jYmUzNjhlM2Q4ODciLCJlbWFpbCI6InRlc3QzQGV4YW1wbGUuY29tIiwianRpIjoiZDUyMDg2YTItNGQ5Zi00MzlmLWE0NGUtODFkYWIyOWNiMThiIiwiZXhwIjoxNzg5MDU3MDYzLCJpc3MiOiJXaWRnZXRQbGF0Zm9ybUFwaSIsImF1ZCI6IldpZGdldFBsYXRmb3JtQ2xpZW50In0.ZnTb1ZzciglOaBDckME917g3ersgjYeTAPB_iEbnljg
-------------------------------------------------------------------------
404 Not Found
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "traceId": "00-1d4e40b030194b44a0aa3384422afe16-b3bf22b1790b7dcb-00"
}
```

**Confirms tenant B's failed delete did nothing — tenant A's widget still exists:**
```
GET https://localhost:7111/api/widgets/07298df4-ee73-4b9f-834c-19987cfe630a
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJjYWEwNzE2OS04ZGMxLTRiYzgtYWJjYS04NGY4M2E0ZmQ1MzMiLCJlbWFpbCI6InRlc3RAZXhhbXBsZS5jb20iLCJqdGkiOiIyMzk2MDQyNy0yNjE1LTQ2NDQtYjUzOC00OGQzNzZjMDllNGIiLCJleHAiOjE3ODkwNTcwMTMsImlzcyI6IldpZGdldFBsYXRmb3JtQXBpIiwiYXVkIjoiV2lkZ2V0UGxhdGZvcm1DbGllbnQifQ.ZpxRG3Y_lalv5bvNmCmvlpb7GBpkUL6V_-xbzyBuxLU
-------------------------------------------------------------------------
200 OK
{
  "id": "07298df4-ee73-4b9f-834c-19987cfe630a",
  "type": "SignupForm",
  "title": "Newsletter Signup",
  "description": "Join our list",
  "fieldsJson": "[{\"name\":\"email\",\"label\":\"Email\",\"type\":\"email\",\"required\":true}]",
  "buttonText": "Subscribe",
  "displayOptionsJson": null,
  "version": 1,
  "createdAt": "2026-09-10T12:38:32.423131Z"
}
```

## Widget delivery

- [x] Embed snippet generated per widget.
- [x] Public config endpoint serves a small payload with correct HTTP cache headers.
- [x] Widget JavaScript is served as a versioned bundle (new version = new URL or cache-bust).
- [x] The widget renders on a page served from a different origin than the API.
- [x] Cross-origin submissions work (rendered widget's own form submits successfully).

**Embed snippet included on every widget response:**
```json
"embedSnippet": "<script src=\"https://localhost:7111/widget.v1.js?id=07298df4-ee73-4b9f-834c-19987cfe630a\"></script>"
```

**Public config endpoint — correct cache header, served anonymously:**
```
GET https://localhost:7111/api/widgets/07298df4-ee73-4b9f-834c-19987cfe630a/config
(no Authorization header)
-------------------------------------------------------------------------
200 OK
Cache-Control: public, max-age=60
Access-Control-Allow-Origin: http://localhost:5500
Content-Type: application/json; charset=utf-8
```

**Versioned script bundle — long-cache, immutable:**
```
GET https://localhost:7111/widget.v1.js?id=07298df4-ee73-4b9f-834c-19987cfe630a
-------------------------------------------------------------------------
200 OK
Cache-Control: public, max-age=31536000, immutable
Content-Type: application/javascript
```
(Both header values confirmed directly in Chrome DevTools Network tab, Headers panel — not
inferred from status code alone.)

**Full embed flow verified from the second-origin test site (http://localhost:5500):**
1. `<script src="https://localhost:7111/widget.v1.js?id=...">` loads (Network tab: `script`, 200).
2. Script fetches `/api/widgets/{id}/config` (Network tab: `fetch`, 200).
3. Form is rendered dynamically in the page from the fetched field definitions (a single "Email"
   input + Subscribe button appeared, matching the widget's FieldsJson — not hardcoded in the
   test page).
4. Submitting the rendered form fires a real cross-origin POST to `/api/submissions`, preceded by
   an OPTIONS preflight (Network tab: `preflight` 204, then `submissions` 201).
5. Page displays "Thank you!" and the row is confirmed stored in pgAdmin.

This is the actual brief-specified mechanism (script tag → config → render → submit) — distinct
from the earlier CORS-only test, which called `/api/submissions` directly with a hardcoded widget
ID and a manually-built form, proving CORS/validation in isolation but not the real embed flow.

## Public submission API

- [x] Cross-origin submissions work: CORS headers correct, preflight (OPTIONS) handled.
- [x] All incoming input validated; malformed and oversized payloads rejected with appropriate 4xx codes and JSON errors.
- [x] Valid submissions stored safely, linked to the right widget and tenant.

**Valid submission — stored and linked to the correct widget:**
```
POST https://localhost:7111/api/submissions
Content-Type: application/json

{
  "widgetId": "07298df4-ee73-4b9f-834c-19987cfe630a",
  "data": { "email": "visitor@example.com" }
}
-------------------------------------------------------------------------
201 Created
{
  "id": "61c742b0-fac4-4a4e-9800-27803af6ab03",
  "widgetId": "07298df4-ee73-4b9f-834c-19987cfe630a",
  "createdAt": "2026-09-21T09:16:27.9134526Z"
}
```
Confirmed in pgAdmin: the row's OwnerId matches the widget's owner (denormalized per DESIGN.md), not left null or mismatched.

**Missing required field — clean 400, not a 500:**
```
POST https://localhost:7111/api/submissions
Content-Type: application/json

{
  "widgetId": "07298df4-ee73-4b9f-834c-19987cfe630a",
  "data": {}
}
-------------------------------------------------------------------------
400 Bad Request
{"error":"'email' is required."}
```

**Nonexistent widget — clean 404:**
```
POST https://localhost:7111/api/submissions
Content-Type: application/json

{
  "widgetId": "00000000-0000-0000-0000-000000000000",
  "data": { "email": "visitor@example.com" }
}
-------------------------------------------------------------------------
404 Not Found
{"error":"Widget not found"}
```

**Oversized payload (>16KB) — clean 413, no leaked stack trace:**
```
POST https://localhost:7111/api/submissions
Content-Type: application/json

{
  "widgetId": "07298df4-ee73-4b9f-834c-19987cfe630a",
  "data": { "email": "<50,000+ char value>@example.com" }
}
-------------------------------------------------------------------------
413 (confirmed via .http client status line)
{"error":"Request payload too large or malformed."}
```
First attempt at this leaked a raw 500 with a full stack trace instead — fixed by adding global
exception-handling middleware (see BUILDLOG.md) before this result was captured.

**CORS — real browser test, not just .http requests (which bypass CORS entirely):**

Built a genuinely separate origin — `test-site/index.html`, a plain HTML+fetch page served via
`npx serve` on `http://localhost:5500` — and tested from an actual browser with DevTools open.

Step 1 — before any CORS config, confirmed the real failure mode:
```
Console: Access to fetch at 'https://localhost:7111/api/submissions' from origin
'http://localhost:5500' has been blocked by CORS policy: Response to preflight request
doesn't pass access control check: No 'Access-Control-Allow-Origin' header is present
on the requested resource.
POST https://localhost:7111/api/submissions net::ERR_FAILED
```

Step 2 — after adding AddCors + [EnableCors] on SubmissionsController, with app.UseCors()
(no default policy) in the pipeline — confirmed via DevTools Network tab, two requests fired
in order:
```
submissions   OPTIONS   204   (Preflight)
submissions   POST      201   (fetch)
```

Step 3 — confirmed the CORS policy is correctly scoped, not globally open: a second button on
the same test page calling `GET /api/widgets` (an owner-only endpoint) from the same origin was
correctly blocked:
```
Console: Access to fetch at 'https://localhost:7111/api/widgets' from origin
'http://localhost:5500' has been blocked by CORS policy: No 'Access-Control-Allow-Origin'
header is present on the requested resource.
GET https://localhost:7111/api/widgets net::ERR_FAILED 401 (Unauthorized)
```
Submissions endpoint still worked in the same test run — confirming the CORS policy is applied
per-endpoint (via [EnableCors]), not globally.

## Abuse protection

- [x] Rate limiting per IP and/or per widget returns 429 under a burst — and the API keeps serving legitimate traffic.
- [x] At least one spam-prevention technique (honeypot field, token, or heuristic) demonstrably blocks a spam submission.

**Rate limiting — burst of 6 rapid requests, then 1 after window reset:**
```
Requests 1-5 (same widget, valid data): 201, 201, 201, 201, 201
Request 6 (immediately after):          429
{"error":"Too many requests. Please try again shortly."}
(waited 10+ seconds)
Request 7:                              201
```
Confirms both halves of the requirement: the burst is rejected, and legitimate traffic is served
again once the window resets rather than being permanently blocked.

**Honeypot field tripped — looks like success to the caller, nothing is stored:**
```
POST https://localhost:7111/api/submissions
Content-Type: application/json

{
  "widgetId": "07298df4-ee73-4b9f-834c-19987cfe630a",
  "data": { "email": "spammer@example.com" },
  "website": "http://spam.com"
}
-------------------------------------------------------------------------
201 Created
(response body returned, but confirmed in pgAdmin: submission count unchanged — no row created)
```
A normal browser submission through the rendered widget (honeypot field stays empty, since real
visitors never see or fill it) was re-confirmed to still store correctly after this change.

## Enrichment & safe side effects
- [ ] Not yet built.

## Documentation
- [ ] Not yet built.
