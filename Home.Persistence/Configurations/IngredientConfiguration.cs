using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class IngredientConfiguration : IEntityTypeConfiguration<Ingredient>
{

    #region Methods

    public void Configure(EntityTypeBuilder<Ingredient> entity)
    {
        _ = entity.ToTable(nameof(Ingredient), DomainValues.Schema);

        _ = entity.HasKey(e => e.IngredientID);
        _ = entity.Property(e => e.IngredientID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();

        _ = entity.Property(e => e.Quantity)
            .HasPrecision(18, 4)
            .IsRequired(false);

        _ = entity.Property(e => e.Volume)
            .HasPrecision(18, 4)
            .IsRequired(false);

        _ = entity.Property(e => e.Weight)
            .HasPrecision(18, 4)
            .IsRequired(false);
    }

    #endregion Methods

}
