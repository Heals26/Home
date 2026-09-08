using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class CalendarEventMemberConfiguration : IEntityTypeConfiguration<CalendarEventMember>
{

    #region Methods

    public void Configure(EntityTypeBuilder<CalendarEventMember> entity)
    {
        _ = entity.ToTable(nameof(CalendarEventMember), DomainValues.Schema);

        _ = entity.HasKey(e => new { e.CalendarEventID, e.UserID });

        _ = entity.Property(e => e.CalendarEventID);
        _ = entity.HasOne(e => e.CalendarEvent)
            .WithMany(e => e.Members)
            .HasForeignKey(e => e.CalendarEventID)
            .HasConstraintName("FK_CalendarEventMember_CalendarEvent")
            .OnDelete(DeleteBehavior.Cascade);

        // NoAction: the household already cascades here through the event, and a second path is
        // rejected. DeleteUser clears a member's rows itself, the same trade ActivityTag makes.
        _ = entity.Property(e => e.UserID);
        _ = entity.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserID)
            .HasConstraintName("FK_CalendarEventMember_User")
            .OnDelete(DeleteBehavior.NoAction);
    }

    #endregion Methods

}
