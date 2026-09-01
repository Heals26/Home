# Decision log

*Why the code is the way it is. Newest first. Every entry: what was decided, why, and what it means
for anyone writing code later. When a decision is reversed, don't delete the entry — add a new one
that supersedes it. See `VISION.md` for what the product is; see `docs/HANDOVER.md` for the
12 Aug 2026 point-in-time state.*

## 2026-09-01 — Card sections belong to the household, and the second board axis is gone

Two decisions from the same complaint: the family board still spoke like a software ticket.

`ActivityStatus` — the global, unscoped Todo/In Progress/Done lookup seeded at API startup — was
**deleted outright** rather than reconciled with the household-scoped `ActivityState` that replaced
it on 15 Aug. It was safe because it was never used: 0 of 3 activities carried a StatusID, and no
UI control ever set or showed one. A card's column already answers "what state is this in", and two
answers to one question is one too many. Nothing is seeded globally any more.

`RegionSE` — Description / **AcceptanceCriteria** / Notes, fixed in code — became `CardSection`, a
household-owned, named, ordered entity alongside board columns and meal slots. Mitch chose this over
renaming the fixed trio, because the 15 Aug reasoning ("columns belong to the household") applies
identically here. Existing households were seeded Details / Steps / Notes and their one existing
region remapped onto the matching row through the card's household, so nothing written was lost.
`ActivityRegion → CardSection` is **NoAction**, not Cascade: the household already reaches sections
through the activity, SQL Server rejects the second cascade path, and refusing to delete a section
still holding writing is what a family wants. The UI refuses it too, and the count that decides
that has to be **projected** — an unloaded `Regions` collection counts zero and would have offered
to delete a section with a card's writing under it.

## 2026-09-01 — Archiving is not deleting, and a copy is one call

A shopping list can now be renamed, copied and archived. Archiving keeps everything — the items,
the prices, what was ticked — and only takes the list out of the picker, so last Christmas's shop
can still be copied a year later; deleting stays the destructive one and asks twice. The list being
looked at stays visible even once archived, because it would otherwise vanish from under the reader
at the moment they archived it.

Copying is a server-side slice, not the screen looping over the item endpoints — the same reasoning
as clearing a list on 17 Aug. Nothing arrives ticked and nothing arrives priced: "this week's like
last week's" means the same things to buy, not last week's trolley or last week's receipt.

## 2026-09-01 — One move control, and the sequence swap is the pattern

`HomeReorder` is the single up/down affordance, now used by recipe ingredients, recipe steps,
shopping items, light scenes, card sections and cards within a board column. Chevrons rather than
drag, everywhere: a finger can hit them, they need no pointer to discover, and they behave the same
on the tablet as on a desktop. Drag stays as a desktop extra where it already existed, and the
meal planner gained it by reusing the board's delegated `[data-drop-target]` listener rather than
writing a second one — the attribute lost its `board-` prefix when the second screen wanted it.

Every reorder is the same **two-call sequence swap** the board already used for its columns, and
every one sends *only* the sequence: a reorder must not overwrite a name or a price someone is
editing on another device. Ends are disabled rather than hidden so rows keep their width and do not
shuffle sideways as things move.

Two new `Sequence` columns fell out of this — `RecipeIngredient.Sequence` (31 Aug) and
`Activity.Sequence` — both backfilled so existing rows keep the order they were already shown in
rather than all landing on zero, which is the arbitrary ordering the column exists to end.

## 2026-08-31 — A modal is a `<dialog>`, so the browser owns its keyboard

`HomeModal` was a `<div>` with `fixed inset-0`, which gets none of the behaviour a modal is
expected to have: Escape did nothing on any of the twenty-one modals, Tab walked straight out of
the panel onto the page behind it, and the background stayed live to pointers and screen readers.
The fix was not to write key handling — it was to stop hand-rolling the element. `showModal()` on
a real `<dialog>` gives Escape, the focus trap, the inert background and the top layer for free,
which is the same reasoning that made the body a `<form>` for its Enter key on 20 Aug.

Two details worth keeping. The `cancel` event is **preventDefault-ed** and routed back into Blazor
through a `[JSInvokable]`, because letting Escape close the element natively would leave the DOM
shut while `Visible` still said open — `Visible` stays the single source of truth. And the dialog
is stretched over the viewport with its user-agent frame stripped (`m-0`, `max-w-none`,
`max-h-none`, `p-0`, `border-0`), because `dialog:modal` ships a `calc(100% - 38px)` max-width and
an `auto` margin that would otherwise inset the whole overlay.

## 2026-08-31 — Tapping a row opens the thing; only its own control acts on it

Supersedes the tap-to-tick half of the 17 Aug shopping list entry. The whole row being the tick
target meant reading a line to check the amount crossed it off — in a supermarket, mid-shop, a
mis-tap you then have to find and undo. The circle now ticks and carries a 56px hit area of its
own so the target did not shrink to the ring you can see; the words open the item.

The same rule settles the recipe page, where the pencil and bin were `opacity-0
group-hover:opacity-100` — invisible on a tablet, which is the only device this runs on. That was
the identical defect already fixed for the delete-list button on 19 Aug, and a sweep found seven
of them across Recipes. Ingredient rows became tap-to-open with removal moved inside the sheet;
steps, notes and recipe cards simply stopped hiding their controls. **Rule: never gate a control
on hover. A finger has no pointer, and `group-hover` is a desktop-only affordance.**

## 2026-08-31 — An ingredient's position belongs to the recipe, not to the ingredient

Ingredients rendered in whatever order the database returned. They are ordered now, and the order
is *order of use in the method* — what a cookbook does, and what you read down while cooking.
Grouping by type (herbs, spices, meats) was considered and rejected for the recipe page: that is
what a shopping list wants, and it is already the aisle-grouping item on the roadmap.

`Sequence` went on `RecipeIngredient`, the join, rather than on `Ingredient`. Today an `Ingredient`
row is created fresh for every line and never reused, so putting it on the ingredient would have
worked — and would have quietly blocked the ingredient-catalogue rework, where one shared onion
cannot hold one recipe's position. Moving is the board's two-call sequence swap, not a new verb.

## 2026-08-31 — A unit knows its own singular, because English does not derive one

"1 packets" and "1 tins" were being rendered because a unit carried exactly one abbreviation.
Each now carries both forms explicitly rather than being pluralised by rule: leaf becomes leaves,
dash becomes dashes, pinch becomes pinches, and no suffix rule gets all three right. The API picks
the form from the amount beside it, so every screen agrees without each one deciding.

There are **three** lists of measurements that cannot see each other — the domain enumeration, the
web app's mirror of it, and the synonyms the shopping list parser accepts — and until now only a
comment held them together. `MeasurementUnitTests` pins all three to the enumeration, so a unit
added to one and forgotten in the others fails the build rather than showing an empty dropdown
entry or silently dropping a typed unit.

## 2026-08-20 — Recipe photos live in the database and stream through the web app

The "no image bytes, that forces a hosting decision" stance on `Recipe.ImageUrl` is superseded:
Mitch asked for his own photos, and for a household app the database *is* the hosting decision.
`RecipeImage` holds the bytes (5 MB cap) one-to-one with the recipe, deliberately with **no
navigation on Recipe** — a stray Include would drag blobs through every book query. What the book
needs instead is `Recipe.ImageUpdatedOnUTC`: a denormalised stamp meaning "has a photo", written
only by the two image use cases, whose ticks double as the cache-buster in the image URL. The
format comes from sniffing the bytes' magic numbers, never from the declared content type — a
renamed file lies, the bytes don't. The browser fetches `/recipe-images/{id}` on the *web app*,
which turns the sign-in cookie into a bearer token and proxies the API — an img tag can only send
a cookie, and the API's household scoping stays in charge. An uploaded photo beats the ImageUrl
link everywhere.

## 2026-08-20 — Applying a light scene first saves how the room looked

"Previous look" is one reserved scene per household (`LightScene.IsPreviousLook`), rewritten by
every scene apply with a snapshot of the *whole* household's lights taken before the apply mutates
anything. That makes it an undo — and tapping it twice toggles, because applying it re-captures
what it replaced. It is pinned first in the scene list, cannot be scheduled (it changes on every
apply, so a schedule would set lights nobody chose), and is never created until the first apply.

## 2026-08-20 — Planning a meal can create the recipe on the spot

The meal-plan picker gained search, orders recipes tagged with the meal being planned first, and
offers "Add *X* as a new dinner recipe" when the search matches nothing exactly — created, tagged
with that meal, and planned in one tap, composed client-side from the existing CreateRecipe,
SetRecipeMealSlots and CreateMealPlanEntry slices rather than a new API shape. Reason: "I want to
add spaghetti bolognese but haven't added it as a recipe" ended in a five-screen round trip, which
is a plan abandoned.

## 2026-08-20 — Nothing defaults to a CLR-evaluated timestamp

`Note.CreatedOnUTC` carried `HasDefaultValue(DateTime.UtcNow)` — frozen at scaffold time, so every
Note defaulted to the same stale moment *and every migration since re-altered the column* as the
"default" moved. It is `HasDefaultValueSql("SYSUTCDATETIME()")` now. Rule: a column default is the
database's clock or a constant, never the model's.

## 2026-08-19 — The session is the cookie; browser storage is out of the auth path entirely

Supersedes the session mechanics of the 15 Aug entry below. That fix kept the token in
`ProtectedLocalStorage`, which means identity had to be *read back out of the browser over JS
interop after every reload* — and every way that read can fail (circuit evicted, interop timeout,
key-ring drift) is indistinguishable from "not signed in". Mitch kept getting the login page on
F5, and rotation compounded it: several circuits share one stored token, so whichever presented
it second looked like a replay attack and revoked the household.

The standard shape replaced all of it:

- **A persistent auth cookie is the session** (`Home.Session`, HttpOnly, essential, 90-day sliding
  expiry matching the refresh token). The browser sends it with the request that starts the
  circuit, so a reload arrives already signed in — there is nothing to read, race or time out.
- **`/login`, `/logout` and `/setup` render statically** — `App.razor.cs` picks the render mode
  per path — because only an HTTP response can carry `Set-Cookie`. They are plain Blazor
  `@formname` forms with `<AntiforgeryToken />` and `[SupplyParameterFromForm]`, and each guards
  against arriving through the interactive router by re-entering with `forceLoad` when there is
  no `HttpContext`. The custom `AuthenticationStateProvider`, `AuthInitialiser` and the OAuth
  view model in browser storage are deleted.
- **The refresh token rides in a claim** inside the encrypted cookie. Per circuit,
  `HouseholdSession` trades it for an access token in memory; `OAuthClient` is the one place that
  talks to the token endpoint. A refused refresh surfaces as an error — nothing client-side can
  end a session, because only the sign-out form can clear the cookie.
- **Refresh tokens do not rotate.** Every tab and device presenting the token it holds must keep
  working; rotation is what made honest concurrency look like theft. A session ends two ways:
  expiry or sign-out. The grace window died with rotation. The access token is only re-minted
  when it has under five minutes left (`SessionValues.AccessTokenReissueFloor`), so tabs sharing
  a session row converge on one token instead of invalidating each other hourly.

Verified end-to-end on 19 Aug against a live browser and a disposable DB user: sign-in sets the
cookie, F5 stays signed in, killing and restarting the WebUI stays signed in, sign-out clears it,
and presenting the same refresh token twice returns the same token both times.

## 2026-08-19 — The reconnect overlay is ours, and a dead circuit reloads itself

Blazor's default overlay is white in an app that is dark by default, and once it gives up it
leaves a page that looks alive and does nothing — the tablet's worst failure mode. The element
with Blazor's `components-reconnect-modal` id in `App.razor` restyles the overlay with theme
tokens, and `reconnect.js` watches the state classes: a rejected reconnect (server restarted,
circuit gone) reloads immediately, and a failed one retries with a HEAD probe and reloads the
moment the server answers. Reloading is safe precisely because of the cookie entry above — the
reload comes back signed in on the same page.

## 2026-08-17 — Adding to a shopping list is one text box, not a form

The add-item modal is gone. There is a single input that stays on screen, takes the whole line as
written — "2 kg potatoes", "500g mince", "1/2 cup rice" — and keeps the cursor after each add, so
writing a list is typing, Enter, typing, Enter. `ShoppingListItemLogic.Parse` does the reading and
is the only place that knows how a line is written; anything it cannot make sense of survives as
the name exactly as typed, because losing what someone wrote is worse than missing an amount.
Reason: this is the most-used action in the app and it was costing four taps and a modal per item,
which is precisely the comparison against Bring! and AnyList that the product has to win.

## 2026-08-17 — What a household buys is offered back to it

`GetShoppingListItemSuggestions` returns the household's distinct item names ordered by how often
they have been added, each carrying the amount and price from the last time it was bought. The
whole set (capped at 200) is fetched once when the screen opens and filtered on the device, not
per keystroke — a phone in a supermarket should not be asking the server on every letter, and
what a family usually buys does not change between keystrokes. Picking a suggestion brings its
last price with it, which is how the running total fills in without anyone typing prices. An
amount already typed beats the remembered one.

## 2026-08-17 — Emptying a list is one call, not one call per line

`DeleteTickedShoppingListItems` and `UntickShoppingListItems` exist as their own slices rather
than the screen looping over the per-item endpoints. Thirty round trips over a supermarket
connection is the difference between a list emptying and a list draining. Clearing is confirmed
because there is no undo; unticking is not, because nothing is lost. Both are household-scoped
the same way as everything else — a list that isn't yours simply matches nothing.

## 2026-08-17 — A shopping item's cost is the line price, not a unit price

`ListTotal` sums `Cost` and no longer multiplies it by an amount. "$3.50 for 2 kg of potatoes" is
three fifty, and multiplying turned it into seven. The field is labelled "Price for the line" so
the meaning is on screen rather than assumed. Consequence: there is no per-kilo price anywhere,
and adding one later means a new column rather than reinterpreting this one.

## 2026-08-17 — The web app's item requests carry Amount and Unit

The web app was still posting `Quantity`/`Volume`/`Weight` after the API moved to `Amount`/`Unit`,
so a quantity typed into the add form was serialised, ignored by the API and silently lost — and
the row rendered the legacy column, which was always empty, so the loss was invisible. Both
request models now match the API. The legacy properties stay on the *response* model because rows
written before units existed still read through them.

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
