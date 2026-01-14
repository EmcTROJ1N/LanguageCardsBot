using Dapper;
using EnglishCardsBot.Application.Interfaces;
using EnglishCardsBot.Domain.Entities;
using EnglishCardsBot.Infrastructure.Data;

namespace EnglishCardsBot.Infrastructure.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly ApplicationDbContext _context;

    public ReviewRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Review?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM reviews WHERE id = @Id";
        return await connection.QueryFirstOrDefaultAsync<Review>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Review>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM reviews";
        return await connection.QueryAsync<Review>(sql);
    }

    public async Task<Review> AddAsync(Review entity, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var sql = @"
            INSERT INTO reviews (card_id, is_correct, reviewed_at)
            VALUES (@CardId, @IsCorrect, @ReviewedAt);
            SELECT last_insert_rowid();";
        
        var id = await connection.QuerySingleAsync<int>(sql, new
        {
            entity.CardId,
            IsCorrect = entity.IsCorrect ? 1 : 0,
            ReviewedAt = entity.ReviewedAt.ToString("O")
        });
        
        entity.Id = id;
        return entity;
    }

    public async Task UpdateAsync(Review entity, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var sql = @"
            UPDATE reviews 
            SET card_id = @CardId, is_correct = @IsCorrect, reviewed_at = @ReviewedAt
            WHERE id = @Id";
        
        await connection.ExecuteAsync(sql, new
        {
            entity.Id,
            entity.CardId,
            IsCorrect = entity.IsCorrect ? 1 : 0,
            ReviewedAt = entity.ReviewedAt.ToString("O")
        });
    }

    public async Task DeleteAsync(Review entity, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var sql = "DELETE FROM reviews WHERE id = @Id";
        await connection.ExecuteAsync(sql, new { entity.Id });
    }

    public async Task<IEnumerable<Review>> GetTodayReviewsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var today = DateTime.UtcNow.Date.ToString("O");
        var sql = @"
            SELECT r.* FROM reviews r
            INNER JOIN cards c ON r.card_id = c.id
            WHERE c.user_id = @UserId AND DATE(r.reviewed_at) = DATE(@Today)";
        
        return await connection.QueryAsync<Review>(sql, new { UserId = userId, Today = today });
    }

    public async Task<(int Total, int Correct)> GetTodayStatsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var today = DateTime.UtcNow.Date.ToString("O");
        var sql = @"
            SELECT 
                COUNT(*) as Total,
                SUM(r.is_correct) as Correct
            FROM reviews r
            INNER JOIN cards c ON r.card_id = c.id
            WHERE c.user_id = @UserId AND DATE(r.reviewed_at) = DATE(@Today)";
        
        var result = await connection.QueryFirstOrDefaultAsync<(int Total, int Correct)>(sql, new { UserId = userId, Today = today });
        return result;
    }

    public async Task<(string? BestDay, int BestCount)> GetBestDayStatsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var sql = @"
            SELECT 
                DATE(r.reviewed_at) as Day,
                COUNT(*) as Count
            FROM reviews r
            INNER JOIN cards c ON r.card_id = c.id
            WHERE c.user_id = @UserId
            GROUP BY DATE(r.reviewed_at)
            ORDER BY Count DESC
            LIMIT 1";
        
        var result = await connection.QueryFirstOrDefaultAsync<(string? Day, int Count)>(sql, new { UserId = userId });
        return result;
    }
}

