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

## Phase 3 · The small unreachable things, S each *(was B3, B8, C2)* **DONE 5 Sep 2026**

Three small items in one pass. Two were backend that already worked with no way to reach it, one
was cleanup. All three shipped. Two of them turned up something the plan had wrong, and both
corrections are recorded rather than quietly absorbed.

- **Ingredient notes.** Done. The note shows under the ingredient on the recipe and is written from
  the row that opens it. Emptying the box removes the note rather than storing a blank one.

  The plan described this as "the household knowledge that makes a shopping list usable by whoever
  is doing the shop". It does not reach the shopping list, and it cannot: `ShoppingListItem` has a
  free-text `Name` and no link to `Ingredient`, so there is nothing to carry a note across. Getting
  it there needs a schema change, now in `BACKLOG.md`.

  An ingredient is never shared between recipes either. The schema models it as many-to-many, but
  `AddRecipeIngredient` and `ImportRecipe` both create a fresh `Ingredient` every time and nothing
  anywhere reuses one, so a note reaches exactly one recipe today. The interface deliberately says
  nothing about scope rather than promising otherwise. Making sharing real is not a small change:
  `Amount` sits on the shared row, so two recipes sharing "olive oil" would share "1 tbsp", which
  would be wrong in one of them.

- **Signed-in devices.** Done. `DeviceLabel` is written at sign-in from the User-Agent ("Chrome on
  Windows", "Safari on iPad"), the session ID travels on the authenticated principal so the list
  can mark the row reading it, and Settings shows the household's devices with per-device sign-out.

  This household had **24** sessions stacked up, all unlabelled, because nothing prunes one and
  sessions last 90 days. A list of 24 identical rows with 23 individual sign-out buttons is not a
  screen anyone would use, so the card shows five and asks before the rest, and a single
  "Sign out N others" ends everything except the device reading it.

- **Drop the superseded columns.** Done for the amount columns: `Quantity`, `Volume` and `Weight`
  are gone from `Ingredient` and `ShoppingListItem` along with the fallback branches, migration
  `DropSupersededAmountColumns`.

  **The `UserAuthentication.Superseded*` columns are a separate thing and were not dead.** The item
  above called them "dead since the 19 Aug no-rotation decision". They are read on every refresh:
  `CreateRefreshGrantInteractor.ResolveSupersededSession` follows the pointer from an old token to
  the session that replaced it. Dropping the columns without removing that branch first would have
  signed out any device still holding a pre-19-Aug rotated token.

  Measured 5 Sep 2026, after the bulk sign-out above: 1 session in the table, 0 rows carrying a
  superseded pointer. Nothing exercises the branch any more and it can now go, in this order:
  delete `ResolveSupersededSession` and its call site, then drop the two columns. Left undone
  deliberately, because it is a behaviour change to the refresh path and does not belong in a pass
  labelled free cleanup. Carried to `BACKLOG.md`.

## Phase 4 · One responsive pass over the app chrome, L *(was A10)* **DONE 5 Sep 2026**

Done by measuring at 375x812 rather than by adding breakpoints until it looked right, which is why
most of what the plan listed turned out not to need changing. What was actually broken:

- **The top bar destroyed the page title.** On a phone the three actions on a recipe took 248px of
  375, leaving the title **23px**: "Pork Roast with Crispy Pork Crackling" rendered as "P.". The
  actions now drop to their own line below the title, and the title wraps to two lines rather than
  truncating, because a page's own name is the one thing worth the extra line. Measured after:
  283px and fully visible.

  That uses `sm:`, not `rail:`, and the difference is the point. The nav asks where a thumb can
  reach, so orientation decides it. The top bar asks whether the text fits, so width decides it.
  A tablet held upright answers those two questions differently and they should not share a
  breakpoint.

- **The shopping list was a two-pane split forced onto a phone**, with the picker permanently
  holding the top of the screen and an empty pane below reading "Choose a shopping list from the
  left" when there was no left. It now collapses to a plain navigation: picker, or items with a
  back button, decided by the route it already had. Both panes still show side by side from `lg:`.

- **`h-screen` on the shell.** On iOS Safari `100vh` is the viewport with the browser toolbars
  hidden, so the shell was laid out taller than the visible screen: the bottom bar sat below the
  fold and the page read as slightly too big. That is both outstanding phone complaints, and an
  emulator cannot reproduce either. Replaced with `.app-viewport` (`100dvh`, `100vh` fallback).
  **Unverified on a real phone**, because the fix is specifically for a browser not available here.

Measured and deliberately left alone:

- **The bottom bar is fine.** 54x61 tap targets across seven items at 375px, no label overflow.
- **No page overflows horizontally at 375px.** The wide containers on `/recipes` and `/activities`
  are intentional `overflow-x-auto` (filter chips, the kanban board), and the ones in Settings are
  the deliberate `-mx-5` card bleed.
- **21 of 30 pages still have no breakpoints, and that is correct.** A single-column page of cards
  does not need one. Adding breakpoints to pages that measure clean is churn.

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
