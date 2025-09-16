using FEwS.Forums.Domain.Monitoring;
using MediatR;

namespace FEwS.Forums.Domain.UseCases.SignOut;

public record SignOutCommand : IRequest, IMonitoredRequest
{
    private const string CounterName = "account.signedout";
    
    public void MonitorSuccess(DomainMetrics metrics)
    {
        metrics.IncrementCount(CounterName, 1, DomainMetrics.ResultTags(true));
    }

    public void MonitorFailure(DomainMetrics metrics)
    {
        metrics.IncrementCount(CounterName, 1, DomainMetrics.ResultTags(false));
    }
}