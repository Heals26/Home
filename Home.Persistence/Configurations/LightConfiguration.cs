using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class LightConfiguration : IEntityTypeConfiguration<Light>
{

    #region Properties

    public void Configure(EntityTypeBuilder<Light> entity)
    {
        _ = entity.ToTable(nameof(Light), DomainValues.Schema);

        _ = entity.HasKey(e => e.LightID);
        _ = entity.Property(e => e.LightID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.ID)
            .HasMaxLength(250)
            .IsRequired();

        _ = entity.Property(e => e.Name)
            .HasMaxLength(250)
            .IsRequired();

        _ = entity.Property(e => e.Brightness);
        _ = entity.Property(e => e.Hue);
        _ = entity.Property(e => e.IsConnected);
        _ = entity.Property(e => e.IsOn);
        _ = entity.Property(e => e.Kelvin);
        _ = entity.Property(e => e.Saturation);
        _ = entity.Property(e => e.StateUpdatedUTC);

        _ = entity.Property<long>("LightGroupID");
        _ = entity.HasOne(e => e.Group)
            .WithMany(e => e.Lights)
            .HasConstraintName("FK_Light_Group")
            .HasForeignKey("LightGroupID")
            .OnDelete(DeleteBehavior.Cascade);

        // Sync matches on the provider's device ID, so looking a light up by it is the hot path.
        _ = entity.HasIndex(e => e.ID);
    }

    #endregion Properties

}
