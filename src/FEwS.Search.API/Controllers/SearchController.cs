using MediatR;
using Microsoft.AspNetCore.Mvc;
using FEwS.Search.Domain.Models;
using FEwS.Search.Domain.UseCases.Index;
using FEwS.Search.Domain.UseCases.Search;

namespace FEwS.Search.API.Controllers;

public class SearchController(IMediator mediator) : ControllerBase
{
    [HttpPost("index")]
    public async Task<IActionResult> Index(
        [FromBody] SearchEntity searchEntity,
        CancellationToken cancellationToken)
    {
        var command = new IndexCommand(
            searchEntity.EntityId, searchEntity.EntityType, searchEntity.Title, searchEntity.Text);
        await mediator.Send(command, cancellationToken);
        return Ok();
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        string query,
        CancellationToken cancellationToken)
    {
        (IEnumerable<SearchResult> resources, int totalCount) = await mediator.Send(new SearchQuery(query), cancellationToken);
        return Ok(new {resources, totalCount});
    }
}