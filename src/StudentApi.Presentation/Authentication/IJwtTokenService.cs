namespace StudentApi.Presentation.Authentication;

public interface IJwtTokenService
{
    string GenerateToken(string username, string role);
}
