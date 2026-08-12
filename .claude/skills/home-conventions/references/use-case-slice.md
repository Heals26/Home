# Adding a use case, end to end

Worked from the real `GetRecipe` and `CreateRecipe` slices. Seven files, four projects. Nothing here
is optional — the pipeline resolves interactors and presenters by convention, so a misnamed or
misplaced type fails at runtime, not compile time.

## The flow

```
Controller  →  Pipeline.InvokeAsync(inputPort, presenter, serviceFactory, ct)
                   ↓  resolves IInteractor<TInputPort, TOutputPort>
              Interactor.HandleAsync(...)
                   ↓  calls one Present*Async on the output port
              Presenter (implements the output port, builds IActionResult)
                   ↓
Controller  →  return presenter.Result
```

The controller never touches the domain, never maps, never branches on the outcome. Every possible
outcome — success, not found, forbidden — is a separate method on the output port.

---

## 1. Input port — `Home.Application/UseCases/{Area}/{UseCase}/{UseCase}InputPort.cs`

A positional `record` implementing `IInputPort<TOutputPort>`. No region block, no body.

```csharp
using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Recipes.CreateRecipe;

public record CreateRecipeInputPort(string Name, string Url) : IInputPort<ICreateRecipeOutputPort>;
```

Parameters are `PascalCase` (record positional parameters become properties). IDs keep the
`ID` casing: `GetRecipeInputPort(long RecipeID)`.

## 2. Output port — `.../I{UseCase}OutputPort.cs`

An interface, `public`, one `Task Present*Async` per outcome, alphabetised, all taking a
`CancellationToken` last.

```csharp
using Home.Domain.Entities;

namespace Home.Application.UseCases.Recipes.GetRecipe;

public interface IGetRecipeOutputPort
{

    #region Methods

    Task PresentRecipeAsync(Recipe recipe, CancellationToken cancellationToken);
    Task PresentRecipeNotFoundAsync(long recipeID, CancellationToken cancellationToken);

    #endregion Methods

}
```

Output ports pass **domain entities**, not DTOs. Mapping to the wire format is the presenter's job.

## 3. Interactor — `.../{UseCase}Interactor.cs`

`internal`, implements `IInteractor<TInputPort, TOutputPort>`. Services come from the
`ServiceFactory`, not constructor injection — the interactor has no constructor.

```csharp
internal class CreateRecipeInteractor : IInteractor<CreateRecipeInputPort, ICreateRecipeOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        CreateRecipeInputPort inputPort,
        ICreateRecipeOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Recipe = new Recipe()
        {
            Household = _AuthorisationService.GetHousehold(),
            Ingredients = [],
            Name = inputPort.Name,
            Notes = [],
            Steps = [],
            Url = inputPort.Url
        };

        _PersistenceContext.Add(_Recipe);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentRecipeCreatedAsync(_Recipe.RecipeID, cancellationToken);
    }

    #endregion Methods

}
```

Points to copy:
- The four parameters each go on their own line.
- Service resolution happens first, one `var _X = serviceFactory.GetService<IX>();` per line.
- Object initialiser properties are **alphabetised**, and use explicit `new Recipe()` with parens.
- `_ = await ...SaveChangesAsync(ct)` — discard the row count.
- The method ends by awaiting exactly one `outputPort.Present*Async`. Use `if`/`else`, never an
  early `return`, so it is obvious every branch presents something.

Multi-entity reads use a projected anonymous type to control EF's eager loading, then take the root:

```csharp
var _Recipe = _PersistenceContext.GetEntities<Recipe>()
    .Where(r => r.RecipeID == inputPort.RecipeID)
    .Select(r => new
    {
        Recipe = r,
        Ingredients = r.Ingredients.Select(ri => new { RecipeIngredient = ri, ri.Ingredient }),
        r.Steps
    })
    .SingleOrDefault()
    ?.Recipe;
```

Chained calls after the source each get their own line, indented one level. The first call stays on
the source line (this differs from the work style guide — see `known-gaps.md`).

## 4. Validator (optional) — `.../{UseCase}InputPortValidator.cs`

Only when the input needs validating. FluentValidation, extends `BaseValidator<T>`, rules in the
constructor, alphabetised by property, each discarded to `_`.

```csharp
public class CreateUserInputPortValidator : BaseValidator<CreateUserInputPort>
{

    #region Constructors

    public CreateUserInputPortValidator()
    {
        _ = this.RuleFor(r => r.Email).EmailAddress().MaximumLength(500);
        _ = this.RuleFor(r => r.FirstName).NotEmpty().MaximumLength(50);
    }

    #endregion Constructors

}
```

The pipeline runs it automatically and short-circuits to a 422 via `OutputPortPresenter`. You do not
call it yourself.

## 5. API request / response — `Home.WebApi/UseCases/{Area}/{UseCase}/`

Requests are positional `record`s. Responses are `class`es with a `Properties` region and
initialised non-nullable members.

```csharp
public record CreateRecipeApiRequest(string Name, string Url);
```

```csharp
public class CreateRecipeApiResponse
{

    #region Properties

    public long RecipeID { get; set; }

    #endregion Properties

}
```

DTOs shared by several use cases in the area go in `{Area}/Models/` and are suffixed `Dto`
(`RecipeIngredientDto`).

## 6. Presenter — `Home.WebApi/Presenters/{Area}/{UseCase}/{UseCase}Presenter.cs`

Primary constructor taking `IMapper`, base `OutputPortPresenter`, output port implemented
**explicitly** (`Task IGetRecipeOutputPort.PresentRecipeAsync`) so the methods don't leak onto the
public surface. Bodies are expression-bodied on the next line.

```csharp
public class GetRecipePresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetRecipeOutputPort
{

    #region Methods

    Task IGetRecipeOutputPort.PresentRecipeAsync(Recipe recipe, CancellationToken cancellationToken)
        => this.OkAsync(new GetRecipeApiResponse()
        {
            RecipeID = recipe.RecipeID,
            Name = recipe.Name,
            Steps = [.. recipe.Steps.OrderBy(s => s.Sequence).Select(s => new RecipeStepDto() { ... })]
        }, cancellationToken);

    Task IGetRecipeOutputPort.PresentRecipeNotFoundAsync(long recipeID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Recipe {recipeID} Not Found", cancellationToken);

    #endregion Methods

}
```

Helpers on the base: `OkAsync`, `CreatedAsync(id, response, ct)`, `NoContentAsync`, `NotFoundAsync`,
`UnauthorisedAsync`. Collections are built with the spread collection expression `[.. ...]`.

Not-found messages are title case with the ID interpolated: `$"Recipe {recipeID} Not Found"`.

Registration is automatic — Scrutor scans for types whose name ends in `Presenter` and registers
them `AsSelf()` with scoped lifetime. Name it `*Presenter` or it will not resolve.

## 7. Controller action — `Home.WebApi/Controllers/{Area}Controller.cs`

Add a method to the existing controller, alphabetised among its siblings. The presenter arrives via
`[FromServices]`.

```csharp
[HttpGet("{recipeID}")]
[ProducesResponseType<GetRecipeApiResponse>(StatusCodes.Status200OK)]
public async Task<IActionResult> GetRecipe(
    [FromServices] GetRecipePresenter presenter,
    [FromRoute] long recipeID,
    CancellationToken cancellationToken)
{
    await this.Pipeline.InvokeAsync(new GetRecipeInputPort(recipeID), presenter, this.ServiceFactory, cancellationToken);

    return presenter.Result;
}
```

Fixed shape:
- Parameter order: `[FromServices] presenter`, then `[FromRoute]`, then `[FromBody]`, then
  `CancellationToken`. Every parameter on its own line, every one explicitly attributed.
- `[ProducesResponseType<T>(StatusCodes.Status200OK)]` — generic form. `204` and `201` use the
  non-generic form where there is no body.
- Blank line between the `InvokeAsync` and `return presenter.Result;`.
- The `InvokeAsync` call stays on one line however long it gets.
- Controllers carry `[Version1]`, `[Route("api/[controller]")]` and
  `[Authorize(Policy = FrameworkValues.ScopeWebApp)]` at class level and extend `BaseController`.

---

## Then the front end

If the use case is called from the UI you also need, in `Home.WebUI`:

- `DataAccess/{Area}/{UseCase}/{UseCase}WebAppRequest.cs` / `...WebAppResponse.cs` — mirrors of the
  API models. **These do carry XML doc comments**, unlike the WebApi ones.
- A method on the partial `ApiProvider` for the area
  (`Infrastructure/ApiProviders/ApiProvider.{Area}.cs`):

```csharp
public static ApiProviderHelper GetRecipe(long recipeID)
    => new(HttpMethod.Get, RouteType.Route, GetRecipeBaseUrl(recipeID));
```

Each area file has a private `#region Base` of URL builders and a public `#region Methods` of
`ApiProviderHelper` factories, both alphabetised.

Call it from a component with `this.ApiAccess.SendRequestAsync<TRequest, TResponse>(...)` — see
`blazor-ui.md`.
