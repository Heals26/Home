using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class LightGroupConfiguration : IEntityTypeConfiguration<LightGroup>
{

    #region Methods

    public void Configure(EntityTypeBuilder<LightGroup> entity)
    {
        _ = entity.ToTable(nameof(LightGroup), DomainValues.Schema);

        _ = entity.HasKey(e => e.LightGroupID);
        _ = entity.Property(e => e.LightGroupID)
            .ValueGeneratedOnAdd();

        // Null for groups created in Home rather than seeded from the provider.
        _ = entity.Property(e => e.ID)
            .HasMaxLength(100);

        _ = entity.Property(e => e.Name)
            .HasMaxLength(250)
            .IsRequired();

        _ = entity.Property(e => e.Sequence);

        _ = entity.Property<long>("LightLocationID");
        _ = entity.HasOne(e => e.Location)
            .WithMany(e => e.Groups)
            .HasConstraintName("FK_LightGroup_Location")
            .HasForeignKey("LightLocationID")
            .OnDelete(DeleteBehavior.Cascade);
    }

    #endregion Methods

}
