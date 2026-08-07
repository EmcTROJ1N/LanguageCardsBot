using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Cards.Infrastructure.Data;

/// <summary>
/// EF Core design-time factory used by the <c>dotnet ef</c> CLI (migrations, database update) to build
/// a <see cref="CardsMysqlDbContext"/> from local appsettings JSON files.
/// </summary>
public sealed class CardsMysqlDbContextFactory
    : IDesignTimeDbContextFactory<CardsMysqlDbContext>
{
    /// <inheritdoc />
    public CardsMysqlDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("CardsMysql")
                               ?? throw new InvalidOperationException(
                                   "Connection string 'CardsMysql' is not configured.");

        var optionsBuilder = new DbContextOptionsBuilder<CardsMysqlDbContext>();

        optionsBuilder.UseMySql(
            connectionString,
            ServerVersion.AutoDetect(connectionString));

        return new CardsMysqlDbContext(optionsBuilder.Options);
    }
}
