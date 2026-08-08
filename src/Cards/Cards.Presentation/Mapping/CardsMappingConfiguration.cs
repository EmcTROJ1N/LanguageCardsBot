using Cards.Application.Cards;
using Cards.Application.Imports;
using Cards.Application.Stats;
using Cards.Application.Users;
using Cards.Domain.Entities;
using Cards.Contracts.Rest.Cards;
using Cards.Contracts.Rest.Users;
using Cards.Contracts.Rest.Imports;
using Cards.Contracts.Rest.Stats;
using Cards.Contracts.Rest.Translations;
using Google.Protobuf.WellKnownTypes;
using LanguageCardsBot.Contracts.Cards.V3;
using Mapster;

using AppTranslationResult = Cards.Application.Translations.TranslationResult;
using GrpcTranslationResult = LanguageCardsBot.Contracts.Cards.V3.TranslationResult;

namespace Cards.Presentation.Mapping;

/// <summary>
/// Configures Mapster mappings used by REST and gRPC transport adapters.
/// </summary>
public static class CardsMappingConfiguration
{
    /// <summary>
    /// Registers cards service mapping rules in Mapster's global configuration.
    /// </summary>
    public static void Register()
    {
        RegisterRestMappings();
        RegisterGrpcMappings();
    }

    /// <summary>
    /// Registers mappings used by REST controllers.
    /// </summary>
    private static void RegisterRestMappings()
    {
        TypeAdapterConfig<UpdateCardRequestDto, UpdateCardCommand>
            .NewConfig()
            .Map(dest => dest.Id, _ => 0);

        TypeAdapterConfig<UserRequestDto, UserCommand>
            .NewConfig()
            .Map(dest => dest.Id, _ => 0);

        TypeAdapterConfig<CardsImportData, CardsImportDataDto>.NewConfig();
        TypeAdapterConfig<OperationErrorResult, OperationErrorDto>.NewConfig();
        TypeAdapterConfig<ImportCardsFromJsonResult, ImportCardsFromJsonResponseDto>.NewConfig();
        TypeAdapterConfig<TodayStatsResult, TodayStatsDto>.NewConfig();
        TypeAdapterConfig<AppTranslationResult, TranslationResultDto>.NewConfig();
    }

    /// <summary>
    /// Registers mappings used by gRPC services.
    /// </summary>
    private static void RegisterGrpcMappings()
    {
        TypeAdapterConfig<CardEntity, Card>
            .NewConfig()
            .Ignore(dest => dest.Example)
            .Ignore(dest => dest.NextReviewAt)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.LastReviewAt)
            .AfterMapping((src, dest) =>
            {
                dest.CreatedAt = ToTimestamp(src.CreatedAt);

                if (!string.IsNullOrWhiteSpace(src.Example))
                    dest.Example = src.Example;

                if (src.NextReviewAt.HasValue)
                    dest.NextReviewAt = ToTimestamp(src.NextReviewAt.Value);

                if (src.LastReviewAt.HasValue)
                    dest.LastReviewAt = ToTimestamp(src.LastReviewAt.Value);
            });

        TypeAdapterConfig<UserEntity, User>
            .NewConfig()
            .Ignore(dest => dest.Username)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.NextReminderAtUtc)
            .AfterMapping((src, dest) =>
            {
                dest.CreatedAt = ToTimestamp(src.CreatedAt);

                if (!string.IsNullOrWhiteSpace(src.Username))
                    dest.Username = src.Username;

                if (src.NextReminderAtUtc.HasValue)
                    dest.NextReminderAtUtc = ToTimestamp(src.NextReminderAtUtc.Value);
            });

        TypeAdapterConfig<User, UserCommand>
            .NewConfig()
            .Map(dest => dest.Username, src => src.HasUsername ? src.Username : null)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt.ToDateTime())
            .Map(dest => dest.NextReminderAtUtc, src => src.NextReminderAtUtc.ToDateTime());

        TypeAdapterConfig<TodayStatsResult, TodayStats>
            .NewConfig()
            .Ignore(dest => dest.BestDay)
            .AfterMapping((src, dest) =>
            {
                if (src.BestDay is not null)
                    dest.BestDay = src.BestDay;
            });

        TypeAdapterConfig<AppTranslationResult, GrpcTranslationResult>.NewConfig();
        TypeAdapterConfig<CardsImportData, CardsImportResult>
            .NewConfig()
            .Ignore(dest => dest.Errors);
        TypeAdapterConfig<OperationErrorResult, OperationError>
            .NewConfig()
            .Ignore(dest => dest.Target)
            .Map(dest => dest.Code, src => src.Code ?? string.Empty)
            .AfterMapping((src, dest) =>
            {
                if (src.Target is not null)
                    dest.Target = src.Target;
            });
    }

    /// <summary>
    /// Converts a date-time value to a gRPC timestamp.
    /// </summary>
    private static Timestamp ToTimestamp(DateTime value)
    {
        return Timestamp.FromDateTime(ToUtc(value));
    }

    /// <summary>
    /// Converts a date-time value to UTC before serializing it across service boundaries.
    /// </summary>
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
