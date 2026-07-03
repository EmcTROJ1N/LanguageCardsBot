using LanguageCardsBot.Contracts.Cards.V3;
using LanguageCardsBot.Presentation.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace LanguageCardsBot.Presentation.Dispatchers;

/// <summary>Extension methods for registering command handlers in the <see cref="CommandRegistry"/>.</summary>
public static class CommandRegistryExtensions
{
    /// <summary>
    /// Registers <typeparamref name="THandler"/> in DI as scoped and maps each trigger string
    /// to its handler delegate in <paramref name="registry"/>.
    /// </summary>
    public static IServiceCollection AddCommand<THandler, TCommand>(
        this IServiceCollection services,
        CommandRegistry registry,
        string[] triggers,
        Func<long, string[], TCommand> cmdFactory)
        where THandler : class, ICommandHandler<TCommand>
    {
        services.AddScoped<THandler>();
        foreach (var trigger in triggers)
            registry.Entries[trigger] = (sp, chatId, args, user, ct) =>
                sp.GetRequiredService<THandler>().HandleAsync(cmdFactory(chatId, args), user, ct);
        return services;
    }
}
