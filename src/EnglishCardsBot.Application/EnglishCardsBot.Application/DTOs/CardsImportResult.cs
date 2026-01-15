namespace EnglishCardsBot.Application.DTOs;

public class CardsImportResult
{
    public int Imported { get; set; }
    public int Skipped { get; set; }
    public List<string> Errors { get; set; } = [];
}