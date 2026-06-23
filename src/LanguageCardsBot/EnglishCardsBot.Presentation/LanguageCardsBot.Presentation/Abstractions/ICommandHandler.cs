
using LanguageCardsBot.Contracts.Cards.V3;

namespace EnglishCardsBot.Presentation.Abstractions;

public interface ICommandHandler<in TCommand>
{
    Task HandleAsync(TCommand command, User user, CancellationToken cancellationToken = default);
}