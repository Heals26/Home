using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
{

    #region Methods

    public void Configure(EntityTypeBuilder<Announcement> entity)
    {
        _ = entity.ToTable(nameof(Announcement), DomainValues.Schema);

        _ = entity.HasKey(e => e.AnnouncementID);
        _ = entity.Property(e => e.AnnouncementID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.Content)
            .HasMaxLength(500)
            .IsRequired();

        _ = entity.Property(e => e.CreatedOnUTC);

        _ = entity.Property<long>("HouseholdID");
        _ = entity.HasOne(e => e.Household)
            .WithMany()
            .HasForeignKey("HouseholdID")
            .HasConstraintName("FK_Announcement_Household")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }

    #endregion Methods

}
