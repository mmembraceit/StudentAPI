namespace StudentApi.Presentation.Authentication;


/// Strongly typed JWT settings bound from configuration.
public sealed class JwtOptions
{
   
    /// Configuration section name used by options binding.
    public const string SectionName = "Jwt";

    /// Token issuer used for generation and validation.
    public string Issuer { get; init; } = string.Empty;


    /// Token audience used for generation and validation.
    public string Audience { get; init; } = string.Empty;


    /// Symmetric signing key used to sign and validate tokens.
    public string Key { get; init; } = string.Empty;

  
    /// Access token lifetime in minutes.
    public int ExpirationMinutes { get; init; } = 60;
}
