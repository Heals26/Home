using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class ActivityTagConfiguration : IEntityTypeConfiguration<ActivityTag>
{

    #region Methods

    public void Configure(EntityTypeBuilder<ActivityTag> entity)
    {
        _ = entity.ToTable(nameof(ActivityTag), DomainValues.Schema);

        _ = entity.HasKey(e => new { e.ActivityID, e.TagID });

        _ = entity.Property(e => e.ActivityID);
        _ = entity.HasOne(e => e.Activity)
            .WithMany(e => e.Tags)
            .HasForeignKey(e => e.ActivityID)
            .HasConstraintName("FK_ActivityTag_Activity")
            .OnDelete(DeleteBehavior.Cascade);

        // NoAction: the activity already cascades from the household. DeleteTag clears these.
        _ = entity.Property(e => e.TagID);
        _ = entity.HasOne(e => e.Tag)
            .WithMany(e => e.Activities)
            .HasForeignKey(e => e.TagID)
            .HasConstraintName("FK_ActivityTag_Tag")
            .OnDelete(DeleteBehavior.NoAction);
    }

    #endregion Methods

}
