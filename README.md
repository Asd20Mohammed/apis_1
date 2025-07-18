# News API - ASP.NET Core Web API with MongoDB

This is a comprehensive News API built with ASP.NET Core Web API and MongoDB for posting and managing news articles.

## Features

- ASP.NET Core Web API with controllers
- **JWT Authentication & Authorization** - Secure token-based authentication
- MongoDB integration for data persistence
- Full CRUD operations for news articles and users
- Search functionality across news content and users
- Category-based filtering and role-based access
- Published/Draft status management
- User registration, login, and profile management
- Password hashing with SHA256 encryption
- OpenAPI (Swagger) support with JWT authentication
- HTTPS redirection
- Protected endpoints requiring authentication

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- MongoDB Community Server (running on localhost:27017)

### Running the Application

1. **Build the project:**
   ```bash
   dotnet build
   ```

2. **Run the application:**
   ```bash
   dotnet run
   ```

3. **Access the API:**
   - The API will be available at `https://localhost:5001` (or the port shown in the console)
   - In development mode, you can access the OpenAPI documentation at `/openapi/v1.json`

### API Endpoints

#### News Management
- `GET /api/news` - Get all news articles
- `GET /api/news/{id}` - Get specific news article by ID
- `GET /api/news/published` - Get only published news articles
- `GET /api/news/category/{category}` - Get news by category
- `GET /api/news/search?searchTerm={term}` - Search news articles
- `POST /api/news` - Create new news article
- `PUT /api/news/{id}` - Update existing news article
- `DELETE /api/news/{id}` - Delete news article

#### JWT Authentication
- `POST /api/auth/register` - Register new user and get JWT token
- `POST /api/auth/login` - Authenticate user and get JWT token
- `GET /api/auth/profile` - Get current user profile (requires JWT)
- `PUT /api/auth/profile` - Update current user profile (requires JWT)
- `POST /api/auth/validate-token` - Validate JWT token
- `POST /api/auth/refresh` - Refresh JWT token (requires JWT)

#### User Management (Admin functions)
- `GET /api/user` - Get all users
- `GET /api/user/{id}` - Get specific user by ID
- `GET /api/user/username/{username}` - Get user by username
- `GET /api/user/role/{role}` - Get users by role
- `GET /api/user/search?searchTerm={term}` - Search users
- `POST /api/user/register` - Register new user (without JWT)
- `POST /api/user/login` - Authenticate user (without JWT)
- `PUT /api/user/{id}` - Update existing user
- `DELETE /api/user/{id}` - Delete user
- `GET /api/user/check-username/{username}` - Check username availability
- `GET /api/user/check-email/{email}` - Check email availability

### Project Structure

- `Program.cs` - Application entry point and MongoDB service configuration
- `Controllers/NewsController.cs` - News API endpoints
- `Models/` - Data models and DTOs
  - `News.cs` - News entity model with MongoDB attributes
  - `CreateNewsDto.cs` - DTO for creating news articles
  - `UpdateNewsDto.cs` - DTO for updating news articles
  - `MongoDbSettings.cs` - MongoDB configuration model
- `Services/` - Business logic and data access
  - `INewsService.cs` - News service interface
  - `NewsService.cs` - MongoDB news service implementation
- `appsettings.json` - Application and MongoDB configuration
- `WebApi.csproj` - Project file with dependencies
- `WebApi.http` - Sample HTTP requests for testing

### Development

To add new controllers:
1. Create a new class in the `Controllers/` folder
2. Inherit from `ControllerBase`
3. Add the `[ApiController]` attribute
4. Define your API endpoints using HTTP method attributes (`[HttpGet]`, `[HttpPost]`, etc.)

### Configuration

#### MongoDB Configuration
Update `appsettings.json` to configure your MongoDB connection:
```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "NewsDatabase",
    "NewsCollectionName": "News",
    "UsersCollectionName": "Users"
  }
}
```

#### JWT Configuration
Configure JWT authentication settings:
```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!",
    "Issuer": "NewsApi",
    "Audience": "NewsApiUsers",
    "ExpirationInMinutes": 60
  }
}
```

**Important**: Change the `SecretKey` to a secure, random string in production!

#### Application Settings
- Modify `appsettings.json` for application and MongoDB settings
- Use `appsettings.Development.json` for development-specific settings

### Data Models

#### News Article Model
The News model includes the following fields:
- `Id` - Unique identifier (MongoDB ObjectId)
- `Title` - Article title (required, 5-200 characters)
- `Content` - Article content (required, 10-10000 characters)
- `Author` - Author name (required, max 100 characters)
- `UserId` - Associated user ID (MongoDB ObjectId, optional)
- `Category` - News category (max 100 characters)
- `Tags` - Array of tags for categorization
- `IsPublished` - Publication status (boolean)
- `Summary` - Brief summary (max 500 characters)
- `ImageUrl` - Optional image URL
- `PublishedDate` - Publication date
- `CreatedAt` - Creation timestamp
- `UpdatedAt` - Last update timestamp

#### User Model
The User model includes the following fields:
- `Id` - Unique identifier (MongoDB ObjectId)
- `Username` - Unique username (required, 3-50 characters)
- `Email` - Unique email address (required)
- `PasswordHash` - Hashed password (not returned in API responses)
- `FirstName` - User's first name (required, max 100 characters)
- `LastName` - User's last name (required, max 100 characters)
- `Role` - User role (User, Admin, Editor - default: User)
- `IsActive` - Account status (boolean, default: true)
- `ProfileImageUrl` - Optional profile image URL
- `Bio` - User biography (max 1000 characters)
- `DateOfBirth` - Optional date of birth
- `CreatedAt` - Account creation timestamp
- `UpdatedAt` - Last update timestamp
- `LastLoginAt` - Last login timestamp 