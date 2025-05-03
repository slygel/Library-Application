using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.DTOs.AccountDto;

public class LoginRequest
{
    [Required]
    public required string Username { get; set; }
    
    [Required]
    public required string Password { get; set; }
}