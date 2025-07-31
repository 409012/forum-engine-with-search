using FEwS.Forums.Domain.Models;
using FEwS.Forums.Domain.Monitoring;
using MediatR;

namespace FEwS.Forums.Domain.UseCases.CreateForum;

public record CreateForumCommand(string Title) : IRequest<Forum>, IMonitoredRequest
{
    private const string CounterName = "forums.created";
    
    public void MonitorSuccess(DomainMetrics metrics) => 
        metrics.IncrementCount(CounterName, 1, DomainMetrics.ResultTags(true));

    public void MonitorFailure(DomainMetrics metrics) => 
        metrics.IncrementCount(CounterName, 1, DomainMetrics.ResultTags(false));
}