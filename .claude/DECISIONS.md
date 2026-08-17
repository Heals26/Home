# Decision log

*Why the code is the way it is. Newest first. Every entry: what was decided, why, and what it means
for anyone writing code later. When a decision is reversed, don't delete the entry — add a new one
that supersedes it. See `VISION.md` for what the product is; see `docs/HANDOVER.md` for the
12 Aug 2026 point-in-time state.*

## 2026-08-17 — The whole AutoMapper configuration is asserted in a test

AutoMapper only validates a map the first time it is used, so a missing one is invisible until
the screen that needs it returns a 500 — which had happened more than once, most recently on
`GetShoppingList`, whose `Items` had no element map at all. `MapperConfigurationTests` now builds
the configuration from the same four assemblies `Program.cs` registers and calls
`AssertConfigurationIsValid`, turning that into a build failure. Two notes for whoever touches it:
the assembly list must stay in step with `Program.cs` or profiles go unchecked (a second test
asserts profiles are actually found, so the assertion can't silently pass over nothing), and the
exception is caught and asserted as a string because `AutoMapperConfigurationException` does not
survive the test runner's serialisation — a test that lets it escape vanishes from the run
instead of failing it. It immediately found four faults: the missing shopping list item map,
`User.Household` unmapped on both user profiles, and `UpdateUserApiRequest -> UpdateUserInputPort`
having no usable constructor. That last one is why `UsersController` now builds its input ports
directly, like every other controller — mapping onto a positional record is fragile, because
`ForMember(...).Ignore()` cannot ignore a constructor parameter.

## 2026-08-15 — Every relationship is configured explicitly, or EF quietly invents a bad one

Deleting a recipe failed on `FK_RecipeStep_Recipe_RecipeID`. The cause was an *absence*:
`RecipeStepConfiguration` never declared the relationship, so EF inferred one from `Recipe.Steps`
— and because `RecipeStep` carries no back-navigation, it inferred it as **optional with no
cascade**, giving a nullable `RecipeID` the database then used to block every delete. Deleting a
recipe with steps had never worked. The giveaway is the constraint name: EF's default
`FK_Child_Parent_Column` shape instead of this repo's `FK_Child_Parent`, so that naming
difference is a reliable way to find unconfigured relationships. A sweep found exactly one other
— `Audit → User`, which blocked deleting any member who had ever done anything. Steps now cascade
from the recipe (a single path: the household already reaches them through it); the audit link is
`SetNull`, because history outlives the person and `Audit.UserName` is denormalised onto the row
for exactly that reason. Rule: configure every relationship explicitly, and never trust an
inferred one — a missing configuration is silent until a delete fails in front of the family.

## 2026-08-15 — There is a light theme now, but dark is still the default

Supersedes the 13 Aug "dark only, no toggle" decision. Mitch listed "No light mode" as a
complaint, so light is now an opt-in **per-device** preference in Settings → Appearance:
Dark / Light / Match device, stored in `localStorage` under `home-theme`. Dark remains what an
unconfigured device gets — a kitchen tablet that already lives on the wall must not change
appearance because someone else's phone chose otherwise, and a device with JavaScript off or
storage blocked falls through to dark as well. The theme is **tokenised, not duplicated**: the
`ink` scale, the five pillar hues and the `surface` aliases moved out of `tailwind.config.js`
into CSS custom properties on `:root` (dark) and `:root[data-theme="light"]`, declared as
`rgb(var(--token) / <alpha-value>)` so the ~400 existing utilities — including opacity
modifiers like `bg-week/10` and `border-lights/40`, which a plain `var()` would have broken —
work in both themes with **no component markup changed**. Read the ink scale by role, not by
lightness: 950 is the page, 900 the surface, 800 raised fills and borders, 50 the primary text.
Light inverts the ramp, so those roles still hold. The pillar hues could not simply be reused —
sky `#7dd3fc` is about 1.4:1 on paper — so each has a darkened light-theme variant that keeps
its identity and clears 4.5:1. The stored choice is applied by a synchronous inline script in
`App.razor` before the body renders; the app renders with `prerender: false`, so there is no
server-side pass to put it on and an inline script is the only thing that beats the first paint.
Rule for later: a new colour goes in `input.css` as a token pair, never as a hex in the config
or in markup.

## 2026-08-15 — A household session is expected to last months, not an hour

Mitch: "If I close the application or browser I have to relog back in. I should not have to."
Three independent faults, all fixed together because fixing one alone changes nothing visible.
(1) The refresh request sent `grant_type` read from the *sign-in* config key, so it always said
`password`; the API routed it into the password branch and 401'd — **token refresh had never
worked once**. (2) Any 401 was treated as fatal, so a 5xx, a timeout or an API that had not
finished starting destroyed a valid refresh token; only an explicit 401/400 *from the token
endpoint* may now sign anyone out. (3) Nothing refreshed at startup, so an expired access token
meant the login page even with a good refresh token in storage — startup now refreshes before
completing initialisation, so `AuthorizeRouteView` holds its Authorizing slot instead. Sessions
carry an absolute expiry, refresh is serialised through one semaphore (the dashboard's six
parallel loads previously raced and consumed each other's single-use token), and the data
protection key ring is pinned with `SetApplicationName` so moving the folder no longer silently
invalidates every device. Refresh tokens now live 90 days and slide. Rule: never let a transport
failure reach `SignOutAsync`.

## 2026-08-15 — Board columns belong to the household, and are named for a home

`ActivityState` was a global lookup seeded with Todo/Refining/Progressing/Blocked/Testing/Done —
software-process jargon on a family board, and the one table the 14 Aug isolation sweep could not
scope. It now carries `HouseholdID`, `Sequence` and `IsComplete` (which column means finished, so
a card moved there stops appearing on the dashboard). Existing columns were **renamed, not
replaced**, so every card stayed where the family left it; new households get
To do → Doing → Waiting on → Done from `IHouseholdSetupLogic`, which also seeds the meal slots.
Seeding moved out of `Program.cs`: a global row is now unreachable by every scoped query.

## 2026-08-15 — One "meal" vocabulary, not two

`MealSlot` is household-defined and serves both jobs: which meal a `MealPlanEntry` is for
(nullable one-to-many) and how the recipe book is filtered (`RecipeMealSlot`, many-to-many —
pancakes are breakfast *and* dessert). Two separate concepts for "dinner" would have drifted
apart in the family's head. `MealPlanEntry → MealSlot` is Restrict, not Cascade: the household is
already reached through the recipe, and a second cascade path is rejected by SQL Server — refusing
to delete a slot still holding a week of dinners is also the behaviour a family wants.

## 2026-08-15 — Migrations against a live family database are additive and rehearsed

The database now holds real data, so the earlier "no rows existed" safety net is gone. This
migration drops **nothing**: measurement units arrived as new `Amount`/`Unit` columns beside the
old unitless `Quantity`/`Volume`/`Weight`, which stay until the move is proven. It was rehearsed
by restoring a copy of the live database and applying it there — which caught a real defect: the
session-expiry backfill was conditional, and because the column default stamps the migration time
the condition never matched, so every existing session would have been born expired. Rule for
later: rehearse a data-moving migration against a restored copy, and read what it actually did.

## 2026-08-14 — Every interactor is scoped to the caller's household

Roughly forty interactors loaded entities by raw ID (`Find<T>(id)`), so any authenticated user
could read, change or delete another household's recipes, lists, activities, members and notes
by guessing IDs. Every lookup now filters through the entity's ownership path to
`IAuthorisationService.GetHousehold()` (e.g. `i.ShoppingList.Household.HouseholdID`), and each
interactor keeps its previous not-found/no-op behaviour so nothing leaks which IDs exist. Found
in the same sweep: CreateUser saved members with **no household at all** (orphans invisible to
every scoped query — now attached to the caller's household), and UpdateShoppingListItem never
called SaveChangesAsync and dereferenced an unloaded navigation, so item updates could never
persist. Rule for later: any interactor that takes an ID must scope it to the household — an
unscoped `Find` is a cross-household hole, not a shortcut.

## 2026-08-14 — Live cross-device updates go through a hub on the API, not in-process events

Mitch: don't assume one Blazor Server instance (Azure auto-scale), and client satisfaction beats
battery when they conflict. So change notifications relay through `ChangeNotificationsHub` on
the API: pages publish after successful mutations, every device in the household sees the change
instantly, and the background light sync pushes too — a wall-switched light now appears without
anyone tapping Sync. Security: the hub derives the SignalR group from the caller's authenticated
claims — a client can neither choose nor spoof a household. Sockets: one shared WebSocket per
household per WebUI instance (never per circuit — the historical TCP-exhaustion trap), WebSockets
only so it can never degrade into long-polling churn, closed when the last subscriber leaves; all
connections are server-to-server, so devices carry nothing extra. The dashboard's poll dropped to
a five-minute fallback for hub outages. If hosting lands on Azure with API scale-out, Azure
SignalR Service is a one-line `.AddAzureSignalR()` swap.

## 2026-08-14 — Meal planning is the connective tissue, not a fifth pillar

`MealPlanEntry` (a recipe on a calendar day, reached through the recipe to keep one cascade
path) powers /meal-plan, the dashboard's "Tonight" hero tile, and "add week to list" — which
funnels the planned window's ingredients into a shopping list server-side, deduplicating a
recipe planned twice (doubling quantities is the shop's decision, not the app's). Reason: the
vision's dashboard question "what's for dinner" had no answer anywhere, and this makes recipes,
shopping and the board reinforce each other rather than stay three separate mini-apps.

## 2026-08-14 — Recipe import reads JSON-LD only, and fails honestly

POST api/Recipes/Import fetches a page and reads the schema.org Recipe most cooking sites embed
as JSON-LD (`JsonLdRecipeImportService`, regex + System.Text.Json, no scraping packages). If a
page carries no structured recipe, the import returns a 422 with a plain explanation instead of
guessing at HTML — a wrong-looking import erodes trust faster than a failed one. Ingredient
lines stay whole ("2 cups flour") because splitting quantities reliably is a losing game.

## 2026-08-14 — The board stays fresh by itself: background sync, sun triggers, auto-refresh

The bulb-list reconcile moved out of SyncLightsInteractor into shared `ILightSyncLogic`, and a
second hosted runner (`LightStateSyncRunner`, five-minute tick) refreshes every tokened
household's bulbs — so a light switched at the wall shows up without anyone pressing Sync. The
dashboard re-reads Home's own records every sixty seconds (free — no provider calls) and now
disposes its `CancellationTokenHandler`, which pages historically never did. Light schedules
gained sunrise/sunset triggers (`Trigger` + `OffsetMinutes`, Almanac `SunCalculator`, household
lat/long) — the "follow the sun" promise the Settings page copy was already making. Both
runners keep the existing single-token background limitation, noted in LifxAuthenticationHandler.

## 2026-08-14 — Members surfaced, assignment shipped, avatar-switching deferred

The Settings page grew a Members card over the existing CreateUser/new GetUsers slices, and
activities now expose the assignee end-to-end (the domain, DB and API always supported it — no
UI ever sent it). Passwordless tap-your-avatar user switching was deliberately NOT built: it
weakens auth on a possibly-internet-facing app, and the first-run registration entry already
rejected auth bypasses. It needs its own decision (per-user PIN? device-trusted sessions?).

## 2026-08-14 — Kitchen-mode details: cook screen, family notes, trolley ticking

/recipes/{id}/cook shows one step at a time in display type with tap-to-start timers parsed
from the step text ("simmer 20 minutes" becomes a button) and holds the tablet awake via the
Screen Wake Lock API (wwwroot/js/cook.js — everything degrades silently). The dashboard gained
anonymous pinned family notes (`Announcement` — the board belongs to the household, not a
member). Shopping list rows are now tap-to-tick using the long-dormant `InBasket` column, with
a running "in the trolley" total against the list total. EF migrations can now be generated
while the API is running via `PersistenceContextDesignTimeFactory`
(`--startup-project Home.Persistence`); `Database.Migrate()` still applies them at API startup.

## 2026-08-14 — PropertyChangeTracker crosses the wire through a JsonConverter

Saving the LIFX token failed with "Name cannot be empty": System.Text.Json deserialised every
tracker property through its `Value` setter, which flips `HasBeenSet` to true — so a partial
update arrived with *all four* settings marked as set (Name as a set-to-null, failing NotEmpty;
worse, a name-only save would have cleared the location and token). Both `PropertyChangeTracker`
structs now carry `[JsonConverter(typeof(PropertyChangeTrackerJsonConverterFactory))]`, which
writes `{hasBeenSet, value}` and on read returns `default` unless `hasBeenSet` is true. Rule for
later: never let a tracker round-trip through property-by-property deserialisation; the converter
is the only wire path, and `Home.Application.Tests/Infrastructure/ChangeTrackers` pins it.

## 2026-08-14 — Form inputs declare autocomplete, and labels are wired to their controls

`HomeTextInput`/`HomePasswordInput` now render a per-instance `id` their label points at, plus
optional `Name`, `AutoComplete` and `InputMode` parameters, and `aria-invalid`/`aria-describedby`
when showing an error. Conventions: login is `username`/`current-password`; setup uses
`given-name`/`family-name`/`email`/`new-password`; secrets that are not login passwords (the LIFX
token) use `new-password` so a password manager never autofills the household login into them;
fields a browser might mistake for personal data (anything labelled "Name") get `off`; numeric
text fields get `InputMode` so tablets show the right keyboard. Raw `<select>`/`<textarea>`/date
and time inputs get explicit `id`/`for` pairs.

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
