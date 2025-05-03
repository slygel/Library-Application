using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.DTOs.AccountDto;

public class RegisterRequest
{
    [Required]
    public required string Name { get; set; }
    
    [Required]
    public required string Username { get; set; }
    
    [Required]
    public required string Email { get; set; }
    
    [Required]
    public required string PhoneNumber { get; set; }
    
    [Required]
    public required string Address { get; set; }
    
    [Required]
    public required string Password { get; set; }
    
    [Required]
    public required string ConfirmPassword { get; set; }
}