using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class CalendarEventConfiguration : IEntityTypeConfiguration<CalendarEvent>
{

    #region Methods

    public void Configure(EntityTypeBuilder<CalendarEvent> entity)
    {
        _ = entity.ToTable(nameof(CalendarEvent), DomainValues.Schema);

        _ = entity.HasKey(e => e.CalendarEventID);
        _ = entity.Property(e => e.CalendarEventID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.DaysOfWeek);
        _ = entity.Property(e => e.EndDate);
        _ = entity.Property(e => e.EndTime);

        _ = entity.Property(e => e.ExternalUID)
            .HasMaxLength(500)
            .IsRequired(false);

        _ = entity.Property(e => e.Frequency);
        _ = entity.Property(e => e.Interval);
        _ = entity.Property(e => e.IsAllDay);

        _ = entity.Property(e => e.Location)
            .HasMaxLength(250)
            .IsRequired(false);

        _ = entity.Property(e => e.Notes)
            .HasMaxLength(2000)
            .IsRequired(false);

        _ = entity.Property(e => e.RepeatsOnWeekdayOfMonth);
        _ = entity.Property(e => e.RepeatUntil);
        _ = entity.Property(e => e.StartDate);
        _ = entity.Property(e => e.StartTime);

        _ = entity.Property(e => e.TimeZoneID)
            .HasMaxLength(100)
            .IsRequired(false);

        _ = entity.Property(e => e.Title)
            .HasMaxLength(250)
            .IsRequired();

        _ = entity.Property<long>("HouseholdID");
        _ = entity.HasOne(e => e.Household)
            .WithMany()
            .HasForeignKey("HouseholdID")
            .HasConstraintName("FK_CalendarEvent_Household")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        // NoAction, not cascade: the household already cascades to this table and SQL Server will
        // not accept a second path. DeleteCalendarSubscription removes the feed's rows itself.
        _ = entity.Property<long?>("SubscriptionID");
        _ = entity.HasOne(e => e.Subscription)
            .WithMany(s => s.Events)
            .HasForeignKey("SubscriptionID")
            .HasConstraintName("FK_CalendarEvent_CalendarSubscription")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        // Every calendar read is a date window, and a series is found by where it starts.
        _ = entity.HasIndex(e => e.StartDate);
    }

    #endregion Methods

}
