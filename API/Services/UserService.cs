using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using WebApi.Models;
using System.Security.Cryptography;
using System.Text;

namespace WebApi.Services;

public class UserService : IUserService
{
    private readonly IMongoCollection<User> _usersCollection;

    public UserService(IOptions<MongoDbSettings> mongoDbSettings)
    {
        var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
        _usersCollection = mongoDatabase.GetCollection<User>(mongoDbSettings.Value.UsersCollectionName);
        
        // Create indexes for better performance
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        var indexKeys = Builders<User>.IndexKeys.Ascending(u => u.Username);
        var indexOptions = new CreateIndexOptions { Unique = true };
        _usersCollection.Indexes.CreateOneAsync(new CreateIndexModel<User>(indexKeys, indexOptions));

        var emailIndexKeys = Builders<User>.IndexKeys.Ascending(u => u.Email);
        var emailIndexOptions = new CreateIndexOptions { Unique = true };
        _usersCollection.Indexes.CreateOneAsync(new CreateIndexModel<User>(emailIndexKeys, emailIndexOptions));
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _usersCollection.Find(_ => true).SortByDescending(u => u.CreatedAt).ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return null;

        return await _usersCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _usersCollection.Find(u => u.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _usersCollection.Find(u => u.Email.ToLower() == email.ToLower()).FirstOrDefaultAsync();
    }

    public async Task<User> CreateUserAsync(CreateUserDto userDto)
    {
        var user = new User
        {
            Username = userDto.Username,
            Email = userDto.Email,
            PasswordHash = HashPassword(userDto.Password),
            FirstName = userDto.FirstName,
            LastName = userDto.LastName,
            Role = userDto.Role,
            ProfileImageUrl = userDto.ProfileImageUrl,
            Bio = userDto.Bio,
            DateOfBirth = userDto.DateOfBirth,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _usersCollection.InsertOneAsync(user);
        return user;
    }

    public async Task<User?> UpdateUserAsync(string id, UpdateUserDto userDto)
    {
        if (!ObjectId.TryParse(id, out _))
            return null;

        var existingUser = await GetUserByIdAsync(id);
        if (existingUser == null)
            return null;

        var updateDefinitions = new List<UpdateDefinition<User>>();

        if (!string.IsNullOrEmpty(userDto.Username))
            updateDefinitions.Add(Builders<User>.Update.Set(u => u.Username, userDto.Username));

        if (!string.IsNullOrEmpty(userDto.Email))
            updateDefinitions.Add(Builders<User>.Update.Set(u => u.Email, userDto.Email));

        if (!string.IsNullOrEmpty(userDto.FirstName))
            updateDefinitions.Add(Builders<User>.Update.Set(u => u.FirstName, userDto.FirstName));

        if (!string.IsNullOrEmpty(userDto.LastName))
            updateDefinitions.Add(Builders<User>.Update.Set(u => u.LastName, userDto.LastName));

        if (!string.IsNullOrEmpty(userDto.Role))
            updateDefinitions.Add(Builders<User>.Update.Set(u => u.Role, userDto.Role));

        if (userDto.IsActive.HasValue)
            updateDefinitions.Add(Builders<User>.Update.Set(u => u.IsActive, userDto.IsActive.Value));

        if (userDto.ProfileImageUrl != null)
            updateDefinitions.Add(Builders<User>.Update.Set(u => u.ProfileImageUrl, userDto.ProfileImageUrl));

        if (userDto.Bio != null)
            updateDefinitions.Add(Builders<User>.Update.Set(u => u.Bio, userDto.Bio));

        if (userDto.DateOfBirth.HasValue)
            updateDefinitions.Add(Builders<User>.Update.Set(u => u.DateOfBirth, userDto.DateOfBirth.Value));

        updateDefinitions.Add(Builders<User>.Update.Set(u => u.UpdatedAt, DateTime.UtcNow));

        if (updateDefinitions.Any())
        {
            var combinedUpdate = Builders<User>.Update.Combine(updateDefinitions);
            await _usersCollection.UpdateOneAsync(u => u.Id == id, combinedUpdate);
        }

        return await GetUserByIdAsync(id);
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return false;

        var deleteResult = await _usersCollection.DeleteOneAsync(u => u.Id == id);
        return deleteResult.DeletedCount > 0;
    }

    public async Task<User?> AuthenticateAsync(string usernameOrEmail, string password)
    {
        var filter = Builders<User>.Filter.Or(
            Builders<User>.Filter.Eq(u => u.Username, usernameOrEmail),
            Builders<User>.Filter.Eq(u => u.Email, usernameOrEmail)
        );

        var user = await _usersCollection.Find(filter).FirstOrDefaultAsync();

        if (user == null || !user.IsActive || !VerifyPassword(password, user.PasswordHash))
            return null;

        // Update last login time
        await _usersCollection.UpdateOneAsync(
            u => u.Id == user.Id,
            Builders<User>.Update.Set(u => u.LastLoginAt, DateTime.UtcNow)
        );

        return user;
    }

    public async Task<bool> IsUsernameAvailableAsync(string username)
    {
        var user = await GetUserByUsernameAsync(username);
        return user == null;
    }

    public async Task<bool> IsEmailAvailableAsync(string email)
    {
        var user = await GetUserByEmailAsync(email);
        return user == null;
    }

    public async Task<List<User>> GetUsersByRoleAsync(string role)
    {
        return await _usersCollection
            .Find(u => u.Role.ToLower() == role.ToLower())
            .SortByDescending(u => u.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<User>> SearchUsersAsync(string searchTerm)
    {
        var filter = Builders<User>.Filter.Or(
            Builders<User>.Filter.Regex(u => u.Username, new BsonRegularExpression(searchTerm, "i")),
            Builders<User>.Filter.Regex(u => u.FirstName, new BsonRegularExpression(searchTerm, "i")),
            Builders<User>.Filter.Regex(u => u.LastName, new BsonRegularExpression(searchTerm, "i")),
            Builders<User>.Filter.Regex(u => u.Email, new BsonRegularExpression(searchTerm, "i"))
        );

        return await _usersCollection
            .Find(filter)
            .SortByDescending(u => u.CreatedAt)
            .ToListAsync();
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "NewsApiSalt"));
        return Convert.ToBase64String(hashedBytes);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        var hashedPassword = HashPassword(password);
        return hashedPassword == hash;
    }
} 