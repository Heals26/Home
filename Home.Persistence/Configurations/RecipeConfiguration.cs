using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;


public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{

    #region Methods

    public void Configure(EntityTypeBuilder<Recipe> entity)
    {
        _ = entity.ToTable(nameof(Recipe), DomainValues.Schema);

        _ = entity.HasKey(e => e.RecipeID);
        _ = entity.Property(e => e.RecipeID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.Name)
            .HasMaxLength(250)
            .IsRequired();

        _ = entity.Property(e => e.Url)
            .IsRequired();

        _ = entity.HasMany(e => e.Audits)
            .WithOne()
            .HasForeignKey(e => new { e.AuditID, e.EntityID, e.Entity })
            .HasConstraintName("FK_Recipe_Audit")
            .OnDelete(DeleteBehavior.Cascade);
    }

    #endregion Methods

}
