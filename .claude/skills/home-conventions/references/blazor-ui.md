# Home.WebUI — Blazor conventions

Blazor Server, .NET 8, Tailwind CSS. MudBlazor was deliberately stripped out
(`Strip MudBlazor, add Tailwind, build custom component library`) — do not reintroduce a component
library. Everything is either a `Home*` component or raw Tailwind utilities.

## Component layout

`@code` blocks are **inline, at the bottom of the `.razor` file**. There are zero `.razor.cs`
code-behind files in this repo. Inside `@code`, the same region discipline as C# applies:

```
Records → Fields → Properties → Lifecycle Methods → Methods
```

```razor
@code {

    #region Fields

    private CancellationTokenHandler m_CancellationTokenHandler = new();
    private ErrorHandler? m_ErrorHandler;
    private GetRecipesWebAppResponse? m_Recipes;
    private bool m_ShowCreate;

    #endregion Fields

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
        => await this.LoadRecipesAsync();

    #endregion Lifecycle Methods

    #region Methods

    private async Task LoadRecipesAsync()
    {
        ...
    }

    #endregion Methods

}
```

Fields group by purpose rather than strict alphabetical order — infrastructure handlers first, then
loaded data, then UI flags. Methods within `#region Methods` are ordered by call sequence, not
alphabetically. This is the one place the alphabetisation rule relaxes.

Markup uses `this.` on every member: `@this.m_Recipes`, `@onclick="this.OpenCreateModal"`,
`@bind-Value="this.m_CreateRequest!.Name"`.

## Where components live

| Folder | Contents |
|---|---|
| `Components/Pages/{Area}/` | Routable pages (`@page`) and area-specific child components |
| `Components/Shared/{Kind}/` | The `Home*` library — `Buttons`, `Cards`, `Feedback`, `Inputs`, `Modals`, `Navigation` |
| `Components/Layout/` | `MainLayout`, `NavigationBarLayout` |
| `Components/Pages/Shared/ErrorHandlers/` | `ErrorHandler` |

New shared components are named `Home{Thing}` and go in the matching `Kind` folder. The existing
set: `HomeButton`, `HomeCard`, `HomeEmptyState`, `HomeLoader`, `HomeModal`, `HomeNavTile`,
`HomePasswordInput`, `HomeSegmentedControl`, `HomeSlider`, `HomeTextInput`, `HomeToggle`,
`HomeTopBar`.

Add the namespace to `Components/_Imports.razor` — that file carries every `@using` for the app, plus
the global `@attribute [Authorize]` and the two `@inject` lines.

## Dependency injection

There is no `[Inject]` in this codebase. Services are injected globally in `_Imports.razor`:

```razor
@inject IHomeHttpClient ApiAccess
@inject NavigationManager NavigationManager
```

Every component therefore has `this.ApiAccess` and `this.NavigationManager` available. A component
needing something else adds its own `@inject` at the top of that file.

## Calling the API

The pattern, in full:

```csharp
private async Task LoadRecipesAsync()
{
    var _Result = await this.ApiAccess.SendRequestAsync<object, GetRecipesWebAppResponse>(
        null!, ApiProvider.GetRecipes(),
        e => this.m_ErrorHandler?.AddError(e),
        this.m_CancellationTokenHandler.Token);

    if (_Result != null)
        this.m_Recipes = _Result;
}
```

- `SendRequestAsync<TRequest, TResponse>` — for a GET or DELETE with no body, `TRequest` is `object`
  and the first argument is `null!`.
- Second argument is always an `ApiProvider.*()` call, never a hand-built URL.
- Third is the error callback, always `e => this.m_ErrorHandler?.AddError(e)`.
- Fourth is `this.m_CancellationTokenHandler.Token`.
- A `null` result means the call failed and the error is already displayed — bail out, don't throw.

Guard re-entrancy on submit handlers with a `m_{Verb}ing` flag:

```csharp
if (this.m_Creating) return;
this.m_Creating = true;
```

## Cancellation and errors

Every page that loads data owns a `CancellationTokenHandler` (an `IDisposable` wrapping a
`CancellationTokenSource`) and cascades its token:

```razor
<CascadingValue Value="this.m_CancellationTokenHandler.Token" Name="CancellationToken">
    ...
</CascadingValue>
```

Children pick it up with
`[CascadingParameter(Name = "CancellationToken")] public CancellationToken CancellationToken { get; set; }`.

Every page ends with `<ErrorHandler @ref="this.m_ErrorHandler" />`. It renders a stack of toasts
bottom-right from `ValidationProblemDetails` returned by the API.

## Design system

Dark only. `darkMode: false` in `tailwind.config.js` — the dark palette is the default palette, there
is no light theme and no toggle.

| Token | Value |
|---|---|
| Page background | `zinc-950` `#09090b` |
| Surface | `zinc-900` |
| Raised surface / hover | `zinc-800` |
| Border | `zinc-800`, hover `zinc-700` |
| Primary text | `zinc-50` |
| Muted text | `zinc-400`, dimmer `zinc-500` |
| Accent | `teal-600` (hover `teal-500`, icons `teal-400`) |
| Danger | `red-600` (hover `red-500`) |
| Font | Inter |

Touch targets are sized for a phone: `min-h-[48px]` default, `36px` small, `56px` large.
`active:scale-95` on pressables.

## Styling rules

Styles are **Tailwind utility classes in the markup**. There is exactly one `.razor.css` file
(`MainLayout.razor.css`); CSS isolation is not the pattern here.

Anything that can't be a utility goes in `wwwroot/css/input.css` under `@layer base` or
`@layer components` (e.g. `.app-scrollable`). Never edit `wwwroot/css/app.css` — it is generated.

### Icons

No icon library. Icons are CSS masks with inline data-URI SVGs, declared once in `input.css`:

```razor
<span class="home-icon home-icon-plus h-4 w-4 inline-block"></span>
```

Colour comes from `currentColor`, so set it with a `text-*` class on the span or an ancestor. Adding
an icon means adding a `.home-icon-{name}` rule to `input.css` with both `mask-image` and
`-webkit-mask-image`. There are 16 today.

### Rebuilding the CSS

```bash
cd Home.WebUI && npm run build:css
```

This runs automatically as an MSBuild pre-build step, so a plain `dotnet build` regenerates it —
which is why `app.css` shows up as a diff constantly. See `known-gaps.md`.

## Component parameters

```csharp
[Parameter] public RenderFragment? ChildContent { get; set; }
[Parameter] public string Variant { get; set; } = "primary";
[Parameter] public bool Disabled { get; set; }
[Parameter] public EventCallback OnClick { get; set; }
[Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
```

- Attribute and property on **one line** — this differs from ordinary C# properties.
- Variants and sizes are **strings**, not enums, resolved by a `switch` expression in a
  `private string GetClasses()` method with `_Base`, `_Size`, `_Variant` locals.
- Purposeful render fragments get purposeful names (`ActionsContent`, `ActionContent`);
  `ChildContent` is for genuinely arbitrary content.
- `[EditorRequired]` is not used anywhere in this repo (see `known-gaps.md`).
