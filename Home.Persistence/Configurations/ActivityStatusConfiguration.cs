using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;


public class ActivityStatusConfiguration : IEntityTypeConfiguration<ActivityStatus>
{

    #region Methods

    public void Configure(EntityTypeBuilder<ActivityStatus> entity)
    {
        _ = entity.ToTable(nameof(ActivityStatus), DomainValues.Schema);
        _ = entity.Property(e => e.ActivityStatusID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();
    }

    #endregion Methods

}
