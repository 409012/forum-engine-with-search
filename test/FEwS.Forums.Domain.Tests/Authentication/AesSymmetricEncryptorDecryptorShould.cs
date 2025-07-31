using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Language.Flow;
using FEwS.Forums.Domain.Authentication;
using Xunit;

namespace FEwS.Forums.Domain.Tests.Authentication;

public class AesSymmetricEncryptorDecryptorShould
{
    public AesSymmetricEncryptorDecryptorShould()
    {
        var options = new Mock<IOptions<AuthenticationConfiguration>>();
        getKeySetup = options.Setup(o => o.Value);
        getKeySetup.Returns(new AuthenticationConfiguration
        {
            Base64Key = "Ztt6ikSw4YuXIyvckDB6aA=="
        });
        sut = new AesSymmetricEncryptorDecryptor(options.Object);
    }
    private readonly AesSymmetricEncryptorDecryptor sut;
    private readonly ISetup<IOptions<AuthenticationConfiguration>, AuthenticationConfiguration> getKeySetup;

    [Fact]
    public async Task ReturnMeaningfulEncryptedString()
    {
        var actual = await sut.EncryptAsync("Hello world!", CancellationToken.None);

        actual.Should().NotBeEmpty();
    }

    [Fact]
    public async Task DecryptEncryptedStringWhenKeyIsSame()
    {
        var encrypted = await sut.EncryptAsync("Hello world!", CancellationToken.None);
        var decrypted = await sut.DecryptAsync(encrypted, CancellationToken.None);
        decrypted.Should().Be("Hello world!");
    }

    [Fact]
    public async Task ThrowExceptionWhenDecryptingWithDifferentKey()
    {
        var encrypted = await sut.EncryptAsync("Hello, world!", CancellationToken.None);
        var decryptorWithWrongKey = new AesSymmetricEncryptorDecryptor(
            Options.Create(new AuthenticationConfiguration
            {
                Base64Key = "25JeIMTuGnbI6f5riIeY0w=="
            }));
        await decryptorWithWrongKey.Invoking(s => s.DecryptAsync(encrypted, CancellationToken.None))
            .Should().ThrowAsync<CryptographicException>();
    }
}