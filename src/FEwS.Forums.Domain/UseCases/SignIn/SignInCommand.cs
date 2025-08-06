using FEwS.Forums.Domain.Authentication;
using FEwS.Forums.Domain.Monitoring;
using MediatR;

namespace FEwS.Forums.Domain.UseCases.SignIn;

public record SignInCommand(string UserName, string Password) : IRequest<(IIdentity identity, string token)>, IMonitoredRequest
{
    private const string CounterName = "account.signedin";
    
    public void MonitorSuccess(DomainMetrics metrics) => 
        metrics.IncrementCount(CounterName, 1, DomainMetrics.ResultTags(true));

    public void MonitorFailure(DomainMetrics metrics) => 
        metrics.IncrementCount(CounterName, 1, DomainMetrics.ResultTags(false));
}