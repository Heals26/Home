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

        _ = entity.Property<long>("LightGroupID");
        _ = entity.HasOne(e => e.Group)
            .WithMany(e => e.Lights)
            .HasConstraintName("FK_Light_Group")
            .HasForeignKey("LightGroupID")
            .OnDelete(DeleteBehavior.Cascade);
    }

    #endregion Properties

}
