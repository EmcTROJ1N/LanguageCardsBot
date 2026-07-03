using LanguageCardsBot.Contracts.Cards.V3;
using LanguageCardsBot.Presentation.Abstractions;

namespace LanguageCardsBot.Presentation.Dispatchers;

/// <summary>Routes a trigger string to the matching command handler using <see cref="CommandRegistry"/>.</summary>
public class CommandDispatcher(CommandRegistry registry, IServiceProvider sp) : ICommandDispatcher
{
    /// <inheritdoc/>
    public async Task<bool> TryDispatchAsync(string trigger, long chatId, string[] args, User user, CancellationToken ct)
    {
        if (!registry.Entries.TryGetValue(trigger, out var factory))
            return false;
        await factory(sp, chatId, args, user, ct);
        return true;
    }
}
