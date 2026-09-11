using FluentValidation;
using Home.Application.Infrastructure.Validation;

namespace Home.Application.UseCases.ShoppingCategories.UpdateShoppingCategory;

public class UpdateShoppingCategoryInputPortValidator : BaseValidator<UpdateShoppingCategoryInputPort>
{

    #region Constructors

    public UpdateShoppingCategoryInputPortValidator()
    {
        _ = this.RuleFor(r => r.Name.Value)
            .NotEmpty()
            .MaximumLength(50)
            .When(r => r.Name.HasBeenSet)
            .WithName("Name");

        _ = this.RuleFor(r => r.Sequence.Value)
            .GreaterThanOrEqualTo(0)
            .When(r => r.Sequence.HasBeenSet)
            .WithName("Sequence");
    }

    #endregion Constructors

}
