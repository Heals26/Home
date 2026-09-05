# Known gaps and drift

Two separate lists. The first is what the repo doesn't do. The second is where the repo disagrees
with the conventions Mitch uses at work (Companion Systems / OnSite Companion), which are codified in
the `anthropic-skills` plugin at `code-review/references/csharp-blazor-style.md`.

Neither list is a licence to refactor. Fix an item when it is the task, or when you are already
editing that exact code and the fix is a line or two. Otherwise mention it and move on.

**Last verified: 4 September 2026** (test coverage and configuration) and **1 September 2026**
(everything else), by rebuilding clean, running the suite, and querying the live database.
Every count below was measured, not remembered. If you are reading this more than a month later,
re-measure before trusting a number.

---

## Part 1: What the repo doesn't do

### Every use case slice is covered

`Home.Application.Tests` (xUnit + Moq + FluentAssertions, pinned to 7.x) runs **557 tests** across
**all 114 use case slices**, reads and writes. Every one seeds a neighbouring household alongside
ours, so the isolation invariant is pinned rather than assumed.

Also covered: `ShoppingListItemLogic` (the text parser), `SunCalculator`,
`PropertyChangeTracker`'s JSON converter, most of `Services/EntityLogic` through the slices that
use it, and guards on the whole AutoMapper configuration, the three measurement-unit lists and the
startup configuration checks.

**What is still uncovered:** `Home.WebUI` components, and presenters other than through the slices
that drive them. Both are markup-heavy and would need a different harness.

Two harnesses live side by side and the choice matters:

- **`InteractorTest`** (with `TestDatabase`) is a real `PersistenceContext` over the EF in-memory
  provider, seeded through one context and read through another, presenting through the real
  presenter. **Use this for anything that queries.** It is the only harness that can see a
  projection which forgets a navigation; see the read-slice trap below.
- **`TestServiceFactory` alone, with a mocked `IPersistenceContext`** is fine for a slice that never
  queries (`GetWeather`, `GetHouseholdSettings`) or where the point is that a service was called.

### The read-slice trap: a presenter reading what the projection never loaded

**If a presenter touches `x.Y.Z`, the interactor's projection must name `x.Y`.** Nothing enforces
this, not the compiler, not the AutoMapper guard and not a mocked context, and it has broken three
screens:

| | Fault | How it surfaced |
|---|---|---|
| 17 Aug | `GetShoppingList` items unmapped | Caught by the AutoMapper configuration guard |
| 1 Sep | `GetCardSections` did not project `Regions` | `CardCount` read 0, so the settings sheet offered to delete a section in use, caught by hand |
| 1 Sep | `GetActivity` did not project `CardSection` | `NullReferenceException`; **every activity card failed to open**, caught by Mitch |

It takes two shapes. A dereferenced navigation (`r.CardSection.Name`) throws and the screen 500s. A
counted collection (`s.Regions.Count`) silently reads zero, which is worse, because nothing looks broken.
Both are pinned by tests now, and reverting any of the three fixes fails the suite.

The reason a mock cannot catch it: `stored.AsQueryable()` hands the interactor an object graph that
is already fully connected, so the projection changes nothing. Against a real context the projection
is what decides.

### Clearing a navigation only works if the query loaded it

The read-side trap has a write-side twin, and it is quieter. Setting a reference navigation to null
on an entity whose navigation was **never loaded** is not a change EF can see: the tracker compares
null against null, finds nothing, and leaves the foreign key exactly where it was. The save
succeeds, the endpoint answers 204, and nothing happened.

It shipped twice. `UpdateActivity` could not unassign a member from a card, and
`UpdateMealPlanEntry` could not take a meal out of its slot. Both were fixed on 4 Sep by projecting
the navigation. Scalars are unaffected, which is what makes it confusing to diagnose: on the same
card, clearing the completion date worked while clearing the column did not.

**If a slice can set a navigation to null, its query has to name that navigation.**

### The harness does not simulate database cascades

EF only cascades a delete to dependents it has loaded. A query that deliberately loads none leaves
them behind in the in-memory harness, while SQL Server carries them away on the foreign key. So a
configured `OnDelete(Cascade)` cannot be asserted either way from a test:
`DeleteActivityRegionInteractorTests` says so where it would otherwise look like an omission.

### Some code leans on the authorisation call having warmed the change tracker

`AuthorisationService.GetHousehold()` queries the **same scoped context** the interactor uses, so
the household ends up tracked and EF fixes it up onto everything loaded afterwards. At least one
place depends on that without saying so: `ActivityLogic.AddRegion` reads `_Activity.Household`
having projected only `a.Regions`, and works solely because the authorisation call already put the
household in the tracker.

`InteractorTest` copies this deliberately, resolving the signed-in household and member through
the read context rather than handing over a detached stand-in, because a harness that skipped it
would fail where production passes. Worth knowing before changing either side.

### The warning count is 1, because `Home.WebApi` opts out of nullable

This corrects a long-standing claim here of "145 warnings, ~115 of them `CS8618`". A clean build of
the whole solution now emits **one** warning: a `CS8625` in `CreateRecipeInteractorTests.cs`.

Do not read that as the nullable backlog having been paid off. `Home.WebApi` still sets
`<Nullable>disable</Nullable>` while every other project enables it, and the API models and
controllers are where most of those `CS8618`s lived. They are suppressed, not fixed. Turning
nullable on in `Home.WebApi` will bring a few hundred warnings back in one go, which is the real
shape of that job. `CS1591` is also suppressed there, because `GenerateDocumentationFile` is on for
Swagger rather than for documentation coverage.

Files that need nullable-aware contracts inside `Home.WebApi` opt in with `#nullable enable` at the
top. `Infrastructure/Lights/LifxLightService.cs` is the example.

### `dotnet ef database update` targets LocalDB, not the real database

`PersistenceContextDesignTimeFactory` exists so migrations can be *added* while the API is running
and holding its output folder locked. But EF prefers an `IDesignTimeDbContextFactory` to the startup
project's service provider, so **`database update` comes through the factory too** and never reads
`Home.WebApi`'s user secrets. It silently falls back to
`Server=(localdb)\MSSQLLocalDB;Database=Home`.

This bit on 31 Aug: a migration reported "Applying… Done" against a LocalDB copy while the real
database was untouched. Set the connection string explicitly:

```bash
HOME_DESIGNTIME_CONNECTIONSTRING="<the real one>" dotnet ef database update --project Home.Persistence --context PersistenceContext --startup-project Home.WebApi
```

The API also calls `Database.Migrate()` at startup, so a missed migration self-corrects the next
time the API runs, which is exactly what makes the failure quiet.

### Setting up a fresh install is manual, on purpose

Two genuine secrets go in user secrets (`databaseConnectionString` on the API, and the pair of
`OAuth:AccessToken:*` credentials on the web app). Everything else moved into `appsettings.json` on
4 Sep, and `RequiredConfiguration` in each project names anything missing or malformed at startup
rather than letting it fail at the point of use. `README.md` is the setup guide and the file to fix
if a step is wrong.

The one step with no automation is inserting the `home.ClientApplication` row. Nothing seeds it,
a fresh database cannot be signed into without it, and **that is deliberate**: nothing is seeded
globally any more (1 Sep). Do not add seeding to make setup easier. The decision to remove the
requirement altogether is parked in `BACKLOG.md`.

### Sessions accumulate and are never cleaned up

`UserAuthentication` had 24 rows for a single household on 1 Sept, one per sign-in, none ever
removed. They are harmless, because every one carries a 90-day expiry and rotation is off, but nothing
prunes expired rows, and the table still carries the `SupersededByAuthenticationMetadataID` /
`SupersededOnUTC` columns that died with the 19 Aug no-rotation decision.

### Minor inconsistency

`CancellationTokenHandler` declares `#region Properties` twice, the first containing a field.

---

## Corrections to what this file used to say

Kept deliberately, because three of these actively misdirected work:

- **"Everything is inline `@code`; 0 `.razor.cs`."** Reversed. There are **49 code-behind partials
  against 52 `.razor` files, and zero files with an inline `@code` block**. The 13 Aug decision was
  carried all the way through. Home now *agrees* with the work style guide here.
- **"`[EditorRequired]` is never used."** It is used **4 times** now, out of 199 `[Parameter]`
  declarations. Still the exception rather than the rule.
- **"`Light`, `LightGroup` and `LightLocation` are dead tables."** They have been live since the
  14 Aug sync work, and `LightStateSyncRunner` writes to them every five minutes. Do not drop them.
- **"`Note.CreatedOnUTC` has a scaffold default, so delete the spurious `AlterColumn` from every
  migration."** Fixed on 20 Aug by moving it to `HasDefaultValueSql("SYSUTCDATETIME()")`. Migrations
  come out clean now; the 31 Aug one needed no hand-editing.

---

## Part 2: Drift from the work style guide

Home is the older codebase and predates some of the work rules. **Home's conventions win inside this
repo** unless Mitch says otherwise, because consistency within the codebase beats consistency with another
codebase. This list exists so the difference is a decision, not an accident.

### Where they already agree

Australian English, file-scoped namespaces, `internal` infrastructure, expression bodies on the line
below the signature, composition over inheritance in components, kebab-case CSS class names, and
since the 13 Aug decision was completed, **code-behind `.razor.cs` for component logic**.

### Where Home differs

| Work rule | Home | Verified 1 Sep |
|---|---|---|
| Required parameters get `[EditorRequired]` | Used, but rarely | 4 of 199 `[Parameter]` declarations |
| Don't use cascading parameters | Core to the cancellation pattern | 6 files |
| Component styles in co-located `.razor.css` | Tailwind utilities inline | 1 `.razor.css` in the whole project |
| Global CSS is theme tokens only | `input.css` also holds the icon system and component classes | `@layer components` |
| Chained calls: every call on its own line | First call stays on the source line | `_PersistenceContext.GetEntities<Recipe>()` then `.Where(...)` indented |
| Booleans set by name only, not `="true"` | Mixed, newer code uses name-only | `ShowBack="true"` ×3, `Propagation="true"`, `Default="true"` |
| Splatted attributes filter `class`/`style` | `HomeButton` builds its own `class` *and* splats `@attributes` unfiltered | `@attributes` on line 10, after `class=` on line 7 |
| Component parameters alphabetically ordered | Ordered by importance | `HomeButton`: `ChildContent`, `Variant`, `Size`, `Disabled`… |
| `[Inject]` fields are private | No `[Inject]`; four services `@inject`-ed globally in `_Imports.razor` | Every component gets `ApiAccess` whether it needs it or not |

### Conventions Home has that work doesn't

These are Home's own and have no work equivalent, and the work guide is silent on all of them, so
there's no conflict, just extra rules that apply here:

`_PascalCase` locals · `m_PascalCase` fields · mandatory `this.` · `#region` blocks with labelled
`#endregion` · alphabetised members within regions · `ID`/`UTC` capitalisation · `_ =` discards on
fluent calls.

### The one worth acting on

`HomeButton` is the only entry above with teeth, and it is still open. Because `@attributes` is
declared *after* `class="@this.GetClasses()"`, a caller who splats a `class` silently replaces the
button's entire computed styling instead of adding to it, and `HomeButton` already has a `Class`
parameter that does the right thing, so there are two ways in and one of them is wrong. No caller
does this today, so it is latent rather than broken. Filtering `class` and `style` out of
`AdditionalAttributes` closes it.

The rest are style, and Home's answers are defensible. Tailwind-in-markup in particular is not
sloppiness. It is what you get from choosing Tailwind, and reversing it would mean unpicking the
design system.
