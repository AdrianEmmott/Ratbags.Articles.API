using Microsoft.AspNetCore.Mvc;
using Ratbags.Articles.API.Interfaces;
using Ratbags.Articles.API.Models.API;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Ratbags.Articles.API.Controllers;

[ApiController]
[Route("api/articles/views")]
public class ArticleViewsController : ControllerBase
{
    private readonly IArticleViewsService _service;
    private readonly ILogger<ArticleViewsController> _logger;

    public ArticleViewsController(
        IArticleViewsService service,
        ILogger<ArticleViewsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [SwaggerOperation(Summary = "Gets article view count", 
        Description = "Gets article view count")]
    public async Task<IActionResult> Get(Guid articleId)
    {
        var model = new ArticleViewsGet { ArticleId = articleId };

        var result = await _service.GetAsync(model);

        return Ok(result);
    }

    [HttpPost]    
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [SwaggerOperation(Summary = "Increments view count for an article", 
        Description = "Increments view count on an article")]
    public async Task<IActionResult> Post([FromBody] ArticleViewsCreate model)
    {
        try
        {
            var result = await _service.CreateAsync(model);

            if (result)
            {
                return Ok();
            }

            return BadRequest($"Failed to increment view count for article {model.ArticleId}");
        }
        catch
        {
            return StatusCode((int)HttpStatusCode.InternalServerError,
                $"An error occurred while incrementing the view count for article {model.ArticleId}");
        }
    }
}