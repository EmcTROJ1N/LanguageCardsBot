namespace Cards.Contracts.Rest.Users;

/// <summary>
/// Represents a REST response containing a user collection.
/// </summary>
public sealed record GetUsersResponseDto(IReadOnlyCollection<UserDto> Users);
