using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class LightGroupConfiguration : IEntityTypeConfiguration<LightGroup>
{

    #region Methods

    public void Configure(EntityTypeBuilder<LightGroup> entity)
    {
        _ = entity.HasKey(e => e.LightGroupID);
        _ = entity.Property(e => e.LightGroupID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.ID)
            .HasMaxLength(100)
            .IsRequired();

        _ = entity.Property(e => e.Name)
            .HasMaxLength(250)
            .IsRequired();

        _ = entity.Property<long>("LightLocationID");
        _ = entity.HasOne(e => e.Location)
            .WithMany(e => e.Groups)
            .HasConstraintName("FK_LightGroup_LightLocation")
            .HasForeignKey("LightLocationID")
            .OnDelete(DeleteBehavior.Cascade);
    }

    #endregion Methods

}
