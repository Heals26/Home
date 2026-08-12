# Known gaps and drift

Two separate lists. The first is what the repo doesn't do. The second is where the repo disagrees
with the conventions Mitch uses at work (Companion Systems / OnSite Companion), which are codified in
the `anthropic-skills` plugin at `code-review/references/csharp-blazor-style.md`.

Neither list is a licence to refactor. Fix an item when it is the task, or when you are already
editing that exact code and the fix is a line or two. Otherwise mention it and move on.

Last verified: 12 August 2026, against commit `8948006`.

---

## Part 1 — What the repo doesn't do

### No tests

Zero test projects. Nothing in `Home.Application` — where all the branching logic lives — has a unit
test. Interactors are cleanly testable (`ServiceFactory` is a delegate, `IPersistenceContext` is an
interface), so the barrier is starting, not design.

### No CI

`.github/workflows/` exists and is empty. Nothing builds or checks the repo on push.

### 618 compiler warnings

A clean `dotnet build` produces 618 warnings, 0 errors. Roughly:

| Code | ~Count | Cause |
|---|---|---|
| `CS1591` | 469 | Missing XML docs. `Home.WebApi` sets `GenerateDocumentationFile` for Swagger but never documents its models and has no `NoWarn` |
| `CS8618` | 115 | Non-nullable property never initialised — mostly domain entities and API models |
| `CS8603` / `CS8601` / `CS8604` | 20 | Possible null return / assignment |
| `CS1998` | 2 | `async` with no `await` |

Because of this, "no new warnings" is not currently an enforceable bar. The practical rule: don't
introduce a *new category* of warning.

### `Home.WebApi` has nullable disabled

Every other project sets `<Nullable>enable</Nullable>`. `Home.WebApi` sets `disable`. Turning it on
would surface a large number of new warnings, so it is a deliberate-looking deferral rather than an
oversight.

### Vulnerable dependencies

- **AutoMapper** — `NU1903`, high severity ([GHSA-rvv3-g6hj-g44x](https://github.com/advisories/GHSA-rvv3-g6hj-g44x)).
  Both versions in use are affected, and there are two of them: `11.0.0` in `Home.Application`,
  `13.0.1` in `Home.WebApi`.
- **npm** — 4 high-severity advisories. Tailwind is pinned to `^2.2.19` (2021).

### The build needs an undocumented step

`Home.WebUI.csproj:15` runs `npm run build:css` as a pre-build `Exec`. On a clean clone
`node_modules` doesn't exist, `tailwindcss` isn't on PATH, the command exits 1, and the **whole
solution build fails**. `cd Home.WebUI && npm install` fixes it. Nothing in the repo says so.

### `app.css` is a 4.4 MB generated file under version control

`Home.WebUI/wwwroot/css/app.css` is the unpurged Tailwind output, committed. Tailwind 2's `purge`
key only activates when `NODE_ENV=production`, which nothing sets — so every build regenerates the
full utility set and dirties the working tree.

### Configuration is undocumented

`Home.WebApi` has no `appsettings.json`. It reads `databaseConnectionString` from user secrets
(`UserSecretsId` `f6b1d435-a9e7-483c-bb25-be7f9fd4bdba`). A fresh clone cannot run the API and gets
no hint why.

### Dead code

- `Home.Domain/Entities/RecipeRegiom.cs` — typo for `RecipeRegion`, holds one property, referenced
  nowhere, not in any EF configuration or migration.
- `Home.WebUI/Components/Layout/LightsLayout.razor` — empty stub. Its one style attribute is also
  malformed: `style="border 1px black;"` is missing the colon.
- `Home.WebUI/wwwroot/app.css` — 0 bytes. The real file is `wwwroot/css/app.css`.

### The Lights feature is half-built

`Light`, `LightGroup` and `LightLocation` have entities, EF configurations and migrations. They have
no use cases, no controller and no UI. Either finish it or drop the entities — right now the schema
carries tables nothing can reach.

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
| Required parameters get `[EditorRequired]` | Never used | 0 of 76 `[Parameter]` declarations |
| Don't use cascading parameters | Core to the cancellation pattern | 6 files |
| Code-behind `.razor.cs` for non-trivial logic | Everything is inline `@code` | 0 `.razor.cs`; 27 of 30 `.razor` have `@code`; `RecipeDetailPage.razor` has a 402-line block |
| Component styles in co-located `.razor.css` | Tailwind utilities inline | 1 `.razor.css` in the whole project |
| Global CSS is theme tokens only | `input.css` also holds the icon system and component classes | `@layer components` |
| Chained calls: every call on its own line | First call stays on the source line | `_PersistenceContext.GetEntities<Recipe>()` then `.Where(...)` indented |
| Booleans set by name only, not `="true"` | Uses the explicit form | `ShowBack="true"` ×4, `Propagation="true"`, `Default="true"` |
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
