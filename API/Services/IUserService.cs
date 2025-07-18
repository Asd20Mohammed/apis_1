using WebApi.Models;

namespace WebApi.Services;

public interface IUserService
{
    Task<List<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(string id);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User> CreateUserAsync(CreateUserDto userDto);
    Task<User?> UpdateUserAsync(string id, UpdateUserDto userDto);
    Task<bool> DeleteUserAsync(string id);
    Task<User?> AuthenticateAsync(string usernameOrEmail, string password);
    Task<bool> IsUsernameAvailableAsync(string username);
    Task<bool> IsEmailAvailableAsync(string email);
    Task<List<User>> GetUsersByRoleAsync(string role);
    Task<List<User>> SearchUsersAsync(string searchTerm);
} 