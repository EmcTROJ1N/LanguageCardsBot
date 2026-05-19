using Cards.Domain.Entities;

namespace Cards.Application.Cards;

public interface ICardApplicationService
{
    Task<CardEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CardEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CardEntity>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<CardEntity?> GetDueCardAsync(int userId, CancellationToken cancellationToken = default);
    Task<CardEntity> AddAsync(AddCardCommand command, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(UpdateCardCommand command, CancellationToken cancellationToken = default);
    Task<bool> UpdateReviewAsync(int cardId, bool isCorrect, CancellationToken cancellationToken = default);
    Task<bool> DeleteByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> DeleteByUserIdAsync(int userId, CancellationToken cancellationToken = default);
}
