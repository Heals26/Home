# Handover — 12 August 2026

Written at the end of a long session on the Surface Pro so the context survives to another machine.
Read `CLAUDE.md` for setup and `.claude/skills/home-conventions/` for how the code is written; this
file is only about **where things stand and what's left**.

---

## Read this first if you have another clone

**`master` history was rewritten on 12 August.** Every commit from 18 May onward has a new SHA — the
`Co-Authored-By: Claude …` trailers were stripped from 33 commits. Content is byte-identical; only
the messages changed.

Any other clone still has the old chain and **must not be merged back in**. Fix it with:

```bash
git fetch origin && git reset --hard origin/master
```

or just re-clone. Do not `git pull` into a stale clone.

`origin/master` is at `404d3a2`. Branch protection has been re-enabled, so a future history rewrite
needs the ruleset disabled first (**Settings → Rules → Rulesets**, not the older Branches page —
`GH013` only comes from rulesets).

---

## State of the repo

| | |
|---|---|
| Build | clean, 0 errors |
| Tests | 47 passing (`dotnet test`) |
| Warnings | 153, all nullable-reference (`CS8618` and friends) |
| Migrations | 12, all applied to LocalDB |
| Branches | `master` only — `main` was deleted 12 Aug (its one unique commit was a merge whose parents were both already on master) |

Local database is SQL Server **LocalDB**: `(localdb)\MSSQLLocalDB`, database `Home`. No service to
start. The SQLite path was removed — the migrations are SQL Server-shaped and SQLite rejects them.

---

## What got built

Lights went from three orphaned entities to a working feature across five stages, all pushed:

1. **Sync** — pulls bulbs from LIFX into Home's own tables and caches their state, so opening the
   Lights page costs no provider calls.
2. **Groups** — create, rename, reorder, delete, move bulbs between them. Home owns the grouping.
3. **Scenes** — capture a look, recall it.
4. **Effects** — breathe, pulse, cancel.
5. **Schedules** — fire a scene at a time of day on chosen days, via a background runner.

Plus, along the way: AutoMapper off a CVE, Tailwind 2→3 (which fixed seven touch-target classes that
were generating no CSS at all), a test project, a CI workflow, `CLAUDE.md`, a conventions skill, and
every clock read moved to `TimeProvider`.

**None of it has touched a real bulb.** There is no LIFX token configured. That is the first real
test and where surprises will be.

---

## Outstanding

### 1. UI — the big one

Scenes, Schedules and Effects have **complete, tested APIs and no screens at all**. Only Lights has a
UI (`Components/Pages/Lights/`, split into `LightsPage`, `LightGroupCard`, `LightControlCard`).

There is an **open design question** that should be answered before building: the current look is a
dark zinc/teal dashboard. Mitch asked for it to "not look AI generated" — unclear whether that means
extend the existing language or actually reconsider it. Those are different jobs.

### 2. Setup has to stop being a CLI exercise

Three settings are `dotnet user-secrets` commands today:

- `databaseConnectionString` (API)
- `lifxApiToken` (API)
- `apiBaseUrl` (WebUI)

Fine for a developer, impossible for anyone else. Given the goal is "anyone downloads this and runs
it on their home network", a first-run setup screen is the single biggest gap. It is also where
latitude/longitude for sunrise/sunset lands, so it blocks item 3.

### 3. Sunrise / sunset triggers

Deliberately not built — they need the household's latitude and longitude, which Home does not
collect. **No API or daily lookup is required**: sunrise and sunset are computed from lat/long and
the date with the NOAA solar position algorithm, offline, in about fifty lines. Browser timezone is
not enough on its own, since a timezone spans thousands of kilometres of longitude. Ask once during
setup.

### 4. Deployment

Runs on Mitch's main machine or a cloud host, so the schedule runner having to stay alive is fine.
For distribution, LocalDB is a developer convenience — expect this to want Docker Compose with
Postgres, which would revisit the decision to delete the SQLite path.

### 5. Debt, not urgent

- **153 nullable warnings**, nearly all `CS8618` on entities and API models. Clearing them means
  `required` modifiers or annotations across the entity layer. Real work, not a drive-by.
- **Test coverage is Lights-shaped.** Activities, Shopping Lists, Users and OAuth have no tests, and
  there are none for WebApi presenters or WebUI components.

---

## Decisions already made — please don't re-litigate

- **Home owns light grouping, not LIFX.** A sync refreshes a bulb's name and state but never moves
  it back to the provider's group. There is a test pinning this.
- **A whole room is one API call.** LIFX accepts 25 comma-separated selectors, so a Home-defined
  group costs the same as a native one. Rate limit is 120 requests per 60 seconds per token; 429s
  are logged with the rate-limit headers and treated as unavailable so the caller backs off.
- **An unreachable provider is a return value, not an exception.** `ILightService` returns null or a
  `LightCommandResult`; the presenter turns that into a 503.
- **Vendor types stay in the adapter.** `LifxLight` never escapes
  `Home.WebApi/Infrastructure/Lights/`.
- **Effects are gated on detected hardware capability**, read from `product.capabilities` on sync.
  Move, morph and flame are excluded — they need multizone strips or tiles.
- **Nothing reads the clock directly.** Everything uses `TimeProvider`.
- **The audit table is polymorphic on purpose** (`ResourceTypeSE Entity` + `long EntityID`). The
  database cannot enforce it, but audit rows should outlive what they describe, and it avoids cycles.

## Traps

- **`Note.CreatedOnUTC` has a scaffold-time default that regenerates on every migration.** Every
  `dotnet ef migrations add` will contain a spurious `AlterColumn` for it. Delete that operation
  before committing or the noise compounds.
- **Two cascade paths were removed to keep SQL Server happy.** `LightSceneState` does not cascade
  from `Light` (SyncLights clears scene entries itself), and `LightSchedule` hangs off its scene
  rather than carrying a second `Household` link. Adding either back will break the migration.
- **A `static` helper cannot capture a primary-constructor parameter** (`CS9105`). Pass the value in.
- **An AutoMapper `Profile` is constructed without DI**, so anything time-dependent needs an
  `IValueResolver` — see `Infrastructure/AutoMapper/Resolvers/TokenExpiresInResolver.cs`.
- **`npm install` in `Home.WebUI` before the first build**, or the whole solution fails with a
  misleading MSB3073.
