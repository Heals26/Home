using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class LightLocationConfiguration : IEntityTypeConfiguration<LightLocation>
{

    #region Methods

    public void Configure(EntityTypeBuilder<LightLocation> entity)
    {
        _ = entity.ToTable(nameof(LightLocation), DomainValues.Schema);

        _ = entity.HasKey(e => e.LightLocationID);
        _ = entity.Property(e => e.LightLocationID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.ID)
            .HasMaxLength(100)
            .IsRequired();

        _ = entity.Property(e => e.Name)
            .HasMaxLength(250)
            .IsRequired();

        _ = entity.Property<long>("HouseholdID");
        _ = entity.HasOne(e => e.Household)
            .WithMany(e => e.LightLocations)
            .HasConstraintName("FK_LightLocation_Household")
            .HasForeignKey("HouseholdID")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }

    #endregion Methods

}
