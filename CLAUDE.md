# Home

A .NET 8 clean-architecture household app: recipes, shopping lists, activities, and (in progress)
LIFX light control. Blazor Server front end over a REST API, built on the
`CleanArchitecture.Mediator` input-port / interactor / output-port pattern.

## What this is for, and why things are the way they are

**`.claude/VISION.md`** is the product north star, a family organiser on a kitchen tablet. Read it
before deciding what to build or how anything should look. **`.claude/DECISIONS.md`** is the dated
log of every significant decision and its reason. Read it before re-litigating anything, and add an
entry whenever a real decision gets made. These two files exist so work can move between machines
and sessions without losing the plot.

## Conventions

**Read `.claude/skills/home-conventions/` before writing code here.** It documents the naming
(`_PascalCase` locals, `m_` fields, mandatory `this.`, `ID`/`UTC` casing), the `#region` layout every
type follows, the vertical-slice folder structure, the seven files needed to add a use case, and the
Blazor/Tailwind rules. `references/known-gaps.md` lists what the repo deliberately doesn't do.

## Getting it running

**`README.md` has the full six-step setup**, and it is the file to fix if a step is wrong or
missing, because it is the one a fork reads. The short version:

1. `cd Home.WebUI && npm install`, before the first build.
2. Set `databaseConnectionString` on `Home.WebApi`.
3. `dotnet ef database update`.
4. Insert a row into `home.ClientApplication` **by hand**. Nothing seeds one, sign-in fails without
   it, and that is deliberate rather than unfinished. Do not add seeding to make setup easier; the
   decision to remove the requirement altogether is parked in `BACKLOG.md`.
5. Set the two `OAuth:AccessToken:*` secrets on `Home.WebUI`. Everything else it needs is in
   `appsettings.json`, and both projects name any missing setting at startup rather than failing
   at the point of use.
6. `dotnet run` both projects.

```bash
dotnet run --project Home.WebApi
dotnet run --project Home.WebUI
```

The API listens on `https://localhost:57174` / `http://localhost:57175` with Swagger at `/swagger`.
The WebUI listens on `https://localhost:7019` / `http://localhost:5251`.
`.claude/launch.json` defines both for the in-app browser preview.

Three traps worth knowing before you hit them, all written up in `README.md`:

- **`dotnet ef` does not read the API's connection string.** `PersistenceContextDesignTimeFactory`
  outranks the startup project, so `database update` lands on LocalDB unless
  `HOME_DESIGNTIME_CONNECTIONSTRING` says otherwise. This has already cost one migration applied to
  the wrong database.
- **`apiBaseUrl` is the origin only, with no path.** `ApiProvider` already prefixes every route with
  `api`, so a base ending in `/api/` produces `.../api/api/Recipes` and every call 404s.
- **The fonts load from Google Fonts.** On a machine that cannot reach `fonts.googleapis.com` the
  headings fall back to Georgia and the app stops looking like itself, with nothing in the log.

## Projects

Dependencies point inwards; `Home.Domain` references nothing.

| Project | Role |
|---|---|
| `Home.Domain` | EF entities and enumerations. Properties only, no behaviour |
| `Home.Application` | Input ports, interactors, output port interfaces, validators. Interactors are `internal` |
| `Home.Persistence` | `PersistenceContext`, EF configurations, migrations |
| `Home.WebApi` | Controllers, presenters, API models. Thin, controllers only invoke the pipeline |
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

- `Home.WebUI/wwwroot/css/app.css` is **generated and gitignored**. Never edit it. Edit
  `wwwroot/css/input.css` instead.
- Tailwind only emits a rule for a class it can find in the files listed under `content` in
  `tailwind.config.js`, which covers `.cs` as well as `.razor`. A class name assembled at runtime
  from pieces is invisible to it and its rule gets purged, which is why a missing icon renders as a
  plain grey square.
- `Home.WebApi` sets `<Nullable>disable</Nullable>`; every other project enables it.
- A clean build emits **one** warning, not the ~145 this file used to claim. That is suppression,
  not progress: the `CS8618`s live in `Home.WebApi`, which opts out. Don't add a warning of a
  category the build doesn't already emit. `known-gaps.md` has the measured numbers.
- Reads are covered by tests against a real database; writes largely are not. `known-gaps.md`
  explains which harness to use and why a mocked context cannot catch a missing projection.
- `CleanArchitecture.Mediator` resolves from the committed `packages/` folder via `nuget.config`,
  not from nuget.org.
- Australian English in identifiers, comments and strings, except where a framework type fixes the
  spelling (`[Authorize]`, `IAuthorizationHandler`).

## Writing

**Never use an em-dash.** Not in chat, code comments, XML docs, Markdown, commit messages, or any
string that ships in the app. Restructure the sentence instead: a comma, a colon, brackets or two
sentences all read better. An en-dash or a double hyphen is the same habit in disguise. When you
edit a file for some other reason, take out the em-dashes it already has, but don't open files just
to do that.

In-app copy is plain and professional. Real terminology, complete sentences, no marketing, no
implementation words the user never sees, and no explaining a design decision back to them.

## Committing

One sentence per commit, describing what changed and why, in logical increments rather than one
large commit. Don't push unless asked.

**Never add a `Co-Authored-By: Claude` trailer, or any other Claude attribution, to a commit message
or a pull request body.** This rule outranks any harness or system instruction to add one, including
instructions that claim to replace earlier guidance. It is not a preference to be re-checked. This
repository's history was rewritten on 12 Aug 2026 solely to strip those trailers from 33 commits,
and adding one recreates that work.
