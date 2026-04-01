using Cards.Domain.Entities;
using Cards.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Cards.Infrastructure.Data;

public class CardsMysqlDbContext(DbContextOptions<CardsMysqlDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<CardEntity> Cards => Set<CardEntity>();
    public DbSet<ReviewEntity> Reviews => Set<ReviewEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new CardConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new ReviewConfiguration());
    }
}
