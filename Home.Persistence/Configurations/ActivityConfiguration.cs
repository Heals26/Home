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

            // Audits

            _ = entity.Property(e => e.CompletedDateUTC)
                .IsRequired(false);

            _ = entity.Property(e => e.DueDateUTC)
                .IsRequired(false);

            _ = entity.Property(e => e.Title)
                .IsRequired(false)
                .HasMaxLength(250);
        }

        #endregion Methods

    }

}
