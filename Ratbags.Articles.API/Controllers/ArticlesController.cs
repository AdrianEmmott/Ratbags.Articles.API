using Microsoft.AspNetCore.Mvc;
using Ratbags.Articles.API.Interfaces;
using Ratbags.Shared.DTOs.Events.DTOs.Articles;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Ratbags.Articles.API.Controllers;

[Route("api/articles")]
public class ArticlesController : ControllerBase
{
    private readonly IArticlesService _service;

    public ArticlesController(IArticlesService service)
    {
        _service = service;
    }

    [HttpDelete("{id}")]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Summary = "Deletes an article by id", Description = "Deletes an article by id")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteArticleAsync(id);

        return NoContent();
    }

    [HttpGet]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(List<ArticleDTO>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Summary = "Gets all articles", Description = "Retrieves a list of all articles")]
    public async Task<IActionResult?> Get()
    {
        var articles = await _service.GetAllArticlesAsync();

        if (articles != null)
        {
            return Ok(articles);
        }

        return null;
    }

    [HttpGet("{id}")]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(ArticleDTO), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Summary = "Gets an article by id", Description = "Retrieves a specific article by its id")]
    public async Task<IActionResult> GetArticleById(Guid id)
    {
        var article = await _service.GetArticleByIdAsync(id);

        if (article != null)
        {
            return Ok(article);
        }

        return NotFound();
    }

    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(Guid), (int)HttpStatusCode.Created)]
    [SwaggerOperation(Summary = "Creates an article", Description = "Creates an article")]
    public async Task<IActionResult?> Post([FromBody] CreateArticleDTO createArticleDTO)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var articleId = await _service.CreateArticleAsync(createArticleDTO);

        if (articleId != Guid.Empty)
        {
            var action = CreatedAtAction(nameof(GetArticleById), new { id = articleId }, createArticleDTO);

            Console.WriteLine(action);
            return action;
        }

        return null;
    }

    [HttpPut]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Summary = "Updates an article", Description = "Updates an article")]
    public async Task<IActionResult> Put([FromBody] ArticleDTO articleDTO)
    {
        var result = await _service.UpdateArticleAsync(articleDTO);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}