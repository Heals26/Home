using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class LightSceneConfiguration : IEntityTypeConfiguration<LightScene>
{

    #region Methods

    public void Configure(EntityTypeBuilder<LightScene> entity)
    {
        _ = entity.ToTable(nameof(LightScene), DomainValues.Schema);

        _ = entity.HasKey(e => e.LightSceneID);
        _ = entity.Property(e => e.LightSceneID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.IsPreviousLook);

        _ = entity.Property(e => e.Name)
            .HasMaxLength(250)
            .IsRequired();

        _ = entity.Property(e => e.Sequence);

        _ = entity.Property<long>("HouseholdID");
        _ = entity.HasOne(e => e.Household)
            .WithMany(e => e.LightScenes)
            .HasConstraintName("FK_LightScene_Household")
            .HasForeignKey("HouseholdID")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }

    #endregion Methods

}
