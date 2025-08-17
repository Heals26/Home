using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations
{

    public class ActivityConfiguration : IEntityTypeConfiguration<Activity>
    {

        #region Methods

        public void Configure(EntityTypeBuilder<Activity> entity)
        {
            _ = entity.ToTable(nameof(Activity), "home");

            _ = entity.HasKey(e => e.ActivityID);
            _ = entity.Property(e => e.ActivityID)
                .ValueGeneratedOnAdd();

            _ = entity.Property(e => e.CompletedDateUTC)
                .IsRequired(false);

            _ = entity.Property(e => e.DueDateUTC)
                .IsRequired(false);

            _ = entity.Property(e => e.Title)
                .IsRequired(false)
                .HasMaxLength(250);

            _ = entity.HasMany(e => e.Audits)
                .WithOne()
                .HasForeignKey(e => e.EntityID)
                .HasPrincipalKey(e => e.ActivityID)
                .HasConstraintName("FK_Activity_Audit")
                .OnDelete(DeleteBehavior.Cascade);

            _ = entity.Property<long>("StatusID");
            _ = entity.HasOne(e => e.Status)
                .WithOne()
                .HasForeignKey<Activity>("StatusID")
                .HasConstraintName("FK_Activity_Status")
                .OnDelete(DeleteBehavior.NoAction);

            _ = entity.Property<long>("UserID");
            _ = entity.HasOne(e => e.User)
                .WithMany(e => e.AssignedActivities)
                .HasForeignKey("UserID")
                .HasConstraintName("FK_Activity_User")
                .OnDelete(DeleteBehavior.Cascade);
        }

        #endregion Methods

    }

}
