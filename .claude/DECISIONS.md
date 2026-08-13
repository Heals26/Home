# Decision log

*Why the code is the way it is. Newest first. Every entry: what was decided, why, and what it means
for anyone writing code later. When a decision is reversed, don't delete the entry — add a new one
that supersedes it. See `VISION.md` for what the product is; see `docs/HANDOVER.md` for the
12 Aug 2026 point-in-time state.*

## 2026-08-13 — Registration is first-run only

`POST api/Households/register` (anonymous) creates the household and its first member in one
step, and refuses with 409 the moment any user exists — the login page offers "Set up your
household" only while `setup-status` says the database is empty. Reason: a fresh install must be
usable without CLI or Swagger, but an open registration endpoint on a possibly-internet-facing
app is a hole. Additional family members are added from inside the app (CreateUser API exists;
a Members section in Settings is the obvious future home — no UI yet). No auth bypass was added
for local use: sessions persist via refresh tokens, and a bypass flag would be a foot-gun given
cloud hosting is still an open option.

## 2026-08-13 — Components use .razor.cs code-behind

Mitch: component logic lives in a `.razor.cs` partial class beside the markup so the C# language
server can analyse it — inline `@code` blocks get little to no LSP support in most editors. Markup
and directives (`@page`, `@inject`, `@typeparam`) stay in the `.razor`; everything else moves to
the partial. Supersedes the earlier inline-`@code` convention.

## 2026-08-13 — Household settings live in SQL, not user secrets

Mitch: assume cloud-hosted SQL storage. Household-wide settings (name, latitude/longitude for
future sunrise/sunset triggers, the LIFX API token) are stored on the household row and edited
from the Settings page — setup must not be a CLI exercise. The token is write-only through the
API: GET returns `HasLifxApiToken`, never the value. `lifxApiToken` in user secrets remains as a
developer fallback when the household has no token stored.

## 2026-08-13 — The design system: warm ink neutrals, Fraunces display, pillar hues

The zinc/teal look was the stock "dark dashboard with a single accent" — indistinguishable from
template output. Replaced with: warm stone neutrals (`ink` scale), Fraunces as an editorial
display face over Inter UI text, light-on-dark primary buttons, and one hue per pillar
(recipes apricot, shopping sage, week sky, lights amber) used for identity only — a family member
navigates by colour without reading. The dashboard is a live "family board" (glance, don't
navigate), and `HomeNavRail` keeps every page one tap from anywhere, so no screen is a dead end.

## 2026-08-13 — UI direction: upgrade, tablet-first, not generic

Mitch: the UI should be "upgraded, not generic, functional, good UX, and won't make a user
frustrated". Combined with the product vision (kitchen tablet, family-proof — see `VISION.md`),
this answers the open design question from the 12 Aug handover: the existing dark zinc/teal
language is the starting point, but the bar is a deliberate, product-specific design — not
extending template defaults. Scenes/Schedules/Effects screens and any reworked pages are built
against that bar.

## 2026-08-13 — Stashed desktop work triaged, not merged wholesale

A GitHub Desktop stash on the desktop clone held pre-rewrite local work. Most of it had been
independently superseded by the remote's Activities feature, so it was *not* applied. The genuinely
unique pieces were ported by hand: the TaskCompletionSource-gated `AuthorisationService` (holds
`AuthorizeRouteView` in its `Authorizing` slot until JS interop can read storage — kills the
"not authorised" flash on load), fresh `HttpRequestMessage` per send attempt (reuse across a 401
retry throws), Basic client credentials on token refresh, and sign-out on failed refresh. The full
stash is preserved on branch `backup/stashed-local-work` if anything else turns out to matter.
`start.bat` was deliberately left behind (`start.ps1` and `.claude/launch.json` cover it).

## 2026-08-13 — Commit messages carry no AI co-author trailers

The 12 Aug history rewrite existed solely to strip `Co-Authored-By: Claude` trailers from 33
commits. Don't add them to new commits; that recreates the problem the rewrite fixed.

## 2026-08-12 — History rewritten; stale clones reset, never merged

Every commit from 18 May 2026 onward has a new SHA (content identical, trailers stripped). A stale
clone that still has the old chain must `git fetch origin && git reset --hard origin/master` — a
pull produces a giant self-merge of identical content (this bit the desktop clone on 13 Aug; it was
recovered by exactly that reset). The `main` branch was deleted; `master` is the only branch.

## 2026-08-12 — Home owns light grouping, not LIFX

A sync refreshes a bulb's name and state but never moves it between Home groups. Reason: the
family's mental model of the house ("kitchen", "kids' rooms") belongs to Home, not to whatever the
provider app happened to be configured with. A test pins this. Related: a whole room is one API
call (LIFX accepts 25 comma-separated selectors), so Home-defined groups cost nothing extra.

## 2026-08-12 — An unreachable provider is a return value, not an exception

`ILightService` returns `null` or a `LightCommandResult`; the presenter maps that to a 503.
Adapters catch `HttpRequestException`/`TaskCanceledException`/`JsonException` themselves, and 429s
are logged with their rate-limit headers and treated as unavailable. Reason: a kitchen tablet must
degrade gracefully — "lights unavailable" is a state, not a crash. The same rule applies to any
future external integration.

## 2026-08-12 — Vendor wire types stay in the adapter

`LifxLight` and friends never escape `Home.WebApi/Infrastructure/Lights/`; they map to
`LightSnapshot` at the boundary. Use cases never learn which vendor is on the other end. This is
the template for every future smart-home integration.

## 2026-08-12 — Light effects are gated on detected hardware capability

Capabilities are read from `product.capabilities` on sync; the UI offers only what the bulb can do.
Move/morph/flame are excluded (they need multizone strips or tiles). Reason: offering a control
that silently does nothing is exactly the frustration the product exists to avoid.

## 2026-08-12 — Nothing reads the clock directly

Everything resolves `TimeProvider` (.NET 8) — interactors via `serviceFactory`, services via
constructor, Razor via the global inject. Reason: testability (`FakeTimeProvider` with exact-time
asserts) and consistent "now" within a render. `DateTime.UtcNow`/`.Now` appear only in migrations.

## 2026-08-12 — SQLite path deleted; SQL Server only

The migrations are SQL Server-shaped (filtered indexes etc.) and SQLite rejects them. LocalDB
serves local dev. Consequence: distribution will likely want Docker Compose with Postgres one day,
which would reopen this — that's the known trade.

## 2026-08-12 — Two cascade paths deliberately removed

`LightSceneState` does not cascade from `Light` (SyncLights clears scene entries itself), and
`LightSchedule` hangs off its scene without a second `Household` FK. SQL Server rejects the
multiple-cascade-path graph otherwise. Adding either back breaks the migration.

## 2026-06 — The audit table is polymorphic on purpose

`ResourceTypeSE` enum + `long EntityID`, no FK. The database can't enforce it, but audit rows must
outlive the entities they describe, and a per-table audit design creates FK cycles.

## 2026-05/06 — MudBlazor stripped; Tailwind + an owned component library

Every UI element is either a `Home*` component or raw Tailwind utilities. Reason: owning the design
language end-to-end (see the 2026-08-13 UI direction entry — this decision is what makes
"not generic" achievable). Icons are CSS masks in `input.css`, no icon library. Dark zinc/teal
palette; `darkMode: false` because dark *is* the palette.

## 2025-09 → — Clean architecture on CleanArchitecture.Mediator, vertical slices

Input port → pipeline (auth → validation → interactor) → output port, one folder per use case in
every layer, interactors `internal`, controllers thin, presenters map to HTTP. The package resolves
from the committed `packages/` folder via `nuget.config`, not nuget.org. The seven-file recipe for
a new use case is in `.claude/skills/home-conventions/references/use-case-slice.md`.
