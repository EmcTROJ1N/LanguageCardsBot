using Cards.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cards.Infrastructure.Data.Configurations;

public class UserConfiguration: IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ChatId)
            .IsRequired();

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

        builder.HasIndex(x => x.ChatId)
            .IsUnique();
    }
}