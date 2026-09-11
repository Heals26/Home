namespace Home.Application.UseCases.ShoppingCategories.DeleteShoppingCategory;

public interface IDeleteShoppingCategoryOutputPort
{

    #region Methods

    Task PresentShoppingCategoryDeletedAsync(CancellationToken cancellationToken);
    Task PresentShoppingCategoryNotFoundAsync(long shoppingCategoryID, CancellationToken cancellationToken);

    #endregion Methods

}
