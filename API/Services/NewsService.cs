using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using WebApi.Models;

namespace WebApi.Services;

public class NewsService : INewsService
{
    private readonly IMongoCollection<News> _newsCollection;

    public NewsService(IOptions<MongoDbSettings> mongoDbSettings)
    {
        var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
        _newsCollection = mongoDatabase.GetCollection<News>(mongoDbSettings.Value.NewsCollectionName);
    }

    public async Task<List<News>> GetAllNewsAsync()
    {
        return await _newsCollection.Find(_ => true).SortByDescending(n => n.CreatedAt).ToListAsync();
    }

    public async Task<News?> GetNewsByIdAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return null;

        return await _newsCollection.Find(n => n.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<News>> GetNewsByCategoryAsync(string category)
    {
        return await _newsCollection
            .Find(n => n.Category.ToLower() == category.ToLower())
            .SortByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<News>> GetPublishedNewsAsync()
    {
        return await _newsCollection
            .Find(n => n.IsPublished)
            .SortByDescending(n => n.PublishedDate)
            .ToListAsync();
    }

    public async Task<News> CreateNewsAsync(CreateNewsDto newsDto)
    {
        var news = new News
        {
            Title = newsDto.Title,
            Content = newsDto.Content,
            Author = newsDto.Author,
            UserId = newsDto.UserId,
            Category = newsDto.Category,
            Tags = newsDto.Tags,
            IsPublished = newsDto.IsPublished,
            Summary = newsDto.Summary,
            ImageUrl = newsDto.ImageUrl,
            PublishedDate = newsDto.PublishedDate ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _newsCollection.InsertOneAsync(news);
        return news;
    }

    public async Task<News?> UpdateNewsAsync(string id, UpdateNewsDto newsDto)
    {
        if (!ObjectId.TryParse(id, out _))
            return null;

        var existingNews = await GetNewsByIdAsync(id);
        if (existingNews == null)
            return null;

        var updateDefinitions = new List<UpdateDefinition<News>>();

        if (!string.IsNullOrEmpty(newsDto.Title))
            updateDefinitions.Add(Builders<News>.Update.Set(n => n.Title, newsDto.Title));

        if (!string.IsNullOrEmpty(newsDto.Content))
            updateDefinitions.Add(Builders<News>.Update.Set(n => n.Content, newsDto.Content));

        if (!string.IsNullOrEmpty(newsDto.Author))
            updateDefinitions.Add(Builders<News>.Update.Set(n => n.Author, newsDto.Author));

        if (!string.IsNullOrEmpty(newsDto.Category))
            updateDefinitions.Add(Builders<News>.Update.Set(n => n.Category, newsDto.Category));

        if (newsDto.Tags != null)
            updateDefinitions.Add(Builders<News>.Update.Set(n => n.Tags, newsDto.Tags));

        if (newsDto.IsPublished.HasValue)
            updateDefinitions.Add(Builders<News>.Update.Set(n => n.IsPublished, newsDto.IsPublished.Value));

        if (!string.IsNullOrEmpty(newsDto.Summary))
            updateDefinitions.Add(Builders<News>.Update.Set(n => n.Summary, newsDto.Summary));

        if (newsDto.ImageUrl != null)
            updateDefinitions.Add(Builders<News>.Update.Set(n => n.ImageUrl, newsDto.ImageUrl));

        if (newsDto.PublishedDate.HasValue)
            updateDefinitions.Add(Builders<News>.Update.Set(n => n.PublishedDate, newsDto.PublishedDate.Value));

        updateDefinitions.Add(Builders<News>.Update.Set(n => n.UpdatedAt, DateTime.UtcNow));

        if (updateDefinitions.Any())
        {
            var combinedUpdate = Builders<News>.Update.Combine(updateDefinitions);
            await _newsCollection.UpdateOneAsync(n => n.Id == id, combinedUpdate);
        }

        return await GetNewsByIdAsync(id);
    }

    public async Task<bool> DeleteNewsAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return false;

        var deleteResult = await _newsCollection.DeleteOneAsync(n => n.Id == id);
        return deleteResult.DeletedCount > 0;
    }

    public async Task<List<News>> SearchNewsAsync(string searchTerm)
    {
        var filter = Builders<News>.Filter.Or(
            Builders<News>.Filter.Regex(n => n.Title, new BsonRegularExpression(searchTerm, "i")),
            Builders<News>.Filter.Regex(n => n.Content, new BsonRegularExpression(searchTerm, "i")),
            Builders<News>.Filter.Regex(n => n.Summary, new BsonRegularExpression(searchTerm, "i")),
            Builders<News>.Filter.AnyEq(n => n.Tags, searchTerm)
        );

        return await _newsCollection
            .Find(filter)
            .SortByDescending(n => n.CreatedAt)
            .ToListAsync();
    }
} 