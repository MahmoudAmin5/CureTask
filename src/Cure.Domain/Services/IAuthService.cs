using Cure.Domain.Common;

namespace Cure.Domain.Services;

public sealed record TokenResponse(string AccessToken, string RefreshToken, DateTime ExpiresAtUtc);

public interface IAuthService
{
    Task<Result<TokenResponse>> RegisterAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string role,
        CancellationToken cancellationToken = default);

    Task<Result<TokenResponse>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<Result<TokenResponse>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task<Result> RevokeRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);
}
