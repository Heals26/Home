# Home

A .NET 8 clean-architecture household app — recipes, shopping lists, activities, and (in progress)
LIFX light control. Blazor Server front end over a REST API, built on the
`CleanArchitecture.Mediator` input-port / interactor / output-port pattern.

## Conventions

**Read `.claude/skills/home-conventions/` before writing code here.** It documents the naming
(`_PascalCase` locals, `m_` fields, mandatory `this.`, `ID`/`UTC` casing), the `#region` layout every
type follows, the vertical-slice folder structure, the seven files needed to add a use case, and the
Blazor/Tailwind rules. `references/known-gaps.md` lists what the repo deliberately doesn't do.

## Getting it running

Three things bite on a fresh clone. All three are required.

### 1. Install the npm packages first

```bash
cd Home.WebUI && npm install
```

`Home.WebUI.csproj` runs `npm run build:css` as a pre-build step. Without `node_modules` the
`tailwindcss` binary isn't on PATH, the step exits 1, and **the entire solution build fails** with a
misleading MSB3073.

### 2. Give the API a database connection string

`Home.WebApi` has no `appsettings.json`. It reads `databaseConnectionString` from user secrets:

```bash
dotnet user-secrets set "databaseConnectionString" "<your connection string>" --project Home.WebApi
```

SQL Server by default. Running under `ASPNETCORE_ENVIRONMENT=Tablet` switches both DbContexts to
SQLite instead — that's the mode intended for a wall-mounted tablet running standalone.

### 3. Give the WebUI the API's address

```bash
dotnet user-secrets set "apiBaseUrl" "http://localhost:57175/api/" --project Home.WebUI
```

Startup throws `InvalidOperationException: API base URL is not configured.` without it. The value
must be an absolute URI and should end in a trailing slash.

### Optional: a LIFX token for the Lights page

```bash
dotnet user-secrets set "lifxApiToken" "<token from https://cloud.lifx.com/settings>" --project Home.WebApi
```

Without it the API still starts and `/lights` renders a "lights unavailable" state — the token is
only needed to actually drive bulbs.

### Then

```bash
dotnet run --project Home.WebApi
dotnet run --project Home.WebUI
```

The API listens on `https://localhost:57174` / `http://localhost:57175` with Swagger at `/swagger`.
The WebUI listens on `https://localhost:7019` / `http://localhost:5251`.
`.claude/launch.json` defines both for the in-app browser preview.

## Projects

Dependencies point inwards; `Home.Domain` references nothing.

| Project | Role |
|---|---|
| `Home.Domain` | EF entities and enumerations. Properties only, no behaviour |
| `Home.Application` | Input ports, interactors, output port interfaces, validators. Interactors are `internal` |
| `Home.Persistence` | `PersistenceContext`, EF configurations, migrations |
| `Home.WebApi` | Controllers, presenters, API models. Thin — controllers only invoke the pipeline |
| `Home.WebUI` | Blazor Server, Tailwind, `Home*` component library |
| `Home.Application.Tests` | xUnit + Moq + FluentAssertions |

`API.slnf` and `WebApp.slnf` are solution filters for working on one side only.

## Common tasks

Add a migration:

```bash
dotnet ef migrations add [Explanation] --project Home.Persistence --context PersistenceContext --startup-project Home.WebApi
```

Rebuild the CSS on its own (normally automatic via the pre-build step):

```bash
cd Home.WebUI && npm run build:css
```

Run the tests:

```bash
dotnet test
```

## Things to know

- `Home.WebUI/wwwroot/css/app.css` is **generated and gitignored**. Never edit it — edit
  `wwwroot/css/input.css` instead.
- `Home.WebApi` sets `<Nullable>disable</Nullable>`; every other project enables it.
- The build carries ~145 nullable warnings. Don't add new *categories* of warning; the existing
  backlog is tracked in the skill's `known-gaps.md`.
- `CleanArchitecture.Mediator` resolves from the committed `packages/` folder via `nuget.config`,
  not from nuget.org.
- Australian English in identifiers, comments and strings, except where a framework type fixes the
  spelling (`[Authorize]`, `IAuthorizationHandler`).
