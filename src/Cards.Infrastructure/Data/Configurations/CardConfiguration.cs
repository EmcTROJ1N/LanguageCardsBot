using Cards.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cards.Infrastructure.Data.Configurations;

public class CardConfiguration: IEntityTypeConfiguration<CardEntity>
{
    public void Configure(EntityTypeBuilder<CardEntity> builder)
    {
        builder.ToTable("cards");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Term)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Translation)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Transcription)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Example)
            .HasColumnType("text");

        builder.Property(x => x.Level)
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(x => x.Learned)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.TotalReviews)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.CorrectReviews)
            .HasDefaultValue(0)
            .IsRequired();

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.NextReviewAt);

        builder.HasOne(x => x.UserEntity)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}