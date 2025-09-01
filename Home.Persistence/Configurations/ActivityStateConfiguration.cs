using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;


public class ActivityStateConfiguration : IEntityTypeConfiguration<ActivityState>
{

    #region Methods

    public void Configure(EntityTypeBuilder<ActivityState> entity)
    {
        _ = entity.ToTable(nameof(ActivityState), DomainValues.Schema);
        _ = entity.Property(e => e.ActivityStateID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();
    }

    #endregion Methods

}
