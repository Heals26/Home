# Known gaps and drift

Two separate lists. The first is what the repo doesn't do. The second is where the repo disagrees
with the conventions Mitch uses at work (Companion Systems / OnSite Companion), which are codified in
the `anthropic-skills` plugin at `code-review/references/csharp-blazor-style.md`.

Neither list is a licence to refactor. Fix an item when it is the task, or when you are already
editing that exact code and the fix is a line or two. Otherwise mention it and move on.

Last verified: 12 August 2026, against commit `30da990`.

---

## Part 1 — What the repo doesn't do

### Test coverage is one slice deep

`Home.Application.Tests` exists (xUnit + Moq + FluentAssertions) and covers the Recipes and Lights
interactors — 19 tests. Everything else in `Home.Application` is untested: Activities, Shopping
Lists, Users, OAuth, and all of the `EntityLogic` services. There are no tests at all for
`Home.WebApi` presenters or `Home.WebUI` components.

The pattern to copy is in `Infrastructure/TestServiceFactory.cs`: interactors have no constructor,
so the `ServiceFactory` delegate is the only seam.

### 145 compiler warnings

A clean `dotnet build` produces 145 warnings, 0 errors. Effectively all of them are nullable
reference warnings:

| Code | ~Count | Cause |
|---|---|---|
| `CS8618` | 115 | Non-nullable property never initialised — mostly domain entities and API models |
| `CS8603` / `CS8601` / `CS8604` | 20 | Possible null return / assignment |
| `CS8765` / `CS8767` | 4 | Nullability mismatch on an override or interface implementation |
| `CS1998` | 2 | `async` with no `await` |

The practical rule: **don't introduce a new *category* of warning.** Clearing the CS8618 backlog
means either `required` modifiers or nullable annotations across the entity layer — a real piece of
work, not a drive-by.

### `Home.WebApi` has nullable disabled

Every other project sets `<Nullable>enable</Nullable>`. `Home.WebApi` sets `disable`, and suppresses
`CS1591` because `GenerateDocumentationFile` is on for Swagger rather than for documentation
coverage. Files that need nullable-aware contracts opt in with a `#nullable enable` at the top —
`Infrastructure/Lights/LifxLightService.cs` is the example.

### The Light entities are now orphaned

`Light`, `LightGroup` and `LightLocation` have entities, EF configurations and migrations, and are
referenced by nothing. The Lights feature proxies LIFX live rather than persisting topology, because
LIFX already returns group and location with every bulb and is the source of truth for state.

They're harmless but they are dead tables. Either drop them in a migration, or use them for the
thing the API can't give you — per-household display order, friendlier room names, favourites.
That's a decision for Mitch, not a cleanup.

### The EF model has drifted from the migrations — blocks new migrations

**Read this before running `dotnet ef migrations add`.** The entity model and the last migration
snapshot disagree about tables nothing recent has touched. Any new migration sweeps the difference
up and will try to apply it.

The largest piece: `Activity.Household` exists on the entity and in `ActivityConfiguration`, but the
only migration mentioning `Activity` is `InitialCommit` (Sept 2025). A generated migration therefore
wants to `AddColumn HouseholdID … nullable: false, defaultValue: 0L` on `Activity` **and** add an FK
to `Household` — which fails, or orphans rows, on a populated table with no household 0.

Also queued up in that same drift:

- `Activity.StateID` / `StatusID` / `UserID` become nullable.
- Duplicate shadow columns `ActivityStateID` and `ActivityStatusID` get dropped.
- `ActivityContent`'s FK column is renamed to `RegionID`.
- `Note.CreatedOnUTC`'s default timestamp is regenerated (harmless churn, reappears every time).

This predates the Lights work — Stage 1 of Lights shipped **without** a migration for exactly this
reason, so the new `Light` columns are not in any database yet. Resolving it means deciding, against
real data, whether those Activity columns are safe to drop and what `HouseholdID` should backfill
to. That is a decision, not a cleanup.

### Configuration lives entirely outside the repo

`Home.WebApi` has no `appsettings.json`. `databaseConnectionString`, `lifxApiToken` (API) and
`apiBaseUrl` (WebUI) all come from user secrets. `CLAUDE.md` documents them; nothing validates them
at startup beyond `apiBaseUrl`.

### Minor inconsistency

`CancellationTokenHandler` declares `#region Properties` twice, the first containing a field.

---

## Part 2 — Drift from the work style guide

Home is the older codebase and predates some of the work rules. **Home's conventions win inside this
repo** unless Mitch says otherwise — consistency within the codebase beats consistency with another
codebase. This list exists so the difference is a decision, not an accident.

### Where they already agree

Australian English, file-scoped namespaces, `internal` infrastructure, expression bodies on the line
below the signature, composition over inheritance in components, kebab-case CSS class names.

### Where Home differs

| Work rule | Home | Verified |
|---|---|---|
| Required parameters get `[EditorRequired]` | Never used | 0 of 77 `[Parameter]` declarations |
| Don't use cascading parameters | Core to the cancellation pattern | 6 files |
| Code-behind `.razor.cs` for non-trivial logic | Everything is inline `@code` | 0 `.razor.cs`; 27 of 30 `.razor` have `@code`; `RecipeDetailPage.razor` has a 402-line block |
| Component styles in co-located `.razor.css` | Tailwind utilities inline | 1 `.razor.css` in the whole project |
| Global CSS is theme tokens only | `input.css` also holds the icon system and component classes | `@layer components` |
| Chained calls: every call on its own line | First call stays on the source line | `_PersistenceContext.GetEntities<Recipe>()` then `.Where(...)` indented |
| Booleans set by name only, not `="true"` | Mixed — newer code uses name-only | `ShowBack="true"` ×3, `Propagation="true"`, `Default="true"` |
| Splatted attributes filter `class`/`style` | `HomeButton` builds its own `class` *and* splats `@attributes` unfiltered | `@attributes` sits after `class=`, so a splatted `class` wins |
| Component parameters alphabetically ordered | Ordered by importance | `HomeButton`: `ChildContent`, `Variant`, `Size`, `Disabled`… |
| `[Inject]` fields are private | No `[Inject]`; two services `@inject`-ed globally in `_Imports.razor` | Every component gets `ApiAccess` whether it needs it or not |

### Conventions Home has that work doesn't

These are Home's own and have no work equivalent — the work guide is silent on all of them, so
there's no conflict, just extra rules that apply here:

`_PascalCase` locals · `m_PascalCase` fields · mandatory `this.` · `#region` blocks with labelled
`#endregion` · alphabetised members within regions · `ID`/`UTC` capitalisation · `_ =` discards on
fluent calls.

### The one worth acting on

`HomeButton` is the only entry above with teeth. Because `@attributes` is declared *after*
`class="@this.GetClasses()"`, a caller who splats a `class` silently replaces the button's entire
computed styling instead of adding to it — and `HomeButton` already has a `Class` parameter that does
the right thing, so there are two ways in and one of them is wrong. No caller does this today, so it
is latent rather than broken. Filtering `class` and `style` out of `AdditionalAttributes` closes it.

The rest are style, and Home's answers are defensible. Tailwind-in-markup and inline `@code` in
particular are not sloppiness — they are what you get from choosing Tailwind, and reversing them
would mean unpicking the design system.
