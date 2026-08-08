namespace Cards.Contracts.Rest.Users;

/// <summary>
/// Represents a REST response containing an optional user.
/// </summary>
public sealed record GetUserResponseDto(UserDto? User);
