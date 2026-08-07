using LanguageCardsBot.Contracts.Cards.V3;

namespace LanguageCardsBot.Presentation.Helpers;

/// <summary>Builds the MarkdownV2 training card message shown during /train sessions.</summary>
public static class TrainingMessageBuilder
{
    /// <summary>
    /// Formats a card for display. When <paramref name="hideTranslation"/> is true,
    /// wraps translation and example in MarkdownV2 spoiler tags.
    /// </summary>
    public static string Build(Card card, bool hideTranslation)
    {
        var translation = hideTranslation ? $"||{card.Translation}||" : card.Translation;
        var example = string.IsNullOrEmpty(card.Example)
            ? string.Empty
            : hideTranslation ? $"||{card.Example}||" : card.Example;

        var text = $"💡 *Слово*: {card.Term}\nПеревод: {translation}";
        if (!string.IsNullOrEmpty(example))
            text += $"\nПример: {example}";

        return text;
    }
}
