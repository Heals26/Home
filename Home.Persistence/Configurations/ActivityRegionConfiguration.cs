using Home.Domain;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class ActivityRegionConfiguration : IEntityTypeConfiguration<ActivityRegion>
{

    #region Methods

    public void Configure(EntityTypeBuilder<ActivityRegion> entity)
    {
        _ = entity.ToTable(nameof(ActivityRegion), DomainValues.Schema);
        _ = entity.Property(e => e.ActivityRegionID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.Region)
            .IsRequired()
            .HasConversion(
                v => v.Value,
                v => BaseEnumeration.FromValue<RegionSE>(v));

        _ = entity.Property(e => e.Sequence)
            .IsRequired();

        _ = entity.Property<long>("ActivityID");
        _ = entity.HasOne(e => e.Activity)
            .WithMany(e => e.Regions)
            .HasForeignKey("ActivityID")
            .HasConstraintName("FK_ActivityRegion_Activity")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }

    #endregion Methods

}
