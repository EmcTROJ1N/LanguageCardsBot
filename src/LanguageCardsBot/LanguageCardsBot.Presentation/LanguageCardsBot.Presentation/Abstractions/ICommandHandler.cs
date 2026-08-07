using LanguageCardsBot.Contracts.Cards.V3;

namespace LanguageCardsBot.Presentation.Abstractions;

public interface ICommandHandler<in TCommand>
{
    Task HandleAsync(TCommand command, User user, CancellationToken cancellationToken = default);
}
