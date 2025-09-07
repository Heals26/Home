using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class AuthenticationMetdataConfiguration : IEntityTypeConfiguration<AuthenticationMetadata>
{

    #region Methods

    public void Configure(EntityTypeBuilder<AuthenticationMetadata> entity)
    {
        _ = entity.ToTable(nameof(AuthenticationMetadata), DomainValues.Schema);

        _ = entity.Property(e => e.AuthenticationMetadataID)
            .ValueGeneratedOnAdd();
        _ = entity.HasKey(e => e.AuthenticationMetadataID);

        _ = entity.Property(e => e.AccessToken)
            .IsRequired();

        _ = entity.Property(e => e.DateSetUTC)
            .IsRequired();

        _ = entity.Property(e => e.RefreshToken)
            .IsRequired();

        _ = entity.Property(e => e.Scopes)
            .IsRequired();

        _ = entity.Property<long?>("ClientApplictionID")
            .IsRequired(false);
        _ = entity.HasOne(e => e.ClientApplication)
            .WithMany()
            .HasForeignKey("ClientApplicationID")
            .HasConstraintName("FK_AuthenticationMetadata_ClientApplication")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        _ = entity.Property<long?>("UserID")
            .IsRequired(false);
        _ = entity.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey("UserID")
            .HasConstraintName("FK_AuthenticationMetadata_User")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }

    #endregion Methods

}
