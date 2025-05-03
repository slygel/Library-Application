using LibraryAPI.DTOs.AccountDto;
using LibraryAPI.IServices;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAPI.Controllers;

[Route("/api/v1/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest loginRequest)
    {
        var result = await _authService.Login(loginRequest);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return StatusCode(result.StatusCode ?? StatusCodes.Status500InternalServerError, new { Error = result.ErrorMessage });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest registerRequest)
    {
        var result = await _authService.Register(registerRequest);
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode ?? StatusCodes.Status500InternalServerError, new { Error = result.ErrorMessage });
    }
    
    [HttpPost("refresh-token")]
    public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] RefreshTokenRequest refreshTokenRequest)
    {
        var result = await _authService.RefreshToken(refreshTokenRequest);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return StatusCode(result.StatusCode ?? StatusCodes.Status500InternalServerError, new { Error = result.ErrorMessage });
    }
    
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest refreshTokenRequest)
    {
        var result = await _authService.Logout(refreshTokenRequest);
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode ?? StatusCodes.Status500InternalServerError, new { Error = result.ErrorMessage });
    }
}