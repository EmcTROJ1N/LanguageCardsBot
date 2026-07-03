using LanguageCardsBot.Contracts.Cards.V3;

namespace LanguageCardsBot.Presentation.Dispatchers;

/// <summary>
/// Singleton dictionary mapping trigger strings (slash commands and menu button texts)
/// to their handler delegates. Populated during DI registration via <c>AddCommand&lt;T&gt;</c>.
/// </summary>
public class CommandRegistry
{
    internal Dictionary<string, Func<IServiceProvider, long, string[], User, CancellationToken, Task>> Entries
        { get; } = new(StringComparer.OrdinalIgnoreCase);
}
