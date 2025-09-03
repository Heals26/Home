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
            v => BaseEnumeration.FromValue<ResourceTypeSE>(v));

        _ = entity.HasIndex(e => new { e.Entity, e.EntityID })
            .HasDatabaseName("IX_Audit_Entity_EntityID");
    }

    #endregion Methods

}
