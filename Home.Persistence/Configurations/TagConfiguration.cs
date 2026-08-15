using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{

    #region Methods

    public void Configure(EntityTypeBuilder<Tag> entity)
    {
        _ = entity.ToTable(nameof(Tag), DomainValues.Schema);

        _ = entity.HasKey(e => e.TagID);
        _ = entity.Property(e => e.TagID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.Colour)
            .HasMaxLength(7)
            .IsRequired();

        _ = entity.Property(e => e.Name)
            .HasMaxLength(50)
            .IsRequired();

        _ = entity.Property<long>("HouseholdID");
        _ = entity.HasOne(e => e.Household)
            .WithMany(e => e.Tags)
            .HasForeignKey("HouseholdID")
            .HasConstraintName("FK_Tag_Household")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        _ = entity.HasIndex("HouseholdID", nameof(Tag.Name))
            .IsUnique();
    }

    #endregion Methods

}
