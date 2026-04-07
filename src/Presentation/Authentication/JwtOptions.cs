namespace StudentApi.Presentation.Authentication;

/// <summary>
/// Strongly typed JWT settings bound from configuration.
/// </summary>
public sealed class JwtOptions
{
    /// <summary>
    /// Configuration section name used by options binding.
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// Token issuer used for generation and validation.
    /// </summary>
    public string Issuer { get; init; } = string.Empty;

    /// <summary>
    /// Token audience used for generation and validation.
    /// </summary>
    public string Audience { get; init; } = string.Empty;

    /// <summary>
    /// Symmetric signing key used to sign and validate tokens.
    /// </summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>
    /// Access token lifetime in minutes.
    /// </summary>
    public int ExpirationMinutes { get; init; } = 60;
}
