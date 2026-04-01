using Cards.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cards.Infrastructure.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<ReviewEntity>
{
    public void Configure(EntityTypeBuilder<ReviewEntity> builder)
    {
        builder.ToTable("reviews");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.IsCorrect)
            .IsRequired();

        builder.Property(x => x.ReviewedAt)
            .IsRequired();

        builder.HasIndex(x => x.CardId);
        builder.HasIndex(x => x.ReviewedAt);

        builder.HasOne(x => x.CardEntity)
            .WithMany(x => x.Reviews)
            .HasForeignKey(x => x.CardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}