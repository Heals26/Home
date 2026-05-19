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
        _ = entity.ToTable(nameof(RecipeStep), DomainValues.Schema);

        _ = entity.HasKey(e => e.RecipeStepID);
        _ = entity.Property(e => e.RecipeStepID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.Content)
            .IsRequired();

        _ = entity.Property(e => e.Title)
            .HasMaxLength(250)
            .IsRequired();

        _ = entity.Property(e => e.Sequence)
            .IsRequired();
    }

    #endregion Methods

}
