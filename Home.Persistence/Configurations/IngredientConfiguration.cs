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
        entity.ToTable(nameof(Ingredient), DomainValues.Schema);

        entity.HasKey(e => e.IngredientID);
        entity.Property(e => e.IngredientID)
            .ValueGeneratedOnAdd();

        entity.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(e => e.Quantity)
            .IsRequired(false);

        entity.Property(e => e.Volumne)
            .IsRequired(false);

        entity.Property(e => e.Weight)
            .IsRequired(false);
    }

    #endregion Methods

}
