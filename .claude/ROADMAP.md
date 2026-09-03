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

### A1 [done 1 Sep] · Members you can actually manage — **S**
`PATCH` and `DELETE api/Users/{id}` both work and nothing calls them. A member added with a typo is
permanent, and someone who leaves can't be removed. Settings' Members card is add-only. The
`Audit → User` relationship was deliberately set to `SetNull` (15 Aug) *specifically* so a member
could be deleted — that groundwork is already paid for.
**Also:** change your own password. The Account card currently contains one Sign out button.

### A2 [done 1 Sep] · Rename and organise shopping lists — **S**
`UpdateShoppingList` is wired in `ApiProvider` and called from nowhere: **a shopping list cannot be
renamed.** Add rename, plus duplicate ("this week's like last week's") and archive so old lists
stop cluttering the picker without being destroyed.

### A3 [done 1 Sep] · Search and sort the recipe book — **S**
The meal-plan picker has search; the recipe book itself does not — the secondary surface
out-features the primary one. Ordering is hard-coded to name. `Complexity`, `PrepMinutes`,
`CookMinutes` and `Servings` are all captured and displayed, and none of them can be filtered or
sorted on. "What can I cook in 30 minutes" is a question the data can already answer.

### A4 [done 1 Sep] · Reorder everything that has a Sequence — **M**
`Sequence` is persisted and honoured on `RecipeStep`, `ShoppingListItem`, `LightScene`,
`ActivityRegion` and `ActivityContent`, and **none of them has a move control**. Recipe steps are
the worst case: editing a step deliberately writes its existing sequence back, so a step added out
of order stays out of order forever. `LightScene.Sequence` is even documented as "display order on
the Lights page" — groups got a reorder, scenes didn't. One shared move-up/move-down affordance,
applied five times.

### A5 [done 1 Sep] · Edit a recipe note — **S**
Notes can be added and removed but not edited, and `UpdateNote` is already wired. Small, and the
kind of thing that makes the app feel unfinished when you hit it.

### A6 [done 1 Sep] · Retire the second board axis — **M**
`ActivityStatus` is a *global, unscoped* lookup still seeded Todo/In Progress/Done at API startup,
sitting beside the household-scoped `ActivityState` that replaced it (15 Aug). `Activity.Status`
round-trips through both interactors and all three presenters, and **no UI control ever sets or
shows it**. Two "what state is this card in" concepts, one invisible. Its global seeding also
contradicts the rule that seeding moved to `IHouseholdSetupLogic`. Pick one, delete the other.

### A7 [done 1 Sep] · Take the software-ticket language off the family board — **M**
`RegionSE` defines `Description`, **`AcceptanceCriteria`** and `Notes`, hard-coded to that trio in
`ActivityDetailPage`. This is the exact mistake the 15 Aug board-columns decision fixed
("software-process jargon on a family board") surviving in a second vocabulary. Nobody writes
acceptance criteria for *mow the lawn*. Household-defined card sections, or a fixed set that reads
like a home.

### A8 [done 1 Sep] · Filter the board, and give it real empty states — **M**
No filter or search of any kind: not by assignee, not by tag, not by date — though all three are
first-class on the card. Cards also can't be reordered *within* a column (drag only moves between).
And the empty states are bare inline text — `"Nothing here yet"`, `"Nothing on"` — where every
other pillar uses `HomeEmptyState` with an action. A family opening an empty board is given no next
step. *(Touch is already handled well here via the chevron move buttons — leave that alone.)*

### A9 [done 1 Sep] · Move a planned meal — **S**
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

*Every number here was measured on 1 Sep 2026 against commit `841af10` — clean rebuild, full test
run, and queries against the live database. Re-measure before trusting them again.*

- **C1 · Widen the test net — M, and half done.** **205 tests** (was 109 on 1 Sep). Every one of
  the **32 read slices** is now covered, each driving its real presenter against a real database
  with a neighbouring household seeded alongside, so both the projection contract and the 14 Aug
  isolation invariant are pinned rather than assumed. Reverting any of the three shipped
  missing-projection bugs now fails the suite. **What remains is writes**: roughly 60 Create /
  Update / Delete / Set slices, the rest of `Services/EntityLogic`, and every `Home.WebUI`
  component. Presenters are exercised, but only through the reads that drive them. The harness to
  copy is `Infrastructure/InteractorTest.cs`; the trap it exists to catch is written up in
  `known-gaps.md`.
- **C2 · Drop the superseded columns — S, and nearly free.** Measured: `ShoppingListItem` has
  **0 of 37 rows** using `Quantity`/`Volume`/`Weight`. `Ingredient` has **1 of 23** — a single row
  ("ham", Quantity 1). Migrate that one row to `Amount`, then drop six columns and the fallback
  branches in `RecipeDisplayLogic` and `ShoppingListItemLogic`. The move is proven; this is now
  smaller than it looks.
- **C3 · Nullable — L, and it is *not* a warning backlog.** A clean build emits **1 warning**, not
  ~145. That is not progress: `Home.WebApi` sets `<Nullable>disable</Nullable>` while every other
  project enables it, and the API models and controllers are where the `CS8618`s lived. They are
  suppressed, not fixed. The job is turning nullable *on* there and absorbing what comes back in
  one go.
- **C4 · Refresh `known-gaps.md` — done 1 Sep 2026.** Rewritten against measured reality, with a
  "corrections to what this file used to say" section kept deliberately, because three of its claims
  had actively misdirected work: it said there were zero `.razor.cs` files (there are 49, against 52
  `.razor`, with no inline `@code` left anywhere), that `[EditorRequired]` was never used (4 uses),
  and that the Light tables were dead (live since 14 Aug). It gained one new landmine: **`dotnet ef
  database update` silently targets LocalDB**, because the design-time factory outranks the startup
  project — that cost a wrong-database migration on 31 Aug. The `HomeButton` finding it flags is
  still real and still open.
- **C5 · Configuration and first-run — M.** No `appsettings.json` at all; three secrets live in
  user secrets and only `apiBaseUrl` is validated at startup. A misconfigured deploy currently
  fails at the point of use. **Also: `CLAUDE.md` documents `apiBaseUrl` wrongly** as
  `http://localhost:57175/api/` — `ApiProvider` already prefixes `api`, so that value yields
  `…/api/api/Recipes`. A fresh clone following the docs breaks on every call.

---

## Suggested order

**Now:** A1, A2, A5 and A3 in one pass — four small gaps that all read as "this app isn't
finished", and none needs a decision.
**Next:** A4 with B5 (both are ordering), then A6 and A7 together (both are the board's vocabulary).
**Then:** the B1 decision, because B10 and half of B2's value depend on knowing who's using it.
**Alongside:** C1 continuously — the read half landed on 3 Sep, so the next tranche is writes.
*(C4 was done on 1 Sep — the numbers in Track C above are measured rather than remembered, and
`known-gaps.md` now records when it was last checked.)*
