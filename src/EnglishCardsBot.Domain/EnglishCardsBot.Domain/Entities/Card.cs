namespace EnglishCardsBot.Domain.Entities;

public class Card
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Term { get; set; } = string.Empty;
    public string Translation { get; set; } = string.Empty;
    public string Transcription { get; set; } = string.Empty;
    public string? Example { get; set; }
    public int Level { get; set; } = 1;
    public DateTime? NextReviewAt { get; set; }
    public bool Learned { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastReviewAt { get; set; }
    public int TotalReviews { get; set; } = 0;
    public int CorrectReviews { get; set; } = 0;
    
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    
    public bool IsDue()
    {
        return !Learned && (NextReviewAt == null || NextReviewAt <= DateTime.UtcNow);
    }
}

