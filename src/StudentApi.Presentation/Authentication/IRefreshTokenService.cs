namespace StudentApi.Presentation.Authentication;

public interface IRefreshTokenService
{
    string GenerateToken();

    string HashToken(string token);
}
