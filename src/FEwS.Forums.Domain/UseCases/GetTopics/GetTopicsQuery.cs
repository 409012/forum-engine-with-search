using FEwS.Forums.Domain.Models;
using FEwS.Forums.Domain.Monitoring;
using MediatR;

namespace FEwS.Forums.Domain.UseCases.GetTopics;

public record GetTopicsQuery(Guid ForumId, int Skip, int Take) : IRequest<TopicsPagedResult>, IMonitoredRequest
{
    private const string CounterName = "topics.fetched";
    
    public void MonitorSuccess(DomainMetrics metrics)
    {
        metrics.IncrementCount(CounterName, 1, DomainMetrics.ResultTags(true));
    }

    public void MonitorFailure(DomainMetrics metrics)
    {
        metrics.IncrementCount(CounterName, 1, DomainMetrics.ResultTags(false));
    }
}