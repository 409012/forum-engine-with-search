using FEwS.Forums.Domain.Authentication;
using FEwS.Forums.Domain.Monitoring;
using MediatR;

namespace FEwS.Forums.Domain.UseCases.SignOn;

public record SignOnCommand(string UserName, string Password) : IRequest<IIdentity>, IMonitoredRequest
{
    private const string CounterName = "account.signedon";
    
    public void MonitorSuccess(DomainMetrics metrics)
    {
        metrics.IncrementCount(CounterName, 1, DomainMetrics.ResultTags(true));
    }

    public void MonitorFailure(DomainMetrics metrics)
    {
        metrics.IncrementCount(CounterName, 1, DomainMetrics.ResultTags(false));
    }
}