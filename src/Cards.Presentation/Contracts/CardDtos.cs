using Cards.Domain.Entities;

namespace Cards.Presentation.Contracts;

public sealed record CardDto(
    int Id,
    int UserId,
    string Term,
    string Translation,
    string Transcription,
    string? Example,
    int Level,
    DateTime? NextReviewAt,
    bool Learned,
    DateTime CreatedAt,
    DateTime? LastReviewAt,
    int TotalReviews,
    int CorrectReviews);

public sealed record GetCardResponseDto(CardDto? Card);

public sealed record GetCardsResponseDto(IReadOnlyCollection<CardDto> Cards);

public sealed record CardResponseDto(CardDto Card);

public sealed class AddCardRequestDto
{
    public int UserId { get; init; }
    public string Term { get; init; } = string.Empty;
    public string Translation { get; init; } = string.Empty;
    public string Transcription { get; init; } = string.Empty;
    public string? Example { get; init; }
}

public sealed class UpdateCardRequestDto
{
    public string Term { get; init; } = string.Empty;
    public string Translation { get; init; } = string.Empty;
    public string Transcription { get; init; } = string.Empty;
    public string? Example { get; init; }
    public bool HasExample { get; init; }
    public bool? Learned { get; init; }
}

public sealed class UpdateCardReviewRequestDto
{
    public bool IsCorrect { get; init; }
}

public sealed record UpdateCardResponseDto(bool Updated);

public sealed record UpdateCardReviewResponseDto(bool Updated);

public sealed record DeleteCardResponseDto(bool Deleted);

public sealed record DeleteCardsByUserIdResponseDto(bool Deleted);

internal static partial class ApiMappingExtensions
{
    public static CardDto ToDto(this CardEntity entity)
    {
        return new CardDto(
            entity.Id,
            entity.UserId,
            entity.Term,
            entity.Translation,
            entity.Transcription,
            entity.Example,
            entity.Level,
            entity.NextReviewAt,
            entity.Learned,
            entity.CreatedAt,
            entity.LastReviewAt,
            entity.TotalReviews,
            entity.CorrectReviews);
    }
}
