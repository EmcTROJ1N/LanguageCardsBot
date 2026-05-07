using Cards.Domain.Common;
using Cards.Domain.ValueObjects;

namespace Cards.Domain.Entities;

public class CardEntity: IEntityWithId
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
    
    public virtual UserEntity UserEntity { get; set; } = null!;
    public virtual ICollection<ReviewEntity> Reviews { get; set; } = new List<ReviewEntity>();
    
    public bool IsDue()
    {
        return !Learned && (NextReviewAt == null || NextReviewAt <= DateTime.UtcNow);
    }

    public ReviewEntity RecordReview(bool isCorrect, DateTime reviewedAtUtc)
    {
        var reviewedAt = reviewedAtUtc.Kind switch
        {
            DateTimeKind.Utc => reviewedAtUtc,
            DateTimeKind.Local => reviewedAtUtc.ToUniversalTime(),
            _ => DateTime.SpecifyKind(reviewedAtUtc, DateTimeKind.Utc)
        };

        TotalReviews++;
        LastReviewAt = reviewedAt;

        if (isCorrect)
        {
            CorrectReviews++;
            Level = Math.Min(Level + 1, ReviewIntervals.Days.Length);
            Learned = Level >= ReviewIntervals.Days.Length;
        }
        else
        {
            Level = 1;
            Learned = false;
        }

        NextReviewAt = Learned
            ? null
            : reviewedAt.AddDays(ReviewIntervals.GetIntervalDays(Level));

        return new ReviewEntity
        {
            CardId = Id,
            IsCorrect = isCorrect,
            ReviewedAt = reviewedAt
        };
    }
}
