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
            if (cmd is "hide" or "show" or "скрыть" or "показать")
                HideTranslations = true;
        
            if (int.TryParse(args[0], out var interval) && interval >= 1)
                ReminderIntervalMinutes = interval;
        }
    }
}