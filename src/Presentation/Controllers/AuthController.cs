using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StudentApi.Application.Interfaces;
using StudentApi.Presentation.Authentication;
using StudentApi.Presentation.Common;

namespace StudentApi.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Authentication endpoints for login and refresh-token rotation.
/// </summary>
public class AuthController : ControllerBase
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

    private readonly IUserAuthRepository _userAuthRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly JwtOptions _jwtOptions;

    public AuthController(
        IUserAuthRepository userAuthRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        IPasswordHasher passwordHasher,
        IOptions<JwtOptions> jwtOptions)
    {
        _userAuthRepository = userAuthRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _passwordHasher = passwordHasher;
        _jwtOptions = jwtOptions.Value;
    }

    /// <summary>
    /// Login request payload.
    /// </summary>
    public sealed record LoginRequest(string Username, string Password);

    /// <summary>
    /// Authentication response with access token and refresh token pair.
    /// </summary>
    public sealed record LoginResponse(string AccessToken, DateTime ExpiresAtUtc, string RefreshToken, DateTime RefreshTokenExpiresAtUtc);

    /// <summary>
    /// Refresh request payload.
    /// </summary>
    public sealed record RefreshRequest(string RefreshToken);

    [AllowAnonymous]
    [HttpPost("login")]
    /// <summary>
    /// Validates credentials and issues a new JWT access token and refresh token.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>
    /// <c>200 OK</c> with tokens when credentials are valid; otherwise <c>401 Unauthorized</c>.
    /// </returns>
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _userAuthRepository.GetByUsernameAsync(request.Username, cancellationToken);

        if (user is null)
        {
            return Unauthorized(ApiResponse<LoginResponse>.FailureResponse(new[] { "Invalid username or password." }));
        }

        var isValidUser = _passwordHasher.Verify(request.Password, user.PasswordHash);

        if (!isValidUser)
        {
            return Unauthorized(ApiResponse<LoginResponse>.FailureResponse(new[] { "Invalid username or password." }));
        }

        var accessToken = _jwtTokenService.GenerateToken(user.Username, user.Role);
        var accessExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes);
        var refreshToken = _refreshTokenService.GenerateToken();
        var refreshTokenHash = _refreshTokenService.HashToken(refreshToken);
        var refreshExpiresAt = DateTime.UtcNow.Add(RefreshTokenLifetime);

        await _refreshTokenRepository.AddAsync(new Domain.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserAccountId = user.Id,
            Username = user.Username,
            Role = user.Role,
            TokenHash = refreshTokenHash,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = refreshExpiresAt
        }, cancellationToken);

        return Ok(ApiResponse<LoginResponse>.SuccessResponse(new LoginResponse(accessToken, accessExpiresAt, refreshToken, refreshExpiresAt)));
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    /// <summary>
    /// Exchanges an active refresh token for a new access/refresh token pair.
    /// </summary>
    /// <param name="request">Refresh-token exchange payload.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>
    /// <c>200 OK</c> with rotated tokens when refresh token is valid; otherwise <c>401 Unauthorized</c>.
    /// </returns>
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        var tokenHash = _refreshTokenService.HashToken(request.RefreshToken);
        var storedToken = await _refreshTokenRepository.GetActiveByTokenHashAsync(tokenHash, cancellationToken);

        if (storedToken is null)
        {
            return Unauthorized(ApiResponse<LoginResponse>.FailureResponse(new[] { "Invalid refresh token." }));
        }

        var user = await _userAuthRepository.GetByUsernameAsync(storedToken.Username, cancellationToken);

        if (user is null)
        {
            return Unauthorized(ApiResponse<LoginResponse>.FailureResponse(new[] { "Invalid refresh token." }));
        }

        var newAccessToken = _jwtTokenService.GenerateToken(user.Username, user.Role);
        var newAccessExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes);
        var newRefreshToken = _refreshTokenService.GenerateToken();
        var newRefreshTokenHash = _refreshTokenService.HashToken(newRefreshToken);
        var newRefreshExpiresAt = DateTime.UtcNow.Add(RefreshTokenLifetime);

        await _refreshTokenRepository.RevokeAsync(storedToken.Id, newRefreshTokenHash, cancellationToken);

        await _refreshTokenRepository.AddAsync(new Domain.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserAccountId = user.Id,
            Username = user.Username,
            Role = user.Role,
            TokenHash = newRefreshTokenHash,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = newRefreshExpiresAt
        }, cancellationToken);

        return Ok(ApiResponse<LoginResponse>.SuccessResponse(new LoginResponse(newAccessToken, newAccessExpiresAt, newRefreshToken, newRefreshExpiresAt)));
    }
}
