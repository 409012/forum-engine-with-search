using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using FEwS.Forums.Domain.Authentication;
using Xunit;

namespace FEwS.Forums.Domain.Tests.Authentication;

public class AesSymmetricEncryptorDecryptorShould
{
    public AesSymmetricEncryptorDecryptorShould()
    {
        var options = new Mock<IOptions<AuthenticationConfiguration>>();
        Moq.Language.Flow.ISetup<IOptions<AuthenticationConfiguration>, AuthenticationConfiguration> getKeySetup = options.Setup(o => o.Value);
        getKeySetup.Returns(new AuthenticationConfiguration
        {
            Base64Key = "Ztt6ikSw4YuXIyvckDB6aA=="
        });
        sut = new AesSymmetricEncryptorDecryptor(options.Object);
    }
    private readonly AesSymmetricEncryptorDecryptor sut;

    [Fact]
    public async Task ReturnMeaningfulEncryptedString()
    {
        string actual = await sut.EncryptAsync("Hello world!", CancellationToken.None);

        actual.Should().NotBeEmpty();
    }

    [Fact]
    public async Task DecryptEncryptedStringWhenKeyIsSame()
    {
        string encrypted = await sut.EncryptAsync("Hello world!", CancellationToken.None);
        string decrypted = await sut.DecryptAsync(encrypted, CancellationToken.None);
        decrypted.Should().Be("Hello world!");
    }

    [Fact]
    public async Task ThrowExceptionWhenDecryptingWithDifferentKey()
    {
        string encrypted = await sut.EncryptAsync("Hello, world!", CancellationToken.None);
        var decryptorWithWrongKey = new AesSymmetricEncryptorDecryptor(
            Options.Create(new AuthenticationConfiguration
            {
                Base64Key = "25JeIMTuGnbI6f5riIeY0w=="
            }));
        await decryptorWithWrongKey.Invoking(s => s.DecryptAsync(encrypted, CancellationToken.None))
            .Should().ThrowAsync<CryptographicException>();
    }
}