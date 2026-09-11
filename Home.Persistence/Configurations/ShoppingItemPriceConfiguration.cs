using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class ShoppingItemPriceConfiguration : IEntityTypeConfiguration<ShoppingItemPrice>
{

    #region Methods

    public void Configure(EntityTypeBuilder<ShoppingItemPrice> entity)
    {
        _ = entity.ToTable(nameof(ShoppingItemPrice), DomainValues.Schema);

        _ = entity.HasKey(e => e.ShoppingItemPriceID);
        _ = entity.Property(e => e.ShoppingItemPriceID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.Amount)
            .HasPrecision(18, 4)
            .IsRequired(false);

        _ = entity.Property(e => e.BoughtOnUTC)
            .IsRequired();

        _ = entity.Property(e => e.Cost)
            .HasPrecision(18, 4)
            .IsRequired();

        _ = entity.Property(e => e.ShoppingListItemID)
            .IsRequired();

        _ = entity.Property(e => e.Unit)
            .IsRequired(false);

        _ = entity.Property<long>("ShoppingItemMemoryID");
        _ = entity.HasOne(e => e.Memory)
            .WithMany(e => e.Prices)
            .HasForeignKey("ShoppingItemMemoryID")
            .HasConstraintName("FK_ShoppingItemPrice_ShoppingItemMemory")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        _ = entity.HasIndex("ShoppingItemMemoryID", nameof(ShoppingItemPrice.BoughtOnUTC));
    }

    #endregion Methods

}
