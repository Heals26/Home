using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class UserAuthentication : IEntityTypeConfiguration<Domain.Entities.UserAuthentication>
{

    #region Methods

    public void Configure(EntityTypeBuilder<Domain.Entities.UserAuthentication> entity)
    {
        _ = entity.ToTable(nameof(Domain.Entities.UserAuthentication), DomainValues.Schema);

        _ = entity.Property(e => e.AuthenticationMetadataID)
            .ValueGeneratedOnAdd();
        _ = entity.HasKey(e => e.AuthenticationMetadataID);

        _ = entity.Property(e => e.AccessToken)
            .IsRequired();

        _ = entity.Property(e => e.DateSetUTC)
            .IsRequired();

        _ = entity.Property(e => e.DeviceLabel)
            .HasMaxLength(200)
            .IsRequired(false);

        _ = entity.Property(e => e.ExpiresOnUTC);
        _ = entity.Property(e => e.LastUsedOnUTC)
            .IsRequired(false);

        // Deliberately a bare column, not a foreign key — a self-reference here would close a
        // cycle. It is rotation metadata, read only to detect a replayed refresh token.
        _ = entity.Property(e => e.SupersededByAuthenticationMetadataID)
            .IsRequired(false);

        _ = entity.Property(e => e.SupersededOnUTC)
            .IsRequired(false);

        // Every refresh looks a token up by its value, and the pruning sweep reads expiry.
        _ = entity.HasIndex(e => e.RefreshToken);
        _ = entity.HasIndex(e => e.ExpiresOnUTC);

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
