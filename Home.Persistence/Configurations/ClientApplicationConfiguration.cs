using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class ClientApplicationConfiguration : IEntityTypeConfiguration<ClientApplication>
{

    #region Methods

    public void Configure(EntityTypeBuilder<ClientApplication> entity)
    {
        _ = entity.ToTable(nameof(ClientApplication), DomainValues.Schema);

        _ = entity.HasKey(e => e.ClientApplicationID);
        _ = entity.Property(e => e.ClientApplicationID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.AccessToken)
            .HasMaxLength(500)
            .IsRequired();

        _ = entity.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        _ = entity.Property(e => e.Secret)
            .HasMaxLength(500)
            .IsRequired();
    }

    #endregion Methods

}
