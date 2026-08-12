using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class LightSceneStateConfiguration : IEntityTypeConfiguration<LightSceneState>
{

    #region Methods

    public void Configure(EntityTypeBuilder<LightSceneState> entity)
    {
        _ = entity.ToTable(nameof(LightSceneState), DomainValues.Schema);

        _ = entity.HasKey(e => e.LightSceneStateID);
        _ = entity.Property(e => e.LightSceneStateID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.Brightness);
        _ = entity.Property(e => e.Hue);
        _ = entity.Property(e => e.IsOn);
        _ = entity.Property(e => e.Kelvin);
        _ = entity.Property(e => e.Saturation);

        _ = entity.Property<long>("LightSceneID");
        _ = entity.HasOne(e => e.Scene)
            .WithMany(e => e.States)
            .HasConstraintName("FK_LightSceneState_Scene")
            .HasForeignKey("LightSceneID")
            .OnDelete(DeleteBehavior.Cascade);

        // Deliberately NOT cascading. Household reaches this table two ways — through LightScene,
        // and through LightLocation → LightGroup → Light — and SQL Server rejects multiple cascade
        // paths to one table. The scene side keeps the cascade because it is the natural owner;
        // removing a bulb clears its scene entries explicitly in SyncLights instead.
        _ = entity.Property<long>("LightID");
        _ = entity.HasOne(e => e.Light)
            .WithMany()
            .HasConstraintName("FK_LightSceneState_Light")
            .HasForeignKey("LightID")
            .OnDelete(DeleteBehavior.NoAction);
    }

    #endregion Methods

}
