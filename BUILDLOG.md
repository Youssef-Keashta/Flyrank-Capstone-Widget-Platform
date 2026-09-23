# Build Log

## Phase 1 — Design
- Used Claude to think through the design doc: tenant model (User = tenant, no
  separate Organization entity), the non-goal (no visual widget-builder UI),
  data model for Widget/Submission, and the three API surfaces (owner,
  customer site, visitor). Wrote DESIGN.md from that discussion.
- Decided on the stack myself with Claude's input: EF Core (matches my IWMS
  experience) and ASP.NET Core Identity + JWT for auth.

## Solution structure
- First attempt was wrong: created Domain/Data/Application/Api as folders
  inside a single project instead of separate class libraries. Claude caught
  this — folders don't enforce the layer boundary (nothing stops Domain code
  from referencing EF Core), separate projects do. Recreated as four real
  projects with explicit ProjectReferences.
- Made two setup mistakes myself along the way: created the three class
  library projects one directory too high (outside the actual git repo,
  as siblings to it rather than inside it) because Visual Studio's "Add
  New Project" dialog remembered a stale default location — didn't check
  the path before confirming. Caught this myself from screenshots and had
  to delete + recreate the three projects in the right place.
- Moved WidgetPlatform.Api into src/ for consistency with the other three
  projects (cosmetic, my request). This surfaced a real bug: because Api's
  .csproj had been sitting at the repo root — a parent directory of the
  other three projects — SDK-style implicit file globbing pulled in their
  generated AssemblyInfo.cs files, causing duplicate-attribute compile
  errors (CS0579) that only affected the Api project's build. Diagnosed via
  the build output showing only Api failing. Fixed by moving Api to be a
  sibling under src/, then fixing a broken relative ProjectReference path
  the move introduced (it pointed into a directory that didn't exist).

## Docker / Postgres
- Claude proposed the docker-compose.yml + .env pattern (env vars for
  Postgres credentials, named volume so data survives `down`/`up`,
  healthcheck). Learned the distinction between .env (feeds Docker Compose
  variable substitution) and appsettings.json/user-secrets (feeds the .NET
  app's IConfiguration) — these aren't interchangeable, and I initially
  conflated them, thinking I'd need to gitignore appsettings.json too, which
  was wrong (it holds no secrets and needs to be committed for the repo to
  be runnable by a stranger).
- Hit a real port conflict: had two local PostgreSQL installs (17 and 18) on
  the machine already occupying 5432 and 5433, colliding with the Docker
  container's default mapping. Settled on host port 5434.
- Found and fixed a real bug myself: my docker-compose.yml had the
  container-side port hardcoded to 5433 instead of the correct fixed value
  5432 (Postgres inside the image always listens on 5432 regardless of the
  host-side port). This caused a connection timeout that looked like a
  bigger problem than it was.
- Verified the whole setup via pgAdmin, including a deliberate down/up cycle
  to confirm the named volume actually persists data.

## Identity + EF Core
- Learned the correct package split: Microsoft.Extensions.Identity.Stores
  in Domain (IdentityUser etc., no EF Core dependency) vs
  Microsoft.AspNetCore.Identity.EntityFrameworkCore in Data (the actual EF
  Core storage bridge). Claude's first answer on this was wrong/imprecise —
  it initially suggested the EF-coupled package in Domain and called it "an
  acceptable exception," then corrected itself to the cleaner answer where
  Domain has zero EF Core exposure, no exception needed.
- Built ApplicationUser, Widget, Submission entities and
  WidgetPlatformDbContext (IdentityDbContext<ApplicationUser>).
- Claude caught a real bug in my Program.cs before I ran it: I had
  UseAuthorization() without UseAuthentication() — would have caused
  confusing 401s on every authenticated request later, since there'd be no
  authentication middleware populating the user for authorization to check.
- Installed dotnet-ef (wasn't installed originally). Generated and applied
  the first migration. Hit a "ConnectionString property has not been
  initialized" error — root cause was two-fold: the UserSecretsId had never
  actually been added to the Api project's .csproj (likely lost somewhere
  in the project-restructuring above), and separately, `dotnet ef` run from
  a plain terminal doesn't set ASPNETCORE_ENVIRONMENT=Development the way
  Visual Studio's F5 does, so user-secrets wouldn't have loaded even once
  they existed. Fixed both.

## Auth endpoints
- Built Register/Login via UserManager<ApplicationUser> in a thin
  AuthService/AuthController split (controller has no business logic).
- Deliberate choice: Login returns the same "Invalid credentials" message
  whether the email doesn't exist or the password is wrong, to avoid a
  user-enumeration oracle.
- Verified via the .http file: valid register, weak-password rejection,
  duplicate-email rejection, valid login, wrong-password rejection all
  behave correctly; confirmed final user count in pgAdmin.
- JWT: added System.IdentityModel.Tokens.Jwt (Application, token
  generation) and Microsoft.AspNetCore.Authentication.JwtBearer (Api, token
  validation middleware) — Claude initially gave contradictory guidance on
  which project should hold the first package (said "not Application" then
  immediately recommended Application); caught and corrected before I
  installed anything. Signing key lives in user-secrets, issuer/audience/
  expiry in appsettings.json. Deliberately set expiry to a few hours for
  now (dev convenience) with a plan to tighten to 15-60 min before
  submission — no refresh-token flow, treating that as an explicit
  non-goal given the brief doesn't require it.
- Login now generates a real signed JWT; AddAuthentication/AddJwtBearer
  pipeline wiring in Program.cs is done and verified (valid token accepted,
  claims correctly read after fixing a claim-remapping issue — see below).

### JWT claim mapping bug (real .NET gotcha, not my mistake)
- First test of the protected /api/auth/me endpoint returned null for both
  claims despite a valid, accepted token (200, not 401). Root cause:
  JwtSecurityTokenHandler silently remaps short claim names ("sub", "email")
  to long legacy URIs when reading inbound tokens by default, so
  User.FindFirst(JwtRegisteredClaimNames.Sub) found nothing even though the
  token genuinely contained a "sub" claim. Fixed with
  JwtSecurityTokenHandler.DefaultMapInboundClaims = false at Program.cs
  startup. Well-documented ASP.NET Core quirk, not something either of us
  did wrong — just an easy trap on a first JWT implementation.

## Decision: skipped the Repository pattern
- Discussed with Claude whether to keep the Repository-over-DbSet pattern
  from IWMS or have the Application service call WidgetPlatformDbContext
  directly. Decided to skip Repository here — EF Core's DbSet/DbContext
  already provides repository + unit-of-work behavior, and an extra
  interface layer over it wouldn't meaningfully help this project, versus
  spending that time on the genuinely new parts (CORS, rate limiting,
  fallback chains).

## Widget CRUD + tenant isolation
- Built WidgetService/WidgetsController with every read/update/delete query
  filtering by (Id == widgetId && OwnerId == ownerId) in the same WHERE
  clause — not fetch-then-check in C# — so a cross-tenant row can't be
  returned even if a later code change forgot an explicit check.
- Controller returns 404 (never 403) for both "doesn't exist" and "exists
  but belongs to another tenant," per the design doc's rule about not
  leaking existence of other tenants' data.
- Hit a real bug during testing: sending the enum Type as a string
  ("SignupForm") failed to deserialize, because ASP.NET Core's default JSON
  serializer expects enums as integers. Fixed properly (not just worked
  around) by adding JsonStringEnumConverter in Program.cs, so the API
  accepts self-documenting string values instead of magic numbers.
- Proved tenant isolation with two real registered users end-to-end: tenant
  B correctly gets 404 reading or deleting tenant A's widget, tenant B's
  list is correctly empty, and tenant A's data is provably unaffected by
  tenant B's failed delete attempt (re-fetched afterward to confirm).

## Public submission endpoint
- Built SubmissionService/SubmissionsController: validates a submission's
  data against the target widget's own FieldsJson field definitions (name,
  type, required) rather than accepting an arbitrary blob, per the brief's
  "validate every field before it touches business logic" requirement.
- Hit a real bug: JsonSerializer.Deserialize<List<WidgetFieldDefinition>>
  on Widget.FieldsJson threw ArgumentNullException because the JSON keys
  were lowercase ("name") but my C# properties are PascalCase ("Name"),
  and System.Text.Json is case-sensitive by default. Model binding on
  incoming HTTP request DTOs handles this automatically, but a manual
  JsonSerializer.Deserialize call (like this one, on a database column) does
  not — fixed by explicitly passing PropertyNameCaseInsensitive = true.
- Added a Kestrel MaxRequestBodySize limit (16KB) so oversized payloads are
  rejected. First pass leaked a full stack trace as a raw 500 instead of a
  clean 4xx (the brief explicitly requires clean JSON errors, never a 500).
  Fixed by adding global exception-handling middleware, registered as the
  very first thing in the pipeline, that catches BadHttpRequestException
  and returns clean JSON with the correct status code (413).
- Verified all cases via .http requests: valid submission (201), missing
  required field (400 with specific message), nonexistent widget (404),
  oversized payload (413 with clean JSON body, no stack trace).

## CORS
- Built a genuinely separate "customer site" (test-site/index.html, plain
  HTML + fetch, served via `npx serve` on port 5500) to test this for real,
  since .http file requests bypass the browser entirely and don't exercise
  CORS at all — a lesson in itself, that testing via Postman/.http doesn't
  prove cross-origin behavior works.
- First run (deliberately, before adding any CORS config) confirmed the
  real failure mode: browser console showed a CORS policy error on the
  preflight OPTIONS request, and the POST never actually reached the
  server — different from a validation or auth failure, which was worth
  seeing directly rather than just being told about it.
- Added AddCors + app.UseCors(policy) globally at first to prove the
  mechanism worked, then narrowed it: moved to app.UseCors() (no default
  policy argument) plus [EnableCors("PublicWidgetPolicy")] on
  SubmissionsController only, so the owner/auth endpoints stay same-origin
  by default.
- Hit a real gotcha narrowing this: I first deleted app.UseCors(policy)
  entirely, assuming [EnableCors] alone was enough. It isn't — the CORS
  middleware has to be present in the pipeline for [EnableCors]/[DisableCors]
  attributes to mean anything; removing the middleware disabled CORS
  everywhere, including on the endpoint that needed it. Fixed by keeping a
  bare app.UseCors() (no default policy) rather than removing it outright.
- Verified end-to-end in the browser: initial failure (blocked), then
  over-broad success (worked, but /api/widgets was also improperly
  reachable), then correctly-scoped success (submissions work,
  /api/widgets correctly still CORS-blocked) — confirmed via DevTools
  Console and Network tab (saw the actual OPTIONS preflight followed by
  the real POST, with Access-Control-Allow-Origin present only where
  expected).

## Widget delivery (embed snippet, public config, versioned script)
- WidgetResponse now generates and returns an embed snippet
  (<script src=".../widget.v1.js?id=...">) built from a configurable
  App:PublicBaseUrl setting rather than hardcoding the host.
- Added GET /api/widgets/{id}/config: anonymous, CORS-enabled
  ([EnableCors] on this action specifically, since the rest of
  WidgetsController stays owner-only/same-origin), Cache-Control:
  public, max-age=60.
- Added GET /widget.v1.js as a minimal-API endpoint serving a static
  JS bundle from a const string in code (not a physical file) with
  Cache-Control: public, max-age=31536000, immutable — a genuinely
  versioned, long-cache bundle per the brief's requirement.
- The script itself: reads its own ?id from the script tag's URL, fetches
  the public config, dynamically renders a form from the widget's
  FieldsJson, and wires the form's submit to POST /api/submissions.
- Rebuilt test-site/index.html to use the REAL embed flow (a single
  <script src="...widget.v1.js?id=..."> tag) instead of the earlier
  manual fetch() button — this is the actual mechanism the brief
  describes (script tag -> config -> render -> submit), not just a proxy
  for testing CORS/submissions in isolation.
- Verified end-to-end from the second-origin test site: script loads,
  config fetches, form renders dynamically with the right fields, real
  submission succeeds and is stored, and both Cache-Control header
  values confirmed exactly correct via DevTools Network tab.

## Honeypot spam prevention
- CreateSubmissionRequest carries an optional Website field, populated by
  a hidden (off-screen, tabIndex -1, autocomplete off) form input real
  visitors never see or fill, but a bot's generic form-filler typically
  will.
- If populated, SubmitAsync returns a normal-looking 201 success without
  actually storing anything — deliberately doesn't reveal to the caller
  that it was caught, since telling a bot "your spam was detected" just
  teaches it to adapt.
- Verified via .http: a request with the honeypot field filled returns
  201 but creates no row in pgAdmin; a normal browser submission (honeypot
  stays empty) still stores correctly.
