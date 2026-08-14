using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class LightScheduleConfiguration : IEntityTypeConfiguration<LightSchedule>
{

    #region Methods

    public void Configure(EntityTypeBuilder<LightSchedule> entity)
    {
        _ = entity.ToTable(nameof(LightSchedule), DomainValues.Schema);

        _ = entity.HasKey(e => e.LightScheduleID);
        _ = entity.Property(e => e.LightScheduleID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.DaysOfWeek);
        _ = entity.Property(e => e.IsEnabled);
        _ = entity.Property(e => e.LastRunUTC);
        _ = entity.Property(e => e.OffsetMinutes);
        _ = entity.Property(e => e.TimeOfDay);
        _ = entity.Property(e => e.Trigger);

        _ = entity.Property(e => e.Name)
            .HasMaxLength(250)
            .IsRequired();

        // The scene is the only owner. Deleting it takes its schedules with it — a schedule
        // pointing at nothing would sit there looking active and silently do nothing every night.
        // Household is reached through the scene, which keeps this to one cascade path.
        _ = entity.Property<long>("LightSceneID");
        _ = entity.HasOne(e => e.Scene)
            .WithMany()
            .HasConstraintName("FK_LightSchedule_Scene")
            .HasForeignKey("LightSceneID")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        // The runner sweeps enabled schedules every minute; this keeps that sweep cheap.
        _ = entity.HasIndex(e => e.IsEnabled);
    }

    #endregion Methods

}
