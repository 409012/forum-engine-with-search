using FluentAssertions;
using Moq;
using Moq.Language.Flow;
using FEwS.Forums.Domain.Models;
using FEwS.Forums.Domain.UseCases.GetForums;
using Xunit;

namespace FEwS.Forums.Domain.Tests.GetForums;

public class GetForumsUseCaseShould
{
    private readonly GetForumsUseCase sut;
    private readonly ISetup<IGetForumsStorage,Task<IEnumerable<Forum>>> getForumsSetup;
    private readonly Mock<IGetForumsStorage> storage;

    public GetForumsUseCaseShould()
    {
        storage = new Mock<IGetForumsStorage>();
        getForumsSetup = storage.Setup(s => s.GetForumsAsync(It.IsAny<CancellationToken>()));

        sut = new GetForumsUseCase(storage.Object);
    }

    [Fact]
    public async Task ReturnForumsFromStorage()
    {
        var forums = new Forum[]
        {
            new() { Id = Guid.Parse("EC04826E-314F-49D9-95ED-56D165D5DF21"), Title = "Test forum 1" },
            new() { Id = Guid.Parse("BFFFBA1A-405F-4B6D-B207-67013A8CC695"), Title = "Test forum 2" }
        };
        getForumsSetup.ReturnsAsync(forums);

        IEnumerable<Forum> actual = await sut.Handle(new GetForumsQuery(), CancellationToken.None);
        actual.Should().BeSameAs(forums);
        storage.Verify(s => s.GetForumsAsync(CancellationToken.None), Times.Once);
        storage.VerifyNoOtherCalls();
    }
}