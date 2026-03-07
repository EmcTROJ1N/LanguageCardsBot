namespace EnglishCardsBot.Presentation.Commands.ReminderSettings;

public class ReminderSettingsCommand
{
    public long ChatId { get; }
    public int? ReminderIntervalMinutes { get; }
    public bool? HideTranslations { get; }
    
    public ReminderSettingsCommand(long chatId, string[] args)
    {
        ChatId = chatId;
        if (args.Any())
        {
            var cmd = args[0].ToLower();
            HideTranslations = cmd switch
            {
                "show" or "показать" => false,
                "hide" or "скрыть" => true,
                _ => HideTranslations
            };

            if (int.TryParse(args[0], out var interval) && interval >= 1)
                ReminderIntervalMinutes = interval;
        }
    }
}