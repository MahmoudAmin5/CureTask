namespace Cure.Application.DTOs.Auth;

public sealed record RegisterDto(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string Role);
