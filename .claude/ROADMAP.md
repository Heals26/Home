# Roadmap

*Ten phases, in the order they should be done. Each one is shippable on its own and leaves the app
better than it found it. Nothing here is half a feature that needs the next phase to be worth
having.*

*Read with `VISION.md` (what the product is for), `DECISIONS.md` (why things are as they are) and
`BACKLOG.md` (things deliberately parked). Sizes are rough: **S** a sitting, **M** a day-ish,
**L** multi-day, **XL** needs its own design decision first.*

*This used to be three parallel tracks lettered A, B and C, which grouped work by what kind of work
it was rather than when to do it, and left the actual ordering in a footnote that went stale the
week it was written. The old identifiers are kept in brackets so entries in `DECISIONS.md` that
name them still make sense.*

---

## Done

| | |
|---|---|
| **A1 to A9** *(1 Sep 2026)* | Members you can manage, rename and archive shopping lists, search and sort the recipe book, reorder everything with a `Sequence`, edit a recipe note, retire the second board axis, household-defined card sections, board filtering and empty states, move a planned meal. |
| **C4** *(1 Sep 2026)* | `known-gaps.md` rewritten against measured reality. |
| **Half of C1** *(3 Sep 2026)* | All 32 read slices tested against a real database through their real presenters. |
| **Phase 1** *(4 Sep 2026)* | Startup validation of every setting, non-secret configuration moved into appsettings, and a sign-in page that says which of three things went wrong. |
| **Phase 2** *(4 Sep 2026)* | All 114 use case slices tested, writes included, at 557 tests. |

---

## Phase 1 · Anyone can install it, done 4 Sep 2026 *(was C5)*

Setting the app up now takes **two secrets instead of six**, and every way of getting it wrong says
so in words rather than as a 401.

- **Both projects check their settings at startup**, list everything that is wrong in one go, and
  print the command that fixes each. `apiBaseUrl` is checked for the documented `/api/` mistake as
  well as for being absent, so that one fails on boot rather than as a 404 on every call.
- **The settings that are the same everywhere moved into `appsettings.json`**: client ID, grant type
  and scope. `apiBaseUrl` sits in `appsettings.Development.json` only, so a real deployment has to
  say where its API is rather than quietly pointing at localhost. `Home.WebApi` gained the
  `appsettings.json` it never had.
- **The sign-in page says which of three things went wrong.** All of them used to read "that
  username and password didn't match", including an API that was not running and an installation
  whose own credentials were refused, and only a real credential refusal now counts towards the
  lockout.
- **A fresh database still cannot be signed into**, and that stays deliberate. Nothing seeds
  `home.ClientApplication`; the decision to remove the requirement is parked in `BACKLOG.md` and
  `README.md` says the step is manual on purpose.

Found and fixed on the way: the password grant looked its client application up by ID alone, so the
secret was required to be present and never compared. The refresh grant had always compared it.

## Phase 2 · Writes are tested, done 4 Sep 2026 *(was the rest of C1)*

**557 tests across all 114 use case slices**, reads and writes, every one with a neighbouring
household seeded alongside so the isolation invariant is pinned rather than assumed. Each write is
read back through a context that never saw it, because an interactor that forgets to save looks
identical from inside its own.

Found and fixed on the way: **clearing a navigation that the query never loaded does nothing at
all.** EF compares null against null, sees no change, and leaves the foreign key alone; the save
succeeds and the endpoint answers 204. It meant a card could not be unassigned from a member and a
meal could not be taken out of its slot. Both are the write-side twin of the missing-projection
trap, and both are written up in `known-gaps.md`.

What remains uncovered is `Home.WebUI` components, and presenters other than through the slices
that drive them. Both are markup-heavy and want a different harness, so they are not a tail of this
phase.

## Phase 3 · The small unreachable things, S each *(was B3, B8, C2)*

Three small items that can go in one pass. Two are backend that already works with no way to reach
it, one is free cleanup.

- **Ingredient notes.** `IngredientNotesController` exists in full (add, edit, remove) and is
  entirely unreachable. "The good olive oil", "Woolies brand only", "the one in the green tin": the
  household knowledge that makes a shopping list usable by whoever is doing the shop.
- **Signed-in devices.** `UserAuthentication.DeviceLabel` carries its own docstring saying it exists
  *"so a future 'signed-in devices' screen can name the tablet rather than a token ID"*, alongside
  `LastUsedOnUTC`. Neither appears in any UI. Show the household's devices and let one be signed out
  remotely. Good hygiene now that sessions last 90 days. The same table still carries the
  `Superseded*` rotation columns, dead since the 19 Aug no-rotation decision.
- **Drop the superseded columns.** Measured: `ShoppingListItem` has 0 of 37 rows using
  `Quantity`/`Volume`/`Weight`; `Ingredient` has 1 of 23, a single "ham" row. Migrate that one row
  to `Amount`, then drop six columns and the fallback branches in `RecipeDisplayLogic` and
  `ShoppingListItemLogic`.

## Phase 4 · One responsive pass over the app chrome, L *(was A10)*

`MainLayout`, `HomeNavRail` and `HomeTopBar` contain **zero** breakpoints, and `input.css` has
exactly one `@media` query (`prefers-reduced-motion`). 21 of 30 pages have no breakpoints at all.

Landscape tablet is the design target and that is fine, but the shopping list is the one screen that
lives on a phone in a supermarket and `ShoppingListComponent` has none. Responsive work has been
reactive and per-screen so far; this is doing it once, deliberately. Folds in the outstanding phone
complaints: viewport slightly zoomed out, bottom bar overlapping content.

Worth knowing before starting: the `rail:` breakpoint is
`(min-width: 768px) and (orientation: landscape)`, so the app already has two quite different
layouts and which one you get depends on window shape, not width. That surprises people.

## Phase 5 · Who is using this, XL *(was B1)*

The biggest gap against VISION's "family-proof… used by every member of the family". There is one
household login. `GetAssignedActivities` is a complete slice with its own presenter and
`User.AssignedActivities` navigation, and **assignment is captured on create and edit while no
screen ever asks "what's mine"**. A child cannot *be* themselves in the app.

Needs the deferred decision first: per-user PIN, device-trusted sessions, something else? Avatar
switching was refused on 14 Aug as weakening auth on a possibly-internet-facing app. The payoff is
a "My day" view, per-person chore lists, and who-did-what that means something.

This phase is why it sits here rather than later: it changes what phases 6 and 10 are worth.

## Phase 6 · The app remembers, M *(was B2, B6)*

Two features that surface data already being captured and stored.

- **Show the audit trail.** `Audit` is written correctly by ~15 interactors and **read by nothing**.
  Every mutation already records who did what and when. "Who ticked this off", "what changed on this
  recipe", a household activity feed. The cheapest large-feeling feature on this list, and much
  better after phase 5 gives the who a name.
- **Leftovers and meal history.** The meal plan knows what was cooked and when. Nothing surfaces
  "you had this three days ago", "cook once eat twice", or "you haven't made this in six months".
  Turns the planner from a schedule into something that gives advice.

## Phase 7 · The shop gets smarter, M *(was B4, B5)*

Both are shopping-list intelligence over the same data, so they share the groundwork.

- **Price memory and shop totals.** The honest version of the store-price-comparison idea. Items
  already carry a line price and suggestions already return the last price paid. Keep a per-item
  price history, show what a list will cost before you leave, and flag when something is dearer than
  usual. The 17 Aug decision says there is no per-kilo price anywhere and adding one means a new
  column, so decide that up front.
- **Aisle grouping.** `ShoppingListItem.Sequence` is read for display only. Let items carry a
  category (produce, dairy, freezer) learned from history, and group the list by it so a shop is one
  walk through the store instead of a scavenger hunt.

## Phase 8 · Nullable on in the API, L *(was C3)*

A clean build emits **one** warning, not the ~145 this file used to claim. That is not progress:
`Home.WebApi` sets `<Nullable>disable</Nullable>` while every other project enables it, and the API
models and controllers are where the `CS8618`s lived. They are suppressed, not fixed.

The job is turning nullable on there and absorbing what comes back in one go. No user-visible value,
which is why it sits this late, but it gets harder every phase that adds API models.

## Phase 9 · Beyond lights: a second device integration, L *(was B9)*

`DECISIONS.md` (12 Aug) establishes the adapter template: service interface in `Application`,
adapter in `WebApi`, an unreachable provider is a return value rather than an exception, vendor wire
types stay at the boundary. VISION says "other devices as they come" and nothing beyond lights is
researched. A thermostat, robot vacuum or smart plug is the obvious next one, and the pattern is
ready and proven.

## Phase 10 · The ones blocked on their own decisions, L each *(was B7, B10)*

Both are in `BACKLOG.md` with the open question written down. Neither is blocked on effort.

- **Feedback button and support reader.** Reading across all households breaks the
  household-isolation invariant every other query obeys, so it needs either a separate operator app
  or an explicit support role with its own policy. `ApiAuditEntry` already captures request bodies,
  IPs, user agents and timing, so the substrate exists.
- **Spotify.** OAuth is *per user* rather than one household token like LIFX, so it needs an
  authorisation-code flow with refresh, a callback URL, and a decision about whose account the
  kitchen tablet plays from. Needs Premium. Naturally follows phase 5, which answers "who is this
  device".
