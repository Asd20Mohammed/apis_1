using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get all users
    /// </summary>
    /// <returns>List of all users ordered by creation date</returns>
    /// <response code="200">Returns the list of users</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<User>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<List<User>>> GetAllUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            // Remove password hash from response
            users.ForEach(u => u.PasswordHash = string.Empty);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">The MongoDB ObjectId of the user</param>
    /// <returns>The user with the specified ID</returns>
    /// <response code="200">Returns the user</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(User), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<User>> GetUserById(string id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound($"User with ID {id} not found");

            // Remove password hash from response
            user.PasswordHash = string.Empty;
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get user by username
    /// </summary>
    /// <param name="username">The username to search for</param>
    /// <returns>The user with the specified username</returns>
    /// <response code="200">Returns the user</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("username/{username}")]
    [ProducesResponseType(typeof(User), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<User>> GetUserByUsername(string username)
    {
        try
        {
            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null)
                return NotFound($"User with username '{username}' not found");

            // Remove password hash from response
            user.PasswordHash = string.Empty;
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with username: {Username}", username);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get users by role
    /// </summary>
    /// <param name="role">The role to filter by</param>
    /// <returns>List of users with the specified role</returns>
    /// <response code="200">Returns the list of users</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("role/{role}")]
    [ProducesResponseType(typeof(List<User>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<List<User>>> GetUsersByRole(string role)
    {
        try
        {
            var users = await _userService.GetUsersByRoleAsync(role);
            // Remove password hash from response
            users.ForEach(u => u.PasswordHash = string.Empty);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users for role: {Role}", role);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Search users
    /// </summary>
    /// <param name="searchTerm">Search term to look for in username, email, first name, or last name</param>
    /// <returns>List of users matching the search term</returns>
    /// <response code="200">Returns the list of users</response>
    /// <response code="400">Search term is required</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<User>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<List<User>>> SearchUsers([FromQuery] string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return BadRequest("Search term is required");

        try
        {
            var users = await _userService.SearchUsersAsync(searchTerm);
            // Remove password hash from response
            users.ForEach(u => u.PasswordHash = string.Empty);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users with term: {SearchTerm}", searchTerm);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="userDto">The user data to create</param>
    /// <returns>The created user with generated ID</returns>
    /// <response code="201">User created successfully</response>
    /// <response code="400">Invalid input data or username/email already exists</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(User), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<User>> RegisterUser([FromBody] CreateUserDto userDto)
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
            
            // Remove password hash from response
            user.PasswordHash = string.Empty;
            
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Authenticate a user
    /// </summary>
    /// <param name="loginDto">Login credentials</param>
    /// <returns>The authenticated user information</returns>
    /// <response code="200">Authentication successful</response>
    /// <response code="401">Invalid credentials</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(User), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<User>> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var user = await _userService.AuthenticateAsync(loginDto.UsernameOrEmail, loginDto.Password);
            if (user == null)
                return Unauthorized("Invalid username/email or password");

            // Remove password hash from response
            user.PasswordHash = string.Empty;
            
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    /// <param name="id">The user ID to update</param>
    /// <param name="userDto">The user data to update</param>
    /// <returns>The updated user</returns>
    /// <response code="200">User updated successfully</response>
    /// <response code="400">Invalid input data or username/email already exists</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(User), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<User>> UpdateUser(string id, [FromBody] UpdateUserDto userDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            // Check if username is available (if being updated)
            if (!string.IsNullOrEmpty(userDto.Username))
            {
                var existingUserWithUsername = await _userService.GetUserByUsernameAsync(userDto.Username);
                if (existingUserWithUsername != null && existingUserWithUsername.Id != id)
                    return BadRequest($"Username '{userDto.Username}' is already taken");
            }

            // Check if email is available (if being updated)
            if (!string.IsNullOrEmpty(userDto.Email))
            {
                var existingUserWithEmail = await _userService.GetUserByEmailAsync(userDto.Email);
                if (existingUserWithEmail != null && existingUserWithEmail.Id != id)
                    return BadRequest($"Email '{userDto.Email}' is already registered");
            }

            var updatedUser = await _userService.UpdateUserAsync(id, userDto);
            if (updatedUser == null)
                return NotFound($"User with ID {id} not found");

            // Remove password hash from response
            updatedUser.PasswordHash = string.Empty;
            
            return Ok(updatedUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user with ID: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    /// <param name="id">The user ID to delete</param>
    /// <returns>No content on successful deletion</returns>
    /// <response code="204">User deleted successfully</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> DeleteUser(string id)
    {
        try
        {
            var success = await _userService.DeleteUserAsync(id);
            if (!success)
                return NotFound($"User with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with ID: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Check if username is available
    /// </summary>
    /// <param name="username">Username to check</param>
    /// <returns>Boolean indicating availability</returns>
    /// <response code="200">Returns availability status</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("check-username/{username}")]
    [ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<bool>> CheckUsernameAvailability(string username)
    {
        try
        {
            var isAvailable = await _userService.IsUsernameAvailableAsync(username);
            return Ok(isAvailable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking username availability: {Username}", username);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Check if email is available
    /// </summary>
    /// <param name="email">Email to check</param>
    /// <returns>Boolean indicating availability</returns>
    /// <response code="200">Returns availability status</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("check-email/{email}")]
    [ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<bool>> CheckEmailAvailability(string email)
    {
        try
        {
            var isAvailable = await _userService.IsEmailAvailableAsync(email);
            return Ok(isAvailable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email availability: {Email}", email);
            return StatusCode(500, "Internal server error");
        }
    }
} 