using FluentAssertions;
using Moq;
using Moq.Language.Flow;
using FEwS.Forums.Domain.Authentication;
using FEwS.Forums.Domain.UseCases.SignOn;
using Microsoft.AspNetCore.Identity;
using Xunit;
using User = FEwS.Forums.Domain.Models.User;

namespace FEwS.Forums.Domain.Tests.SignOn;

public class SignOnUseCaseShould
{
    private readonly SignOnUseCase sut;
    private readonly ISetup<ISignOnStorage,Task<Guid>> createUserSetup;
    private readonly Mock<ISignOnStorage> storage;
    private readonly ISetup<IPasswordHasher<User>, string> generatePasswordHash;

    public SignOnUseCaseShould()
    {
        var passwordHasher = new Mock<IPasswordHasher<User>>();
        generatePasswordHash = passwordHasher.Setup(x => x.HashPassword(It.IsAny<User>(), It.IsAny<string>()));

        storage = new Mock<ISignOnStorage>();
        createUserSetup = storage.Setup(s =>
            s.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));

        sut = new SignOnUseCase(passwordHasher.Object, storage.Object);
    }

    [Fact]
    public async Task CreateUserWithGeneratedPasswordParts()
    {
        var hash = "2";
        generatePasswordHash.Returns(hash);

        await sut.Handle(new SignOnCommand("Test", "qwerty"), CancellationToken.None);
        
        storage.Verify(s => s.CreateUserAsync("Test", hash, It.IsAny<CancellationToken>()), Times.Once);
        storage.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ReturnIdentityOfNewlyCreatedUser()
    {
        generatePasswordHash.Returns("2");
        createUserSetup.ReturnsAsync(Guid.Parse("7483221E-FE0E-44EE-85B6-94D5279A8988"));

        var actual = await sut.Handle(new SignOnCommand("Test", "qwerty"), CancellationToken.None);
        actual.UserId.Should().Be(Guid.Parse("7483221E-FE0E-44EE-85B6-94D5279A8988"));
    }
}