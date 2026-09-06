# Roadmap

*Fourteen phases, in the order they should be done. Each one is shippable on its own and leaves the
app better than it found it. Nothing here is half a feature that needs the next phase to be worth
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
| **Phase 3** *(5 Sep 2026)* | Ingredient notes reachable, a signed-in devices card with bulk sign-out, and the superseded amount columns dropped. |
| **Phase 4** *(5 Sep 2026)* | The page title stopped losing to its own buttons on a phone, the shopping list collapses to one pane, and the shell is sized in `dvh`. |

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

## Phase 5 · One way to draw each control, M *(new, 5 Sep 2026)*

Home already has a component library, 19 `Home*` components, and the pages mostly use it: 107
`<HomeButton>` against 72 raw `<button>`, 43 `<HomeTextInput>` against 13 raw `<input>`. This is not
about building the layer. It is about the controls that never got one, and the pages that quietly
went around the ones that exist.

**There is no `HomeSelect`, and it shows.** `<select>` appears **18 times across 9 files**, and the
same control is written six different ways:

| Written as | Times |
|---|---|
| `bg-ink-800` + focus ring + `focus:border-transparent`, `min-h-[48px]` | 4 |
| `bg-ink-800`, `min-h-[48px]`, **no focus styling at all** | 4 |
| `bg-ink-800` + focus ring, no `focus:border-transparent` | 4 |
| `bg-ink-900`/`border-ink-800`, `min-h-[44px]`, `text-ink-200` | 3 |
| two more one-offs | 3 |

Three focus treatments, two colour schemes and two tap-target heights for one control. **12 form
controls across the app have no visible focus ring**, which is not an inconsistency, it is a
keyboard user unable to see where they are. That is the reason to do this, not tidiness.

There is no `HomeTextArea` either: 4 raw ones across 2 files, and one of those was added during
phase 3 by copying the classes off a neighbouring field, which is exactly how the six variants above
happened.

The work:

- **`HomeSelect`**, then the 18 call sites. One focus treatment, one height, one palette.
- **`HomeTextArea`**, then the 4 call sites.
- **Audit the 72 raw `<button>`s.** Many are legitimate: a whole list row that happens to be
  tappable is not a `HomeButton`, and forcing it to be one would be worse. The ones to convert are
  those wearing button styling by hand. Do not convert on sight, convert what duplicates.
- **Write the rule down** in `home-conventions/references/blazor-ui.md`: a form control gets a
  `Home*` component before it gets a second call site, and copying a class list off a neighbouring
  field is the smell that says one is missing.

Deliberately not in scope: extracting a component that wraps a single element. That reads worse and,
for anything reading the code afterwards, costs more to follow than the markup it replaced. The rule
that pays is extracting what is repeated **across files**.

## Phase 6 · A shared calendar, and the time axis under it, XL *(new, 6 Sep 2026)*

`VISION.md` says the dashboard answers "what's happening this week" without navigation. Today it
answers that by making **seven separate API calls and assembling the answer by hand**, and there is
still nowhere to put an appointment. No event, no calendar entity, nothing that holds "swimming,
Tuesday, 4pm". A family keeps that somewhere, and while it is somewhere else, this is the second
thing they have to look at rather than the one application they rely on.

That is the feature. The reason it gets a planning stage instead of a design paragraph is
underneath it: **the app already holds time four ways and they do not agree with each other.**

| Where | How time is held | What that makes it |
|---|---|---|
| `MealPlanEntry.Date` | `DateTime`, deliberately no `UTC` suffix | a local calendar day |
| `Activity.DueDateUTC`, `CompletedDateUTC` | `DateTime?` in UTC | an instant |
| `LightSchedule` | `DaysOfWeek` bitmask, `TimeOfDay` as `TimeSpan`, `LastRunUTC` | a recurring wall-clock time |
| `Announcement.CreatedOnUTC` | `DateTime` in UTC | an instant |

Three genuinely different models of time, and `LightSchedule` is already the repo's only working
recurrence. A calendar bolted on without reconciling them becomes a fourth model, and then every
future feature that needs to ask "what is on this day" has to know which of four answers to trust.
Since a lot of what comes after this is time-shaped, that question gets settled once, here, in
writing, before any of it is built.

### Stage 1 · The planning, and it is the bulk of the phase

Nothing is built until `DECISIONS.md` carries an answer to each of these. Each one is cheap to
decide now and expensive to change after there is data in the table.

1. **Is the calendar a store, or a view over what already exists?** A planned meal, a task with a
   due date and a light schedule are all already "things on a day". If the calendar stores its own
   copy of them, they drift. If it only aggregates, it has nowhere to keep a plain appointment that
   belongs to nothing else. The likely answer is both, an events table plus a read model that
   folds the other four sources in, but "likely" is not a decision and this is the one that shapes
   every other answer.

2. **Whose event is it?** Household-level events need no identity and can be built now.
   Per-person events cannot, and retrofitting an owner onto a table full of rows is the expensive
   version. **If the answer is per-person, phase 7 moves in front of stage 2.** Deciding this
   first is most of why the planning stage exists.

3. **Recurrence, which is the hard part of every calendar ever written.** "Every Tuesday in term
   time", "the first Monday of the month", "every Tuesday except this one". The choice is a full
   RFC 5545 recurrence rule against a smaller home-grown model, and the trap in the smaller one is
   never recurrence itself but the **exceptions**: one moved or deleted occurrence in an infinite
   series. `LightSchedule.DaysOfWeek` is the prior art and it deliberately cannot express any of
   the above. Decide how far this goes, and write down what it will not do.

4. **All-day against timed, and what a "day" is.** An all-day event is a date, not an instant, and
   storing it as midnight-UTC is the classic way to make a birthday land on the wrong day for half
   the year. Everything in this repo is UTC-suffixed by convention, which is right for instants and
   wrong for dates: `MealPlanEntry.Date` already got this right by leaving the suffix off. The
   household row already carries latitude and longitude for sunrise, so there is a location to
   derive a zone from if a zone is wanted.

5. **Does it read the calendar the family already keeps?** Almost certainly the single highest
   value question here, and the largest. A one-way ICS subscription is a modest piece of work and
   makes the wall screen show the calendar that already exists. Two-way CalDAV is a different
   project. Read-only import, two-way, or neither, but decide it now, because "we will add sync
   later" is a sentence that changes the schema when it comes due.

6. **What the dashboard actually shows.** The glanceable promise is the point of the whole product,
   and today seven calls are stitched together in `DashboardPage`. Decide what one day looks like
   at a glance from across a kitchen before deciding what the full calendar screen looks like, not
   after. The dashboard is the screen that gets read; the calendar screen is the one that gets
   edited, and it is used far less often.

Stage 1 is done when those six have dated entries in `DECISIONS.md` and a schema sketch exists.
Not before.

### Stage 2 · The build

Shaped by stage 1, so this is the expected shape rather than a commitment:

- The entity, its EF configuration and a migration, scoped to the household through the same
  ownership path everything else uses.
- The slices, following the seven-file recipe: create, update, delete, and the read that answers
  "what is on between these two dates". The read is the one that matters and the one to test first,
  against a real database, because it is a projection across four sources and that is exactly the
  bug class this repo keeps hitting.
- A month and a week view, touch-first, and a day list that a phone can carry.
- The dashboard, rebuilt on the new read model rather than on seven hand-assembled calls.

### Deliberately not in scope

- **Invitations, attendees and RSVPs.** This is a household's own calendar on its own wall, not a
  scheduling product. Someone being on an event is a name on it.
- **Notifications and reminders.** A separate concern with its own delivery problem, and worth
  nothing until phase 7 knows who to notify.
- **Rewriting `LightSchedule` onto whatever recurrence lands.** Tempting and wrong to bundle: it
  works, it has no UI complaints against it, and folding a working feature into a new abstraction
  on day one is how the abstraction gets shaped by the wrong requirement. Revisit once the calendar
  has been in use.

## Phase 7 · Who is using this, XL *(was B1)*

The biggest gap against VISION's "family-proof… used by every member of the family". There is one
household login. `GetAssignedActivities` is a complete slice with its own presenter and
`User.AssignedActivities` navigation, and **assignment is captured on create and edit while no
screen ever asks "what's mine"**. A child cannot *be* themselves in the app.

Needs the deferred decision first: per-user PIN, device-trusted sessions, something else? Avatar
switching was refused on 14 Aug as weakening auth on a possibly-internet-facing app. The payoff is
a "My day" view, per-person chore lists, and who-did-what that means something.

This phase is why it sits here rather than later: it changes what phases 8 and 11 are worth.

## Phase 8 · The app remembers, M *(was B2, B6)*

Two features that surface data already being captured and stored.

- **Show the audit trail.** `Audit` is written correctly by ~15 interactors and **read by nothing**.
  Every mutation already records who did what and when. "Who ticked this off", "what changed on this
  recipe", a household activity feed. The cheapest large-feeling feature on this list, and much
  better after phase 7 gives the who a name.
- **Leftovers and meal history.** The meal plan knows what was cooked and when. Nothing surfaces
  "you had this three days ago", "cook once eat twice", or "you haven't made this in six months".
  Turns the planner from a schedule into something that gives advice.

## Phase 9 · The shop gets smarter, M *(was B4, B5)*

Both are shopping-list intelligence over the same data, so they share the groundwork.

- **Price memory and shop totals.** The honest version of the store-price-comparison idea. Items
  already carry a line price and suggestions already return the last price paid. Keep a per-item
  price history, show what a list will cost before you leave, and flag when something is dearer than
  usual. The 17 Aug decision says there is no per-kilo price anywhere and adding one means a new
  column, so decide that up front.
- **Aisle grouping.** `ShoppingListItem.Sequence` is read for display only. Let items carry a
  category (produce, dairy, freezer) learned from history, and group the list by it so a shop is one
  walk through the store instead of a scavenger hunt.

## Phase 10 · Nullable on in the API, L *(was C3)*

A clean build emits **one** warning, not the ~145 this file used to claim. That is not progress:
`Home.WebApi` sets `<Nullable>disable</Nullable>` while every other project enables it, and the API
models and controllers are where the `CS8618`s lived. They are suppressed, not fixed.

The job is turning nullable on there and absorbing what comes back in one go. No user-visible value,
which is why it sits this late, but it gets harder every phase that adds API models.

## Phase 11 · Beyond lights: a second device integration, L *(was B9)*

`DECISIONS.md` (12 Aug) establishes the adapter template: service interface in `Application`,
adapter in `WebApi`, an unreachable provider is a return value rather than an exception, vendor wire
types stay at the boundary. VISION says "other devices as they come" and nothing beyond lights is
researched. A thermostat, robot vacuum or smart plug is the obvious next one, and the pattern is
ready and proven.

## Phase 12 · The ones blocked on their own decisions, L each *(was B7, B10)*

Both are in `BACKLOG.md` with the open question written down. Neither is blocked on effort.

- **Feedback button and support reader.** Reading across all households breaks the
  household-isolation invariant every other query obeys, so it needs either a separate operator app
  or an explicit support role with its own policy. `ApiAuditEntry` already captures request bodies,
  IPs, user agents and timing, so the substrate exists.
- **Spotify.** OAuth is *per user* rather than one household token like LIFX, so it needs an
  authorisation-code flow with refresh, a callback URL, and a decision about whose account the
  kitchen tablet plays from. Needs Premium. Naturally follows phase 7, which answers "who is this
  device".

## Phase 13 · Where we have been, L *(from the phase ideas list, 6 Sep 2026)*

The household already keeps what it cooks, what it buys and what it has to do. It keeps nothing
about where it went. "That playground with the shade", "the Thai place we liked", "the beach we
drove an hour to and would not again" is exactly the kind of thing a family asks each other and
nobody can remember, and it is the same shape as the notes work already done twice: knowledge that
belongs to the household rather than to whoever happened to be there.

Two things in one, and the phase has to decide how much of each:

- **A place the household knows.** A name, roughly where it is, what kind of thing it is, and what
  the family thinks of it. This is a directory and it is the half that answers "where could we go".
- **A visit.** A place on a date, with who went and how it was. This is a log and it is the half
  that answers "when were we last there".

The directory is worth having on its own; the log is not, which is the order to build them in.

Decide first:

- **Does a visit become a calendar entry?** Phase 6 settles what a thing on a day is, and a visit
  is one. Building this before that means a fifth answer to a question that phase exists to have
  one answer to, so this sits behind it.
- **How is a place located?** A name and a suburb costs nothing and is probably enough to jog a
  memory. Coordinates and a map is a different project, and the household row already carries a
  latitude and longitude for sunrise if it turns out to be wanted.
- **Whose opinion is it?** A single household verdict, or one per person. Per person needs phase 7,
  the same coupling the calendar has.

Deliberately not in scope: check-ins, anything that tracks location automatically, and anything
that talks to a third party for reviews or opening hours. This is the household's own memory of its
own outings, which is the only version of this that is worth keeping and the only one that does not
need a privacy decision first.

## Phase 14 · It looks like something, M *(from the phase ideas list, 6 Sep 2026)*

`VISION.md` asks for a product that does not look assembled from template defaults, and the type
and colour work carries that. The identity does not exist at all yet:

- **The favicon is the framework's.** `wwwroot/favicon.png` is the stock 32px purple mark that came
  with the project on 19 May and has not been touched since. Its purple appears nowhere else in the
  app.
- **There is no app icon and no web manifest.** The delivery surface for this product is a tablet
  running it full screen on a kitchen bench. Added to a home screen today it gets a generic icon
  and a URL for a name, which is the first thing anyone sees of it and the last thing anyone
  configured.
- **There is no wordmark.** Every place the product names itself, it does so in body type.

The work is an icon at the sizes a home screen and a browser tab actually ask for, a manifest so
installing it produces something with a name, and a wordmark that uses the display face already
chosen rather than introducing another. Everything else in the visual system stays: this phase is
the mark, not a redesign.

**No generated images.** Drawn, photographed, set in type or built from the geometry of the palette.
This applies to the icon, to any illustration an empty state ever gets, and to anything that ships
in `wwwroot`. A house that cooks its own food should not have a stock photo of a kitchen on the
wall.

