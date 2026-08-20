using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class RecipeImageConfiguration : IEntityTypeConfiguration<RecipeImage>
{

    #region Methods

    public void Configure(EntityTypeBuilder<RecipeImage> entity)
    {
        _ = entity.ToTable(nameof(RecipeImage), DomainValues.Schema);

        _ = entity.HasKey(e => e.RecipeImageID);
        _ = entity.Property(e => e.RecipeImageID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.Content)
            .IsRequired();

        _ = entity.Property(e => e.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        // One photo per recipe, dying with it. Deliberately no navigation on Recipe — a stray
        // Include there would drag image bytes through every book query.
        _ = entity.Property<long>("RecipeID");
        _ = entity.HasIndex("RecipeID").IsUnique();
        _ = entity.HasOne(e => e.Recipe)
            .WithOne()
            .HasConstraintName("FK_RecipeImage_Recipe")
            .HasForeignKey<RecipeImage>("RecipeID")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }

    #endregion Methods

}
