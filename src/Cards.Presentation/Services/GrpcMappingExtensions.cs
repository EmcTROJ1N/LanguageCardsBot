using Cards.Domain.Entities;
using Google.Protobuf.WellKnownTypes;
using LanguageCardsBot.Contracts.Cards.V3;

namespace Cards.Presentation.Services;

internal static class GrpcMappingExtensions
{
    public static Card ToGrpcCard(this CardEntity entity)
    {
        var card = new Card
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Term = entity.Term,
            Translation = entity.Translation,
            Transcription = entity.Transcription,
            Level = entity.Level,
            Learned = entity.Learned,
            CreatedAt = ToTimestamp(entity.CreatedAt),
            TotalReviews = entity.TotalReviews,
            CorrectReviews = entity.CorrectReviews
        };

        if (!string.IsNullOrWhiteSpace(entity.Example))
            card.Example = entity.Example;

        if (entity.NextReviewAt.HasValue)
            card.NextReviewAt = ToTimestamp(entity.NextReviewAt.Value);

        if (entity.LastReviewAt.HasValue)
            card.LastReviewAt = ToTimestamp(entity.LastReviewAt.Value);

        return card;
    }

    public static User ToGrpcUser(this UserEntity entity)
    {
        var user = new User
        {
            Id = entity.Id,
            ChatId = entity.ChatId,
            CreatedAt = ToTimestamp(entity.CreatedAt),
            ReminderIntervalMinutes = entity.ReminderIntervalMinutes,
            HideTranslations = entity.HideTranslations
        };

        if (!string.IsNullOrWhiteSpace(entity.Username))
            user.Username = entity.Username;

        if (entity.NextReminderAtUtc.HasValue)
            user.NextReminderAtUtc = ToTimestamp(entity.NextReminderAtUtc.Value);

        return user;
    }

    public static UserEntity ToUserEntity(this User user)
    {
        return new UserEntity
        {
            Id = user.Id,
            ChatId = user.ChatId,
            Username = string.IsNullOrWhiteSpace(user.Username) ? null : user.Username,
            CreatedAt = user.CreatedAt?.ToDateTime() ?? DateTime.UtcNow,
            ReminderIntervalMinutes = Math.Max(1, user.ReminderIntervalMinutes),
            NextReminderAtUtc = user.NextReminderAtUtc?.ToDateTime(),
            HideTranslations = user.HideTranslations
        };
    }

    public static Timestamp ToTimestamp(DateTime value)
    {
        return Timestamp.FromDateTime(ToUtc(value));
    }

    private static DateTime ToUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
