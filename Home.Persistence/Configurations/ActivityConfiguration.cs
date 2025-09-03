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

        _ = entity.HasMany(e => e.Audits)
            .WithOne()
            .HasForeignKey(e => e.EntityID)
            .HasConstraintName("FK_Activity_Audit")
            .OnDelete(DeleteBehavior.ClientCascade);

        _ = entity.Property<long>("StateID");
        _ = entity.HasOne(e => e.State)
            .WithOne()
            .HasForeignKey<Activity>("StateID")
            .HasConstraintName("FK_Activity_State")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        _ = entity.Property<long>("StatusID");
        _ = entity.HasOne(e => e.Status)
            .WithOne()
            .HasForeignKey<Activity>("StatusID")
            .HasConstraintName("FK_Activity_Status")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        _ = entity.Property<long>("UserID");
        _ = entity.HasOne(e => e.User)
            .WithMany(e => e.AssignedActivities)
            .HasForeignKey("UserID")
            .HasConstraintName("FK_Activity_User")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
    }

    #endregion Methods

}
