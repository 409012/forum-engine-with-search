using AutoMapper;
using FEwS.Forums.API.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using FEwS.Forums.Domain.UseCases.CreateForum;
using FEwS.Forums.Domain.UseCases.CreateTopic;
using FEwS.Forums.Domain.UseCases.GetForums;
using FEwS.Forums.Domain.UseCases.GetTopics;

namespace FEwS.Forums.API.Controllers;

[ApiController]
[Route("forums")]
public class ForumController(
    ISender mediator,
    IMapper mapper) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(201, Type = typeof(Forum))]
    public async Task<IActionResult> CreateForumAsync(
        [FromBody] CreateForumRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateForumCommand(request.Title);
        Domain.Models.Forum forum = await mediator.Send(command, cancellationToken);
        return CreatedAtRoute(nameof(GetForumsAsync), mapper.Map<Forum>(forum));
    }

    [HttpGet(Name = nameof(GetForumsAsync))]
    [ProducesResponseType(200, Type = typeof(Forum[]))]
    public async Task<IActionResult> GetForumsAsync(
        CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Models.Forum> forums = await mediator.Send(new GetForumsQuery(), cancellationToken);
        return Ok(forums.Select(mapper.Map<Forum>));
    }

    [HttpPost("{forumId:guid}/topics")]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(410)]
    [ProducesResponseType(201, Type = typeof(Topic))]
    public async Task<IActionResult> CreateTopicAsync(
        Guid forumId,
        [FromBody] CreateTopicRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateTopicCommand(forumId, request.Title);
        Domain.Models.Topic topic = await mediator.Send(command, cancellationToken);
        return CreatedAtRoute(nameof(GetForumsAsync), mapper.Map<Topic>(topic));
    }

    [HttpGet("{forumId:guid}/topics")]
    [ProducesResponseType(400)]
    [ProducesResponseType(410)]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetTopicsAsync(
        [FromRoute] Guid forumId,
        [FromQuery] int skip,
        [FromQuery] int take,
        CancellationToken cancellationToken)
    {
        var query = new GetTopicsQuery(forumId, skip, take);
        Domain.Models.TopicsPagedResult topicsPagedResult = await mediator.Send(query, cancellationToken);
        return Ok(mapper.Map<TopicsPagedResult>(topicsPagedResult));
    }
}