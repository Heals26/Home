using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class CalendarSubscriptionConfiguration : IEntityTypeConfiguration<CalendarSubscription>
{

    #region Methods

    public void Configure(EntityTypeBuilder<CalendarSubscription> entity)
    {
        _ = entity.ToTable(nameof(CalendarSubscription), DomainValues.Schema);

        _ = entity.HasKey(e => e.CalendarSubscriptionID);
        _ = entity.Property(e => e.CalendarSubscriptionID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.LastError)
            .HasMaxLength(500)
            .IsRequired(false);

        _ = entity.Property(e => e.LastFetchedUTC);

        _ = entity.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        _ = entity.Property(e => e.Url)
            .HasMaxLength(2000)
            .IsRequired();

        _ = entity.Property<long>("HouseholdID");
        _ = entity.HasOne(e => e.Household)
            .WithMany()
            .HasForeignKey("HouseholdID")
            .HasConstraintName("FK_CalendarSubscription_Household")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }

    #endregion Methods

}
