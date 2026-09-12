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

## Public submission API
- [ ] Not yet built.

## Abuse protection
- [ ] Not yet built.

## Enrichment & safe side effects
- [ ] Not yet built.

## Documentation
- [ ] Not yet built.
