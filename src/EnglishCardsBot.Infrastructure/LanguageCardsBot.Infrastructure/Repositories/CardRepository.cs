using Dapper;
using EnglishCardsBot.Application.Interfaces;
using EnglishCardsBot.Domain.Entities;
using EnglishCardsBot.Infrastructure.Data;

namespace EnglishCardsBot.Infrastructure.Repositories;

public class CardRepository(ApplicationDbContext context) : ICardRepository
{
    public async Task<Card?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = context.CreateConnection();
        var sql = "SELECT * FROM cards WHERE id = @Id";
        return await connection.QueryFirstOrDefaultAsync<Card>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Card>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = context.CreateConnection();
        var sql = "SELECT * FROM cards";
        return await connection.QueryAsync<Card>(sql);
    }

    public async Task<Card> AddAsync(Card entity, CancellationToken cancellationToken = default)
    {
        using var connection = context.CreateConnection();
        var sql = @"
            INSERT INTO cards (user_id, term, translation, transcription, example, level, 
                             next_review_at, learned, created_at, last_review_at, total_reviews, correct_reviews)
            VALUES (@UserId, @Term, @Translation, @Transcription, @Example, @Level,
                   @NextReviewAt, @Learned, @CreatedAt, @LastReviewAt, @TotalReviews, @CorrectReviews);
            SELECT last_insert_rowid();";
        
        var id = await connection.QuerySingleAsync<int>(sql, new
        {
            entity.UserId,
            entity.Term,
            entity.Translation,
            entity.Transcription,
            entity.Example,
            entity.Level,
            NextReviewAt = entity.NextReviewAt?.ToString("O"),
            Learned = entity.Learned ? 1 : 0,
            CreatedAt = entity.CreatedAt.ToString("O"),
            LastReviewAt = entity.LastReviewAt?.ToString("O"),
            entity.TotalReviews,
            entity.CorrectReviews
        });
        
        entity.Id = id;
        return entity;
    }

    public async Task UpdateAsync(Card entity, CancellationToken cancellationToken = default)
    {
        using var connection = context.CreateConnection();
        var sql = @"
            UPDATE cards 
            SET user_id = @UserId, term = @Term, translation = @Translation, transcription = @Transcription,
                example = @Example, level = @Level, next_review_at = @NextReviewAt, learned = @Learned,
                created_at = @CreatedAt, last_review_at = @LastReviewAt, total_reviews = @TotalReviews,
                correct_reviews = @CorrectReviews
            WHERE id = @Id";
        
        await connection.ExecuteAsync(sql, new
        {
            entity.Id,
            entity.UserId,
            entity.Term,
            entity.Translation,
            entity.Transcription,
            entity.Example,
            entity.Level,
            NextReviewAt = entity.NextReviewAt?.ToString("O"),
            Learned = entity.Learned ? 1 : 0,
            CreatedAt = entity.CreatedAt.ToString("O"),
            LastReviewAt = entity.LastReviewAt?.ToString("O"),
            entity.TotalReviews,
            entity.CorrectReviews
        });
    }

    public async Task DeleteAsync(Card entity, CancellationToken cancellationToken = default)
    {
        using var connection = context.CreateConnection();
        var sql = "DELETE FROM cards WHERE id = @Id";
        await connection.ExecuteAsync(sql, new { entity.Id });
    }

    public async Task<Card?> GetDueCardAsync(int userId, CancellationToken cancellationToken = default)
    {
        using var connection = context.CreateConnection();
        var now = DateTime.UtcNow.ToString("O");
        var sql = @"
            SELECT * FROM cards
            WHERE user_id = @UserId AND learned = 0
              AND (next_review_at IS NULL OR next_review_at <= date('now'))
            ORDER BY next_review_at ASC;  
            ";
        var dueCards = (await connection
            .QueryAsync<Card>(sql, new { UserId = userId, Now = now }))
            .AsList();

        return dueCards[Random.Shared.Next(dueCards.Count)];
    }

    public async Task<Card?> GetRandomActiveCardAsync(int userId, CancellationToken cancellationToken = default)
    {
        using var connection = context.CreateConnection();
        var sql = @"
            SELECT * FROM cards
            WHERE user_id = @UserId AND learned = 0
            ORDER BY RANDOM()
            LIMIT 1";
        
        return await connection.QueryFirstOrDefaultAsync<Card>(sql, new { UserId = userId });
    }

    public async Task<IEnumerable<Card>> GetAllByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        using var connection = context.CreateConnection();
        var sql = "SELECT * FROM cards WHERE user_id = @UserId ORDER BY created_at";
        return await connection.QueryAsync<Card>(sql, new { UserId = userId });
    }

    public async Task<int> DeleteAllByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        using var connection = context.CreateConnection();
        var sql = "DELETE FROM cards WHERE user_id = @UserId";
        return await connection.ExecuteAsync(sql, new { UserId = userId });
    }
}

