using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{

    #region Methods

    public void Configure(EntityTypeBuilder<User> entity)
    {
        _ = entity.ToTable(nameof(User), DomainValues.Schema);

        _ = entity.HasKey(e => e.UserID);
        _ = entity.Property(e => e.UserID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.Email)
            .HasMaxLength(500)
            .IsRequired();

        _ = entity.Property(e => e.FirstName)
            .HasMaxLength(50)
            .IsRequired();

        _ = entity.Property(e => e.LastName)
            .HasMaxLength(50)
            .IsRequired();

        _ = entity.Property(e => e.MiddleNames)
            .HasMaxLength(50)
            .IsRequired(false);

        _ = entity.Property(e => e.Password)
            .HasMaxLength(100)
            .IsRequired();

        _ = entity.Property(e => e.PasswordLastChanged)
            .IsRequired();

        _ = entity.Ignore(e => e.Audits);
    }

    #endregion Methods

}
