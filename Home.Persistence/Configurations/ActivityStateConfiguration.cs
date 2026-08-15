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

        _ = entity.Property(e => e.IsComplete);
        _ = entity.Property(e => e.Sequence);

        _ = entity.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        // Cascades from the household, which is safe only because FK_Activity_State is
        // NoAction — making that one cascade too would give SQL Server two paths and the
        // migration would be rejected.
        _ = entity.Property<long>("HouseholdID");
        _ = entity.HasOne(e => e.Household)
            .WithMany(e => e.ActivityStates)
            .HasForeignKey("HouseholdID")
            .HasConstraintName("FK_ActivityState_Household")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }

    #endregion Methods

}
