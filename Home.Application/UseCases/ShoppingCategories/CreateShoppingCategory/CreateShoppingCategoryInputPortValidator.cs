using FluentValidation;
using Home.Application.Infrastructure.Validation;

namespace Home.Application.UseCases.ShoppingCategories.CreateShoppingCategory;

public class CreateShoppingCategoryInputPortValidator : BaseValidator<CreateShoppingCategoryInputPort>
{

    #region Constructors

    public CreateShoppingCategoryInputPortValidator()
    {
        _ = this.RuleFor(r => r.Name)
            .NotEmpty()
            .MaximumLength(50);
    }

    #endregion Constructors

}
