# Home.WebUI — Blazor conventions

Blazor Server, .NET 8, Tailwind CSS. MudBlazor was deliberately stripped out
(`Strip MudBlazor, add Tailwind, build custom component library`) — do not reintroduce a component
library. Everything is either a `Home*` component or raw Tailwind utilities.

## Component layout

Logic lives in a **`.razor.cs` code-behind partial class** beside the markup (decided 13 Aug 2026 —
inline `@code` gets no C# language-server support in most editors). The `.razor` file keeps markup
and directives (`@page`, `@inject`, `@typeparam`, `@using`); everything else goes in the partial:

```csharp
// RecipesPage.razor.cs
namespace Home.WebUI.Components.Pages.Recipes;

public partial class RecipesPage
{
    ...
}
```

Members injected via `_Imports.razor` (`ApiAccess`, `NavigationManager`, `TimeProvider`) are
generated onto the component class, so the partial can use `this.ApiAccess` without declaring
anything. Inside the partial, the same region discipline as C# applies:

```
Records → Fields → Properties → Lifecycle Methods → Methods
```

```csharp
public partial class RecipesPage
{

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

Dark by default — the app lives on an always-on kitchen tablet. A light theme was added
15 Aug 2026 as an opt-in per-device preference (Settings → Appearance: Dark / Light / Match
device); an unconfigured device still gets dark. Redesigned 13 Aug 2026: warm stone neutrals
(the `ink` scale), an editorial display face, and one hue per pillar so colour encodes *place*
in the app.

**Colours are tokens, never hexes.** The `ink` scale and pillar hues live in `input.css` as CSS
custom properties on `:root` (dark) and `:root[data-theme="light"]`, and `tailwind.config.js`
consumes them as `rgb(var(--token) / <alpha-value>)` — the alpha form matters, because opacity
modifiers like `bg-week/10` are used everywhere and a plain `var()` breaks them. A new colour
goes in as a token *pair*, never as a hex in the config, in markup, or in a `.razor.css`. The
values below are the dark theme; each has a light counterpart darkened enough to clear 4.5:1 on
paper (sky at `#7dd3fc` is about 1.4:1 on white, so the hues could not simply be reused).

| Token | Value |
|---|---|
| Page background | `ink-950` `#0c0a09` |
| Surface | `ink-900`, raised `ink-800` |
| Border | `ink-800`, hover `ink-700` |
| Primary text | `ink-50`, muted `ink-400`, dimmer `ink-500` |
| Primary button | light-on-dark: `bg-ink-50 text-ink-950` |
| Pillar hues | `recipes` apricot `#fb923c` · `shopping` sage `#a3b18a` · `week` sky `#7dd3fc` · `lights` amber `#fbbf24` · `household` neutral |
| Danger | `red-600` (hover `red-500`) |
| Display font | Fraunces (`font-display`) — page titles, greetings, modal titles |
| Body font | Inter (`font-sans`) |

Pillar hues are for identity only — nav active states, eyebrows, icons, count pills — never for
large surfaces. Page headers use `HomeTopBar` with `Eyebrow`/`EyebrowClass` naming the pillar.
Persistent navigation is `HomeNavRail` (left rail on `md:`, bottom bar below), so pages don't
need back-to-home buttons; `ShowBack` is only for drill-in pages like a recipe's detail.

Touch targets are sized for fingers: `min-h-[48px]` default, `36px` small, `56px` large.
`active:scale-95` on pressables.

## Styling rules

Styles are **Tailwind utility classes in the markup**. There is exactly one `.razor.css` file
(`MainLayout.razor.css`); CSS isolation is not the pattern here.

Anything that can't be a utility goes in `wwwroot/css/input.css` under `@layer base` or
`@layer components` (e.g. `.app-scrollable`). Never edit `wwwroot/css/app.css`, it is generated.

### Breakpoints: pick the one that matches the question

There are two kinds and they are not interchangeable.

`rail:` is `(min-width: 768px) and (orientation: landscape)`. Use it for **where navigation lives**,
and nothing else. It is a question about reach: a tall device is held in two hands, the top-left
corner is out of thumb range, so the nav becomes a bottom bar however wide the screen is.

Plain width breakpoints (`sm:`, `lg:`) are for **whether content fits**. A title and three buttons
sharing a line, two panes sharing a screen, a grid changing column count. None of that depends on
how the device is held.

The case that separates them is a tablet held upright: `rail:` is false so it wants the bottom bar,
but 768px is plenty of width so it wants the wide content layout. Reaching for `rail:` on a fitting
question gives that device a phone layout on a screen with room to spare.

### Full-height layouts

Use `.app-viewport`, not `h-screen`, for anything sized to the whole window. `100vh` on iOS Safari
means the viewport with the browser toolbars hidden, which is taller than what is on screen, so a
fixed bottom bar ends up below the fold. `.app-viewport` is `100dvh` with a `100vh` fallback, in
that order, which is why it is a rule in `input.css` rather than two classes in the markup.

`min-h-screen` is fine and is still used on the auth pages: a floor is not a bottom edge.

### Icons

No icon library. Icons are CSS masks with inline data-URI SVGs, declared once in `input.css`:

```razor
<span class="home-icon home-icon-plus h-4 w-4 inline-block"></span>
```

Colour comes from `currentColor`, so set it with a `text-*` class on the span or an ancestor. Adding
an icon means adding a `.home-icon-{name}` rule to `input.css` with both `mask-image` and
`-webkit-mask-image`. There are 43 today, counted 5 Sep 2026.

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
