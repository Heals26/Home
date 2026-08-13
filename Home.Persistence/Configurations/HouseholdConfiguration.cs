using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class HouseholdConfiguration : IEntityTypeConfiguration<Household>
{

    #region Methods

    public void Configure(EntityTypeBuilder<Household> entity)
    {
        _ = entity.ToTable(nameof(Household), DomainValues.Schema);

        _ = entity.HasKey(e => e.HouseholdID);
        _ = entity.Property(e => e.HouseholdID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.LifxApiToken)
            .HasMaxLength(500);

        _ = entity.Property(e => e.Name)
            .HasMaxLength(250)
            .IsRequired();
    }

    #endregion Methods

}
