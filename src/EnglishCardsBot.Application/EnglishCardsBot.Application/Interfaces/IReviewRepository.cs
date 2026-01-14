using EnglishCardsBot.Domain.Entities;

namespace EnglishCardsBot.Application.Interfaces;

public interface IReviewRepository : IRepository<Review>
{
    Task<IEnumerable<Review>> GetTodayReviewsByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<(int Total, int Correct)> GetTodayStatsByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<(string? BestDay, int BestCount)> GetBestDayStatsByUserIdAsync(int userId, CancellationToken cancellationToken = default);
}

