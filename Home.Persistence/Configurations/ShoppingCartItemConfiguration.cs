using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class ShoppingCartItemConfiguration : IEntityTypeConfiguration<ShoppingCartItem>
{

    #region Methods

    public void Configure(EntityTypeBuilder<ShoppingCartItem> entity)
    {
        _ = entity.ToTable(nameof(ShoppingCartItem), DomainValues.Schema);

        _ = entity.HasKey(e => e.ShoppingCartItemID);
        _ = entity.Property(e => e.ShoppingCartItemID)
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
        _ = entity.HasOne(e => e.ShoppingCart)
            .WithMany(e => e.Items)
            .HasConstraintName("FK_ShoppingListItem_ShoppingList")
            .HasForeignKey("ShoppingListID")
            .OnDelete(DeleteBehavior.Cascade);
    }

    #endregion Methods

}
