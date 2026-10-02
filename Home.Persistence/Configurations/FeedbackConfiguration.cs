using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
{

    #region Methods

    public void Configure(EntityTypeBuilder<Feedback> entity)
    {
        _ = entity.ToTable(nameof(Feedback), DomainValues.Schema);

        _ = entity.HasKey(e => e.FeedbackID);
        _ = entity.Property(e => e.FeedbackID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.Content)
            .HasMaxLength(2000)
            .IsRequired();

        _ = entity.Property(e => e.CreatedOnUTC);

        _ = entity.Property(e => e.DeviceLabel)
            .HasMaxLength(200)
            .IsRequired(false);

        _ = entity.Property<long>("HouseholdID");
        _ = entity.HasOne(e => e.Household)
            .WithMany()
            .HasForeignKey("HouseholdID")
            .HasConstraintName("FK_Feedback_Household")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        _ = entity.Property(e => e.Kind);

        _ = entity.Property(e => e.Page)
            .HasMaxLength(2000)
            .IsRequired(false);

        _ = entity.Property(e => e.ScreenHeight);
        _ = entity.Property(e => e.ScreenWidth);

        // The household already cascades here, and a second path through the member is one SQL
        // Server refuses, so DeleteUser unhooks a departing member's feedback itself.
        _ = entity.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey("UserID")
            .HasConstraintName("FK_Feedback_User")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);
    }

    #endregion Methods

}
