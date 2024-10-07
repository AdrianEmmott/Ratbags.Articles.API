using Microsoft.AspNetCore.Authorization;
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
    private readonly IService _service;
    private readonly ILogger<ArticlesController> _logger;

    public ArticlesController(IService service, ILogger<ArticlesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [Authorize]
    [HttpDelete("{id}")]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Summary = "Deletes an article by id", 
        Description = "Deletes an article by id")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _service.DeleteAsync(id);

            return result ? NoContent() : NotFound();
        }
        catch (Exception e)
        {
            _logger.LogError($"Error deleting article {id}: {e.Message}");

            return StatusCode((int)HttpStatusCode.InternalServerError, 
                "An error occurred while deleting the article");
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ArticleDTO>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Summary = "Gets all articles", 
        Description = "Returns a list of all articles or an empty list")]
    public async Task<IActionResult> Get()
    {
        // debug - simulate slow response
        var result = await _service.GetAsync();
        System.Threading.Thread.Sleep(200);

        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(ArticleDTO), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Summary = "Gets an article by id", 
        Description = "Returns a specific article by its id")]
    public async Task<IActionResult> Get(Guid id)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Invalid article id format");
        }

        var result = await _service.GetByIdAsync(id);
        
        return result == null ? NotFound() : Ok(result);
    }

    [Authorize]
    [HttpPost]    
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(Guid), (int)HttpStatusCode.Created)]
    [SwaggerOperation(Summary = "Creates an article", 
        Description = "Creates an article")]
    public async Task<IActionResult> Post([FromBody] CreateArticleDTO createArticleDTO)
    {
        try
        {
            var articleId = await _service.CreateAsync(createArticleDTO);

            if (articleId != Guid.Empty)
            {
                // set location header
                return CreatedAtAction(nameof(Get), new { id = articleId }, articleId);
            }

            return BadRequest("Failed to create article");
        }
        catch (Exception e)
        {
            _logger.LogError($"Error creating article: {e.Message}");

            return StatusCode((int)HttpStatusCode.InternalServerError, 
                $"An error occurred while creating the article");
        }
    }

    [Authorize]
    [HttpPut]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Summary = "Updates an article", 
        Description = "Updates an article")]
    public async Task<IActionResult> Put([FromBody] ArticleDTO articleDTO)
    {
        try
        {
            var result = await _service.UpdateAsync(articleDTO);

            return result ? NoContent() : NotFound();
        }
        catch (Exception e)
        {
            _logger.LogError($"Error updating article {articleDTO.Id}: {e.Message}");

            return StatusCode((int)HttpStatusCode.InternalServerError, 
                "An error occurred while updating the article");
        }
    }
}