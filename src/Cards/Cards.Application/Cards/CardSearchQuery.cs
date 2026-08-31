namespace Cards.Application.Cards;

/// <summary>
/// Parameters for the card search use case.
/// </summary>
/// <param name="Q">Optional substring to match against term and translation (case-insensitive).</param>
/// <param name="Sort">Sort column: term | translation | level | accuracy | next. Defaults to term.</param>
/// <param name="SortDir">Sort direction: asc | desc. Defaults to asc.</param>
/// <param name="Filter">Status filter: all | due | new | learned. Defaults to all.</param>
public sealed record CardSearchQuery(
    string? Q,
    string Sort = "term",
    string SortDir = "asc",
    string Filter = "all");
