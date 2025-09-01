using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

internal class ApiAuditEntryConfiguration : IEntityTypeConfiguration<ApiAuditEntry>
{

    #region IEntityTypeConfiguration Implementation

    public void Configure(EntityTypeBuilder<ApiAuditEntry> entity)
    {
        _ = entity.ToTable(nameof(ApiAuditEntry), DomainValues.Schema);

        _ = entity.HasKey(e => e.ApiAuditEntryID);
        _ = entity.Property(e => e.ApiAuditEntryID)
            .HasColumnName("ApiAuditEntryID")
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.ActionName)
            .HasMaxLength(250);

        _ = entity.Property(e => e.Details)
            .HasMaxLength(4000)
            .IsRequired(false);

        _ = entity.Property(e => e.CreatedResourceID)
            .IsRequired(false);

        _ = entity.Property(e => e.RemoteIPAddress)
            .IsRequired()
            .HasMaxLength(45)
            .IsRequired(false);

        _ = entity.Property(e => e.RequestBody)
            .IsRequired(false);

        _ = entity.Property(e => e.RequestReceivedOnUTC)
            .IsRequired();

        _ = entity.Property(e => e.RequestUri)
            .IsRequired();

        _ = entity.Property(e => e.ResponseSentOnUTC)
            .IsRequired();

        _ = entity.Property(e => e.UserAgent)
            .HasMaxLength(500)
            .IsRequired(false);

        _ = entity.Property(e => e.Version)
            .HasMaxLength(50)
            .IsRequired(false);

        _ = entity.Property<long?>("UserID")
            .IsRequired(false);

        _ = entity.HasOne(e => e.User)
            .WithMany()
            .HasConstraintName("FK_ApiAuditEntry_User")
            .HasForeignKey(e => e.UserID)
            .OnDelete(DeleteBehavior.SetNull);
    }

    #endregion IEntityTypeConfiguration Implementation

}
