using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class ShoppingListConfiguration : IEntityTypeConfiguration<ShoppingList>
{

    #region - - - - - - Methods - - - - - -

    public void Configure(EntityTypeBuilder<ShoppingList> entity)
    {
        _ = entity.ToTable(nameof(ShoppingList), DomainValues.Schema);

        _ = entity.HasKey(e => e.ShoppingListID);
        _ = entity.Property(e => e.ShoppingListID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.Name)
            .HasMaxLength(250)
            .IsRequired(false);
    }

    #endregion Methods

}
