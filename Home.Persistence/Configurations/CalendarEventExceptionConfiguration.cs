using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class CalendarEventExceptionConfiguration : IEntityTypeConfiguration<CalendarEventException>
{

    #region Methods

    public void Configure(EntityTypeBuilder<CalendarEventException> entity)
    {
        _ = entity.ToTable(nameof(CalendarEventException), DomainValues.Schema);

        _ = entity.HasKey(e => e.CalendarEventExceptionID);
        _ = entity.Property(e => e.CalendarEventExceptionID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.OccurrenceDate);

        _ = entity.Property<long>("CalendarEventID");
        _ = entity.HasOne(e => e.CalendarEvent)
            .WithMany(e => e.Exceptions)
            .HasForeignKey("CalendarEventID")
            .HasConstraintName("FK_CalendarEventException_CalendarEvent")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        // Skipping the same occurrence twice is the same skip.
        _ = entity.HasIndex("CalendarEventID", nameof(CalendarEventException.OccurrenceDate))
            .IsUnique();
    }

    #endregion Methods

}
