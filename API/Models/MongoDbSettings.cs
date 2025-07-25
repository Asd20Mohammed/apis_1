namespace WebApi.Models;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string NewsCollectionName { get; set; } = string.Empty;
    public string UsersCollectionName { get; set; } = string.Empty;
} 