using FEwS.Forums.Domain.Models;
using FEwS.Forums.Domain.Monitoring;
using MediatR;

namespace FEwS.Forums.Domain.UseCases.GetForums;

public record GetForumsQuery : IRequest<IEnumerable<Forum>>, IMonitoredRequest
{
    private const string CounterName = "forums.fetched";
    
    public void MonitorSuccess(DomainMetrics metrics) => 
        metrics.IncrementCount(CounterName, 1, DomainMetrics.ResultTags(true));

    public void MonitorFailure(DomainMetrics metrics) => 
        metrics.IncrementCount(CounterName, 1, DomainMetrics.ResultTags(false));
}