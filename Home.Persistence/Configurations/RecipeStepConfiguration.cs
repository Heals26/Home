using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class RecipeStepConfiguration : IEntityTypeConfiguration<RecipeStep>
{

    #region Methods

    public void Configure(EntityTypeBuilder<RecipeStep> entity)
    {
        entity.ToTable(nameof(RecipeStep), DomainValues.Schema);

        entity.HasKey(e => e.RecipeStepID);
        entity.Property(e => e.RecipeStepID)
            .ValueGeneratedOnAdd();

        entity.Property(e => e.Content)
            .IsRequired();

        entity.Property(e => e.Title)
            .HasMaxLength(250)
            .IsRequired();

        entity.Property(e => e.Sequence)
            .IsRequired();
    }

    #endregion Methods

}
