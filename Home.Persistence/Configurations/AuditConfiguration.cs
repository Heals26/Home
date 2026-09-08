using Home.Domain;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class AuditConfiguration : IEntityTypeConfiguration<Audit>
{

    #region Methods

    public void Configure(EntityTypeBuilder<Audit> entity)
    {
        _ = entity.ToTable(nameof(Audit), DomainValues.Schema);

        _ = entity.HasKey(e => e.AuditID);
        _ = entity.Property(e => e.AuditID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.Content)
            .IsRequired(false) // Optional, for change details
            .HasMaxLength(1000);

        _ = entity.Property(e => e.ModifiedDateUTC)
            .IsRequired();

        _ = entity.Property(e => e.UserName)
            .IsRequired(false)
            .HasMaxLength(152);

        _ = entity.Property(e => e.EntityID)
            .IsRequired();

        _ = entity.Property(e => e.Entity)
            .IsRequired()
            .HasConversion(
            v => v.Value,
            v => (ResourceTypeSE)v);

        // Also never configured, which left deleting a member blocked by their own audit trail.
        // The history outlives the person — UserName is already denormalised onto the row for
        // exactly that reason — so the link is cleared rather than the record destroyed.
        // WithMany() without the navigation: UserConfiguration ignores User.Audits, because an
        // audit is reached by Entity + EntityID rather than by walking a person's history.
        _ = entity.Property<long?>("UserID");
        _ = entity.HasOne(e => e.User)
            .WithMany()
            .HasConstraintName("FK_Audit_User")
            .HasForeignKey("UserID")
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        _ = entity.Ignore(e => e.Subject);

        _ = entity.Property(e => e.Summary)
            .IsRequired(false)
            .HasMaxLength(250);

        // The feed is a household's history, so every row says whose it is. Cascade: a household
        // that goes takes its history with it, which is the one deletion history should not survive.
        _ = entity.Property<long?>("HouseholdID");
        _ = entity.HasOne(e => e.Household)
            .WithMany()
            .HasConstraintName("FK_Audit_Household")
            .HasForeignKey("HouseholdID")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        _ = entity.HasIndex(e => new { e.Entity, e.EntityID })
            .HasDatabaseName("IX_Audit_Entity_EntityID");

        // The feed reads newest first within a household.
        _ = entity.HasIndex("HouseholdID", nameof(Audit.AuditID))
            .HasDatabaseName("IX_Audit_HouseholdID_AuditID");
    }

    #endregion Methods

}
