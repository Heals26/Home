---
name: home-conventions
description: The coding conventions of the Home solution (C:\Repos\Home) — a .NET 8 clean-architecture household app (recipes, shopping lists, activities) built on CleanArchitecture.Mediator with a Blazor Server + Tailwind front end. Load this BEFORE writing, editing, reviewing or reorganising any file in Home.Domain, Home.Application, Home.Persistence, Home.WebApi or Home.WebUI — including .cs, .razor and .css files. Covers naming (_PascalCase locals, m_ fields, mandatory this., ID/UTC casing), the #region layout every type follows, the vertical-slice folder structure, and the end-to-end recipe for adding a use case. Also triggers on "add a use case", "new endpoint", "new page", "does this match my conventions", "why is this file laid out like that", or any question about how Home is structured. Do NOT use for the backgammon app in Documents\Claude or for work (OnSite Companion / OSCAPI) code — those follow different rules.
---

# Home solution conventions

Conventions in this repo are **descriptive** — they are what the 483 `.cs` and 30 `.razor` files
actually do, not aspirations. New code blends in with old code. Where the repo has genuine gaps or
disagrees with Mitch's work standards, see `references/known-gaps.md` — do not silently "fix" those
while doing unrelated work.

## Solution map

Dependencies point inwards. `Home.Domain` references nothing.

| Project | Holds | Notes |
|---|---|---|
| `Home.Domain` | EF entities, enumerations, domain services | Anaemic entities: properties only, no behaviour |
| `Home.Application` | Input ports, interactors, output port interfaces, entity logic, validators | The use case layer. Interactors are `internal` |
| `Home.Persistence` | `PersistenceContext`, EF configurations, migrations | SQL Server; WebApi can also run SQLite |
| `Home.WebApi` | Controllers, presenters, API request/response models | Thin. Controllers only invoke the pipeline |
| `Home.WebUI` | Blazor Server components, DataAccess models, API providers | Tailwind, no MudBlazor |

Two solution filters exist: `API.slnf` (backend only) and `WebApp.slnf` (front end).

## The non-negotiables

These appear in essentially every file. Get them wrong and the code looks foreign.

### 1. `#region` blocks, with the name repeated on `#endregion`

Every type body opens with a blank line, is divided into named regions, and closes with a blank
line. Region order is fixed:

```
Fields → Constructors → Properties → Lifecycle Methods → Methods
```

Only include the regions you need. Blazor components add `Lifecycle Methods` for
`OnInitializedAsync` and friends.

```csharp
public class Recipe
{

    #region Properties

    public long RecipeID { get; set; }

    #endregion Properties

}
```

### 2. Members are alphabetised within their region

Strictly. `Audits`, `Household`, `Ingredients`, `Notes`, `Steps`. A commit exists purely to enforce
this (`Alphabetise methods within regions across all new files`). The exception is Blazor component
fields, which group by purpose — see `references/blazor-ui.md`.

### 3. Local variables are `_PascalCase`

```csharp
var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
var _Recipe = _PersistenceContext.GetEntities<Recipe>()...
```

Parameters stay `camelCase`. Private fields are `m_PascalCase`.

### 4. `this.` is mandatory

On every instance member access, in C# and in Razor markup:

```csharp
=> this.NavigationManager.NavigateTo($"/recipes/{recipeID}");
```
```razor
@onclick="() => this.OpenRecipe(_Recipe.RecipeID)"
```

### 5. `ID` and `UTC` are fully capitalised

`RecipeID`, `UserID`, `CreatedOnUTC`. Never `Id`, `Utc`. This extends to parameters: `long recipeID`.

### 6. Australian English

`Authorisation`, `Initialise`, `Colour`, `Licence`. Only deviate when an external API fixes the
spelling (`IAuthorizationHandler`, `[Authorize]` — framework types keep their US spelling).

### 7. Discard the return of fluent/builder calls

Assign to `_` to keep the builder chain from looking like a forgotten result:

```csharp
_ = services.AddScoped<IRecipeLogic, RecipeLogic>();
_ = this.RuleFor(r => r.Email).EmailAddress().MaximumLength(500);
_ = options.UseSqlServer(_ConnectionString, o => ...);
```

### 8. Expression bodies go on the next line

```csharp
private static string GetRecipesBaseUrl()
    => $"{GetBaseApiUrl()}/Recipes";
```

### 9. Non-nullable members get an initialiser

```csharp
public List<RecipeIngredientDto> Ingredients { get; set; } = [];
public string Name { get; set; } = string.Empty;
```

Use collection expressions (`[]`, `[.. source.Select(...)]`) rather than `new List<T>()` or
`.ToList()`.

## File-level rules

- File-scoped namespaces, always.
- Files are saved with a UTF-8 BOM.
- `using` directives are alphabetised, no blank-line grouping, no `global using` beyond
  `ImplicitUsings`.
- One type per file. The file is named after the type.
- `Nullable` is `enable` everywhere except `Home.WebApi` (see `references/known-gaps.md`).

## Folder structure — vertical slices

Every use case gets its own folder, named after the use case, in every layer that participates:

```
Home.Application/UseCases/Recipes/GetRecipe/
    GetRecipeInputPort.cs
    GetRecipeInteractor.cs
    IGetRecipeOutputPort.cs
Home.WebApi/UseCases/Recipes/GetRecipe/GetRecipeApiResponse.cs
Home.WebApi/Presenters/Recipes/GetRecipe/GetRecipePresenter.cs
Home.WebUI/DataAccess/Recipes/GetRecipe/GetRecipeWebAppResponse.cs
```

Types shared across the slices of one area live in a sibling `Models/` folder
(`Home.WebApi/UseCases/Recipes/Models/RecipeIngredientDto.cs`).

Verbs are consistent: `Create`, `Get`, `GetMany` (pluralised, e.g. `GetRecipes`), `Update`,
`Delete`. Child collections owned by a parent use `Add`/`Remove` instead
(`AddRecipeIngredient`, `RemoveRecipeNote`).

## Adding a use case

Read `references/use-case-slice.md`. It walks the seven files end to end with the exact shape of
each, using `GetRecipe` as the worked example. Do not improvise this — the pipeline wiring is
convention-driven and easy to get subtly wrong.

## Working on the Blazor front end

Read `references/blazor-ui.md`. Covers the `Home*` component library, the Tailwind
zinc/teal design system, the `home-icon` mask pattern, `CancellationTokenHandler`, `ErrorHandler`,
and the `ApiProvider` + `IHomeHttpClient` call pattern.

## Building and running

The solution **will not build from a clean clone** until npm packages are installed —
`Home.WebUI.csproj` runs `npm run build:css` as a pre-build step:

```bash
cd Home.WebUI && npm install
```

The API needs a `databaseConnectionString` user secret (`UserSecretsId`
`f6b1d435-a9e7-483c-bb25-be7f9fd4bdba`); there is no `appsettings.json` in `Home.WebApi`.

Migrations:

```bash
dotnet ef migrations add [Explanation] --project Home.Persistence --context PersistenceContext --startup-project Home.WebApi
```

## Before you finish

- Members alphabetised within regions, `#endregion` labels match.
- No new compiler warnings **of a type the file doesn't already emit** — the repo carries 618
  existing warnings, so "zero warnings" is not the bar yet. Don't add new categories.
- Don't commit `Home.WebUI/wwwroot/css/app.css` regenerations as incidental diff noise; it is a
  4.4 MB generated file (see `references/known-gaps.md`).
