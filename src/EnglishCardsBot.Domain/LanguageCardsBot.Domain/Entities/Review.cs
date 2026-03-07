namespace EnglishCardsBot.Domain.Entities;

public class Review
{
    public int Id { get; set; }
    public int CardId { get; set; }
    public bool IsCorrect { get; set; }
    public DateTime ReviewedAt { get; set; }
    
    public virtual Card Card { get; set; } = null!;
}

