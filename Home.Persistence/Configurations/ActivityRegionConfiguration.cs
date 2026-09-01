using Home.Domain;
using Home.Domain.Entities;
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

        _ = entity.Property(e => e.Sequence)
            .IsRequired();

        // NoAction rather than Cascade: the household already reaches a card's sections through
        // the activity, and SQL Server rejects a second cascade path into the same table. Deleting
        // a section the household still uses is refused, which is also what a family wants.
        _ = entity.Property<long>("CardSectionID");
        _ = entity.HasOne(e => e.CardSection)
            .WithMany(e => e.Regions)
            .HasForeignKey("CardSectionID")
            .HasConstraintName("FK_ActivityRegion_CardSection")
            .OnDelete(DeleteBehavior.NoAction)
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
