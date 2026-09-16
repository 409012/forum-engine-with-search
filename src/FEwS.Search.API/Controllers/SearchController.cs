using MediatR;
using Microsoft.AspNetCore.Mvc;
using FEwS.Search.Domain.Models;
using FEwS.Search.Domain.UseCases.Index;
using FEwS.Search.Domain.UseCases.Search;
using FEwS.Search.API.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace FEwS.Search.API.Controllers;

public class SearchController(IMediator mediator) : ControllerBase
{
    [HttpPost("index")]
    [Authorize(Policy = IndexingAuthenticationOptions.SchemeName)]
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
        [FromQuery] SearchEntityType[]? searchIn,
        CancellationToken cancellationToken,
        int skip = 0,
        int size = SearchQuery.DefaultSize)
    {
        searchIn ??= [];
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(query) || skip < 0
            || size is < 1 or > SearchQuery.MaximumSize
            || searchIn.Any(entityType => !Enum.IsDefined(entityType)))
        {
            return BadRequest();
        }

        var searchQuery = new SearchQuery(query, searchIn.Distinct().ToArray(), skip, size);
        (IEnumerable<SearchResult> resources, int totalCount) = await mediator.Send(searchQuery, cancellationToken);
        return Ok(new {resources, totalCount});
    }
}
