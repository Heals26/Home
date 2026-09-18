# Roadmap

*Twenty phases, in the order they should be done. Each one is shippable on its own and leaves the
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
| **Phase 5** *(7 Sep 2026)* | Six new components and the whole app moved onto them: 72 raw buttons down to 2, 118 icon spans, 18 selects, 13 inputs and 5 textareas all down to 0. |
| **Phase 6** *(8 Sep 2026)* | The shared calendar: six decisions recorded, `CalendarEvent` with simple repeats and skips, read-only iCalendar subscriptions, month, week and list views, and the dashboard rebuilt on one calendar read. |
| **Phase 7** *(8 Sep 2026)* | Identity decided as no switching on shared devices; members without a login; who ticked a chore off recorded and shown; a per-device "Just me" switch on the board and member chips on the calendar. |
| **Phase 8** *(8 Sep 2026)* | History in words: every change recorded with a household and a summary, a `/history` page and a Recently tile, per-thing history on chores and recipes, and a planner that remembers what was last had. |
| **Phase 9** *(11 Sep 2026)* | Aisles and price memory: the household's own aisles with a My order or By aisle switch on each list, and each line saying what it usually costs, whether this price is dearer and roughly what an unpriced one will come to. |
| **Phase 10** *(16 Sep 2026)* | Shopping mode: a shop with a start and an end on each list that every phone joins, prices recorded only during a shop and tied to it, and an aisle layout per phone with whole-row ticks, a pencil to edit and the trolley total pinned. |

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

## Phase 5 · One way to draw each control, M *(new, 5 Sep 2026)* **DONE 7 Sep 2026**

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

### What was done, 6 Sep 2026

**No raw `<select>` or `<textarea>` is left in the app.** `HomeSelect<TValue>` took all 18 call
sites and `HomeTextArea` took 5, one more than this phase counted, because the shopping list note
added during the bug list was written the copied-classes way this rule exists to stop. Both carry
the same fill, border, height and focus ring as `HomeTextInput`, so the three read as siblings in
the forms where they sit together.

`HomeSelect` is generic and takes an `Options` list rather than `<option>` markup, which had a
consequence worth the extra work: four page fields that were strings only because a raw `<select>`
speaks strings are now `long?` or `int?`, and eight `.ToString()` calls and two `ParseLong` helpers
went with them.

**15 of the 72 raw buttons were the same square icon button** written in five treatments across six
files, so `HomeButton` gained `IconOnly` and took them. Its hover and press states are now gated on
`enabled:`, because a button that is genuinely off should not light up under a cursor. Two of the 15
were the lights page reordering its groups with literal arrow glyphs when `HomeReorder` already
existed. That leaves 57, and they are the legitimate ones: tappable rows, segmented cells and
link-styled text.

### The wider goal, and where it landed

Mitch, 6 Sep 2026: the point of this phase is **minimising raw HTML tags**. A tag should be written
in one file and everything else should inherit from it. That is what keeps the app consistent, and
it applies to `<span>`, `<p>` and `<img>` as much as to `<button>`.

Measured against that bar, on 7 Sep 2026:

| Tag | Before | After | Where the survivors are |
|---|---|---|---|
| `<button>` | 72 | **2** | `HomeButton`, and Blazor's reconnect overlay in `App.razor` |
| `<span class="home-icon">` | 118 | **0** | all through `HomeIcon` |
| `<input>` | 13 | **0** | in the pages; 5 remain inside the input components themselves |
| `<select>` | 18 | **0** | `HomeSelect` |
| `<textarea>` | 5 | **0** | `HomeTextArea` |

Six components came out of it. `HomeIcon` took all 118 icon spans, which needed `home-icon-`
safelisting in the Tailwind config because the name is now only half-written in the source.
`HomeButton` gained `bare` and `link` variants, a `none` size, and four event flags
(`StopPropagation`, `PreventDefault`, `KeepFocusOnMouseDown`, `OnDragStart`) for the Blazor
directives a component cannot forward. `HomeListRow`, `HomeChip`, `HomeCardAction` and `HomeSwatch`
are the four shapes that repeated across files, and each renders a bare `HomeButton` rather than a
second `<button>`. `HomeCheckbox` took the one raw checkbox.

Two defects fell out of the sweep. `HomeButton` was forcing `relative` on every caller, which
Tailwind emits after `absolute` and which had dropped the delete button meant to float over a
recipe photo into the flow. And four `aria-pressed`/`aria-checked`/`aria-expanded` attributes were
bound straight to a bool, which Blazor renders by dropping the attribute when false, so a switch
read to a screen reader as an ordinary button.

**Still standing:** `<span>` at 116 and `<p>` at 166 across the app. Those are text and layout
rather than controls, so there is no obvious component behind most of them, and turning every
paragraph into `HomeText` would cost more than it pays. A `@foreach` body that is a whole card is
the better next target, and phase 17 will want that anyway.
## Phase 6 · A shared calendar, and the time axis under it, XL *(new, 6 Sep 2026)* **DONE 8 Sep 2026**

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

### What shipped, 8 Sep 2026

Stage 1 first: the six questions have dated entries in `DECISIONS.md` (all 8 Sep 2026) and the
schema sketch sits in the first of them. Then stage 2, as the expected shape said, with one
difference: the read model went in before any screen and was tested against a real database
through its presenter, which caught nothing this time, and that is the point.

- `CalendarEvent`, `CalendarEventMember`, `CalendarEventException` and `CalendarSubscription`,
  one migration (`AddCalendar`), every row reaching the household directly.
- Slices: `GetCalendar` (the projection across events, meals and tasks, placed on the viewer's
  local day), `GetCalendarEvent`, `CreateCalendarEvent`, `UpdateCalendarEvent`,
  `DeleteCalendarEvent` (whole event, or one skipped occurrence), and five for subscriptions
  including the runner's `RefreshAllCalendarSubscriptions`. 37 tests.
- `/calendar` with Month, Week and List views (List is what every narrow screen gets), a sheet per
  item that offers only what that kind of item can do, and an editor whose repeat controls appear
  only once a repeat is chosen. Editing or removing "this time only" on a series is a skip plus a
  standalone copy, as decided.
- Settings gained an "Other calendars" card; the nav rail gained Calendar with its own lavender
  hue; the dashboard's "This week" tile became "Today" and the board makes six calls, not seven.

One live defect fell out of driving the new screens: `HomeToggle` carried a Razor comment inside
its `<HomeButton` tag, which Blazor sends to the browser as an attribute name; setting it throws
and the whole circuit dies the moment any toggle renders. Every page with a toggle had been broken
since the phase 5 sweep. The comment moved above the tag.

**Left for later, on purpose:** filtering the calendar by person (waits on phase 7 to mean
anything), dragging an occurrence to move it (the sheet does it in two taps and touch fires no
drag events), showing a subscribed calendar in its own colour, and converting the rest of the app
from server-local time to `IViewerClock`, which is a separate job now the seam exists.

## Phase 7 · Who is using this, XL *(was B1)* **DONE 8 Sep 2026**

The biggest gap against VISION's "family-proof… used by every member of the family". There is one
household login. `GetAssignedActivities` is a complete slice with its own presenter and
`User.AssignedActivities` navigation, and **assignment is captured on create and edit while no
screen ever asks "what's mine"**. A child cannot *be* themselves in the app.

Needs the deferred decision first: per-user PIN, device-trusted sessions, something else? Avatar
switching was refused on 14 Aug as weakening auth on a possibly-internet-facing app. The payoff is
a "My day" view, per-person chore lists, and who-did-what that means something.

This phase is why it sits here rather than later: it changes what phases 8 and 11 are worth.

### What shipped, 8 Sep 2026

The deferred decision was taken first and it went the strict way: **no switching on shared
devices**, so a device is one member and stays that member (three entries in `DECISIONS.md`, all
8 Sep 2026). What that left to build:

- **Members without a login.** `User.Email` and `User.Password` are optional as a pair. The
  Settings members card adds a member with or without a sign-in, shows "No sign-in" on those
  without, and can give one a login later or take it away. `RegisterHousehold` still requires
  both, because someone has to be able to open the door. Migration
  `MembersWithoutLoginAndCompletedBy`.
- **Who did what.** `Activity.CompletedByUser`, set from the session whichever route ticked the
  card off, shown on the card, the detail page and the calendar. `DeleteUser` unhooks both
  activity links itself, since neither can cascade.
- **The personal lens.** `PersonUserIDs` on every calendar item; an "Everyone / Just me" switch on
  the dashboard's Today tile that is a per-device choice in the browser; member chips on the
  calendar page.

Fixed along the way: `UpdateUser`'s email conflict check counted the member being edited as a
clash with themselves.

**Left for later, on purpose:** roles and permissions (declined for now, not forever), a greeting
by first name, and per-person chore lists beyond the "mine" filter the board already had. Phase 8's
audit trail now has a real "who" to show.

## Phase 8 · The app remembers, M *(was B2, B6)* **DONE 8 Sep 2026**

Two features that surface data already being captured and stored.

- **Show the audit trail.** `Audit` is written correctly by ~15 interactors and **read by nothing**.
  Every mutation already records who did what and when. "Who ticked this off", "what changed on this
  recipe", a household activity feed. The cheapest large-feeling feature on this list, and much
  better after phase 7 gives the who a name.
- **Leftovers and meal history.** The meal plan knows what was cooked and when. Nothing surfaces
  "you had this three days ago", "cook once eat twice", or "you haven't made this in six months".
  Turns the planner from a schedule into something that gives advice.

### What shipped, 8 Sep 2026

Three decisions in `DECISIONS.md` (all 8 Sep 2026) and then the build:

- **The audit row grew up.** `Household` and `Summary` on `home.Audit`, backfilled; every
  `AuditBase` subclass puts what happened into words from the change tracker; deleting a thing
  writes "removed …" instead of erasing its rows; meal plan entries, recipes, calendar events and
  subscriptions are recorded for the first time. Migration `AuditHouseholdAndSummary`.
- **Reading it.** `GetHistory` (paged, newest first, narrowed by category) and `GetEntityHistory`;
  a `/history` page with per-device category chips and "Show earlier"; a Recently tile on the
  board that reloads on any household change; a History card on the chore page and the recipe page.
- **The planner remembers.** `LastHadDate`, `NextPlannedDate` and `TimesHad` on every recipe, read
  off the meal plan; the picker says "Had 3 days ago" in amber when recent, offers a "Due a turn"
  strip, and makes a one-tap leftovers offer after a recipe is planned; the recipe page says when it
  was last had and when it is next.

**Left for later, on purpose:** a per-household (rather than per-device) default for the feed's
kinds, undo from the feed, and recording individual shopping ticks, which would drown everything.

## Phase 9 · The shop gets smarter, M *(was B4, B5)* **DONE 11 Sep 2026**

Both are shopping-list intelligence over the same data, so they share the groundwork.

- **Price memory and shop totals.** The honest version of the store-price-comparison idea. Items
  already carry a line price and suggestions already return the last price paid. Keep a per-item
  price history, show what a list will cost before you leave, and flag when something is dearer than
  usual. The 17 Aug decision says there is no per-kilo price anywhere and adding one means a new
  column, so decide that up front.
- **Aisle grouping.** `ShoppingListItem.Sequence` is read for display only. Let items carry a
  category (produce, dairy, freezer) learned from history, and group the list by it so a shop is one
  walk through the store instead of a scavenger hunt.

### What shipped, 11 Sep 2026

Four decisions in `DECISIONS.md` (all 11 Sep 2026) and then the build:

- **A memory of the shop.** `ShoppingItemMemory` keeps, per household and per item name (matched
  without regard to case), the aisle an item goes in and what it has cost, in `ShoppingItemPrice`.
  It outlives the list lines, which "Clear ticked" deletes. A priced line ticked into the trolley is
  a purchase; a new price or an untick within twelve hours corrects it; "Untick all" keeps it.
  Migration `ShoppingAislesAndPriceMemory`, which also gives every existing household the eight
  starter aisles.
- **Prices compared per unit.** No per-kilo column after all: purchases that share a unit compare on
  cost divided by amount (grams with kilograms, millilitres with litres, anything else only with
  itself). Each line carries what it usually costs at its amount, a flag when its price is more than
  a tenth over that, and a guess for an unpriced line from the last purchase. The list shows "About
  $X" for the whole shop and says how many lines it could not price.
- **Aisles.** The household's own aisles (`ShoppingCategory`), eight to start, renamed, reordered,
  added and removed in an Aisles editor on the list. An item has no aisle until someone picks one in
  its edit sheet, and from then on it lands there on every list. Each list has its own "My order /
  By aisle" switch, and reordering stays with My order.

**Left for later, on purpose:** suggestions still read the last price off list lines rather than
the memory, nothing shows or edits an item's price history, and a renamed item starts a fresh memory
rather than carrying its prices across.

## Phase 10 · Shopping mode, L *(new, 14 Sep 2026)* **DONE 16 Sep 2026**

The shopping list is written at home and used in a supermarket, and it behaves the same in both
places. Two problems come from that, one found in real use and one in the dry run of phase 9 on
14 Sep.

**The row is built for reading, not for a trolley.** The 31 Aug decision split each row into two
targets, the circle ticks and the words open the item, because a whole-row tick crossed things off
when someone only meant to read the amount. On a phone in a shop that trades one mis-tap for
another: reaching to tick opens the sheet, and reaching to change something ticks it. The decision
was right at home. It is wrong in the aisle.

**A shop has no edges.** Phase 9 records a purchase when a priced line is ticked, and guesses where
the shop ends with a twelve-hour window. The dry run found two ways that guess loses:

- After Untick all, ticking a line by mistake and unticking it again within twelve hours of the real
  shop deletes the price that was actually paid.
- Saving a ticked line's sheet more than twelve hours after the tick records its price a second
  time.

The 17 Aug decision that unticking needs no confirmation "because nothing is lost" stopped being
true when a tick started recording what something cost.

Shopping mode gives a shop a start and an end. While it is on, the list is laid out for the aisle, a
tick is a purchase and an untick takes that tick back. Ending it settles the shop, and nothing done
to the list afterwards can reach those purchases.

The data half does not strictly need the mode: remembering when each line was ticked would stop both
losses with nothing changing on screen. It is the first step of this phase either way, because
everything else here rests on knowing which tick recorded which price.

### What the planning has to settle

1. **Is it the phone's or the list's?** The layout belongs to the device: the phone in the shop wants
   big targets and the kitchen tablet at home does not. The shop belongs to the list: two people who
   split up at the door are doing one shop. The likely answer is both, a per-device layout like the
   board's "Just me" switch over a per-list shop that any phone can start.

2. **How does a shop start and end?** A Start and a Done button is honest and easy to forget. The
   end matters more than the start. A phone goes back into a pocket without anyone pressing Done, so
   a shop has to end by itself after a quiet spell, and that quiet spell is what replaces the
   twelve-hour window. Untick all and Clear ticked should end one too.

3. **What is a tick outside a shop?** Ticking things off while unpacking at home is not buying them.
   Either a tick only records a purchase during a shop, which rules out accidental purchases and
   relies on someone starting one, or a tick outside a shop starts one, which needs no button and
   brings the guessing back.

4. **What does the row do in the aisle?** The whole row ticks. Editing has to go somewhere a thumb
   will not hit by accident and can still find: a control at the end of the row rather than a long
   press, for the same reason the 31 Aug decision ruled out hover. The edit that matters most in a
   shop is typing what something cost, so decide whether a tick offers that straight away instead of
   opening the whole sheet.

5. **What else is different in the aisle?** Candidates: the add box steps back, the reorder controls
   go, By aisle is the default, the screen stays awake and the trolley total stays in view. Each is
   small. Decide which belong to the mode and which are simply better for the list.

### Deliberately not in scope

- **Working without signal.** Blazor Server needs a live connection, and a supermarket's back aisle
  is where it drops. That is a real problem for the people this phase is for, and it is a different
  phase's work.
- **Anything that notices you are at the shop.** No location and no geofence. A person starts a shop.

### What shipped, 16 Sep 2026

Four decisions in `DECISIONS.md` (all 16 Sep 2026) and then the build:

- **A shop on each list.** `ShoppingTrip` holds when a shop started, when anything last happened on
  it and when it ended. Start shopping joins the shop already going rather than starting another,
  even when two phones press it together, and Done, Untick all, Clear ticked or two quiet hours end
  it. Migration `ShoppingTrips`.
- **Prices tied to the shop.** A priced line ticked during a shop records one purchase against that
  shop. Unticking while the shop is going takes it back; after the shop the purchase stands, and a
  price typed in later for a line still ticked corrects that purchase instead of adding another. The
  twelve-hour window is gone, and both losses from the 14 Sep dry run have tests.
- **The aisle layout.** Each phone remembers the shop it joined. While it is in one, the list reads
  by aisle, the whole row ticks and a pencil opens the item, reordering and the view switch go, and a
  bar pinned to the foot of the screen shows what the trolley comes to, how many lines are in it,
  and Done. A phone that has not joined sees "Shopping in progress" with a Join button instead of
  Start shopping.

**Left for later, on purpose:** a ticked line still leaves its aisle for the trolley section straight
away, so a mis-tap is put right by opening the trolley and unticking it, which is where the undo
question from 14 Sep comes back in; a purchase recorded before trips existed belongs to no trip, so a
line that was already ticked when the migration ran counts its own price towards its usual until it
is unticked; and the screen is not kept awake.

## Phase 11 · Nullable on in the API, and undo, XL *(was C3; undo added 17 Sep 2026)* **DONE 18 Sep 2026**

Two jobs, done in this order. Undo adds API models, and every API model written before nullable is on
is one more to fix after it.

### Nullable on in the API

A clean build emits **one** warning, not the ~145 this file used to claim. That is not progress:
`Home.WebApi` sets `<Nullable>disable</Nullable>` while every other project enables it, and the API
models and controllers are where the `CS8618`s lived. They are suppressed, not fixed.

The job is turning nullable on there and absorbing what comes back in one go. It has no
user-visible value on its own, and it gets harder every phase that adds API models.

### Undo

Undo exists in exactly one place. Applying a light scene saves the room as it was, and "Previous
look" puts it back (20 Aug). Everywhere else a mistake is fixed by hand or not at all, and the app
leans on confirmations instead: Clear ticked asks first "because there is no undo" (17 Aug), and
deleting a list wants a second tap.

Phase 10 left the case that raised it. In the aisle a tick moves the line out of its aisle and into
the trolley section straight away, so a mis-tap means opening the trolley and unticking the line
there, one-handed, beside a trolley. Mitch, 17 Sep 2026: undo goes in with the nullable work.

A confirmation interrupts every time to guard against the rare mistake. An undo costs nothing until
the mistake happens. On a shared kitchen screen used by people who never read a manual, that is the
better trade wherever the undo can be made honest.

#### What the planning has to settle

1. **Which actions can be undone?** Ticks and unticks, removing a line, Clear ticked and Untick all,
   deleting a list, a chore or a recipe, and edits. Ticks are the cheap ones and deletes are the
   valuable ones. Decide whether undo is everywhere or a named set, and whether an action's
   confirmation goes once it can be undone.

2. **How long is an undo on offer?** An "Undo" shown for a few seconds after the action is the
   version everyone already understands, and the app has nothing to show one in yet. Undoing from
   the history feed, which phase 8 left for later, is the powerful version, and it collides with
   everything that has changed since. Probably the first; decide whether the second is ever in
   scope.

3. **How does a deleted row come back?** A delete is gone for good today, so undo needs the row
   from somewhere. A soft delete means every query filters it, and household isolation already runs
   through every one of those queries. A copy kept for the length of the undo is contained but has
   to carry everything that went with the row. Holding the delete back until the undo expires shows
   other devices something that is about to vanish. The choice decides how much of the app this
   phase touches.

4. **Whose undo is it?** The device that did it, or anyone in the household. Two people shopping one
   list see each other's ticks live, so an undo of something another phone has changed since is the
   conflict to decide.

5. **What else has to be put back?** Undoing a tick during a shop takes back the price that tick
   recorded, which an untick already does. Undoing Clear ticked restores the lines, and has to say
   whether the shop it ended opens again. Undoing Done reopens a shop. Every action with a side
   effect needs its answer written down.

#### Deliberately not in scope

- **Redo.** One step back is the need. A stack of them is an editor, not a household app.
- **Versions.** This puts back the last thing done, not what a recipe said in June.
- **Light commands.** The lights already have their undo, and a command a bulb has carried out
  cannot be recalled.

### What shipped, 17 and 18 Sep 2026

**Nullable on in the API, 17 Sep.** `Home.WebApi` enables nullable like every other project, and the
175 warnings that came back were fixed rather than suppressed. MVC's implicit `[Required]` is off
with it, so a request still fails in the input port validator's words with a 422 rather than in
MVC's with a 400. The one warning a clean build emits is now a real one.

**Undo, 18 Sep.** Three decisions in `DECISIONS.md` (all 17 Sep 2026) and then the build:

- **A delete sent with an undo token is held back rather than carried out.** `DeletedOnUTC` on the
  30 kinds of row a household owns, a query filter on each so a held-back row is gone as far as
  every query is concerned, filtered unique indexes so a name a delete freed can be used again, and
  a purge every minute that carries out the deletes no undo can reach any more. Migration `Undo`.
- **What a request changed is written down against its token.** A device sends `X-Undo-Token` with
  an action it may offer to take back, and everything that request adds, changes or holds back is
  kept as JSON on `UndoableAction`. `POST api/Undo/{token}` puts back only what is still as the
  request left it, and refuses whole an action it could only half undo. The API honours a token for
  a minute and the purge waits two, so an Undo tapped in time never finds its rows gone.
- **The bar.** Every tick and every delete in the app goes through the web app's undo logic, and for
  about ten seconds a bar says what was done and offers Undo. It belongs to the tab that did it, a
  newer action takes it from an older one, and where the foot of the page is covered, inside an open
  dialog or over the trolley bar in the aisle, it shows there instead so Done never moves. Undoing
  Done puts this device back into the shop it left, and an undo that arrives too late says so.
- **Twenty tests** over undoing a line, a tick, a purchase, Clear ticked, Untick all, Done, a list,
  an aisle, a member, a recipe ingredient, a planned meal and a chore, over what is refused (another
  device's change, an expired token, a second undo, another household, an action that could only be
  half undone), and over the purge.

**Left for later, on purpose:** an edit is not undone, and neither is signing a device out or
clearing a note by emptying its box, which are an edit and a session rather than a delete; the
history feed still offers nothing to undo from, which phase 8 left for later; and the bar holds one
action at a time, so a run of ticks can only take the last one back.

## Phase 12 · Beyond lights: a second device integration, L *(was B9)*

`DECISIONS.md` (12 Aug) establishes the adapter template: service interface in `Application`,
adapter in `WebApi`, an unreachable provider is a return value rather than an exception, vendor wire
types stay at the boundary. VISION says "other devices as they come" and nothing beyond lights is
researched. A thermostat, robot vacuum or smart plug is the obvious next one, and the pattern is
ready and proven.

## Phase 13 · Feedback button and support reader, L *(was B7)*

In `BACKLOG.md` with the open question written down, and blocked on that decision rather than on
effort.

Reading across all households breaks the household-isolation invariant every other query obeys, so
it needs either a separate operator app or an explicit support role with its own policy.
`ApiAuditEntry` already captures request bodies, IPs, user agents and timing, so the substrate
exists.

## Phase 14 · Spotify, L *(was B10)*

In `BACKLOG.md` with the open question written down, and blocked on that decision rather than on
effort.

OAuth is *per user* rather than one household token like LIFX, so it needs an authorisation-code
flow with refresh, a callback URL, and a decision about whose account the kitchen tablet plays
from. Needs Premium. Naturally follows phase 7, which answers "who is this device".

## Phase 15 · Where we have been, L *(from the phase ideas list, 6 Sep 2026)*

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

## Phase 16 · It looks like something, M *(from the phase ideas list, 6 Sep 2026)*

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


## Phase 17 · A board the household arranges itself, L *(new, 6 Sep 2026)*

The dashboard fits without scrolling as of 6 Sep, on a landscape tablet, with the seven tiles that
exist today. That is the cheaper half of the answer and it is already showing its edges: "fits"
was measured at 1280x800 and 1024x768, it needed a third column and a density pass to get there,
and it holds only until the eighth tile is added or a household plans enough activities to make a
tile taller. Every tile added from here reopens the question.

The other half is letting the household decide. Which tiles it wants, in what order, at what size.
That is also the honest answer to why a tile should be there at all: a home with no smart lights
has a Lights tile it will never look at, and no way to be rid of it.

**Gridstack is the candidate.** It is the library for exactly this shape of problem: a vanilla
TypeScript grid whose cells drag, resize and reflow, built for dashboards, and it carries its own
drag implementation rather than leaning on HTML5 drag events. That last point is worth more here
than it sounds: this app's existing drag works with a mouse and does nothing on a touch screen,
which is why every drag added so far has a button beside it. A library that drags under a finger
would be the first thing in the app that does.

### What the planning has to settle

1. **Whose layout is it?** A board arranged on the kitchen tablet and a board on someone's phone
   are not the same board, and the same screen may be read by four people. Per device is the
   simplest and probably right, since the appearance preference already works that way. Per person
   needs phase 7 first, the same coupling the calendar has.

2. **Does Blazor or Gridstack own those DOM nodes?** This is the technical risk and it should be
   proven before anything is designed around it. Gridstack moves elements; Blazor's renderer
   believes it owns the tree and will fight anything that rearranges it underneath. The workable
   shape is usually that Blazor renders each tile's contents and never re-renders the container
   Gridstack is managing, which means deliberate `ShouldRender` and `@key` discipline. There are
   community Blazor wrappers to look at rather than starting cold. **Spike this first**: if it
   cannot be made stable, the phase becomes a simpler show-and-hide-and-order with no resizing,
   which delivers most of the value.

3. **Is this the app's first external JavaScript dependency?** Everything in `wwwroot/js` today is
   ours and small: `board.js` is 25 lines. Taking a library is a reasonable trade for this problem
   and a decision to make on purpose, including where it is served from, given the fonts already
   taught us what an unreachable CDN does to a kitchen tablet.

4. **What does resizing actually mean for a tile?** A tile is not a chart. Half-width Shopping
   showing four lists and full-width Shopping showing four lists is the same tile in a wider box.
   Either sizes change what a tile shows, which is real work per tile, or sizing is dropped and
   only order and visibility are offered. Decide before building, because the answer changes how
   every tile is written.

5. **How does it get back to normal?** Anything arrangeable needs a way to undo an arrangement,
   and on a shared screen it needs it more, because whoever broke the layout is not necessarily
   whoever wants it fixed.

### Deliberately not in scope

- **Tiles from anywhere but this app.** No embeds, no arbitrary widgets.
- **A layout that syncs between devices.** That is the per-person question above wearing a hat, and
  it is the wrong default: the tablet's board and a phone's board want different things.
- **Retiring the fitted layout.** It stays as what a household gets before it has arranged anything,
  because a board that starts empty or starts scrolling is a worse first run than the one that
  exists now.


## Phase 18 · What this house pays for, M *(new, 6 Sep 2026)*

Home knows what is for dinner, what is on the shopping list and what has to be done this week, and
nothing at all about the eleven direct debits leaving the account. Streaming, insurance, the gym,
school fees, the phone plan, the thing somebody signed up to in March and nobody has watched since.
This is household admin of exactly the kind the other pillars already handle, and it is the one
where forgetting costs money.

The app already tracks spend in precisely one narrow place, `ShoppingListItem.Cost` and the
list total, so money on a screen is not a new idea here. What is new is money that repeats.

The single most valuable thing it can do is not the list. It is **the warning before the annual one
lands**, and the free trial that is about to become a payment. Everything else is bookkeeping.

### What it holds

A subscription is a name, an amount, how often it is charged, and when it next is. Add to that
whether it is still live, because a cancelled one is worth keeping: half the value of a list like
this is seeing what you stopped paying for and when.

### What the planning has to settle

1. **How the repeat is expressed, which is phase 6's question again.** "The 15th of every month",
   "annually in March", "every four weeks" are the same shape of problem the calendar has to solve,
   and this must not become a fifth answer to it. **This phase sits behind phase 6** for that
   reason alone, and it is a good early test of whatever recurrence model that phase lands on: if
   it cannot express a billing cycle it is not finished.

2. **What the headline number is.** A monthly total is the number people want, which means an
   annual subscription has to be divisible by twelve to sit alongside a monthly one. Decide whether
   the app normalises to a monthly equivalent, shows the real cadence, or shows both, because that
   choice decides what the screen is for.

3. **Does it warn, and how?** The calendar phase already parked notifications, because there is
   nobody to notify until phase 7 knows who is here. Until then a warning is something the
   dashboard shows rather than something that reaches a phone, which is a fair first version on a
   screen that is always on anyway.

4. **Does it earn a dashboard tile?** Probably, and that is exactly the pressure phase 17 exists
   to relieve: the board fits today with seven tiles and an eighth reopens it. Whichever of the two
   phases is done second inherits the problem.

### Deliberately not in scope

- **Reading a bank feed.** Connecting to an account to find subscriptions automatically is a
  different product with a different trust conversation. This is a list the household keeps.
- **Multiple currencies.** One household, one currency, until there is a reason.
- **Paying anything.** It records what is charged. It never charges.

## Phase 19 · Other households can pay for this, XL *(new, 6 Sep 2026)*

The product bar for this app has always been that a family would pay for it rather than use the
free thing. This is the phase that finds out.

**The architecture is already multi-tenant and the front door is not.** Every query in the
application scopes through an ownership path to `IAuthorisationService.GetHousehold()`, and every
one of the 608 tests seeds a second household alongside the first to prove nothing leaks between
them. That invariant is the expensive part of selling this to more than one family and it is
already done and pinned.

What is not done is letting a second family in at all:

- **Registration is first-run only, on purpose.** `GetSetupStatus` returns true only while the
  database has **no users whatsoever**, and the setup page disappears the moment the first one
  exists. Today: 1 household, 1 user. The second household cannot sign up, by design.
- **`home.ClientApplication` is still inserted by hand.** Deliberate, documented in `README.md`,
  and parked in `BACKLOG.md`. A product people pay for cannot have a manual database step between
  someone deciding to try it and seeing it.
- **Phase 7 has not happened.** There is one login per household. Selling a family organiser whose
  members cannot be themselves is selling the wrong thing.

### What the planning has to settle

1. **Where it runs.** `VISION.md` deliberately left hosting open, cloud or local, and asked that
   nothing close either door without a decision here. **Charging for it closes that door**: taking
   money means running it, backing it up, and being reachable when it breaks. That is the decision
   this phase actually turns on, and it is bigger than the billing code.

2. **What is free.** A paid tier only means something against a free one. Self-hosting staying free
   and unlimited is one honest answer and it fits how this was built.

3. **Who takes the money.** A payment provider, which brings a webhook, a subscription state
   machine, dunning, refunds and tax. None of that is interesting and all of it is required. Note
   the irony worth avoiding: this app would then have both a `Subscription` the household tracks
   (phase 18) and a subscription the household *is*. Two different things and they must not share
   a name in the code.

4. **What happens when someone stops paying.** The answer cannot be that a family loses the
   shopping list they are standing in a supermarket holding. Read-only, an export, a grace period:
   decide it before the first payment, not after the first lapse.

5. **What it costs.** The comparison is a category where a device plus a subscription runs to
   hundreds a year and a good list app runs to about fifteen. Somewhere in between is a product;
   at either end is a hobby or a bad deal.

### Order

Behind phase 7 and behind the hosting decision, both of which it depends on outright. Nothing here
is hard next to what is already built; it is late because it is the one phase that changes what
this project *is*, and the only one that cannot be undone by deleting some code.

## Phase 20 · A receipt fills in the prices, XL *(new, 17 Sep 2026)*

Price memory (phase 9) is only as good as the prices typed into it, and nobody types prices at the
shelf. Phase 10 made that the rule: in the aisle a tick only ticks, and a price can be filled in
afterwards for as long as the line stays ticked. That is the honest shape, and filling in thirty
prices by hand at the kitchen bench is exactly the chore that does not get done.

The receipt already has every price on it. Mitch, 16 Sep 2026: take a photo of the receipt and let
it fill in the items.

Phase 10 is what makes this buildable. A shop has edges now, so a receipt belongs to one shop, the
lines ticked on that shop are the ones to price, and a price filled in after Done already corrects
the purchase it belongs to instead of adding another.

### What the planning has to settle

1. **What reads the receipt?** Nothing in the app reads an image today. On the device, on the
   server, or a paid service that reads documents or images: each differs in how well it reads a
   crumpled thermal slip, what a receipt costs to read, and what leaves the house. A service needs a
   key, and phase 1 took setup from six secrets to two, so it has to be optional to install as well
   as to use. Whatever it is sits behind the adapter template the lights proved (12 Aug), so an
   unreachable reader is a message rather than a crash. **Prove the reading first** on real
   receipts from the shops this household uses, before anything is built around it.

2. **How does a receipt line find its list line?** Receipts abbreviate ("FC MILK 2L"), print weighed
   produce as a price per kilo, split multibuys, and put discounts on lines of their own. Matching
   has to learn: once someone confirms a receipt name is Milk, the household should not be asked
   again, which probably means aliases on `ShoppingItemMemory` beside the name key it already
   matches on.

3. **Does anything save without a look?** Almost certainly not. The first version shows what it
   matched, what it could not, and what would change, and writes nothing until someone confirms. A
   wrong price saved quietly skews "usually" for weeks, which is worse than no price at all.

4. **What about things that were never on the list?** The bread nobody wrote down is still a
   purchase. Recording it teaches the memory with no list line to hang it on, and a purchase belongs
   to a line today (`ShoppingItemPrice.ShoppingListItemID` is required). Decide whether that changes
   or those lines are left out.

5. **Is the photo kept?** Recipe photos already live in the database (20 Aug), so there is a place
   for the bytes. A receipt also carries the last digits of a card and a loyalty number, so the
   likely answer is read it and throw it away, and the answer gets written down either way.

6. **When is it offered?** Just after Done is the obvious moment. A shop ended by two quiet hours,
   and a receipt found in a bag the next day, both need a way back to their shop.

### Deliberately not in scope

- **Comparing supermarkets.** One household's own receipts, not a price index.
- **Budgets, spending reports and splitting the bill.**
- **Online orders.** An order confirmation is a different input with a different trust conversation.

