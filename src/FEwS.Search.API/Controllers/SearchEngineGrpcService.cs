using FEwS.Search.API.Grpc;
using FEwS.Search.Domain.UseCases.Index;
using FEwS.Search.Domain.UseCases.Search;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;

namespace FEwS.Search.API.Controllers;

internal class SearchEngineGrpcService(IMediator mediator) : SearchEngine.SearchEngineBase
{
    public override async Task<Empty> Index(IndexRequest request, ServerCallContext context)
    {
        var command = new IndexCommand(
            Guid.Parse(request.Id),
            request.Type switch
            {
                SearchEntityType.ForumTopic => Domain.Models.SearchEntityType.ForumTopic,
                SearchEntityType.ForumComment => Domain.Models.SearchEntityType.ForumComment,
                SearchEntityType.Unknown => throw new NotImplementedException(),
                _ => throw new ArgumentOutOfRangeException(request.Type.ToString())
            },
            request.Title,
            request.Text);
        await mediator.Send(command, context.CancellationToken);
        return new Empty();
    }

    public override async Task<SearchResponse> Search(SearchRequest request, ServerCallContext context)
    {
        var query = new SearchQuery(request.Query);
        (IEnumerable<Domain.Models.SearchResult> resources, int totalCount) = await mediator.Send(query, context.CancellationToken);
        return new SearchResponse
        {
            Total = totalCount,
            Entities =
            {
                resources.Select(r => new SearchResponse.Types.SearchResultEntity
                {
                    Id = r.EntityId.ToString(),
                    Type = r.EntityType switch
                    {
                        Domain.Models.SearchEntityType.ForumTopic => SearchEntityType.ForumTopic,
                        Domain.Models.SearchEntityType.ForumComment => SearchEntityType.ForumComment,
                        _ => SearchEntityType.Unknown
                    },
                    Highlights = { r.Highlights }
                })
            }
        };
    }
}