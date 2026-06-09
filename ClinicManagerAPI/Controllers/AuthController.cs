using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ClinicManagerAPI.DTOs.Auth;
using ClinicManagerAPI.Models;

namespace ClinicManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });
            }

            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Errors = new List<string> { "Użytkownik z tym adresem email już istnieje." }
                });
            }

            var user = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Specialization = model.Specialization,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                });
            }

            // Przypisz rolę (domyślnie Rejestratorka)
            var role = model.Role;
            if (!await _roleManager.RoleExistsAsync(role))
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Errors = new List<string> { $"Rola '{role}' nie istnieje." }
                });
            }

            await _userManager.AddToRoleAsync(user, role);

            return Ok(new AuthResponseDto
            {
                Success = true,
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = new List<string> { role }
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Errors = new List<string> { "Nieprawidłowy email lub hasło." }
                });
            }

            if (!user.IsActive)
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Errors = new List<string> { "Konto użytkownika jest nieaktywne." }
                });
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordValid)
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Errors = new List<string> { "Nieprawidłowy email lub hasło." }
                });
            }

            var token = await GenerateJwtToken(user);
            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new AuthResponseDto
            {
                Success = true,
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(
                    double.Parse(_configuration["JwtSettings:ExpirationInMinutes"] ?? "60")),
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToList()
            });
        }

        [HttpPost("assign-role")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return NotFound(new { message = "Użytkownik nie został znaleziony." });
            }

            var roleExists = await _roleManager.RoleExistsAsync(model.Role);
            if (!roleExists)
            {
                return BadRequest(new { message = $"Rola '{model.Role}' nie istnieje. Dostępne role: Admin, Lekarz, Rejestratorka." });
            }

            var isInRole = await _userManager.IsInRoleAsync(user, model.Role);
            if (isInRole)
            {
                return BadRequest(new { message = $"Użytkownik już posiada rolę '{model.Role}'." });
            }

            var result = await _userManager.AddToRoleAsync(user, model.Role);
            if (!result.Succeeded)
            {
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
            }

            return Ok(new { message = $"Rola '{model.Role}' została przypisana użytkownikowi {user.Email}." });
        }

        [HttpPost("remove-role")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> RemoveRole([FromBody] AssignRoleDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return NotFound(new { message = "Użytkownik nie został znaleziony." });
            }

            var isInRole = await _userManager.IsInRoleAsync(user, model.Role);
            if (!isInRole)
            {
                return BadRequest(new { message = $"Użytkownik nie posiada roli '{model.Role}'." });
            }

            var result = await _userManager.RemoveFromRoleAsync(user, model.Role);
            if (!result.Succeeded)
            {
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
            }

            return Ok(new { message = $"Rola '{model.Role}' została usunięta użytkownikowi {user.Email}." });
        }

        [HttpGet("roles")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public IActionResult GetRoles()
        {
            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return Ok(roles);
        }

        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<AuthResponseDto>> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new AuthResponseDto
            {
                Success = true,
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToList()
            });
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]!;
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expirationMinutes = double.Parse(jwtSettings["ExpirationInMinutes"] ?? "60");

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
