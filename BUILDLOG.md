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
  pipeline wiring in Program.cs is the next step, not yet done.
