using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations
{

    public class ActivityContentConfiguration : IEntityTypeConfiguration<ActivityContent>
    {

        #region Methods

        public void Configure(EntityTypeBuilder<ActivityContent> entity)
        {
            _ = entity.ToTable(nameof(ActivityContent), DomainValues.Schema);
            _ = entity.Property(e => e.ActivityContentID)
                .ValueGeneratedOnAdd();

            _ = entity.Property(e => e.Content)
                .IsRequired();

            _ = entity.Property(e => e.Sequence)
                .IsRequired();

            _ = entity.Property<long>("RegionID");
            _ = entity.HasOne(e => e.Region)
                .WithMany(e => e.Fields)
                .HasForeignKey("FK_ActivityContent_ActivityRegion")
                .HasConstraintName("FK_ActivityContent_ActivityRegion")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        }

        #endregion Methods

    }

}
