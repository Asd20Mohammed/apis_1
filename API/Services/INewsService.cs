using WebApi.Models;

namespace WebApi.Services;

public interface INewsService
{
    Task<List<News>> GetAllNewsAsync();
    Task<News?> GetNewsByIdAsync(string id);
    Task<List<News>> GetNewsByCategoryAsync(string category);
    Task<List<News>> GetPublishedNewsAsync();
    Task<News> CreateNewsAsync(CreateNewsDto newsDto);
    Task<News?> UpdateNewsAsync(string id, UpdateNewsDto newsDto);
    Task<bool> DeleteNewsAsync(string id);
    Task<List<News>> SearchNewsAsync(string searchTerm);
} 