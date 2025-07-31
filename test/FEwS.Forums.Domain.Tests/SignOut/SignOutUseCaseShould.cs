using Moq;
using Moq.Language.Flow;
using FEwS.Forums.Domain.Authentication;
using FEwS.Forums.Domain.UseCases.SignOut;
using Xunit;

namespace FEwS.Forums.Domain.Tests.SignOut;

public class SignOutUseCaseShould
{
    private readonly SignOutUseCase sut;
    private readonly Mock<ISignOutStorage> storage;
    private readonly ISetup<ISignOutStorage, Task> removeSessionSetup;
    private readonly ISetup<IIdentityProvider, IIdentity> currentIdentitySetup;

    public SignOutUseCaseShould()
    {
        storage = new Mock<ISignOutStorage>();
        removeSessionSetup = storage.Setup(s => s.RemoveSessionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()));

        var identityProvider = new Mock<IIdentityProvider>();
        currentIdentitySetup = identityProvider.Setup(p => p.Current);

        sut = new SignOutUseCase(
            identityProvider.Object,
            storage.Object);
    }

    [Fact]
    public async Task RemoveCurrentIdentitySession()
    {
        var sessionId = Guid.Parse("DC933EC9-C7B4-4CAA-A3DE-A394FF55BBEF");
        currentIdentitySetup.Returns(new User(Guid.Empty, sessionId));
        removeSessionSetup.Returns(Task.CompletedTask);

        await sut.Handle(new SignOutCommand(), CancellationToken.None);

        storage.Verify(s => s.RemoveSessionAsync(sessionId, It.IsAny<CancellationToken>()), Times.Once);
        storage.VerifyNoOtherCalls();
    }
}