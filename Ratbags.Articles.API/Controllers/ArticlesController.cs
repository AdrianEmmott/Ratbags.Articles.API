using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ratbags.Articles.API.Interfaces;
using Ratbags.Articles.API.Models;
using Ratbags.Articles.API.Models.DB;
using Ratbags.Core.DTOs.Articles;
using Ratbags.Core.Models.Articles;
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

    [Authorize]
    [HttpDelete("{id}")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
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

    [HttpGet("{skip}/{take}")]
    [ProducesResponseType(typeof(List<ArticleDTO>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Summary = "Gets all articles", 
        Description = "Returns a list of all articles or an empty list")]
    public async Task<IActionResult> Get([FromRoute] GetArticlesParameters model)
    {
        var result = await _service.GetAsync(model);

        // debug - simulate slow response
        //System.Threading.Thread.Sleep(200);

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
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType(typeof(Guid), (int)HttpStatusCode.Created)]
    [SwaggerOperation(Summary = "Creates an article", 
        Description = "Creates an article")]
    public async Task<IActionResult> Post([FromBody] CreateArticleModel model)
    {
        try
        {
            var articleId = await _service.CreateAsync(model);

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
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Summary = "Updates an article", 
        Description = "Updates an article")]
    public async Task<IActionResult> Put([FromBody] UpdateArticleModel model)
    {
        try
        {
            var result = await _service.UpdateAsync(model);

            return result ? NoContent() : NotFound();
        }
        catch (Exception e)
        {
            _logger.LogError($"Error updating article {model.Id}: {e.Message}");

            return StatusCode((int)HttpStatusCode.InternalServerError, 
                "An error occurred while updating the article");
        }
    }

    //[HttpGet("seed")]
    //public IActionResult Seed([FromServices] ApplicationDbContext context)
    //{
    //    var seeder = new ArticleSeeder(context);
    //    seeder.SeedArticles(500000); 

    //    return Ok();
    //}
}