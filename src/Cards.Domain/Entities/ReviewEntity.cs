using Cards.Domain.Common;

namespace Cards.Domain.Entities;

public class ReviewEntity: IEntity
{
    public int Id { get; set; }
    public int CardId { get; set; }
    public bool IsCorrect { get; set; }
    public DateTime ReviewedAt { get; set; }
    
    public virtual CardEntity CardEntity { get; set; } = null!;
}