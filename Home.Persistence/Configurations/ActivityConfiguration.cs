using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{

    #region Methods

    public void Configure(EntityTypeBuilder<Activity> entity)
    {
        _ = entity.ToTable(nameof(Activity), DomainValues.Schema);

        _ = entity.HasKey(e => e.ActivityID);
        _ = entity.Property(e => e.ActivityID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.CompletedDateUTC)
            .IsRequired(false);

        _ = entity.Property(e => e.DueDateUTC)
            .IsRequired(false);

        _ = entity.Property(e => e.Title)
            .HasMaxLength(250)
            .IsRequired(false);

        _ = entity.Ignore(e => e.Audits);

        _ = entity.Property<long>("HouseholdID");
        _ = entity.HasOne(e => e.Household)
            .WithMany(e => e.Activities)
            .HasForeignKey("HouseholdID")
            .HasConstraintName("FK_Activity_Household")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        _ = entity.Property<long?>("StateID");
        _ = entity.HasOne(e => e.State)
            .WithMany(e => e.Activities)
            .HasForeignKey("StateID")
            .HasConstraintName("FK_Activity_State")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        _ = entity.Property<long?>("StatusID");
        _ = entity.HasOne(e => e.Status)
            .WithMany(e => e.Activities)
            .HasForeignKey("StatusID")
            .HasConstraintName("FK_Activity_Status")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        _ = entity.Property<long?>("UserID");
        _ = entity.HasOne(e => e.User)
            .WithMany(e => e.AssignedActivities)
            .HasForeignKey("UserID")
            .HasConstraintName("FK_Activity_User")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);
    }

    #endregion Methods

}
