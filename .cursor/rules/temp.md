---
alwaysApply: true
description: ASP.NET Core Web API project structure guidelines and folder organization
---

# ASP.NET Core Web API Project Structure Guidelines

## Recommended Folder Structure

Follow this standardized folder structure for all ASP.NET Core Web API projects:

```

API/                             # Main project (ASP.NET Core Web API)
├── Controllers/                # All Controllers for Endpoints
│   ├─ AuthController.cs
│   ├── ProductsController.cs
│   └── UsersController.cs
│
├── Models/                     # Data Models (Requests & Responses)
│   ├── Auth/
│   │   ├── LoginRequest.cs
│   │   └── LoginResponse.cs
│   ├── Products/
│   │   ├── ProductRequest.cs
│   │   └── ProductResponse.cs
│   └── Shared/
│       ├── ApiResponse.cs
│       └── Pagination.cs
│
├── Middlewares/               # Custom Middleware (e.g., ExceptionHandling)
│   └── ErrorHandlingMiddleware.cs
│
├── Filters/                   # Filters like Authorization or Validation
│   └── ValidateModelAttribute.cs
│
├── Extensions/                # Extensions for IServiceCollection or IApplicationBuilder
│   ├── ServiceCollectionExtensions.cs
│   └── ApplicationBuilderExtensions.cs
│
├── Services/                  # General Services (e.g., EmailService, JwtService)
│   ├── Interfaces/
│   │   └── IJwtService.cs
│   └── Implementations/
│       └── JwtService.cs
│
├── appsettings.json           # Configuration settings
├── Program.cs                 # Application startup point (with WebApplication configuration)
└── API.csproj                 # Project file
```

## Folder Organization Rules

### Controllers/
- Place all API controllers here
- Use descriptive names ending with "Controller"
- Group related controllers in subfolders if needed
- Example: `AuthController.cs`, `ProductsController.cs`

### Models/
- Organize models by feature/domain
- Use subfolders for different model types:
  - `Auth/` - Authentication-related models
  - `Products/` - Product-related models
  - `Shared/` - Common models used across features
- Separate Request and Response models
- Use descriptive naming: `LoginRequest.cs`, `ProductResponse.cs`

### Middlewares/
- Custom middleware components
- Exception handling middleware
- Logging middleware
- Authentication middleware extensions

### Filters/
- Custom action filters
- Authorization filters
- Validation filters
- Exception filters

### Extensions/
- Extension methods for `IServiceCollection`
- Extension methods for `IApplicationBuilder`
- Helper extension methods

### Services/
- Business logic services
- External service integrations
- Separate interfaces and implementations:
  - `Interfaces/` - Service contracts
  - `Implementations/` - Service implementations

## Naming Conventions

- **Controllers**: `{Feature}Controller.cs`
- **Models**: `{Feature}{Type}.cs` (e.g., `LoginRequest.cs`, `ProductResponse.cs`)
- **Services**: `I{Service}Service.cs` (interface), `{Service}Service.cs` (implementation)
- **Middleware**: `{Feature}Middleware.cs`
- **Filters**: `{Feature}Filter.cs` or `{Feature}Attribute.cs`

## File Organization Best Practices

1. **Keep related files together** in feature-based folders
2. **Use clear, descriptive names** for all files and folders
3. **Separate concerns** - Controllers, Models, Services in different folders
4. **Group by feature** when possible (Auth, Products, Users)
5. **Maintain consistency** across all projects

## Current Project Structure

Based on the current News API project, the structure follows these guidelines:

- [Controllers/](mdc:Controllers/) - API controllers
- [Models/](mdc:Models/) - Data models and DTOs
- [Services/](mdc:Services/) - Business logic services
- [Program.cs](mdc:Program.cs) - Application configuration
- [appsettings.json](mdc:appsettings.json) - Configuration settings

This structure provides a clean, maintainable, and scalable foundation for ASP.NET Core Web API projects.
