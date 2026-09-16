# Decision log

*Why the code is the way it is. Newest first. Every entry: what was decided, why, and what it means
for anyone writing code later. When a decision is reversed, don't delete the entry. Add a new one
that supersedes it. See `VISION.md` for what the product is; see `docs/HANDOVER.md` for the
12 Aug 2026 point-in-time state.*
## 2026-09-16 · Shopping mode decision 4 of 4: in the aisle the list reads by aisle with its total in view

Mitch, 16 Sep 2026: while shopping mode is on, the list groups by aisle whatever it is set to at
home, the reorder arrows and the My order or By aisle switch go, and how many things are in the
trolley and what they come to stays pinned to the bottom of the screen. The screen is not kept
awake and the add box stays where it is.

**Chosen, not asked:** Untick all and Clear ticked are not offered in the aisle, where they could only
be mis-tapped, and Done takes their place in the pinned bar. The bar's total takes an unpriced line
at what it cost last time and says "About" when it does, as the whole-list total already did, and
the list's own progress bar moves into the bar's top edge rather than showing twice.

## 2026-09-16 · Shopping mode decision 3 of 4: in the aisle a tap ticks and a pencil edits

Mitch, 16 Sep 2026: in shopping mode the whole row ticks, and a pencil at the end of the row opens
the item. A tick does nothing else: no price box opens.

At home the row stays as the 31 Aug decision left it, the circle ticks and the words open the item,
because that decision is right for reading a list and wrong for pushing a trolley. Drag is off in
the aisle as well.

## 2026-09-16 · Shopping mode decision 2 of 4: only a shop remembers prices

Supersedes the twelve-hour rules in shopping decision 1 of 4 (11 Sep).

Mitch, 16 Sep 2026: a shop starts when someone switches shopping mode on for a list and ends at Done
or after two quiet hours. A tick outside a shop remembers no price. Nothing is guessed, and a
household that never switches the mode on remembers nothing, which is the trade taken.

- **A shop is a `ShoppingTrip` on the list**, holding when it started, when anything last happened
  during it, and when it was ended. Two quiet hours end it without anything being written.
- **A line remembers the trip it was ticked on** (`ShoppingListItem.ShoppingTripID`) for as long as it
  stays ticked, and a purchase records its trip (`ShoppingItemPrice.ShoppingTripID`). One line
  bought on one trip is one record, however often its price is corrected.
- **Unticking while the trip is still going takes that line's purchase back**; unticking after the
  trip is over leaves it standing. That closes both losses the 14 Sep dry run found: a mis-tap after
  Untick all cannot reach an earlier shop's price, and saving a ticked line's sheet days later
  corrects its record instead of adding a second.
- **Chosen, not asked:** a price typed on a line after its trip is over, while the line is still
  ticked, counts for that trip, so prices can be filled in at home from the receipt, and it is dated
  to the last thing that happened on the trip so last week's receipt does not rank ahead of this
  week's shop. Any change to a line during a trip, and anyone joining it, keeps it going, so the two
  quiet hours count from the last of those. Untick all and Clear ticked end the trip, because both
  mean the shop is over. Switching shopping mode on while a trip is going joins it rather than
  starting another, and two phones that start one at the same moment both keep whichever was saved
  first. Neither trip column is a foreign key: a list already cascades to both its lines and its
  trips and SQL Server refuses a second path, and what was paid has to outlive the trip anyway.

## 2026-09-16 · Shopping mode decision 1 of 4: the layout belongs to the phone and the shop to the list

Mitch, 16 Sep 2026: each phone turns the aisle layout on for itself, so the kitchen tablet stays as
it is, and the shop belongs to the list, so two people who split up at the door are one shop with
one set of prices.

The phone remembers the trip it joined, in the browser like the board's "Just me" switch, rather
than the list. A trip that ends anywhere, at Done on the other phone or after two quiet hours, takes
every phone out of the aisle layout, and a later trip on the same list never pulls a phone back in
until someone on it chooses to shop. A phone that is not in the layout still counts its ticks towards
a trip going on with the list, because the shop is the list's.

**Chosen, not asked:** "Start shopping" sits under the add box, and reads "Shopping in progress" with
"Join" when a trip is already going. The phone remembers one trip per list, so shopping with two lists
in the same supermarket keeps both in the aisle layout. Starting, joining and finishing a shop are
written to the history feed under Shopping.

## 2026-09-11 · Shopping decision 4 of 4: each list chooses between its own order and the aisles

Mitch, 11 Sep 2026: a toggle per list, "My order" or "By aisle", remembered. It is stored on the
list (`ShoppingList.GroupByAisle`) rather than on the device, because a list is shopped from more
than one phone and the grouping should read the same on all of them. The two lists this household
keeps as to-do lists simply stay on "My order".

In "By aisle" the groups follow the household's aisle order, the items inside a group keep the
list's own order, and anything not yet filed gathers at the end. Reordering is offered only in "My
order", because that is where the order is made; in "By aisle" the chevrons would move an item
somewhere that the grouping then puts it back from.

## 2026-09-11 · Shopping decision 3 of 4: ask once, then remember, and never guess

Mitch, 11 Sep 2026: no guessing from a word list. An item has no aisle until someone gives it one,
and after that the same item lands there every time.

"Every time" has to survive **Clear ticked**, which deletes the lines, so the memory cannot live on
the line. `ShoppingItemMemory` holds it, one row per household per item name, and the list reads an
item's aisle through it by name. The consequence is the wanted one: filing "Milk" under Dairy from
any list files it on every list, now and later, with no fan-out of writes. `NameKey` is the name
trimmed and lower-cased, so the match does not depend on the database's collation.

Measured when this was decided: 38 shopping lines in this household, 38 different names, none
bought twice. There was nothing to learn from, which is why this starts from the family's own
answers rather than a model of them.

## 2026-09-11 · Shopping decision 2 of 4: a starter set of aisles the household makes its own

Mitch, 11 Sep 2026: Home starts every household with Fruit and veg, Dairy, Meat, Bakery, Frozen,
Pantry, Drinks and Household, which the household can rename, add to, reorder and delete. The same
shape as meal slots and card sections: `ShoppingCategory`, seeded by `HouseholdSetupLogic` for a new
household and backfilled into existing ones by the migration that adds it, and the two lists of
names must stay in step.

Deleting an aisle takes nothing with it. The items filed under it go back to unsorted; the aisle is
a filing, not a container.

## 2026-09-11 · Shopping decision 1 of 4: prices compare per unit, worked out when read

Mitch, 11 Sep 2026: "$7 for 2 kg" and "$4 for 1 kg" are the same price per kilo and must not read
as a rise, so a comparison works out cost divided by amount whenever both purchases share a unit.
Nothing is stored per unit: `Cost` is still the line price, as the 17 Aug decision said, and this
reads it rather than reinterpreting it.

- **Units are put on one scale first.** Grams and kilograms compare as kilograms, millilitres and
  litres as litres. A count unit (tins, packets, each) compares only with itself. A line with no
  amount compares its line price with other lines that had none.
- **A purchase is remembered when the item goes into the trolley with a price on it**, as a
  `ShoppingItemPrice` under the item's memory. Ticking the same line again within twelve hours
  corrects that record rather than adding a second, and unticking a single line within twelve hours
  takes it back, because that was a mis-tap. "Untick all" at the start of a new week keeps the
  history, because that shop happened.
- **Chosen, not asked:** "usual" is the median of the last five comparable purchases, leaving out
  only the purchase the line's own tick recorded, and a line is flagged when it is more than 10%
  dearer than that. An item with no price on the list is estimated from the most recent purchase,
  scaled per unit when the two share a unit and taken as it was otherwise, so the list can say what
  the shop will roughly cost before anyone leaves.

Left alone on purpose: suggestions still read the last price off the list lines, not the memory, so
clearing a list still costs its suggestions their prices. Moving them onto the memory is the natural
next step and is its own change.

## 2026-09-08 · Memory decision 3 of 3: the planner remembers, off the meal plan and nowhere else

Mitch, 8 Sep 2026, took all four: "last had" on each recipe, a nudge when planning something had
in the last few days, a one-tap "leftovers" offer for the day after, and "haven't had in a while"
suggestions.

None of it is stored. `RecipeMealHistoryReader` folds the household's meal plan per recipe at read
time into `LastHadDate` (the latest planned day up to today), `NextPlannedDate` and `TimesHad`, and
the recipe list and recipe page carry those three fields. The thresholds and their words live in
one place on the web side (`MealMemory`): three days is "recent" and said in amber in the picker,
180 days is "due a turn", and the "Due a turn" strip only appears once the family has had
something, because a brand-new recipe book is all "never had" and that is not advice. Leftovers is
an occasion entry titled "Leftovers: X" for the next day in the same meal slot, offered once,
straight after a recipe is planned, and never for an occasion.

"Today" for these reads is the server's UTC date, which is the one place the calendar's
browser-zone rule was not followed: the recipe list has no viewer and being a few hours out on
"last had 3 days ago" costs nothing.

## 2026-09-08 · Memory decision 2 of 3: everything is recorded, and the feed's kinds are a per-device choice

Mitch, 8 Sep 2026, asked for "a default and it be configured". So the API records every kind of
change and does not filter; the History page shows the kinds this device has chosen, remembered
in the browser (`IDevicePreferences`, key `history-categories`), and the board's Recently tile
reads the same choice. The default is chores, meals, recipes, calendar and members, with shopping
off, because a shopping list is touched more often than anything else in the house and would drown
the rest. Individual item ticks are never recorded at all; adding an item, clearing the ticked ones
and unticking a list are.

Per-device rather than per-household for the same reason the board's "Just me" switch is: the
kitchen tablet and a phone want different feeds, and neither choice is the household's business.

## 2026-09-08 · Memory decision 1 of 3: the feed is a page and a tile, and history is never erased

Mitch, 8 Sep 2026: a `/history` page with the full feed and a short "Recently" tile on the board.
Chores and recipes also show their own history on their pages. All of it reads `home.Audit`, which
until today was written by fourteen interactors and read by nothing.

Three changes to the audit row made that possible:

- **It says whose it is.** `Audit.Household` is a cascading link set at write time from the
  session (or from the member, during first-run registration when nobody is signed in). The feed
  scopes by it rather than by walking to the member, so a removed member's doings stay in the
  family's history with their name still on the row. Existing rows were backfilled from their
  member in the migration; rows whose member was already gone stay unowned and unseen.
- **It says what happened in words.** `Audit.Summary` is plain English in the past tense with the
  doer left off ("ticked off 'Bins'", "planned Tacos for Wednesday dinner"); the feed puts the name
  in front. Each `AuditBase` subclass decides the words from what actually changed in the change
  tracker, so "ticked off" against "renamed" is not guessed, and an interactor that knows better
  passes its own ("imported the recipe 'X'", "skipped 'Swimming' on Thu 10 Sep"). Rows written
  before today have no summary and get a bland one from the technical record.
- **Deleting a thing writes a row instead of erasing its rows.** The old logics deleted an entity's
  audits when the entity went, which contradicted the June entry that says history outlives what
  it describes. `DeleteAudit` now writes "removed the chore 'Bins'" and keeps the rest.

Four kinds gained history that had none: meal plan entries, recipes, calendar events and calendar
subscriptions (add and remove only; a feed refresh is the house talking to itself). Test slices
that do not care get a no-op audit logic from `TestServiceFactory`; the ones that do register the
real logic, which is how the words themselves are pinned.

## 2026-09-08 · Identity decision 3 of 3: being yourself changes attribution and the lens, not what you may do

Mitch, 8 Sep 2026: identity is attribution and a personal view, nothing more. Roles and permissions
("a child cannot delete recipes") were offered and declined for now; everyone signed in can do
everything, as before.

What shipped on that basis:

- **Who ticked it off.** `Activity.CompletedByUser` is set from the session, never the request, by
  `SetActivityCompletion`, by a column move in `UpdateActivity`, and by `CreateActivity` when a card
  is born in a done column. Unticking clears it; a second tap on a done card keeps whoever did it
  first. The board card and the detail page say "Done by Ava", and the calendar's task items carry
  it as their subtitle once done. Like the assignee link it is `NoAction` in the database (two
  SetNull paths from User onto one table are refused), so `DeleteUser` unhooks both itself.
- **The lens.** The calendar read now carries `PersonUserIDs` on every item: who is on an event,
  who a chore is assigned to, nobody for a meal. An item that names nobody is the household's and
  always shows. The dashboard's Today tile gained an "Everyone / Just me" switch that is a
  **per-device** choice kept in the browser (`IDevicePreferences`, `preferences.js`): the kitchen
  tablet shows everyone, a phone shows its owner, and neither setting ever reaches the API. The
  calendar page gained a row of member chips that filter the same way.

Not built, on purpose: a "My day" page of its own (the switch on the board and the chips on the
calendar are that view), a greeting by first name (the cookie carries the username, and fetching
the member's name would put the dashboard back to seven calls), and any notion of what a member
may not do.

## 2026-09-08 · Identity decision 2 of 3: a member does not need a login

Mitch, 8 Sep 2026: "They can exist, but a login is not required. Actioning something requires a
valid login but assigning something to your 5 year old who you don't trust with a login is
perfectly valid."

So `User.Email` and `User.Password` are now optional and come as a pair: both or neither. A member
with neither is a name, someone to assign chores to and put on events, who can never sign in. The
first member of a household still needs both (`RegisterHousehold` is unchanged), because someone
has to be able to open the door. A login can be added later from the member's edit sheet, and taken
away by blanking the email, which also drops the password; a password without an email is refused
with a 422, because nothing can sign in without an address.

Two things fell out of touching the slices:

- `UpdateUser`'s conflict check compared the new email against every member *including the one
  being edited*, so saving a member with their own address counted as a clash. It now excludes the
  member and ignores blank emails.
- The password grant already refused a null password (`PasswordService.VerifyPasswordAsync`), so a
  member without a login was never able to sign in by accident; the interactor needed no change.

## 2026-09-08 · Identity decision 1 of 3: no switching on shared devices

Mitch, 8 Sep 2026, choosing between tap-and-PIN, a "family tablet" mode, and no switching: **no
switching on shared devices.** A device is signed in as one member with their email and password
and stays that member. The kitchen tablet is whoever set it up; a phone is its owner. Nothing
passwordless was added, which is the same line the 13 Aug registration entry and the 14 Aug
avatar-switching deferral drew, now drawn for good.

What this means for anyone building later: there is no "acting as" state, no PIN table and no
device trust flag, and none should be added without reopening this entry. "Who did this" is
always the session's member. The payoff phase 7 promised, a personal view and meaningful
attribution, is delivered through the two entries above rather than through switching.

## 2026-09-08 · Calendar decision 6 of 6: the dashboard shows today in time order, then the next few days

Mitch, 8 Sep 2026: one "Today" column listing timed events, meals and due tasks in time order,
followed by the next two or three days compressed. The month and week views live on `/calendar`
and are the screens that get edited; the board is the one that gets read.

This replaces the "This week" tile, which was four undated-or-dated chores sorted by hand, and it
feeds the "Today's meals" tile from the same read, so the dashboard's seven hand-assembled calls
drop to six (weather, lights, recipes, shopping, notes, calendar). Ticking a chore off stays on the
board: the agenda row carries the same complete button the old tile had, because taking an action
away from the most-glanced screen in the house is a regression however tidy the tile gets.

## 2026-09-08 · Calendar decision 5 of 6: read-only iCalendar subscriptions, now

Mitch, 8 Sep 2026: Home reads the calendar the family already keeps. A household pastes the
"secret address" (`.ics`) link Google, Apple or Outlook publish, Home fetches it on a timer
(`CalendarSubscriptionRunner`, every 30 minutes) and shows those events beside its own. They
cannot be edited or deleted from Home; the sheet says where to change them instead. Two-way
CalDAV was declined as a separate project.

How it is stored is the part worth recording. The feed is the source of truth, so Home does not
try to translate the feed's recurrence rules into its own smaller model. Each refresh expands the
feed's occurrences inside a rolling window (31 days back, 366 forward), **deletes the
subscription's rows and writes the expanded occurrences back** as plain one-off `CalendarEvent`
rows carrying `SubscriptionID` and `ExternalUID`. Nothing about a subscribed row can drift from
the feed for longer than one refresh, and the read model needs no special case: a subscribed
event is an event whose `Subscription` is not null. Timed occurrences from a feed are stored with
`TimeZoneID = "UTC"`, which the decision-4 model handles like any other zone.

`Ical.Net` 4.3.1 does the parsing and expansion inside `IcsCalendarFeedService` in the API. Its
types never leave that adapter: the boundary is `ICalendarFeedService` in
`Home.Application/Services/Calendar/`, which returns null when the feed cannot be reached, the
same shape as lights and weather. A subscription that cannot be reached keeps its last good rows
and records `LastError` for the Settings card, because a stale calendar beats a blank one.

## 2026-09-08 · Calendar decision 4 of 6: wall-clock plus zone in the table, UTC on the wire, the browser's zone on screen

Mitch, 8 Sep 2026: he does not mind whether the table holds UTC or an offset, but **every date
and time is displayed in the viewer's browser time zone**. Follow-up, same day: an all-day event
is a date, not a moment, and a repeating event keeps its wall-clock time and the zone it was
created in, so "swimming 4pm every Tuesday" is still 4pm after daylight saving changes.

The model that satisfies all three at once:

- **Every event stores `StartDate`/`EndDate` as `DateOnly`** (inclusive), and a timed event adds
  `StartTime`/`EndTime` as `TimeOnly` plus an IANA `TimeZoneID` taken from the browser that
  created it. A one-off timed event and a repeating one are stored identically; there is one
  representation, not one for each.
- **The API converts to UTC instants when it reads.** `GetCalendar` takes the viewer's zone,
  expands each occurrence into `StartUTC`/`EndUTC` through the event's own zone, then buckets
  it into the viewer's local day. A 9pm Brisbane event viewed from Perth lands on the right day
  at 7pm.
- **The web app never calls `.ToLocalTime()` for calendar data.** Blazor Server runs on the
  server, so `DateTime.ToLocalTime()` and `TimeProvider.GetLocalNow()` give the *server's* zone,
  which happens to match on a home server and is wrong the day this is cloud hosted. A new
  per-circuit `IViewerClock` reads the browser's zone once through JS interop
  (`Intl.DateTimeFormat().resolvedOptions().timeZone`) and is the only place calendar dates are
  converted. The rest of the app still uses server-local time; converting it is a separate job.

The two existing sources fold in as they are: a `MealPlanEntry.Date` is already a local calendar
day and becomes an all-day item, and an `Activity` contributes the date part of `DueDateUTC` with
`DueTime` as its local time. `DueDateUTC` is stored as the date the user picked at midnight,
mislabelled, so treating it as a calendar day matches what every screen already does with it.
Nothing about `Activity` is changed in this phase.

## 2026-09-08 · Calendar decision 3 of 6: simple repeats, with exceptions, and a list of what it will not do

Mitch, 8 Sep 2026: daily, weekly on chosen days, monthly (same date, or the same weekday such
as "first Monday"), yearly, each with an optional repeat interval and an optional end date, and
the ability to skip or move one occurrence. Full RFC 5545 rules were declined as more than a
household needs and more than anyone would fill in on a kitchen tablet.

The model is five columns on `CalendarEvent`: `Frequency`, `Interval`, `DaysOfWeek` (a bitmask,
bit 0 Sunday, the same shape `LightSchedule` uses), `RepeatsOnWeekdayOfMonth` and
`RepeatUntil`. Expansion is a pure static `RecurrenceExpander` in the application layer with its
own tests, because recurrence is exactly the kind of logic that looks right and is wrong on the
fifth Monday of a month.

**Exceptions are the hard part and they are kept deliberately dumb.** A skipped occurrence is a
row in `CalendarEventException` (series, date). A *moved* occurrence is that same skip plus an
ordinary standalone event; there is no link between the two. The alternative, a self-referencing
foreign key from the moved event back to its series, was rejected because SQL Server refuses any
cascade on a self-reference, and a link that cannot cascade is a link that can dangle. The cost
is that deleting a whole series leaves its moved occurrences standing as their own events, which
is defensible: someone moved it, it is theirs now.

Written down as not supported, so nobody looks for it: "every Tuesday in term time", "the last
Friday of the month", "every weekday", counts ("ten times"), and editing "this and following".
Each is a decision of its own if it is ever wanted.

## 2026-09-08 · Calendar decision 2 of 6: events belong to the household, with people optionally on them

Mitch, 8 Sep 2026: an event belongs to the household. Zero or more existing members can be put
on it (`CalendarEventMember`, a plain join), which is display and, later, filtering, not
ownership. Per-person events were declined, so **phase 7 (per-user identity) stays where it is**
and the calendar builds now; that was the whole reason this question was asked first.

The join cascades from the event and is `NoAction` from the user, because the household already
cascades to both and a second cascade path onto the join is rejected by SQL Server, the same trade
`ActivityTag` made. `DeleteUser` therefore clears a member's calendar rows itself before removing
the member.

## 2026-09-08 · Calendar decision 1 of 6: the calendar stores its own events and reads everything else

Mitch, 8 Sep 2026: both. A new `CalendarEvent` table holds appointments that belong to nothing
else ("swimming, Tuesday 4pm"), and one read, `GetCalendar(from, to, zone)`, answers "what is on
between these days" by folding in the planned meals and the tasks with a due date alongside it.
Nothing is stored twice: a meal stays a `MealPlanEntry`, a chore stays an `Activity`, and the
calendar shows them with a kind and a link back. Light schedules and family notes are deliberately
left out of the read: a schedule is automation the house does to itself, not something the family
attends, and a note is not on a day.

This settles the phase-6 planning question that shaped the other five, and the schema sketch the
roadmap asked for is the four tables below. Every row reaches the household directly, the same
ownership path everything else uses since 14 Aug.

| Table | Columns | Notes |
|---|---|---|
| `CalendarEvent` | `Title`, `Location?`, `Notes?`, `IsAllDay`, `StartDate`, `EndDate`, `StartTime?`, `EndTime?`, `TimeZoneID?`, `Frequency`, `Interval`, `DaysOfWeek`, `RepeatsOnWeekdayOfMonth`, `RepeatUntil?`, `ExternalUID?`, FK `Household` (cascade), FK `Subscription?` (no action) | The subscription FK cannot cascade: household already cascades here, and `DeleteCalendarSubscription` removes its rows itself |
| `CalendarEventMember` | `CalendarEventID`, `UserID` | Composite key. Event cascades, user does not (see decision 2) |
| `CalendarEventException` | `CalendarEventID`, `OccurrenceDate` | A skipped occurrence of a series. Unique per series and date |
| `CalendarSubscription` | `Name`, `Url`, `LastFetchedUTC?`, `LastError?`, FK `Household` (cascade) | A read-only feed (see decision 5) |

The read model is a projection across three sources and is the thing tested first, against a real
database through its presenter, because a projection that forgets a navigation is the bug class
this repo keeps meeting.

## 2026-09-07 · One file per HTML tag, and HomeButton is the only button

Every tag the app draws more than once now has a component, and that component is the only place
the tag is written. `Home.WebUI` holds exactly **two** `<button>` elements: the one inside
`HomeButton`, and the reconnect overlay in `App.razor`, which Blazor shows when the circuit is gone
and there is nothing left to render a component with. 72 raw buttons, 118 icon spans, 18 selects,
13 inputs and 5 textareas went to 2, 0, 0, 0 and 0.

The reason is consistency, not tidiness. A change to how a button presses, focuses or disables now
lands everywhere at once, and no screen can invent a fourth way to draw the same control. The sweep
found two live defects that had survived precisely because each control was written by hand: a
delete button meant to float over a recipe photo was sitting in the flow, and four ARIA state
attributes were bound straight to a bool, which Blazor renders by dropping the attribute when
false, so a switch read to a screen reader as an ordinary button.

**Anything shaped differently from a button composes `HomeButton` with `Variant="bare"` rather than
writing a second `<button>`.** `HomeListRow`, `HomeChip`, `HomeCardAction` and `HomeSwatch` all do
this; bare gives them the focus ring and the disabled handling and nothing else.

Three things that shape how this has to be written, all learned the hard way:

- **A Blazor directive cannot ride through `@attributes`.** Plain attributes forward to a component
  fine. `@onclick:stopPropagation`, `@onclick:preventDefault`, `@onmousedown:preventDefault` and
  `@ondragstart` do not, so `HomeButton` carries each as a parameter. That is the ceiling on how far
  this can go: a control needing a directive nobody anticipated needs a new parameter, not a new
  `<button>`.
- **Two utilities for the same CSS property collide on stylesheet order, not attribute order.**
  `HomeButton` carried `relative` to anchor its `DisabledReason` bubble, and Tailwind emits
  `relative` after `absolute`, so a caller asking to float was silently overruled. It now drops its
  own `relative` when the caller's `Class` names a position.
- **A class assembled at render time is invisible to the Tailwind scanner.** `HomeIcon` builds
  `home-icon-{Name}`, so `home-icon-` is safelisted in `tailwind.config.js`. Without it every icon
  renders as a bare grey square, which is the same trap the nav rail hit in August.

Left alone: `<span>` at 116 and `<p>` at 166. Those are text and layout rather than controls, and a
`HomeText` wrapping every paragraph would cost more than it pays. The better next target is a
`@foreach` body that is a whole card.

## 2026-09-06 · A picker takes an options list, not option markup

`HomeSelect<TValue>` could have taken `<option>` tags as child content, which would have been a
smaller diff at every call site. It takes a `List<HomeSelect<TValue>.SelectOption>` instead, the
same shape `HomeSegmentedControl` already used, for a reason that only showed up once the call
sites were converted.

A raw `<select>` speaks strings, so a page holding an ID in one had to hold it as a string and
parse it back. Four fields were like that (`m_EditComplexity`, `m_IngUnit`, `m_EditUnit`,
`m_ListMealSlotFilter`), and each one carried a `ParseLong` on the way out and a `.ToString()` on
the way in. A typed `Options` list makes the page hold `long?` and the conversion happen once,
inside the component, through `BindConverter`. Eight `.ToString()` calls and two helpers went.

The cost is that a caller cannot write conditional or grouped option markup. Nothing in the app
wanted to: all 18 call sites were a flat list, most of them a `@foreach` over a collection with an
optional "nothing chosen" entry in front.

An option with an empty value is the one that means nothing is chosen. The conversion fails for it
on a type that cannot hold nothing, which is why a failed conversion lands on `default` rather than
being treated as an error.

## 2026-09-06 · IconOnly on HomeButton, not a HomeIconButton

Fifteen square icon buttons across six files were written by hand in five treatments. The obvious
fix was a new `HomeIconButton`; what shipped is a boolean on `HomeButton` that swaps its horizontal
padding for a square of the same height.

A separate component would have had to re-implement the focus ring, the loading spinner and the
`DisabledReason` behaviour, or go without them, and going without them is how those fifteen buttons
came to have no focus ring in the first place. Shape is not a different control.

The same pass gated `HomeButton`'s hover and press states on `enabled:`. A hard-disabled button
used to still light up under a cursor. One held open by a `DisabledReason` is deliberately not
`disabled` in the DOM, so it keeps reacting, which is the point of it.

## 2026-09-06 · A planned meal owns its household, and does not have to be a recipe

A meal can be an occasion rather than something cooked. "Father's Day", "leftovers", "out for
dinner" all belong on the week, and making a recipe out of one to get it there is silly. A
`MealPlanEntry` now carries an optional `Recipe` and an optional `Title`, and exactly one of them
is set.

That forced an ownership change, which is the part worth recording. The entry used to reach its
household *through the recipe*, and the configuration said so: "The recipe is the only owner.
Household is reached through it, which keeps SQL Server to one cascade path." An entry that is only
a title has no recipe to be reached through, so every query scoping the planner to one household
would have had nothing to scope by. `MealPlanEntry` now has its own `HouseholdID`.

Two cascade paths to one table is not allowed, so the household relationship is **Restrict** and
the recipe stays **Cascade**:

- Deleting a recipe still takes its planned meals with it, which is right. An entry pointing at a
  deleted recipe is an answer to "what's for dinner" that nobody can cook.
- Deleting a household is refused while it has a plan. Nothing in the application deletes a
  household, so this costs nothing today.

The migration backfills `HouseholdID` from the recipe before adding the constraint. Without it,
every existing row keeps EF's `defaultValue: 0`, points at a household that does not exist, and the
foreign key is rejected. Five rows on this database, all backfilled and verified at zero remaining.

The read model exposes `Name` and a nullable `RecipeID` rather than `RecipeID` and `RecipeName`.
`Name` is whichever of the two the entry actually has, so nothing rendering a week needs to know
which kind it is looking at, and the nullable ID is what says whether the name is something that
can be opened.


## 2026-09-06 · The calendar is a phase of its own, and its planning is front-loaded

`VISION.md` has four pillars and a shared calendar is not one of them. It becomes one now, as
phase 6, ahead of per-user identity.

Two reasons it earns a phase rather than a backlog line:

- **The category this product sits in is led by the calendar.** Every always-on family screen a
  household could buy instead of this leads with it, because a shared calendar is what a family
  goes looking for. Without one, this is the second thing they check rather than the one
  application they rely on, which is the opening sentence of `VISION.md`.
- **A lot of what comes after it is time-shaped**, so the model underneath it gets used by more
  than the calendar screen.

The planning is deliberately the bulk of the phase, and nothing gets built before six questions
have their own dated entries here. That is not process for its own sake: **the app already holds
time four ways and they disagree.** `MealPlanEntry.Date` is a local calendar day, `Activity`'s
dates are UTC instants, `LightSchedule` is a `DaysOfWeek` bitmask with a wall-clock `TimeOfDay`,
and `Announcement` is an instant. A calendar added without reconciling those becomes a fourth
model, and every later feature asking "what is on this day" then has to know which answer to
trust. The six questions are listed in `ROADMAP.md`; the two that reach furthest are whether the
calendar stores events or aggregates what already exists, and whether an event belongs to the
household or to a person. **If events turn out to be per-person, the identity phase moves in front
of the build**, because retrofitting an owner onto a populated table is the expensive version.

Nothing is committed about recurrence or external sync yet, on purpose. Both are named in the
roadmap as decisions the planning stage owes an answer to, including the option of answering
"not this".


## 2026-09-05 · A session that has ended navigates to login, it does not report itself

Refines the 19 Aug session entry, which said "a refused refresh surfaces as an error". That is
still right about what this app may not do and wrong about what it should do instead.

The failure it produced: the sign-in cookie can be perfectly valid while the session behind it is
gone, because the two are separate things. The cookie carries the refresh token; the session is a
row in `home.UserAuthentication`. Sign that row out from another device and the cookie is untouched,
so `<NotAuthorized>` never fires, the route authorises, the page renders, the navigation works, and
every request for data fails. Three stacked "Not signed in. Please sign in to access this resource."
messages on a screen with nowhere to sign in, one per call the page made.

`ISessionEndedNavigator` replaces those messages with a `forceLoad` to
`/login?returnUrl=<where they were>`. Three things make that correct rather than a workaround:

- **It is not signing out.** Nothing client-side clears the cookie, which is the rule the 19 Aug
  entry set and it stands. This navigates. The login page is the one that renders statically, so
  going there is precisely how the cookie reaches a response that can deal with it.
- **It latches, scoped to the circuit.** A page asks the API for six things at once and all six
  fail identically. The first navigates and the rest return quietly, which is what stops the stack
  of identical messages as much as the navigation does.
- **Only three branches use it**: no access token at all, a refresh the token endpoint refused, and
  a 401 against a token minted moments earlier. An unreachable API keeps its own message and does
  **not** navigate, because "the API is not running" and "you are signed out" are different
  problems and only one of them is fixed by signing in again.

Found by signing 23 stale sessions out of this household while testing the devices card, which is
also the honest reason it had never been seen: the state needs someone else to end your session
while you are sitting in the app.


## 2026-09-05 · Two breakpoints, asking two different questions

The app now uses `rail:` and `sm:`/`lg:` for different things, and mixing them up will produce
layouts that are wrong in one hand position or another.

`rail: (min-width: 768px) and (orientation: landscape)` decides **where navigation lives**. That is
a question about reach: a tall device is held in two hands and the top-left corner is out of thumb
range, so the nav becomes a bottom bar regardless of how wide the screen is.

Plain width breakpoints decide **whether content fits**. The top bar puts its actions on their own
line below `sm:` because three buttons and a display-face title cannot share 375px, and the shopping
list shows one pane below `lg:` because two will not fit. Neither has anything to do with how the
device is held.

A tablet held upright is the case that separates them: it wants the bottom bar (`rail:` is false)
and it wants the wide content layout (768px is plenty). Using `rail:` for a fitting question would
give it a phone layout on a screen with room to spare.

## 2026-09-05 · The app shell is sized in dvh, not vh

`MainLayout` uses `.app-viewport` (`height: 100dvh` with a `100vh` fallback) rather than Tailwind's
`h-screen`. On iOS Safari `100vh` means the viewport with the browser's toolbars hidden, which is
taller than what is actually visible while they show, so a shell sized to it puts the fixed bottom
bar below the fold and the whole page reads as slightly too large for the screen. Both of those had
been reported from a phone and neither reproduces in a desktop emulator.

It is a rule in `input.css` rather than two Tailwind classes because the fallback only works if
`100vh` is declared before `100dvh`, and the order Tailwind emits utilities in is not ours to set.

Left alone deliberately: the three auth pages use `min-h-screen`, which is a floor rather than a
fixed height, so nothing is pinned to a bottom edge that moves.


## 2026-09-05 · An ingredient is not shared, so an ingredient note reaches one recipe

The schema models `Recipe` to `Ingredient` as many-to-many through `RecipeIngredient`, which reads
as though "olive oil" is one thing the whole household knows about. It is not. `AddRecipeIngredient`
and `ImportRecipe` both create a fresh `Ingredient` row every time, and nothing anywhere reuses one,
so every recipe has its own private copy of every ingredient it uses.

That was discovered while making ingredient notes reachable, and it decided the copy: the note field
says nothing about where the note reaches, because the obvious sentence ("shows in every recipe that
uses it") would have been a promise the application does not keep.

Deduplicating is not a small fix and is deliberately not being done now. `Amount` and `Unit` live on
the `Ingredient` row rather than on the join, so sharing a row would mean two recipes sharing one
amount. `Amount` and `Unit` move to the join first, then deduplication, then a shopping list item
can point at an ingredient and carry its note to the shop. All three steps are in `BACKLOG.md`.

## 2026-09-05 · The devices card shows five, and offers one control for the rest

This household had 24 signed-in sessions when the screen was first built, every one unlabelled.
Nothing prunes a session and they last 90 days, so that number only grows.

Twenty-four rows each with its own sign-out button is a wall, not a screen, and clearing it would
have been 23 taps. The card shows five, asks before rendering the rest, and offers a single
"Sign out N others" that ends everything except the device reading it. The per-device button stays,
because signing out one specific device is a different intent from clearing the pile.

The current session is identified by an `AuthenticationMetadataID` claim added at authentication.
Without it the bulk action cannot tell which row to keep, so it refuses rather than guessing and
signing the caller out along with everything else.


## 2026-09-04 · A slice that can clear a navigation has to project it

The write-side twin of the missing-projection trap, and quieter. Setting a reference navigation to
null on an entity whose navigation was never loaded is not a change EF can see: the tracker compares
null against null, finds nothing, and leaves the foreign key where it was. The save succeeds, the
endpoint answers 204, and nothing happened.

`UpdateActivity` could not unassign a member from a card. `UpdateMealPlanEntry` could not take a
meal out of its slot, and its own comment claimed the behaviour it did not have. Both are fixed by
naming the navigation in the query.

Scalars are unaffected, which is what makes it hard to spot: on the same card, clearing the
completion date worked while clearing the column did not. Both were found by writing the tests, and
neither was reachable by reading the code carefully.

## 2026-09-04 · The password grant compares the client secret

`CreatePasswordGrantInteractor` looked its `ClientApplication` up by ID alone. The secret was
required to be present and then never compared, so anyone who could reach the token endpoint could
mint a token with `client_id=1` and any secret at all, given a real username and password. The
endpoint is anonymous, so the `Authorization: Basic` header the web app also sends it is never read;
`BasicAuthenticationHandler` guards the rest of the API, not this.

`CreateRefreshGrantInteractor` has always compared the secret, which is what says this was an
oversight rather than a decision, and is also what made the fix safe to make without being able to
test a real sign-in: refresh working in daily use is proof the configured secret matches the row.

The setup ceremony this makes real is still worth removing, for the reasons in `BACKLOG.md`. It
should be removed deliberately, not by leaving it unenforced.

## 2026-09-04 · Configuration is checked at startup, and the defaults live in appsettings

Setting the app up took six user secrets, five of them undocumented, and every way of getting one
wrong produced a 401 that the sign-in page reported as a wrong password. That cost an evening on a
second machine on 3 Sep.

Three changes. `RequiredConfiguration` in each project lists everything wrong at once with the
command that fixes each, rather than failing at the point of use. The values that are identical on
every installation moved into `appsettings.json`, leaving two genuine secrets. And the sign-in page
now distinguishes an unreachable API, an installation whose own credentials were refused, and a
wrong password, because telling somebody to retype a password that was never the problem is the
worst of the three answers.

`apiBaseUrl` deliberately stayed out of `appsettings.json` and sits in the Development file only, so
a deployment has to say where its API is instead of silently pointing at localhost.

The token endpoint now answers with the RFC 6749 error code in the body rather than an empty 401.
That is what lets the client tell `invalid_client` from `invalid_grant`. Neither code reveals
whether a username exists.

## 2026-09-04 · Focus moves to the heading on navigation, without dragging the page with it

Blazor's `FocusOnNavigate` parks focus on the new page's `h1` so a screen reader announces where you
have landed. It does that with a plain `.focus()`, and focusing an element also scrolls it into
view, so a moment after the dashboard settled the browser pulled the heading up under the top edge
and the whole page appeared to lurch. Mitch reported it as the heading looking highlighted and the
screen auto-scrolling. Both were the same call.

`HomeFocusOnNavigate` replaces it: same contract, but it takes the scroll container to the top
first and then focuses with `preventScroll`. The announcement is kept, the lurch is gone, and
navigating to a page still starts you at the top of it rather than wherever the last page was left.
A companion rule in `input.css` drops the focus ring on any `[tabindex="-1"]`, since nothing with
that attribute is reachable by tab and a ring around something nobody clicked reads as the app
highlighting itself.

## 2026-09-04 · Tailwind scans the code-behind as well as the markup

The `content` globs covered `.razor`, `.html` and `.cshtml`, so any class named only in a
`.razor.cs` was invisible to the scanner and its rule was purged. `HomeNavRail` builds its whole
icon list in code-behind, which is why the Home icon rendered as a bare grey square while every
other icon in the rail was fine. Adding `./**/*.cs` costs about 5% on the built stylesheet and
removes a failure mode that gives no error anywhere.

## 2026-09-03 · Read slices are tested against a real database and their real presenter

Three screens have now shipped broken the same way: a presenter reads `x.Y.Z` and the interactor's
projection never named `x.Y`, so EF hands back a null and the screen either throws or quietly
answers zero. `GetShoppingList` (17 Aug, caught by the AutoMapper guard), the `CardCount` delete
guard (1 Sep, caught by hand) and every activity card (1 Sep, caught by Mitch).

**A mocked `IPersistenceContext` cannot catch this**, which is why the existing tests never did.
`stored.AsQueryable()` hands the interactor an object graph that is already fully connected, so a
projection that forgets a navigation still passes: nothing was ever unloaded. Tests over that mock
prove the `Where` clause and nothing about what gets loaded.

So `TestDatabase` builds a real `PersistenceContext` over the EF in-memory provider, seeding through
one context and reading through another. The projection then decides what is loaded, exactly as in
production. `InteractorTest` drives the **real presenter** rather than a mocked output port, because
the fault only exists where the two meet: a mocked port reads nothing, so it can never dereference
a null. Reverting any of the three fixes now fails the suite with the same `NullReferenceException`
the family saw.

Every one of the 24 read slices is covered, each with a neighbouring household seeded alongside so
the isolation invariant (14 Aug) is pinned rather than assumed. The in-memory provider was chosen
over SQLite because it ignores DDL entirely. The 12 Aug decision deleted the SQLite path precisely
because the migrations are SQL Server-shaped, and that reasoning still holds; this is not a route
back to it.

Writes remain untested. That is the next tranche, not a decision against it.

## 2026-09-03 · Adding a card section refuses cleanly instead of failing

`ActivityLogic.AddRegion` returns null for a section belonging to another household (the guard
that stops one family writing under another family's heading), and `CreateActivityRegionInteractor`
added that null to the card's collection and then read an ID off it. The guard was a 500. It never
fired in normal use, because the UI only ever offers the household's own sections, which is why it
survived a manual test two days earlier; it was the solution's only production compiler warning
(`CS8604`) all along. The port gained `PresentCardSectionNotFoundAsync` so it answers 404 like every
other slice.

Writing the test for it is also what established that the harness must resolve the signed-in
household through the read context: `AddRegion` reads `Activity.Household` without projecting it and
depends on the authorisation call having already tracked it. See `known-gaps.md`.

## 2026-09-03 · A member lookup answers with a response model, not the entity

`GetUserPresenter` passed the `User` entity to `OkAsync` whole, putting the stored password on the
wire for any member who asked, and the interactor handed a null straight to the same method, so a
member of another household got 200 with an empty body while `PresentUserNotFoundAsync` sat
unreachable. Both were found by writing the tests for the slice, and both were fixed rather than
pinned, because a test asserting the current behaviour would have cemented it. The endpoint is
reachable only from the API; nothing in the WebUI calls it.

## 2026-09-01 · Card sections belong to the household, and the second board axis is gone

Two decisions from the same complaint: the family board still spoke like a software ticket.

`ActivityStatus`, the global unscoped Todo/In Progress/Done lookup seeded at API startup, was
**deleted outright** rather than reconciled with the household-scoped `ActivityState` that replaced
it on 15 Aug. It was safe because it was never used: 0 of 3 activities carried a StatusID, and no
UI control ever set or showed one. A card's column already answers "what state is this in", and two
answers to one question is one too many. Nothing is seeded globally any more.

`RegionSE` (Description / **AcceptanceCriteria** / Notes, fixed in code) became `CardSection`, a
household-owned, named, ordered entity alongside board columns and meal slots. Mitch chose this over
renaming the fixed trio, because the 15 Aug reasoning ("columns belong to the household") applies
identically here. Existing households were seeded Details / Steps / Notes and their one existing
region remapped onto the matching row through the card's household, so nothing written was lost.
`ActivityRegion → CardSection` is **NoAction**, not Cascade: the household already reaches sections
through the activity, SQL Server rejects the second cascade path, and refusing to delete a section
still holding writing is what a family wants. The UI refuses it too, and the count that decides
that has to be **projected**: an unloaded `Regions` collection counts zero and would have offered
to delete a section with a card's writing under it.

## 2026-09-01 · Archiving is not deleting, and a copy is one call

A shopping list can now be renamed, copied and archived. Archiving keeps everything (the items,
the prices, what was ticked) and only takes the list out of the picker, so last Christmas's shop
can still be copied a year later; deleting stays the destructive one and asks twice. The list being
looked at stays visible even once archived, because it would otherwise vanish from under the reader
at the moment they archived it.

Copying is a server-side slice, not the screen looping over the item endpoints, for the same reason
as clearing a list on 17 Aug. Nothing arrives ticked and nothing arrives priced: "this week's like
last week's" means the same things to buy, not last week's trolley or last week's receipt.

## 2026-09-01 · One move control, and the sequence swap is the pattern

`HomeReorder` is the single up/down affordance, now used by recipe ingredients, recipe steps,
shopping items, light scenes, card sections and cards within a board column. Chevrons rather than
drag, everywhere: a finger can hit them, they need no pointer to discover, and they behave the same
on the tablet as on a desktop. Drag stays as a desktop extra where it already existed, and the
meal planner gained it by reusing the board's delegated `[data-drop-target]` listener rather than
writing a second one, and the attribute lost its `board-` prefix when the second screen wanted it.

Every reorder is the same **two-call sequence swap** the board already used for its columns, and
every one sends *only* the sequence: a reorder must not overwrite a name or a price someone is
editing on another device. Ends are disabled rather than hidden so rows keep their width and do not
shuffle sideways as things move.

Two new `Sequence` columns fell out of this, `RecipeIngredient.Sequence` (31 Aug) and
`Activity.Sequence`, both backfilled so existing rows keep the order they were already shown in
rather than all landing on zero, which is the arbitrary ordering the column exists to end.

## 2026-08-31 · A modal is a `<dialog>`, so the browser owns its keyboard

`HomeModal` was a `<div>` with `fixed inset-0`, which gets none of the behaviour a modal is
expected to have: Escape did nothing on any of the twenty-one modals, Tab walked straight out of
the panel onto the page behind it, and the background stayed live to pointers and screen readers.
The fix was not to write key handling. It was to stop hand-rolling the element. `showModal()` on
a real `<dialog>` gives Escape, the focus trap, the inert background and the top layer for free,
which is the same reasoning that made the body a `<form>` for its Enter key on 20 Aug.

Two details worth keeping. The `cancel` event is **preventDefault-ed** and routed back into Blazor
through a `[JSInvokable]`, because letting Escape close the element natively would leave the DOM
shut while `Visible` still said open. `Visible` stays the single source of truth. And the dialog
is stretched over the viewport with its user-agent frame stripped (`m-0`, `max-w-none`,
`max-h-none`, `p-0`, `border-0`), because `dialog:modal` ships a `calc(100% - 38px)` max-width and
an `auto` margin that would otherwise inset the whole overlay.

## 2026-08-31 · Tapping a row opens the thing; only its own control acts on it

Supersedes the tap-to-tick half of the 17 Aug shopping list entry. The whole row being the tick
target meant reading a line to check the amount crossed it off, and in a supermarket that is a
mis-tap you then have to find and undo. The circle now ticks and carries a 56px hit area of its
own so the target did not shrink to the ring you can see; the words open the item.

The same rule settles the recipe page, where the pencil and bin were `opacity-0
group-hover:opacity-100`, invisible on a tablet, which is the only device this runs on. That was
the identical defect already fixed for the delete-list button on 19 Aug, and a sweep found seven
of them across Recipes. Ingredient rows became tap-to-open with removal moved inside the sheet;
steps, notes and recipe cards simply stopped hiding their controls. **Rule: never gate a control
on hover. A finger has no pointer, and `group-hover` is a desktop-only affordance.**

## 2026-08-31 · An ingredient's position belongs to the recipe, not to the ingredient

Ingredients rendered in whatever order the database returned. They are ordered now, and the order
is *order of use in the method*: what a cookbook does, and what you read down while cooking.
Grouping by type (herbs, spices, meats) was considered and rejected for the recipe page: that is
what a shopping list wants, and it is already the aisle-grouping item on the roadmap.

`Sequence` went on `RecipeIngredient`, the join, rather than on `Ingredient`. Today an `Ingredient`
row is created fresh for every line and never reused, so putting it on the ingredient would have
worked, and would have quietly blocked the ingredient-catalogue rework, where one shared onion
cannot hold one recipe's position. Moving is the board's two-call sequence swap, not a new verb.

## 2026-08-31 · A unit knows its own singular, because English does not derive one

"1 packets" and "1 tins" were being rendered because a unit carried exactly one abbreviation.
Each now carries both forms explicitly rather than being pluralised by rule: leaf becomes leaves,
dash becomes dashes, pinch becomes pinches, and no suffix rule gets all three right. The API picks
the form from the amount beside it, so every screen agrees without each one deciding.

There are **three** lists of measurements that cannot see each other (the domain enumeration, the
web app's mirror of it, and the synonyms the shopping list parser accepts) and until now only a
comment held them together. `MeasurementUnitTests` pins all three to the enumeration, so a unit
added to one and forgotten in the others fails the build rather than showing an empty dropdown
entry or silently dropping a typed unit.

## 2026-08-20 · Recipe photos live in the database and stream through the web app

The "no image bytes, that forces a hosting decision" stance on `Recipe.ImageUrl` is superseded:
Mitch asked for his own photos, and for a household app the database *is* the hosting decision.
`RecipeImage` holds the bytes (5 MB cap) one-to-one with the recipe, deliberately with **no
navigation on Recipe**: a stray Include would drag blobs through every book query. What the book
needs instead is `Recipe.ImageUpdatedOnUTC`: a denormalised stamp meaning "has a photo", written
only by the two image use cases, whose ticks double as the cache-buster in the image URL. The
format comes from sniffing the bytes' magic numbers, never from the declared content type. A
renamed file lies, the bytes don't. The browser fetches `/recipe-images/{id}` on the *web app*,
which turns the sign-in cookie into a bearer token and proxies the API: an img tag can only send
a cookie, and the API's household scoping stays in charge. An uploaded photo beats the ImageUrl
link everywhere.

## 2026-08-20 · Applying a light scene first saves how the room looked

"Previous look" is one reserved scene per household (`LightScene.IsPreviousLook`), rewritten by
every scene apply with a snapshot of the *whole* household's lights taken before the apply mutates
anything. That makes it an undo, and tapping it twice toggles, because applying it re-captures
what it replaced. It is pinned first in the scene list, cannot be scheduled (it changes on every
apply, so a schedule would set lights nobody chose), and is never created until the first apply.

## 2026-08-20 · Planning a meal can create the recipe on the spot

The meal-plan picker gained search, orders recipes tagged with the meal being planned first, and
offers "Add *X* as a new dinner recipe" when the search matches nothing exactly: created, tagged
with that meal, and planned in one tap, composed client-side from the existing CreateRecipe,
SetRecipeMealSlots and CreateMealPlanEntry slices rather than a new API shape. Reason: "I want to
add spaghetti bolognese but haven't added it as a recipe" ended in a five-screen round trip, which
is a plan abandoned.

## 2026-08-20 · Nothing defaults to a CLR-evaluated timestamp

`Note.CreatedOnUTC` carried `HasDefaultValue(DateTime.UtcNow)`, frozen at scaffold time, so every
Note defaulted to the same stale moment *and every migration since re-altered the column* as the
"default" moved. It is `HasDefaultValueSql("SYSUTCDATETIME()")` now. Rule: a column default is the
database's clock or a constant, never the model's.

## 2026-08-19 · The session is the cookie; browser storage is out of the auth path entirely

Supersedes the session mechanics of the 15 Aug entry below. That fix kept the token in
`ProtectedLocalStorage`, which means identity had to be *read back out of the browser over JS
interop after every reload*, and every way that read can fail (circuit evicted, interop timeout,
key-ring drift) is indistinguishable from "not signed in". Mitch kept getting the login page on
F5, and rotation compounded it: several circuits share one stored token, so whichever presented
it second looked like a replay attack and revoked the household.

The standard shape replaced all of it:

- **A persistent auth cookie is the session** (`Home.Session`, HttpOnly, essential, 90-day sliding
  expiry matching the refresh token). The browser sends it with the request that starts the
  circuit, so a reload arrives already signed in, with nothing to read, race or time out.
- **`/login`, `/logout` and `/setup` render statically** (`App.razor.cs` picks the render mode
  per path) because only an HTTP response can carry `Set-Cookie`. They are plain Blazor
  `@formname` forms with `<AntiforgeryToken />` and `[SupplyParameterFromForm]`, and each guards
  against arriving through the interactive router by re-entering with `forceLoad` when there is
  no `HttpContext`. The custom `AuthenticationStateProvider`, `AuthInitialiser` and the OAuth
  view model in browser storage are deleted.
- **The refresh token rides in a claim** inside the encrypted cookie. Per circuit,
  `HouseholdSession` trades it for an access token in memory; `OAuthClient` is the one place that
  talks to the token endpoint. A refused refresh surfaces as an error: nothing client-side can
  end a session, because only the sign-out form can clear the cookie.
- **Refresh tokens do not rotate.** Every tab and device presenting the token it holds must keep
  working; rotation is what made honest concurrency look like theft. A session ends two ways:
  expiry or sign-out. The grace window died with rotation. The access token is only re-minted
  when it has under five minutes left (`SessionValues.AccessTokenReissueFloor`), so tabs sharing
  a session row converge on one token instead of invalidating each other hourly.

Verified end-to-end on 19 Aug against a live browser and a disposable DB user: sign-in sets the
cookie, F5 stays signed in, killing and restarting the WebUI stays signed in, sign-out clears it,
and presenting the same refresh token twice returns the same token both times.

## 2026-08-19 · The reconnect overlay is ours, and a dead circuit reloads itself

Blazor's default overlay is white in an app that is dark by default, and once it gives up it
leaves a page that looks alive and does nothing, the tablet's worst failure mode. The element
with Blazor's `components-reconnect-modal` id in `App.razor` restyles the overlay with theme
tokens, and `reconnect.js` watches the state classes: a rejected reconnect (server restarted,
circuit gone) reloads immediately, and a failed one retries with a HEAD probe and reloads the
moment the server answers. Reloading is safe precisely because of the cookie entry above: the
reload comes back signed in on the same page.

## 2026-08-17 · Adding to a shopping list is one text box, not a form

The add-item modal is gone. There is a single input that stays on screen, takes the whole line as
written ("2 kg potatoes", "500g mince", "1/2 cup rice") and keeps the cursor after each add, so
writing a list is typing, Enter, typing, Enter. `ShoppingListItemLogic.Parse` does the reading and
is the only place that knows how a line is written; anything it cannot make sense of survives as
the name exactly as typed, because losing what someone wrote is worse than missing an amount.
Reason: this is the most-used action in the app and it was costing four taps and a modal per item,
which is precisely the comparison against Bring! and AnyList that the product has to win.

## 2026-08-17 · What a household buys is offered back to it

`GetShoppingListItemSuggestions` returns the household's distinct item names ordered by how often
they have been added, each carrying the amount and price from the last time it was bought. The
whole set (capped at 200) is fetched once when the screen opens and filtered on the device, not
per keystroke, because a phone in a supermarket should not be asking the server on every letter, and
what a family usually buys does not change between keystrokes. Picking a suggestion brings its
last price with it, which is how the running total fills in without anyone typing prices. An
amount already typed beats the remembered one.

## 2026-08-17 · Emptying a list is one call, not one call per line

`DeleteTickedShoppingListItems` and `UntickShoppingListItems` exist as their own slices rather
than the screen looping over the per-item endpoints. Thirty round trips over a supermarket
connection is the difference between a list emptying and a list draining. Clearing is confirmed
because there is no undo; unticking is not, because nothing is lost. Both are household-scoped
the same way as everything else: a list that isn't yours simply matches nothing.

## 2026-08-17 · A shopping item's cost is the line price, not a unit price

`ListTotal` sums `Cost` and no longer multiplies it by an amount. "$3.50 for 2 kg of potatoes" is
three fifty, and multiplying turned it into seven. The field is labelled "Price for the line" so
the meaning is on screen rather than assumed. Consequence: there is no per-kilo price anywhere,
and adding one later means a new column rather than reinterpreting this one.

## 2026-08-17 · The web app's item requests carry Amount and Unit

The web app was still posting `Quantity`/`Volume`/`Weight` after the API moved to `Amount`/`Unit`,
so a quantity typed into the add form was serialised, ignored by the API and silently lost, and
the row rendered the legacy column, which was always empty, so the loss was invisible. Both
request models now match the API. The legacy properties stay on the *response* model because rows
written before units existed still read through them.

## 2026-08-17 · The whole AutoMapper configuration is asserted in a test

AutoMapper only validates a map the first time it is used, so a missing one is invisible until
the screen that needs it returns a 500, which had happened more than once, most recently on
`GetShoppingList`, whose `Items` had no element map at all. `MapperConfigurationTests` now builds
the configuration from the same four assemblies `Program.cs` registers and calls
`AssertConfigurationIsValid`, turning that into a build failure. Two notes for whoever touches it:
the assembly list must stay in step with `Program.cs` or profiles go unchecked (a second test
asserts profiles are actually found, so the assertion can't silently pass over nothing), and the
exception is caught and asserted as a string because `AutoMapperConfigurationException` does not
survive the test runner's serialisation, because a test that lets it escape vanishes from the run
instead of failing it. It immediately found four faults: the missing shopping list item map,
`User.Household` unmapped on both user profiles, and `UpdateUserApiRequest -> UpdateUserInputPort`
having no usable constructor. That last one is why `UsersController` now builds its input ports
directly, like every other controller. Mapping onto a positional record is fragile, because
`ForMember(...).Ignore()` cannot ignore a constructor parameter.

## 2026-08-15 · Every relationship is configured explicitly, or EF quietly invents a bad one

Deleting a recipe failed on `FK_RecipeStep_Recipe_RecipeID`. The cause was an *absence*:
`RecipeStepConfiguration` never declared the relationship, so EF inferred one from `Recipe.Steps`,
and because `RecipeStep` carries no back-navigation, it inferred it as **optional with no
cascade**, giving a nullable `RecipeID` the database then used to block every delete. Deleting a
recipe with steps had never worked. The giveaway is the constraint name: EF's default
`FK_Child_Parent_Column` shape instead of this repo's `FK_Child_Parent`, so that naming
difference is a reliable way to find unconfigured relationships. A sweep found exactly one other,
`Audit → User`, which blocked deleting any member who had ever done anything. Steps now cascade
from the recipe (a single path: the household already reaches them through it); the audit link is
`SetNull`, because history outlives the person and `Audit.UserName` is denormalised onto the row
for exactly that reason. Rule: configure every relationship explicitly, and never trust an
inferred one, because a missing configuration is silent until a delete fails in front of the family.

## 2026-08-15 · There is a light theme now, but dark is still the default

Supersedes the 13 Aug "dark only, no toggle" decision. Mitch listed "No light mode" as a
complaint, so light is now an opt-in **per-device** preference in Settings → Appearance:
Dark / Light / Match device, stored in `localStorage` under `home-theme`. Dark remains what an
unconfigured device gets, because a kitchen tablet that already lives on the wall must not change
appearance because someone else's phone chose otherwise, and a device with JavaScript off or
storage blocked falls through to dark as well. The theme is **tokenised, not duplicated**: the
`ink` scale, the five pillar hues and the `surface` aliases moved out of `tailwind.config.js`
into CSS custom properties on `:root` (dark) and `:root[data-theme="light"]`, declared as
`rgb(var(--token) / <alpha-value>)` so the ~400 existing utilities, including opacity
modifiers like `bg-week/10` and `border-lights/40` that a plain `var()` would have broken,
work in both themes with **no component markup changed**. Read the ink scale by role, not by
lightness: 950 is the page, 900 the surface, 800 raised fills and borders, 50 the primary text.
Light inverts the ramp, so those roles still hold. The pillar hues could not simply be reused:
sky `#7dd3fc` is about 1.4:1 on paper, so each has a darkened light-theme variant that keeps
its identity and clears 4.5:1. The stored choice is applied by a synchronous inline script in
`App.razor` before the body renders; the app renders with `prerender: false`, so there is no
server-side pass to put it on and an inline script is the only thing that beats the first paint.
Rule for later: a new colour goes in `input.css` as a token pair, never as a hex in the config
or in markup.

## 2026-08-15 · A household session is expected to last months, not an hour

Mitch: "If I close the application or browser I have to relog back in. I should not have to."
Three independent faults, all fixed together because fixing one alone changes nothing visible.
(1) The refresh request sent `grant_type` read from the *sign-in* config key, so it always said
`password`; the API routed it into the password branch and 401'd, so **token refresh had never
worked once**. (2) Any 401 was treated as fatal, so a 5xx, a timeout or an API that had not
finished starting destroyed a valid refresh token; only an explicit 401/400 *from the token
endpoint* may now sign anyone out. (3) Nothing refreshed at startup, so an expired access token
meant the login page even with a good refresh token in storage. Startup now refreshes before
completing initialisation, so `AuthorizeRouteView` holds its Authorizing slot instead. Sessions
carry an absolute expiry, refresh is serialised through one semaphore (the dashboard's six
parallel loads previously raced and consumed each other's single-use token), and the data
protection key ring is pinned with `SetApplicationName` so moving the folder no longer silently
invalidates every device. Refresh tokens now live 90 days and slide. Rule: never let a transport
failure reach `SignOutAsync`.

## 2026-08-15 · Board columns belong to the household, and are named for a home

`ActivityState` was a global lookup seeded with Todo/Refining/Progressing/Blocked/Testing/Done,
software-process jargon on a family board, and the one table the 14 Aug isolation sweep could not
scope. It now carries `HouseholdID`, `Sequence` and `IsComplete` (which column means finished, so
a card moved there stops appearing on the dashboard). Existing columns were **renamed, not
replaced**, so every card stayed where the family left it; new households get
To do → Doing → Waiting on → Done from `IHouseholdSetupLogic`, which also seeds the meal slots.
Seeding moved out of `Program.cs`: a global row is now unreachable by every scoped query.

## 2026-08-15 · One "meal" vocabulary, not two

`MealSlot` is household-defined and serves both jobs: which meal a `MealPlanEntry` is for
(nullable one-to-many) and how the recipe book is filtered (`RecipeMealSlot`, many-to-many, because
pancakes are breakfast *and* dessert). Two separate concepts for "dinner" would have drifted
apart in the family's head. `MealPlanEntry → MealSlot` is Restrict, not Cascade: the household is
already reached through the recipe, and a second cascade path is rejected by SQL Server. Refusing
to delete a slot still holding a week of dinners is also the behaviour a family wants.

## 2026-08-15 · Migrations against a live family database are additive and rehearsed

The database now holds real data, so the earlier "no rows existed" safety net is gone. This
migration drops **nothing**: measurement units arrived as new `Amount`/`Unit` columns beside the
old unitless `Quantity`/`Volume`/`Weight`, which stay until the move is proven. It was rehearsed
by restoring a copy of the live database and applying it there, which caught a real defect: the
session-expiry backfill was conditional, and because the column default stamps the migration time
the condition never matched, so every existing session would have been born expired. Rule for
later: rehearse a data-moving migration against a restored copy, and read what it actually did.

## 2026-08-14 · Every interactor is scoped to the caller's household

Roughly forty interactors loaded entities by raw ID (`Find<T>(id)`), so any authenticated user
could read, change or delete another household's recipes, lists, activities, members and notes
by guessing IDs. Every lookup now filters through the entity's ownership path to
`IAuthorisationService.GetHousehold()` (e.g. `i.ShoppingList.Household.HouseholdID`), and each
interactor keeps its previous not-found/no-op behaviour so nothing leaks which IDs exist. Found
in the same sweep: CreateUser saved members with **no household at all** (orphans invisible to
every scoped query, now attached to the caller's household), and UpdateShoppingListItem never
called SaveChangesAsync and dereferenced an unloaded navigation, so item updates could never
persist. Rule for later: any interactor that takes an ID must scope it to the household. An
unscoped `Find` is a cross-household hole, not a shortcut.

## 2026-08-14 · Live cross-device updates go through a hub on the API, not in-process events

Mitch: don't assume one Blazor Server instance (Azure auto-scale), and client satisfaction beats
battery when they conflict. So change notifications relay through `ChangeNotificationsHub` on
the API: pages publish after successful mutations, every device in the household sees the change
instantly, and the background light sync pushes too, so a wall-switched light now appears without
anyone tapping Sync. Security: the hub derives the SignalR group from the caller's authenticated
claims, so a client can neither choose nor spoof a household. Sockets: one shared WebSocket per
household per WebUI instance (never per circuit, the historical TCP-exhaustion trap), WebSockets
only so it can never degrade into long-polling churn, closed when the last subscriber leaves; all
connections are server-to-server, so devices carry nothing extra. The dashboard's poll dropped to
a five-minute fallback for hub outages. If hosting lands on Azure with API scale-out, Azure
SignalR Service is a one-line `.AddAzureSignalR()` swap.

## 2026-08-14 · Meal planning is the connective tissue, not a fifth pillar

`MealPlanEntry` (a recipe on a calendar day, reached through the recipe to keep one cascade
path) powers /meal-plan, the dashboard's "Tonight" hero tile, and "add week to list", which
funnels the planned window's ingredients into a shopping list server-side, deduplicating a
recipe planned twice (doubling quantities is the shop's decision, not the app's). Reason: the
vision's dashboard question "what's for dinner" had no answer anywhere, and this makes recipes,
shopping and the board reinforce each other rather than stay three separate mini-apps.

## 2026-08-14 · Recipe import reads JSON-LD only, and fails honestly

POST api/Recipes/Import fetches a page and reads the schema.org Recipe most cooking sites embed
as JSON-LD (`JsonLdRecipeImportService`, regex + System.Text.Json, no scraping packages). If a
page carries no structured recipe, the import returns a 422 with a plain explanation instead of
guessing at HTML, because a wrong-looking import erodes trust faster than a failed one. Ingredient
lines stay whole ("2 cups flour") because splitting quantities reliably is a losing game.

## 2026-08-14 · The board stays fresh by itself: background sync, sun triggers, auto-refresh

The bulb-list reconcile moved out of SyncLightsInteractor into shared `ILightSyncLogic`, and a
second hosted runner (`LightStateSyncRunner`, five-minute tick) refreshes every tokened
household's bulbs, so a light switched at the wall shows up without anyone pressing Sync. The
dashboard re-reads Home's own records every sixty seconds (free, with no provider calls) and now
disposes its `CancellationTokenHandler`, which pages historically never did. Light schedules
gained sunrise/sunset triggers (`Trigger` + `OffsetMinutes`, Almanac `SunCalculator`, household
lat/long), which is the "follow the sun" promise the Settings page copy was already making. Both
runners keep the existing single-token background limitation, noted in LifxAuthenticationHandler.

## 2026-08-14 · Members surfaced, assignment shipped, avatar-switching deferred

The Settings page grew a Members card over the existing CreateUser/new GetUsers slices, and
activities now expose the assignee end-to-end (the domain, DB and API always supported it, but no
UI ever sent it). Passwordless tap-your-avatar user switching was deliberately NOT built: it
weakens auth on a possibly-internet-facing app, and the first-run registration entry already
rejected auth bypasses. It needs its own decision (per-user PIN? device-trusted sessions?).

## 2026-08-14 · Kitchen-mode details: cook screen, family notes, trolley ticking

/recipes/{id}/cook shows one step at a time in display type with tap-to-start timers parsed
from the step text ("simmer 20 minutes" becomes a button) and holds the tablet awake via the
Screen Wake Lock API (wwwroot/js/cook.js, where everything degrades silently). The dashboard gained
anonymous pinned family notes (`Announcement`, because the board belongs to the household, not a
member). Shopping list rows are now tap-to-tick using the long-dormant `InBasket` column, with
a running "in the trolley" total against the list total. EF migrations can now be generated
while the API is running via `PersistenceContextDesignTimeFactory`
(`--startup-project Home.Persistence`); `Database.Migrate()` still applies them at API startup.

## 2026-08-14 · PropertyChangeTracker crosses the wire through a JsonConverter

Saving the LIFX token failed with "Name cannot be empty": System.Text.Json deserialised every
tracker property through its `Value` setter, which flips `HasBeenSet` to true, so a partial
update arrived with *all four* settings marked as set (Name as a set-to-null, failing NotEmpty;
worse, a name-only save would have cleared the location and token). Both `PropertyChangeTracker`
structs now carry `[JsonConverter(typeof(PropertyChangeTrackerJsonConverterFactory))]`, which
writes `{hasBeenSet, value}` and on read returns `default` unless `hasBeenSet` is true. Rule for
later: never let a tracker round-trip through property-by-property deserialisation; the converter
is the only wire path, and `Home.Application.Tests/Infrastructure/ChangeTrackers` pins it.

## 2026-08-14 · Form inputs declare autocomplete, and labels are wired to their controls

`HomeTextInput`/`HomePasswordInput` now render a per-instance `id` their label points at, plus
optional `Name`, `AutoComplete` and `InputMode` parameters, and `aria-invalid`/`aria-describedby`
when showing an error. Conventions: login is `username`/`current-password`; setup uses
`given-name`/`family-name`/`email`/`new-password`; secrets that are not login passwords (the LIFX
token) use `new-password` so a password manager never autofills the household login into them;
fields a browser might mistake for personal data (anything labelled "Name") get `off`; numeric
text fields get `InputMode` so tablets show the right keyboard. Raw `<select>`/`<textarea>`/date
and time inputs get explicit `id`/`for` pairs.

## 2026-08-13 · Registration is first-run only

`POST api/Households/register` (anonymous) creates the household and its first member in one
step, and refuses with 409 the moment any user exists, and the login page offers "Set up your
household" only while `setup-status` says the database is empty. Reason: a fresh install must be
usable without CLI or Swagger, but an open registration endpoint on a possibly-internet-facing
app is a hole. Additional family members are added from inside the app (CreateUser API exists;
a Members section in Settings is the obvious future home, with no UI yet). No auth bypass was added
for local use: sessions persist via refresh tokens, and a bypass flag would be a foot-gun given
cloud hosting is still an open option.

## 2026-08-13 · Components use .razor.cs code-behind

Mitch: component logic lives in a `.razor.cs` partial class beside the markup so the C# language
server can analyse it. Inline `@code` blocks get little to no LSP support in most editors. Markup
and directives (`@page`, `@inject`, `@typeparam`) stay in the `.razor`; everything else moves to
the partial. Supersedes the earlier inline-`@code` convention.

## 2026-08-13 · Household settings live in SQL, not user secrets

Mitch: assume cloud-hosted SQL storage. Household-wide settings (name, latitude/longitude for
future sunrise/sunset triggers, the LIFX API token) are stored on the household row and edited
from the Settings page: setup must not be a CLI exercise. The token is write-only through the
API: GET returns `HasLifxApiToken`, never the value. `lifxApiToken` in user secrets remains as a
developer fallback when the household has no token stored.

## 2026-08-13 · The design system: warm ink neutrals, Fraunces display, pillar hues

The zinc/teal look was the stock "dark dashboard with a single accent", indistinguishable from
template output. Replaced with: warm stone neutrals (`ink` scale), Fraunces as an editorial
display face over Inter UI text, light-on-dark primary buttons, and one hue per pillar
(recipes apricot, shopping sage, week sky, lights amber) used for identity only, so a family member
navigates by colour without reading. The dashboard is a live "family board" (glance, don't
navigate), and `HomeNavRail` keeps every page one tap from anywhere, so no screen is a dead end.

## 2026-08-13 · UI direction: upgrade, tablet-first, not generic

Mitch: the UI should be "upgraded, not generic, functional, good UX, and won't make a user
frustrated". Combined with the product vision (kitchen tablet, family-proof, see `VISION.md`),
this answers the open design question from the 12 Aug handover: the existing dark zinc/teal
language is the starting point, but the bar is a deliberate, product-specific design, not
extending template defaults. Scenes/Schedules/Effects screens and any reworked pages are built
against that bar.

## 2026-08-13 · Stashed desktop work triaged, not merged wholesale

A GitHub Desktop stash on the desktop clone held pre-rewrite local work. Most of it had been
independently superseded by the remote's Activities feature, so it was *not* applied. The genuinely
unique pieces were ported by hand: the TaskCompletionSource-gated `AuthorisationService` (holds
`AuthorizeRouteView` in its `Authorizing` slot until JS interop can read storage, which kills the
"not authorised" flash on load), fresh `HttpRequestMessage` per send attempt (reuse across a 401
retry throws), Basic client credentials on token refresh, and sign-out on failed refresh. The full
stash is preserved on branch `backup/stashed-local-work` if anything else turns out to matter.
`start.bat` was deliberately left behind (`start.ps1` and `.claude/launch.json` cover it).

## 2026-08-13 · Commit messages carry no AI co-author trailers

The 12 Aug history rewrite existed solely to strip `Co-Authored-By: Claude` trailers from 33
commits. Don't add them to new commits; that recreates the problem the rewrite fixed.

## 2026-08-12 · History rewritten; stale clones reset, never merged

Every commit from 18 May 2026 onward has a new SHA (content identical, trailers stripped). A stale
clone that still has the old chain must `git fetch origin && git reset --hard origin/master`. A
pull produces a giant self-merge of identical content (this bit the desktop clone on 13 Aug; it was
recovered by exactly that reset). The `main` branch was deleted; `master` is the only branch.

## 2026-08-12 · Home owns light grouping, not LIFX

A sync refreshes a bulb's name and state but never moves it between Home groups. Reason: the
family's mental model of the house ("kitchen", "kids' rooms") belongs to Home, not to whatever the
provider app happened to be configured with. A test pins this. Related: a whole room is one API
call (LIFX accepts 25 comma-separated selectors), so Home-defined groups cost nothing extra.

## 2026-08-12 · An unreachable provider is a return value, not an exception

`ILightService` returns `null` or a `LightCommandResult`; the presenter maps that to a 503.
Adapters catch `HttpRequestException`/`TaskCanceledException`/`JsonException` themselves, and 429s
are logged with their rate-limit headers and treated as unavailable. Reason: a kitchen tablet must
degrade gracefully: "lights unavailable" is a state, not a crash. The same rule applies to any
future external integration.

## 2026-08-12 · Vendor wire types stay in the adapter

`LifxLight` and friends never escape `Home.WebApi/Infrastructure/Lights/`; they map to
`LightSnapshot` at the boundary. Use cases never learn which vendor is on the other end. This is
the template for every future smart-home integration.

## 2026-08-12 · Light effects are gated on detected hardware capability

Capabilities are read from `product.capabilities` on sync; the UI offers only what the bulb can do.
Move/morph/flame are excluded (they need multizone strips or tiles). Reason: offering a control
that silently does nothing is exactly the frustration the product exists to avoid.

## 2026-08-12 · Nothing reads the clock directly

Everything resolves `TimeProvider` (.NET 8): interactors via `serviceFactory`, services via
constructor, Razor via the global inject. Reason: testability (`FakeTimeProvider` with exact-time
asserts) and consistent "now" within a render. `DateTime.UtcNow`/`.Now` appear only in migrations.

## 2026-08-12 · SQLite path deleted; SQL Server only

The migrations are SQL Server-shaped (filtered indexes etc.) and SQLite rejects them. LocalDB
serves local dev. Consequence: distribution will likely want Docker Compose with Postgres one day,
which would reopen this, and that's the known trade.

## 2026-08-12 · Two cascade paths deliberately removed

`LightSceneState` does not cascade from `Light` (SyncLights clears scene entries itself), and
`LightSchedule` hangs off its scene without a second `Household` FK. SQL Server rejects the
multiple-cascade-path graph otherwise. Adding either back breaks the migration.

## 2026-06 · The audit table is polymorphic on purpose

`ResourceTypeSE` enum + `long EntityID`, no FK. The database can't enforce it, but audit rows must
outlive the entities they describe, and a per-table audit design creates FK cycles.

## 2026-05/06 · MudBlazor stripped; Tailwind + an owned component library

Every UI element is either a `Home*` component or raw Tailwind utilities. Reason: owning the design
language end-to-end (see the 2026-08-13 UI direction entry, because this decision is what makes
"not generic" achievable). Icons are CSS masks in `input.css`, no icon library. Dark zinc/teal
palette; `darkMode: false` because dark *is* the palette.

## 2025-09 → · Clean architecture on CleanArchitecture.Mediator, vertical slices

Input port → pipeline (auth → validation → interactor) → output port, one folder per use case in
every layer, interactors `internal`, controllers thin, presenters map to HTTP. The package resolves
from the committed `packages/` folder via `nuget.config`, not nuget.org. The seven-file recipe for
a new use case is in `.claude/skills/home-conventions/references/use-case-slice.md`.
