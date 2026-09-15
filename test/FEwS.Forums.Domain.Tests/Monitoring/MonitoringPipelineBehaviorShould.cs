using System.Diagnostics.Metrics;
using FEwS.Forums.Domain.Monitoring;
using FEwS.Forums.Domain.UseCases.SignIn;
using FEwS.Forums.Domain.UseCases.SignOn;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FEwS.Forums.Domain.Tests.Monitoring;

public sealed class MonitoringPipelineBehaviorShould : IDisposable
{
    private const string UserName = "private-user-name";
    private const string Password = "private-test-password";
    private readonly Meter meter = new("MonitoringPipelineBehaviorShould");
    private readonly Mock<ILogger<MonitoringPipelineBehavior<object, bool>>> logger = new();
    private readonly MonitoringPipelineBehavior<object, bool> sut;

    public MonitoringPipelineBehaviorShould()
    {
        var meterFactory = new Mock<IMeterFactory>();
        meterFactory.Setup(factory => factory.Create(It.IsAny<MeterOptions>())).Returns(meter);
        sut = new MonitoringPipelineBehavior<object, bool>(new DomainMetrics(meterFactory.Object), logger.Object);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task LogCommandNameWithoutCredentialsOnSuccess(bool signIn)
    {
        object command = CreateCommand(signIn);

        bool result = await sut.Handle(command, _ => Task.FromResult(true), CancellationToken.None);

        result.Should().BeTrue();
        AssertSafeLog(command, LogLevel.Information, null);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task LogCommandNameWithoutCredentialsOnFailure(bool signIn)
    {
        object command = CreateCommand(signIn);
        var exception = new InvalidOperationException("Storage unavailable");

        InvalidOperationException thrown = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.Handle(command, _ => Task.FromException<bool>(exception), CancellationToken.None));

        thrown.Should().BeSameAs(exception);
        AssertSafeLog(command, LogLevel.Error, exception);
    }

    public void Dispose()
    {
        meter.Dispose();
    }

    private static object CreateCommand(bool signIn)
    {
        return signIn
            ? new SignInCommand(UserName, Password)
            : new SignOnCommand(UserName, Password);
    }

    private void AssertSafeLog(object command, LogLevel level, Exception? exception)
    {
        IInvocation invocation = logger.Invocations.Should().ContainSingle().Subject;
        invocation.Arguments[0].Should().Be(level);
        invocation.Arguments[3].Should().BeSameAs(exception);

        object state = invocation.Arguments[2];
        string message = state.ToString() ?? string.Empty;
        message.Should().Contain(command.GetType().Name);
        message.Should().NotContain(UserName).And.NotContain(Password);

        IEnumerable<KeyValuePair<string, object?>> properties = state.Should()
            .BeAssignableTo<IEnumerable<KeyValuePair<string, object?>>>().Subject;
        properties.Should().Contain(property => property.Key == "CommandName"
            && Equals(property.Value, command.GetType().Name));
        properties.Should().OnlyContain(property => property.Value is string);
        properties.Should().NotContain(property => Equals(property.Value, command)
            || Equals(property.Value, UserName) || Equals(property.Value, Password));
    }
}
