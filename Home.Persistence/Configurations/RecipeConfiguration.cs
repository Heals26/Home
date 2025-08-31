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
        entity.ToTable(nameof(Recipe), DomainValues.Schema);

        entity.HasKey(e => e.RecipeID);
        entity.Property(e => e.RecipeID)
            .ValueGeneratedOnAdd();

        entity.Property(e => e.Name)
            .HasMaxLength(250)
            .IsRequired();

        entity.Property(e => e.Url)
            .IsRequired();

        _ = entity.HasMany(e => e.Audits)
            .WithOne()
            .HasForeignKey(e => new { e.AuditID, e.EntityID, e.Entity })
            .HasConstraintName("FK_Recipe_Audit")
            .OnDelete(DeleteBehavior.Cascade);
    }

    #endregion Methods

}
