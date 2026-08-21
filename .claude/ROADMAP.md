# Roadmap

*Twenty phases, split into new capability and finishing what exists. Each phase is deliverable on
its own and leaves the app better than it found it — nothing here is a half-feature that needs the
next phase to be worth shipping. Ordered within each track by a mix of value and dependency, not
by effort. Specifics are open for discussion; this is the shape, not the spec.*

*Read with `VISION.md` (what the product is for), `DECISIONS.md` (why things are as they are) and
`BACKLOG.md` (things deliberately parked). Sizes are rough: **S** a sitting, **M** a day-ish,
**L** multi-day, **XL** needs its own design decision first.*

---

## Track A — Enhancements: finishing what's already there

These are the ones that make the app feel finished. Most are small, several are embarrassing gaps,
and a few are backend capability that already works with no way to reach it.

### A1 · Members you can actually manage — **S**
`PATCH` and `DELETE api/Users/{id}` both work and nothing calls them. A member added with a typo is
permanent, and someone who leaves can't be removed. Settings' Members card is add-only. The
`Audit → User` relationship was deliberately set to `SetNull` (15 Aug) *specifically* so a member
could be deleted — that groundwork is already paid for.
**Also:** change your own password. The Account card currently contains one Sign out button.

### A2 · Rename and organise shopping lists — **S**
`UpdateShoppingList` is wired in `ApiProvider` and called from nowhere: **a shopping list cannot be
renamed.** Add rename, plus duplicate ("this week's like last week's") and archive so old lists
stop cluttering the picker without being destroyed.

### A3 · Search and sort the recipe book — **S**
The meal-plan picker has search; the recipe book itself does not — the secondary surface
out-features the primary one. Ordering is hard-coded to name. `Complexity`, `PrepMinutes`,
`CookMinutes` and `Servings` are all captured and displayed, and none of them can be filtered or
sorted on. "What can I cook in 30 minutes" is a question the data can already answer.

### A4 · Reorder everything that has a Sequence — **M**
`Sequence` is persisted and honoured on `RecipeStep`, `ShoppingListItem`, `LightScene`,
`ActivityRegion` and `ActivityContent`, and **none of them has a move control**. Recipe steps are
the worst case: editing a step deliberately writes its existing sequence back, so a step added out
of order stays out of order forever. `LightScene.Sequence` is even documented as "display order on
the Lights page" — groups got a reorder, scenes didn't. One shared move-up/move-down affordance,
applied five times.

### A5 · Edit a recipe note — **S**
Notes can be added and removed but not edited, and `UpdateNote` is already wired. Small, and the
kind of thing that makes the app feel unfinished when you hit it.

### A6 · Retire the second board axis — **M**
`ActivityStatus` is a *global, unscoped* lookup still seeded Todo/In Progress/Done at API startup,
sitting beside the household-scoped `ActivityState` that replaced it (15 Aug). `Activity.Status`
round-trips through both interactors and all three presenters, and **no UI control ever sets or
shows it**. Two "what state is this card in" concepts, one invisible. Its global seeding also
contradicts the rule that seeding moved to `IHouseholdSetupLogic`. Pick one, delete the other.

### A7 · Take the software-ticket language off the family board — **M**
`RegionSE` defines `Description`, **`AcceptanceCriteria`** and `Notes`, hard-coded to that trio in
`ActivityDetailPage`. This is the exact mistake the 15 Aug board-columns decision fixed
("software-process jargon on a family board") surviving in a second vocabulary. Nobody writes
acceptance criteria for *mow the lawn*. Household-defined card sections, or a fixed set that reads
like a home.

### A8 · Filter the board, and give it real empty states — **M**
No filter or search of any kind: not by assignee, not by tag, not by date — though all three are
first-class on the card. Cards also can't be reordered *within* a column (drag only moves between).
And the empty states are bare inline text — `"Nothing here yet"`, `"Nothing on"` — where every
other pillar uses `HomeEmptyState` with an action. A family opening an empty board is given no next
step. *(Touch is already handled well here via the chevron move buttons — leave that alone.)*

### A9 · Move a planned meal — **S**
A planned meal can't be shifted; delete-and-re-add is the only path, which on a tablet is three
taps and a search to fix a mistake. Drag on desktop, move buttons on touch — same pattern the board
already uses.

### A10 · One responsive pass over the app chrome — **L**
`MainLayout`, `HomeNavRail` and `HomeTopBar` contain **zero** breakpoints, and `input.css` has
exactly one `@media` query (`prefers-reduced-motion`). 21 of 30 pages have no breakpoints at all.
Landscape tablet is the design target and that's fine — but the shopping list is the one screen
that lives on a phone in a supermarket, and `ShoppingListComponent` has none. Responsive work has
been reactive and per-screen so far; this is doing it once, deliberately. Folds in the outstanding
phone complaints: viewport slightly zoomed out, bottom bar overlapping content.

---

## Track B — New features

### B1 · Make the household real: per-member identity — **XL**
The biggest gap against VISION's "family-proof… used by every member of the family". There is one
household login. `GetAssignedActivities` is a complete slice with its own presenter and
`User.AssignedActivities` navigation — **assignment is captured on create and edit and no screen
ever asks "what's mine"**. A child cannot *be* themselves in the app. Needs the deferred decision
first (per-user PIN? device-trusted sessions? avatar switching was refused on 14 Aug as weakening
auth on a possibly-internet-facing app) — but the payoff is a "My day" view, per-person chore
lists, and who-did-what that means something.

### B2 · Show the audit trail — **M**
`Audit` is written correctly by ~15 interactors and **read by nothing**. Every mutation already
records who did what and when. "Who ticked this off", "what changed on this recipe", a household
activity feed — the instrumentation is done, there is simply no reader. Cheapest large-feeling
feature on this list.

### B3 · Ingredient notes — **S**
`IngredientNotesController` exists in full (add, edit, remove) and is entirely unreachable. "The
good olive oil", "Woolies brand only", "the one in the green tin" — the household knowledge that
makes a shopping list actually usable by whoever's doing the shop.

### B4 · Price memory and shop totals — **M**
The honest version of the store-price-comparison idea. Shopping items already carry a line price
and suggestions already return the last price paid. Keep a per-item price history, show what a
list will cost before you leave, and flag when something's dearer than usual. **Note:** the 17 Aug
decision says there is no per-kilo price anywhere and adding one means a new column — decide that
up front.

### B5 · Aisle grouping for the shop — **M**
`ShoppingListItem.Sequence` is read for display only. Let items carry a category (produce, dairy,
freezer) learned from history, and group the list by it so a shop is one walk through the store
instead of a scavenger hunt. Pairs naturally with A4.

### B6 · Leftovers and meal history — **M**
The meal plan knows what was cooked and when. Nothing surfaces "you had this three days ago",
"cook once eat twice", or "you haven't made this in six months". Turns the planner from a schedule
into something that gives advice.

### B7 · Feedback button and support reader — **L**
Already in `BACKLOG.md`, asked for 17 Aug. The blocker is a real decision, not effort: reading
across all households breaks the household-isolation invariant every other query obeys, so it needs
either a separate operator app or an explicit support role with its own policy. `ApiAuditEntry`
already captures request bodies, IPs, user agents and timing — the substrate exists.

### B8 · Signed-in devices — **S**
`UserAuthentication.DeviceLabel` carries its own docstring saying it exists *"so a future
'signed-in devices' screen can name the tablet rather than a token ID"*, alongside `LastUsedOnUTC`.
Neither appears in any UI. Show the household's devices and let one be signed out remotely. Good
hygiene now that sessions last 90 days. **Housekeeping:** the same table still carries the
`Superseded*` rotation columns, dead since the 19 Aug no-rotation decision.

### B9 · Beyond lights: a second device integration — **L**
`DECISIONS.md` (12 Aug) establishes the adapter template — service interface in `Application`,
adapter in `WebApi`, unreachable provider is a return value not an exception, vendor wire types stay
at the boundary. VISION says "other devices as they come" and nothing beyond lights is researched.
Thermostat, robot vacuum, or a smart plug is the obvious next one; the pattern is ready and proven.

### B10 · Spotify — **L**
`BACKLOG.md`, asked 17 Aug. Blocked on account identity, not the API: Spotify is OAuth *per user*
rather than one household token like LIFX, so it needs an authorisation-code flow with refresh, a
callback URL, and a decision about whose account the kitchen tablet plays from. Needs Premium.
Naturally follows B1, which answers "who is this device".

---

## Track C — Housekeeping worth scheduling

Not features, but they set the ceiling on everything above.

- **C1 · Widen the test net — L.** Tests are one slice deep: Recipes and Lights interactors plus
  the parser and mapper guards. Activities, Shopping Lists, Users, OAuth and *all* of
  `Services/EntityLogic` are untested, as is every presenter and component. The session rework and
  the photo pipeline were both verified by hand against a live browser — that doesn't scale.
- **C2 · Drop the superseded columns — S.** `Ingredient` and `ShoppingListItem` both still carry
  `Quantity`/`Volume`/`Weight` behind `Amount`/`Unit`, kept "until the move is proven" (15 Aug).
  Prove it, then drop six columns and the fallback branches that read them.
- **C3 · Nullable and the warning backlog — L.** ~145 warnings, ~115 of them `CS8618`, and
  `Home.WebApi` alone has `<Nullable>disable</Nullable>` while every other project enables it.
- **C4 · Refresh `known-gaps.md` — S.** It's stale in ways that could misdirect a decision: it
  claims `Light`, `LightGroup` and `LightLocation` "are dead tables" (they've been live since the
  14 Aug sync work), and its per-migration cleanup instruction was made obsolete by the 20 Aug
  column-default fix. The `HomeButton` finding it flags *is* still real — `@attributes` splats
  after `class`, so a caller passing `class` silently wipes the button's styling.
- **C5 · Configuration and first-run — M.** No `appsettings.json` at all; three secrets live in
  user secrets and only `apiBaseUrl` is validated at startup. A misconfigured deploy currently
  fails at the point of use.

---

## Suggested order

**Now:** A1, A2, A5 and A3 in one pass — four small gaps that all read as "this app isn't
finished", and none needs a decision.
**Next:** A4 with B5 (both are ordering), then A6 and A7 together (both are the board's vocabulary).
**Then:** the B1 decision, because B10 and half of B2's value depend on knowing who's using it.
**Alongside:** C4 immediately (it's stale documentation actively misinforming), C1 continuously.
