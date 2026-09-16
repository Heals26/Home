using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class ShoppingTripConfiguration : IEntityTypeConfiguration<ShoppingTrip>
{

    #region Methods

    public void Configure(EntityTypeBuilder<ShoppingTrip> entity)
    {
        _ = entity.ToTable(nameof(ShoppingTrip), DomainValues.Schema);

        _ = entity.HasKey(e => e.ShoppingTripID);
        _ = entity.Property(e => e.ShoppingTripID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.EndedOnUTC)
            .IsRequired(false);

        _ = entity.Property(e => e.LastActivityOnUTC)
            .IsRequired();

        _ = entity.Property(e => e.StartedOnUTC)
            .IsRequired();

        _ = entity.Property<long>("ShoppingListID");
        _ = entity.HasOne(e => e.ShoppingList)
            .WithMany(e => e.Trips)
            .HasForeignKey("ShoppingListID")
            .HasConstraintName("FK_ShoppingTrip_ShoppingList")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        _ = entity.HasIndex("ShoppingListID", nameof(ShoppingTrip.EndedOnUTC));
    }

    #endregion Methods

}
