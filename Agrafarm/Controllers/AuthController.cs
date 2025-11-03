using Agrafarm.Data;
using Agrafarm.DTO;
using Agrafarm.Interfaces;
using Agrafarm.Model;
using Agrafarm.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Agrafarm.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepo;
        private readonly ITokenService _tokenService;
        private readonly AuthService _authService;

        public AuthController(IAuthRepository authRepo, ITokenService tokenService, AuthService authService)
        {
            _authRepo = authRepo;
            _tokenService = tokenService;
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto dto)
        {
            if (await _authRepo.GetByUsernameAsync(dto.Username) != null)
                return BadRequest("Username already exists");

            _authService.CreatePasswordHash(dto.Password, out var hash, out var salt);

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = "Admin" // Only you as admin for now
            };

            await _authRepo.AddUserAsync(user);
            return Ok("User registered successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto dto)
        {
            var user = await _authRepo.GetByUsernameAsync(dto.Username);
            if (user == null) return Unauthorized("Invalid credentials");

            if (!_authService.VerifyPassword(dto.Password, user.PasswordHash, user.PasswordSalt))
                return Unauthorized("Invalid password");

            var token = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken(HttpContext.Connection.RemoteIpAddress?.ToString() ?? "local");

            user.RefreshTokens.Add(refreshToken);
            await _authRepo.SaveAsync();

            return Ok(new TokenResponseDto
            {
                AccessToken = token,
                RefreshToken = refreshToken.Token
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenRequestDto dto)
        {
            if (string.IsNullOrEmpty(dto.RefreshToken))
                return BadRequest("Refresh token is required.");

            var user = await _authRepo.GetByRefreshTokenAsync(dto.RefreshToken);
            if (user == null)
                return Unauthorized("Invalid refresh token.");

            var oldToken = user.RefreshTokens.FirstOrDefault(t => t.Token == dto.RefreshToken);
            if (oldToken == null || !oldToken.IsActive)
                return Unauthorized("Invalid or expired refresh token.");

            // 🧩 Revoke old token
            oldToken.Revoked = DateTime.UtcNow;

            // 🆕 Generate new tokens
            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken(HttpContext.Connection.RemoteIpAddress?.ToString() ?? "local");

            oldToken.ReplacedByToken = newRefreshToken.Token;
            user.RefreshTokens.Add(newRefreshToken);

            await _authRepo.SaveAsync();

            // 🧹 Cleanup: Remove all old inactive or expired tokens
            await _authRepo.DeleteOldTokensAsync(user.Id);

            return Ok(new
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token,
                Message = "Token refreshed successfully."
            });
        }




    }

}
