using System.Security.Cryptography;
namespace LibraryAPI.Services;

public static class PasswordHashHandler
{
    public static string HashPassword(string password)
    {
        var saltBytes = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }

        using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100000, HashAlgorithmName.SHA256))
        {
            var hashBytes = pbkdf2.GetBytes(32);
            var hash = Convert.ToBase64String(hashBytes);
            var salt = Convert.ToBase64String(saltBytes);
            return $"{salt}:{hash}";
        }
    }
    
    public static bool VerifyPassword(string password, string storedPassword)
    {
        var parts = storedPassword.Split(':');
        if (parts.Length != 2)
        {
            throw new ArgumentException("Stored password format is invalid.");
        }

        var storedSalt = parts[0];
        var storedHash = parts[1];

        var saltBytes = Convert.FromBase64String(storedSalt);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100000, HashAlgorithmName.SHA256);
        var hashBytes = pbkdf2.GetBytes(32);
        var computedHash = Convert.ToBase64String(hashBytes);
        return computedHash == storedHash;
    }
}