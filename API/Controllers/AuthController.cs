using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, IJwtService jwtService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _jwtService = jwtService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user and return JWT token
    /// </summary>
    /// <param name="userDto">User registration data</param>
    /// <returns>Authentication response with JWT token and user info</returns>
    /// <response code="201">User registered successfully with JWT token</response>
    /// <response code="400">Invalid input data or username/email already exists</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] CreateUserDto userDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            // Check if username is available
            if (!await _userService.IsUsernameAvailableAsync(userDto.Username))
                return BadRequest($"Username '{userDto.Username}' is already taken");

            // Check if email is available
            if (!await _userService.IsEmailAvailableAsync(userDto.Email))
                return BadRequest($"Email '{userDto.Email}' is already registered");

            var user = await _userService.CreateUserAsync(userDto);
            
            // Generate JWT token
            var token = _jwtService.GenerateToken(user);
            var expiresAt = _jwtService.GetTokenExpiration(token);

            // Remove password hash from response
            user.PasswordHash = string.Empty;

            var authResponse = new AuthResponse
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = user
            };

            _logger.LogInformation("User {Username} registered successfully", user.Username);
            
            return CreatedAtAction(nameof(GetProfile), authResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Authenticate user and return JWT token
    /// </summary>
    /// <param name="loginDto">Login credentials</param>
    /// <returns>Authentication response with JWT token and user info</returns>
    /// <response code="200">Authentication successful with JWT token</response>
    /// <response code="401">Invalid credentials</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var user = await _userService.AuthenticateAsync(loginDto.UsernameOrEmail, loginDto.Password);
            if (user == null)
                return Unauthorized("Invalid username/email or password");

            // Generate JWT token
            var token = _jwtService.GenerateToken(user);
            var expiresAt = _jwtService.GetTokenExpiration(token);

            // Remove password hash from response
            user.PasswordHash = string.Empty;

            var authResponse = new AuthResponse
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = user
            };

            _logger.LogInformation("User {Username} logged in successfully", user.Username);
            
            return Ok(authResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get current user profile (requires authentication)
    /// </summary>
    /// <returns>Current user information</returns>
    /// <response code="200">Returns current user profile</response>
    /// <response code="401">Authentication required</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(User), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<User>> GetProfile()
    {
        try
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Invalid token");

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            // Remove password hash from response
            user.PasswordHash = string.Empty;
            
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update current user profile (requires authentication)
    /// </summary>
    /// <param name="userDto">User update data</param>
    /// <returns>Updated user information</returns>
    /// <response code="200">Profile updated successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Authentication required</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(typeof(User), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<User>> UpdateProfile([FromBody] UpdateUserDto userDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Invalid token");

            // Check if username is available (if being updated)
            if (!string.IsNullOrEmpty(userDto.Username))
            {
                var existingUserWithUsername = await _userService.GetUserByUsernameAsync(userDto.Username);
                if (existingUserWithUsername != null && existingUserWithUsername.Id != userId)
                    return BadRequest($"Username '{userDto.Username}' is already taken");
            }

            // Check if email is available (if being updated)
            if (!string.IsNullOrEmpty(userDto.Email))
            {
                var existingUserWithEmail = await _userService.GetUserByEmailAsync(userDto.Email);
                if (existingUserWithEmail != null && existingUserWithEmail.Id != userId)
                    return BadRequest($"Email '{userDto.Email}' is already registered");
            }

            var updatedUser = await _userService.UpdateUserAsync(userId, userDto);
            if (updatedUser == null)
                return NotFound("User not found");

            // Remove password hash from response
            updatedUser.PasswordHash = string.Empty;
            
            _logger.LogInformation("User {UserId} updated profile successfully", userId);
            
            return Ok(updatedUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Validate JWT token
    /// </summary>
    /// <param name="request">Token validation request</param>
    /// <returns>Token validation result</returns>
    /// <response code="200">Token is valid</response>
    /// <response code="400">Invalid or expired token</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("validate-token")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public ActionResult ValidateToken([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var principal = _jwtService.ValidateToken(request.Token);
            if (principal == null)
                return BadRequest("Invalid token");

            if (_jwtService.IsTokenExpired(request.Token))
                return BadRequest("Token has expired");

            var userId = principal.FindFirst("UserId")?.Value;
            var username = principal.FindFirst(ClaimTypes.Name)?.Value;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            var role = principal.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new
            {
                IsValid = true,
                UserId = userId,
                Username = username,
                Email = email,
                Role = role,
                ExpiresAt = _jwtService.GetTokenExpiration(request.Token)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Refresh JWT token (requires authentication)
    /// </summary>
    /// <returns>New JWT token</returns>
    /// <response code="200">Token refreshed successfully</response>
    /// <response code="401">Authentication required</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("refresh")]
    [Authorize]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<AuthResponse>> RefreshToken()
    {
        try
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Invalid token");

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            if (!user.IsActive)
                return Unauthorized("User account is deactivated");

            // Generate new JWT token
            var token = _jwtService.GenerateToken(user);
            var expiresAt = _jwtService.GetTokenExpiration(token);

            // Remove password hash from response
            user.PasswordHash = string.Empty;

            var authResponse = new AuthResponse
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = user
            };

            _logger.LogInformation("Token refreshed for user {UserId}", userId);
            
            return Ok(authResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return StatusCode(500, "Internal server error");
        }
    }
} 