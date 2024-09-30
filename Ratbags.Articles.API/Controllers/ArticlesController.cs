using Microsoft.AspNetCore.Mvc;
using Ratbags.Articles.API.Interfaces;
using Ratbags.Shared.DTOs.Events.DTOs.Articles;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Ratbags.Articles.API.Controllers;

[ApiController]
[Route("api/articles")]
public class ArticlesController : ControllerBase
{
    private readonly IArticlesService _service;
    private readonly ILogger<ArticlesController> _logger;

    public ArticlesController(IArticlesService service, ILogger<ArticlesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpDelete("{id}")]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Summary = "Deletes an article by id", Description = "Deletes an article by id")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _service.DeleteArticleAsync(id);

            if (!result)
            {
                return NotFound(); // no article exists
            }

            return NoContent();
        }
        catch (Exception e)
        {
            _logger.LogError($"Error deleting article {id}: {e.Message}");
            return StatusCode(500, "An error occurred while deleting the article");
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ArticleDTO>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Summary = "Gets all articles", Description = "Retrieves a list of all articles")]
    public async Task<IActionResult> Get()
    {
        var result = await _service.GetAllArticlesAsync();

        if (result != null)
        {
            return Ok(result);
        }

        return Ok(new List<ArticleDTO>());
    }

    [HttpGet("{id}")]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(ArticleDTO), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Summary = "Gets an article by id", Description = "Retrieves a specific article by its id")]
    public async Task<IActionResult> Get(Guid id)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Invalid article id format");
        }

        var result = await _service.GetArticleByIdAsync(id);

        if (result != null)
        {
            return Ok(result);
        }

        return NotFound();
    }

    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(Guid), (int)HttpStatusCode.Created)]
    [SwaggerOperation(Summary = "Creates an article", Description = "Creates an article")]
    public async Task<IActionResult> Post([FromBody] CreateArticleDTO createArticleDTO)
    {
        try
        {
            var articleId = await _service.CreateArticleAsync(createArticleDTO);

            if (articleId != Guid.Empty)
            {
                return CreatedAtAction(nameof(Get), new { id = articleId }, articleId);
            }

            return BadRequest("Failed to create article");
        }
        catch (Exception e)
        {
            _logger.LogError($"Error creating article: {e.Message}");
            return StatusCode(500, "An error occurred while creating the article");
        }
    }

    [HttpPut]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Summary = "Updates an article", Description = "Updates an article")]
    public async Task<IActionResult> Put([FromBody] ArticleDTO articleDTO)
    {
        try
        {
            var result = await _service.UpdateArticleAsync(articleDTO);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception e)
        {
            _logger.LogError($"Error updating article {articleDTO.Id}: {e.Message}");
            return StatusCode(500, "An error occurred while updating the article");
        }
    }
}