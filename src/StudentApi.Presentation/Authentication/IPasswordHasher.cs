namespace StudentApi.Presentation.Authentication;

public interface IPasswordHasher
{
    bool Verify(string password, string storedHash);
}
