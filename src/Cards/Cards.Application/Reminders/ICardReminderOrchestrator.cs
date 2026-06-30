using Cards.Domain.Entities;

namespace Cards.Application.Reminders;

/// <summary>
/// Manages the lifecycle of per-user reminder loops.
/// </summary>
public interface ICardReminderOrchestrator
{
    /// <summary>
    /// Starts a reminder loop for the given user if one is not already running.
    /// </summary>
    void RegisterUser(UserEntity user);

    /// <summary>
    /// Cancels and removes the reminder loop for the given user.
    /// </summary>
    void UnregisterUser(int userId);
}
