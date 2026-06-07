namespace Cure.Application.DTOs.Auth;

public sealed record TokenResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAtUtc);
