# Home

A family organiser built for a kitchen tablet. Recipes, shopping lists, a weekly meal plan, a chore
board and LIFX light control, in one .NET 8 application.

It is a Blazor Server front end over a REST API, both in this repository, sharing a SQL Server
database.

## What you need

| | |
|---|---|
| .NET 8 SDK | The whole solution targets `net8.0`. |
| Node 20 or later | Only to build the CSS. There is no JavaScript bundler. |
| SQL Server | LocalDB ships with the SQL Server tooling, needs no service running, and is enough for development. There is no SQLite option. |

## Getting it running

Six steps. Every one is required, and the app fails in a different confusing way for each one you
skip, so do not stop early. Run everything from the repository root unless a step says otherwise.

### 1. Install the npm packages

```bash
cd Home.WebUI && npm install
```

Do this before your first build. `Home.WebUI.csproj` runs `npm run build:css` as a pre-build step,
and without `node_modules` the `tailwindcss` binary is not on the PATH. The step exits 1 and **the
entire solution build fails** with an MSB3073 that says nothing about npm.

### 2. Point the API at a database

`Home.WebApi` has no `appsettings.json` on purpose. Everything it needs comes from user secrets.

```bash
dotnet user-secrets set "databaseConnectionString" "Server=(localdb)\MSSQLLocalDB;Database=Home;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True" --project Home.WebApi
```

### 3. Create the schema

```bash
dotnet ef database update --project Home.Persistence --context PersistenceContext --startup-project Home.WebApi
```

If you used a connection string other than LocalDB in step 2, read
[the design-time factory warning](#dotnet-ef-ignores-your-connection-string) before running this.
It will quietly build the database somewhere you did not ask for.

### 4. Create a client application row

The API will not accept a sign-in from a client it does not know. Nothing creates that row for you,
so a fresh database has no way to log in until you add one.

Pick two random strings of your own. They are shared secrets between the two projects and nothing
outside this repository ever sees them.

```sql
INSERT INTO home.ClientApplication (Name, AccessToken, Secret)
VALUES ('Home Web App', 'replace-with-a-random-string', 'replace-with-another-random-string');

SELECT ClientApplicationID, Name FROM home.ClientApplication;
```

Keep the `ClientApplicationID` that comes back. It is almost always `1`, and step 5 needs it.

### 5. Give the web app its secrets

`Home.WebUI` needs six values. The three OAuth credentials have to match the row you just inserted,
or every sign-in returns 401 with no explanation on screen.

```bash
dotnet user-secrets set "apiBaseUrl" "http://localhost:57175" --project Home.WebUI
dotnet user-secrets set "OAuth:AccessToken:ClientID" "1" --project Home.WebUI
dotnet user-secrets set "OAuth:AccessToken:AccessToken" "replace-with-a-random-string" --project Home.WebUI
dotnet user-secrets set "OAuth:AccessToken:ClientSecret" "replace-with-another-random-string" --project Home.WebUI
dotnet user-secrets set "OAuth:AccessToken:GrantType" "password" --project Home.WebUI
dotnet user-secrets set "OAuth:AccessToken:Scope" "WebApp" --project Home.WebUI
```

`apiBaseUrl` must be **the origin only, with no path**. `ApiProvider` already puts `api` in front of
every route, so a value ending in `/api/` produces `.../api/api/Recipes` and every call 404s.
It is the one setting validated at startup, and only for being a well formed absolute URI.

### 6. Run both projects

```bash
dotnet run --project Home.WebApi
```

```bash
dotnet run --project Home.WebUI
```

The API listens on `https://localhost:57174` and `http://localhost:57175`, with Swagger at
`/swagger`. The web app listens on `https://localhost:7019` and `http://localhost:5251`. Open the
web app, and the sign-in page offers to set up a household on a database with no users in it.

`.claude/launch.json` defines both for anyone driving the app through an agent.

### Optional: LIFX lights

The Lights page renders an "unavailable" state without a token, so this is only needed to drive real
bulbs. The normal path is the in-app **Settings** page: paste a token from
https://cloud.lifx.com/settings and it is stored against your household. A user secret works as a
developer fallback, and the household token overrides it.

```bash
dotnet user-secrets set "lifxApiToken" "<token>" --project Home.WebApi
```

## When it does not look right

### The app looks different on a different machine

Two causes, and both are worth ruling out before assuming something is broken.

**The fonts come from Google Fonts over the network.** `App.razor` loads Inter and Fraunces from
`fonts.googleapis.com`. Fraunces is the display face on every heading and carries most of the
app's character. On a machine behind a corporate proxy, or offline, both requests fail silently and
the headings fall back to Georgia while the body text falls back to the system UI font. Nothing
errors, it just stops looking like itself. Check the Network tab for two blocked font requests.

**The layout switches on window shape, not width alone.** The `rail:` breakpoint is
`(min-width: 768px) and (orientation: landscape)`. In landscape you get the left navigation rail and
two columns. Anything narrower or taller than it is wide gets a bottom bar and a single column,
which is the deliberate phone and upright-tablet layout. A window that is not wide and landscape is
a different design, not a broken one.

### The stylesheet is generated, and it is not in the repository

`Home.WebUI/wwwroot/css/app.css` is built by Tailwind from `wwwroot/css/input.css` and is
gitignored. Edit `input.css`, never `app.css`. The build regenerates it, or you can run it alone:

```bash
cd Home.WebUI && npm run build:css
```

Tailwind only emits a rule for a class it can find in the files listed under `content` in
`tailwind.config.js`, which includes `.cs` as well as `.razor`. A class name assembled at runtime
from pieces will not be found, and its rule will be dropped. If an icon renders as a plain grey
square, that is why: the mask rule was purged.

The stylesheet is served without a cache-busting query string, so a browser that has already seen
one version can hold on to it. Hard reload if a CSS change does not appear.

<a id="dotnet-ef-ignores-your-connection-string"></a>
### `dotnet ef` ignores your connection string

`Home.Persistence` has a `PersistenceContextDesignTimeFactory`, and EF prefers an
`IDesignTimeDbContextFactory` over anything the startup project would give it. So
`dotnet ef database update --startup-project Home.WebApi` does **not** read the
`databaseConnectionString` secret you set in step 2. It uses the factory's own default, which is
LocalDB.

If your real database is anywhere else, set the environment variable the factory reads:

```bash
HOME_DESIGNTIME_CONNECTIONSTRING="Server=.\;Database=Home;Trusted_Connection=True;TrustServerCertificate=True" dotnet ef database update --project Home.Persistence --context PersistenceContext --startup-project Home.WebApi
```

This has already cost one migration applied to the wrong database.

### Signing in returns 401 and the page says nothing useful

Almost always step 4 or step 5. Check, in order:

1. There is a row in `home.ClientApplication`.
2. `OAuth:AccessToken:AccessToken` and `OAuth:AccessToken:ClientSecret` match that row's
   `AccessToken` and `Secret` exactly.
3. `OAuth:AccessToken:ClientID` matches its `ClientApplicationID`.
4. The API is running, and `apiBaseUrl` points at it with no trailing path.

The API records every rejected request in `home.ApiAuditEntry` with the reason, which is the fastest
way to tell a bad client credential from a bad password.

## Working on it

```bash
dotnet test
```

```bash
dotnet ef migrations add [Explanation] --project Home.Persistence --context PersistenceContext --startup-project Home.WebApi
```

`API.slnf` and `WebApp.slnf` are solution filters for working on one side without loading the other.

| Project | Role |
|---|---|
| `Home.Domain` | EF entities and enumerations. Properties only, no behaviour. |
| `Home.Application` | Input ports, interactors, output port interfaces, validators. Interactors are `internal`. |
| `Home.Persistence` | `PersistenceContext`, EF configurations, migrations. |
| `Home.WebApi` | Controllers, presenters, API models. Controllers only invoke the pipeline. |
| `Home.WebUI` | Blazor Server, Tailwind, the `Home*` component library. |
| `Home.Application.Tests` | xUnit, Moq and FluentAssertions. |

Dependencies point inwards, and `Home.Domain` references nothing. The application layer is built on
`CleanArchitecture.Mediator`, which resolves from the committed `packages/` folder through
`nuget.config` rather than from nuget.org.

Four documents carry the reasoning, and they are worth reading before changing anything structural:

| | |
|---|---|
| `.claude/VISION.md` | What the product is for. |
| `.claude/DECISIONS.md` | Every significant decision, dated, with its reason. |
| `.claude/ROADMAP.md` | What is planned and what state it is in. |
| `.claude/skills/home-conventions/` | The coding conventions, and `references/known-gaps.md` for what the repository deliberately does not do. |

## Licence

MIT. See `LICENSE`.
