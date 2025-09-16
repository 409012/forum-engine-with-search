using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Language.Flow;
using FEwS.Forums.Domain.Authentication;
using Xunit;

namespace FEwS.Forums.Domain.Tests.Authentication;

public class AuthenticationServiceShould
{
    private readonly AuthenticationService sut;
    private readonly ISetup<ISymmetricDecryptor,Task<string>> setupDecrypt;
    private readonly ISetup<IAuthenticationStorage,Task<Session?>> findSessionSetup;

    public AuthenticationServiceShould()
    {
        var decryptor = new Mock<ISymmetricDecryptor>();
        setupDecrypt = decryptor.Setup(d => d.DecryptAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()));

        var storage = new Mock<IAuthenticationStorage>();
        findSessionSetup = storage.Setup(s => s.FindSessionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()));

        var options = new Mock<IOptions<AuthenticationConfiguration>>();
        options
            .Setup(o => o.Value)
            .Returns(new AuthenticationConfiguration
            {
                Base64Key = "XtDotH86WLjaEoFev6uZFN/3C0EQIApoD+5iqqmPtpg="
            });
        
        sut = new AuthenticationService(
            decryptor.Object,
            storage.Object,
            NullLogger<AuthenticationService>.Instance);
    }

    [Fact]
    public async Task ReturnGuestIdentityWhenTokenCannotBeDecrypted()
    {
        setupDecrypt.Throws<CryptographicException>();
        IIdentity actual = await sut.AuthenticateAsync("hahaha-bad-token", CancellationToken.None);

        actual.Should().BeEquivalentTo(User.Guest);
    }

    [Fact]
    public async Task ReturnGuestIdentityWhenTokenIsInvalid()
    {
        setupDecrypt.ReturnsAsync("not-a-guid");
        IIdentity actual = await sut.AuthenticateAsync("bad-token", CancellationToken.None);

        actual.Should().BeEquivalentTo(User.Guest);
    }

    [Fact]
    public async Task ReturnGuestIdentityWhenSessionNotFound()
    {
        setupDecrypt.ReturnsAsync("EE88598F-5896-4885-BE61-9B31171EAB9E");
        findSessionSetup.ReturnsAsync(() => null);

        IIdentity actual = await sut.AuthenticateAsync("good-token", CancellationToken.None);
        actual.Should().BeEquivalentTo(User.Guest);
    }

    [Fact]
    public async Task ReturnGuestIdentityWhenSessionIsExpired()
    {
        setupDecrypt.ReturnsAsync("EE88598F-5896-4885-BE61-9B31171EAB9E");
        findSessionSetup.ReturnsAsync(new Session
        {
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(-1)
        });

        IIdentity actual = await sut.AuthenticateAsync("good-token", CancellationToken.None);
        actual.Should().BeEquivalentTo(User.Guest);
    }

    [Fact]
    public async Task ReturnIdentityWhenSessionIsValid()
    {
        var sessionId = Guid.Parse("B08B27F8-76DA-44B3-B264-E59A2858669E");
        var userId = Guid.Parse("7C4F18A3-52D8-473A-990E-BABD142876D9");

        setupDecrypt.ReturnsAsync("B08B27F8-76DA-44B3-B264-E59A2858669E");
        findSessionSetup.ReturnsAsync(new Session
        {
            UserId = userId,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1)
        });

        IIdentity actual = await sut.AuthenticateAsync("good-token", CancellationToken.None);

        actual.Should().BeEquivalentTo(new User(userId, sessionId));
    }
}