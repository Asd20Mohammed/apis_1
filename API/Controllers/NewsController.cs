using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class NewsController : ControllerBase
{
    private readonly INewsService _newsService;
    private readonly ILogger<NewsController> _logger;

    public NewsController(INewsService newsService, ILogger<NewsController> logger)
    {
        _newsService = newsService;
        _logger = logger;
    }

    /// <summary>
    /// Get all news articles
    /// </summary>
    /// <returns>List of all news articles ordered by creation date</returns>
    /// <response code="200">Returns the list of news articles</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<News>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<List<News>>> GetAllNews()
    {
        try
        {
            var news = await _newsService.GetAllNewsAsync();
            return Ok(news);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all news");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get news by ID
    /// </summary>
    /// <param name="id">The MongoDB ObjectId of the news article</param>
    /// <returns>The news article with the specified ID</returns>
    /// <response code="200">Returns the news article</response>
    /// <response code="404">News article not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(News), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<News>> GetNewsById(string id)
    {
        try
        {
            var news = await _newsService.GetNewsByIdAsync(id);
            if (news == null)
                return NotFound($"News with ID {id} not found");

            return Ok(news);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving news with ID: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get published news articles
    /// </summary>
    [HttpGet("published")]
    public async Task<ActionResult<List<News>>> GetPublishedNews()
    {
        try
        {
            var news = await _newsService.GetPublishedNewsAsync();
            return Ok(news);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving published news");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get news by category
    /// </summary>
    [HttpGet("category/{category}")]
    public async Task<ActionResult<List<News>>> GetNewsByCategory(string category)
    {
        try
        {
            var news = await _newsService.GetNewsByCategoryAsync(category);
            return Ok(news);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving news for category: {Category}", category);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Search news articles
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<List<News>>> SearchNews([FromQuery] string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return BadRequest("Search term is required");

        try
        {
            var news = await _newsService.SearchNewsAsync(searchTerm);
            return Ok(news);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching news with term: {SearchTerm}", searchTerm);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new news article (requires authentication)
    /// </summary>
    /// <param name="newsDto">The news article data to create</param>
    /// <returns>The created news article with generated ID</returns>
    /// <response code="201">News article created successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Authentication required</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(News), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<News>> CreateNews([FromBody] CreateNewsDto newsDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var news = await _newsService.CreateNewsAsync(newsDto);
            return CreatedAtAction(nameof(GetNewsById), new { id = news.Id }, news);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating news");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update an existing news article (requires authentication)
    /// </summary>
    /// <param name="id">The news article ID to update</param>
    /// <param name="newsDto">The news article data to update</param>
    /// <returns>The updated news article</returns>
    /// <response code="200">News article updated successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Authentication required</response>
    /// <response code="404">News article not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(News), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<News>> UpdateNews(string id, [FromBody] UpdateNewsDto newsDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var updatedNews = await _newsService.UpdateNewsAsync(id, newsDto);
            if (updatedNews == null)
                return NotFound($"News with ID {id} not found");

            return Ok(updatedNews);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating news with ID: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a news article (requires authentication)
    /// </summary>
    /// <param name="id">The news article ID to delete</param>
    /// <returns>No content on successful deletion</returns>
    /// <response code="204">News article deleted successfully</response>
    /// <response code="401">Authentication required</response>
    /// <response code="404">News article not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> DeleteNews(string id)
    {
        try
        {
            var success = await _newsService.DeleteNewsAsync(id);
            if (!success)
                return NotFound($"News with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting news with ID: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}