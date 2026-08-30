namespace Cards.Application.Cards;

/// <summary>Holds the raw bytes and HTTP metadata for a card export file.</summary>
public sealed record CardExportResult(byte[] Content, string ContentType, string FileName);
