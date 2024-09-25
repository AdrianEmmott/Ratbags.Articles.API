using Microsoft.AspNetCore.Mvc;
using Ratbags.Articles.API.Interfaces;
using Ratbags.Shared.DTOs.Events.DTOs.Articles;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Ratbags.Articles.API.Controllers;

[ApiController]
//[Route("api/[controller]")]
[Route("api/articles")]
public class ArticlesController : ControllerBase
{
    private readonly IArticlesService _service;

    public ArticlesController(IArticlesService service)
    {
        _service = service;
    }

    [HttpPost("Create")]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(Guid), (int)HttpStatusCode.Created)]
    [SwaggerOperation(Summary = "Creates an article", Description = "Creates an article")]
    public async Task<IActionResult> Create([FromBody] CreateArticleDTO createArticleDTO)
    {
        try
        {
            if (createArticleDTO != null)
            {
                var articleId = await _service.CreateArticleAsync(createArticleDTO);
                return CreatedAtAction(nameof(Create),
                    new
                    {
                        id = articleId
                    });
            }

            return NotFound();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
            throw;
        }
    }

    [HttpGet("all")]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(List<ArticleDTO>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Summary = "Gets all articles", Description = "Retrieves a list of all articles")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var articles = await _service.GetAllArticlesAsync();

            if (articles != null)
            {
                return Ok(articles);
            }

            return NotFound();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
            throw;
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(ArticleDTO), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Summary = "Gets an article by id", Description = "Retrieves a specific article by its id")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var article = await _service.GetArticleByIdAsync(id);

            if (article != null)
            {
                return Ok(article);
            }

            return NotFound();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
            throw;
        }
    }

    [HttpPost("Update")]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Summary = "Updates an article", Description = "Updates an article")]
    public async Task<IActionResult> Update([FromBody] ArticleDTO articleDTO)
    {
        try
        {
            if (articleDTO != null)
            {
                if (articleDTO.Id != null)
                {
                    await _service.UpdateArticleAsync(articleDTO);
                    return NoContent();
                }

                return BadRequest("Article id missing");
            }
            
            return NotFound();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
            throw;
        }
    }
}