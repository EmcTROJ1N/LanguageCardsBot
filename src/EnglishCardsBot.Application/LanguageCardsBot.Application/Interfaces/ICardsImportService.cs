using EnglishCardsBot.Application.DTOs;

namespace EnglishCardsBot.Application.Interfaces;

public interface ICardsImportService
{
    Task<Result<CardsImportResult>> ImportCardsFromJsonAsync(string json, int userId, CancellationToken cancellationToken);
}