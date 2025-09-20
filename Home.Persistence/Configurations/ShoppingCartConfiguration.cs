using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class ShoppingCartConfiguration : IEntityTypeConfiguration<ShoppingCart>
{

    #region Methods

    public void Configure(EntityTypeBuilder<ShoppingCart> entity)
    {
        _ = entity.ToTable(nameof(ShoppingCart), DomainValues.Schema);

        _ = entity.HasKey(e => e.ShoppingCartID);
        _ = entity.Property(e => e.ShoppingCartID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.Name)
            .HasMaxLength(250)
            .IsRequired(false);

        _ = entity.Ignore(e => e.Audits);
    }

    #endregion Methods

}
