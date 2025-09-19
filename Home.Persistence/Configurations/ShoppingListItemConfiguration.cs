using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class ShoppingListItemConfiguration : IEntityTypeConfiguration<ShoppingListItem>
{

    #region - - - - - - Methods - - - - - -

    public void Configure(EntityTypeBuilder<ShoppingListItem> entity)
    {
        _ = entity.ToTable(nameof(ShoppingListItem), DomainValues.Schema);

        _ = entity.HasKey(e => e.ShoppingListItemID);
        _ = entity.Property(e => e.ShoppingListItemID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.Cost)
            .HasDefaultValue(0)
            .IsRequired();

        _ = entity.Property(e => e.InBasket)
            .HasDefaultValue(false)
            .IsRequired();

        _ = entity.Property(e => e.Quantity)
            .HasDefaultValue(1)
            .IsRequired();

        _ = entity.Property(e => e.Sequence)
            .HasDefaultValue(0)
            .IsRequired();

        _ = entity.Property<long>("ShoppingListID");
        _ = entity.HasOne(e => e.ShoppingList)
            .WithMany(e => e.Items)
            .HasConstraintName("FK_ShoppingListItem_ShoppingList")
            .HasForeignKey("ShoppingListID")
            .OnDelete(DeleteBehavior.Cascade);
    }

    #endregion Methods

}
