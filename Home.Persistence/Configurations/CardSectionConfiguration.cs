using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class CardSectionConfiguration : IEntityTypeConfiguration<CardSection>
{

    #region Methods

    public void Configure(EntityTypeBuilder<CardSection> entity)
    {
        _ = entity.ToTable(nameof(CardSection), DomainValues.Schema);

        _ = entity.HasKey(e => e.CardSectionID);
        _ = entity.Property(e => e.CardSectionID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        _ = entity.Property(e => e.Sequence)
            .IsRequired();

        _ = entity.HasOne(e => e.Household)
            .WithMany(e => e.CardSections)
            .HasConstraintName("FK_CardSection_Household")
            .HasForeignKey("HouseholdID")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }

    #endregion Methods

}
