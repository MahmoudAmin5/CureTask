using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Cure.Application.Abstractions;
using Cure.Application.DTOs.Auth;
using Cure.Domain.Common;
using Cure.Domain.Entities;
using Cure.Domain.Entities.Identity;
using Cure.Domain.Errors;
using Cure.Domain.Services;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Cure.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IValidator<RegisterDto> _registerValidator;
    private readonly IValidator<LoginDto> _loginValidator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        IValidator<RegisterDto> registerValidator,
        IValidator<LoginDto> loginValidator,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _logger = logger;
    }

    public async Task<Result<TokenResponse>> RegisterAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string role,
        CancellationToken cancellationToken = default)
    {
        var dto = new RegisterDto(email, password, firstName, lastName, role);
        var validationResult = await _registerValidator.ValidateAsync(dto, cancellationToken);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(e => new Error(e.PropertyName, e.ErrorMessage))
                .ToArray();

            return Result<TokenResponse>.ValidationFailure(errors);
        }

        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            return Result<TokenResponse>.Failure(DomainErrors.Authentication.UserAlreadyExists);
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName
        };

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var createResult = await _userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                var errors = createResult.Errors
                    .Select(e => new Error(e.Code, e.Description))
                    .ToArray();

                return Result<TokenResponse>.ValidationFailure(errors);
            }

            await _userManager.AddToRoleAsync(user, role);

            var tokenResponse = await GenerateTokenPairAsync(user, role, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("User {Email} registered successfully with role {Role}", email, role);
            return Result<TokenResponse>.Success(tokenResponse);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            _logger.LogError(ex, "Unexpected error during registration for {Email}", email);
            throw;
        }
    }

    public async Task<Result<TokenResponse>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var dto = new LoginDto(email, password);
        var validationResult = await _loginValidator.ValidateAsync(dto, cancellationToken);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(e => new Error(e.PropertyName, e.ErrorMessage))
                .ToArray();

            return Result<TokenResponse>.ValidationFailure(errors);
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return Result<TokenResponse>.Failure(DomainErrors.Authentication.InvalidCredentials);
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!passwordValid)
        {
            return Result<TokenResponse>.Failure(DomainErrors.Authentication.InvalidCredentials);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? string.Empty;

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var tokenResponse = await GenerateTokenPairAsync(user, primaryRole, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("User {Email} logged in successfully", email);
            return Result<TokenResponse>.Success(tokenResponse);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            _logger.LogError(ex, "Unexpected error during login for {Email}", email);
            throw;
        }
    }

    public async Task<Result<TokenResponse>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<RefreshToken>();

        var storedToken = await repo.FirstOrDefaultAsync(
            t => t.Token == refreshToken,
            cancellationToken);

        if (storedToken is null)
        {
            return Result<TokenResponse>.Failure(DomainErrors.Authentication.InvalidRefreshToken);
        }

        if (!storedToken.IsActive)
        {
            return Result<TokenResponse>.Failure(
                storedToken.IsExpired
                    ? DomainErrors.Authentication.RefreshTokenExpired
                    : DomainErrors.Authentication.InvalidRefreshToken);
        }

        var user = await _userManager.FindByIdAsync(storedToken.UserId);
        if (user is null)
        {
            return Result<TokenResponse>.Failure(DomainErrors.Authentication.InvalidCredentials);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? string.Empty;

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            
            storedToken.RevokedAtUtc = DateTime.UtcNow;

            var tokenResponse = await GenerateTokenPairAsync(user, primaryRole, cancellationToken);

           
            storedToken.ReplacedByToken = tokenResponse.RefreshToken;
            repo.Update(storedToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Refresh token rotated successfully for user {UserId}", user.Id);
            return Result<TokenResponse>.Success(tokenResponse);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            _logger.LogError(ex, "Unexpected error refreshing token for user {UserId}", storedToken.UserId);
            throw;
        }
    }

    public async Task<Result> RevokeRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<RefreshToken>();

        var storedToken = await repo.FirstOrDefaultAsync(
            t => t.Token == refreshToken,
            cancellationToken);

        if (storedToken is null)
        {
            return Result.Failure(DomainErrors.Authentication.InvalidRefreshToken);
        }

        if (storedToken.IsRevoked)
        {
            return Result.Failure(DomainErrors.Authentication.InvalidRefreshToken);
        }

        storedToken.RevokedAtUtc = DateTime.UtcNow;
        repo.Update(storedToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Refresh token revoked for user {UserId}", storedToken.UserId);
        return Result.Success();
    }

    private async Task<TokenResponse> GenerateTokenPairAsync(
        ApplicationUser user,
        string role,
        CancellationToken cancellationToken)
    {
        var accessToken = GenerateAccessToken(user, role);
        var refreshTokenEntity = GenerateRefreshToken(user.Id);

        _unitOfWork.Repository<RefreshToken>().Add(refreshTokenEntity);

        var expirationMinutes = int.TryParse(
            _configuration["JwtSettings:AccessTokenExpirationMinutes"], out var minutes)
            ? minutes
            : 30;

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(expirationMinutes);

        return await Task.FromResult(new TokenResponse(
            accessToken,
            refreshTokenEntity.Token,
            expiresAtUtc));
    }

    private string GenerateAccessToken(ApplicationUser user, string role)
    {
        var key = _configuration["JwtSettings:Key"]
            ?? throw new InvalidOperationException("JwtSettings:Key is not configured.");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var expirationMinutes = int.TryParse(
            _configuration["JwtSettings:AccessTokenExpirationMinutes"], out var minutes)
            ? minutes
            : 30;

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("firstName", user.FirstName),
            new Claim("lastName", user.LastName)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private RefreshToken GenerateRefreshToken(string userId)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var refreshTokenDays = int.TryParse(
            _configuration["JwtSettings:RefreshTokenExpirationDays"], out var days)
            ? days
            : 7;

        return new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Token = Convert.ToBase64String(randomBytes),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(refreshTokenDays),
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
