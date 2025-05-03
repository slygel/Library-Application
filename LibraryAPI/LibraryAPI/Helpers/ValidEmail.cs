using System.Text.RegularExpressions;

namespace LibraryAPI.Helpers;

public static class ValidEmail
{
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        // Regex pattern for email validation
        const string emailPattern = @"^(?=.{1,254}$)(?=.{1,64}@)[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}$";

        try
        {
            // Normalize email
            email = email.Trim().ToLowerInvariant();
            
            // Check with regex
            return Regex.IsMatch(email, emailPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}