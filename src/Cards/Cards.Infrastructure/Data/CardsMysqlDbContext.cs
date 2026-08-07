using Cards.Domain.Entities;
using Cards.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Cards.Infrastructure.Data;

/// <summary>
/// EF Core database context for the Cards MySQL schema, exposing users, cards, and reviews.
/// </summary>
public class CardsMysqlDbContext(DbContextOptions<CardsMysqlDbContext> options) : DbContext(options)
{
    /// <summary>Gets the users table.</summary>
    public DbSet<UserEntity> Users => Set<UserEntity>();
    /// <summary>Gets the cards table.</summary>
    public DbSet<CardEntity> Cards => Set<CardEntity>();
    /// <summary>Gets the reviews table.</summary>
    public DbSet<ReviewEntity> Reviews => Set<ReviewEntity>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new CardConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new ReviewConfiguration());
    }
}
