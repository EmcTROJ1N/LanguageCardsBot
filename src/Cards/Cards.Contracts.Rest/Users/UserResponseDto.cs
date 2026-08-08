namespace Cards.Contracts.Rest.Users;

/// <summary>
/// Represents a REST response containing a user.
/// </summary>
public sealed record UserResponseDto(UserDto User);
