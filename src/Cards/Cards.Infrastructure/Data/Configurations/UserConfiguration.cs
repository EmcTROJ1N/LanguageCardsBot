using Cards.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cards.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core entity type configuration for <see cref="UserEntity"/>.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.KeycloakId)
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(x => x.ChatId)
            .IsRequired(false);

        builder.Property(x => x.Username)
            .HasMaxLength(255);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.ReminderIntervalMinutes)
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(x => x.HideTranslations)
            .HasDefaultValue(true)
            .IsRequired();

        // Sparse unique index: MySQL allows multiple NULLs in a unique index
        builder.HasIndex(x => x.KeycloakId)
            .IsUnique()
            .HasFilter("`KeycloakId` IS NOT NULL");

        builder.HasIndex(x => x.ChatId)
            .IsUnique()
            .HasFilter("`ChatId` IS NOT NULL");
    }
}
