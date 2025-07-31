using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace FEwS.Forums.Domain.Authentication;

internal class AesSymmetricEncryptorDecryptor(IOptions<AuthenticationConfiguration> options) : ISymmetricEncryptor, ISymmetricDecryptor
{
    private const int IvSize = 16;
    private readonly Lazy<Aes> aes = new(Aes.Create);
    private readonly AuthenticationConfiguration configuration = options.Value;
    
    public async Task<string> EncryptAsync(string plainText, CancellationToken cancellationToken)
    {
        var iv = RandomNumberGenerator.GetBytes(IvSize);

        using var encryptedStream = new MemoryStream();
        await encryptedStream.WriteAsync(iv, cancellationToken);
        var encryptor = aes.Value.CreateEncryptor(configuration.Key, iv);
        await using (var stream = new CryptoStream(encryptedStream, encryptor, CryptoStreamMode.Write))
        {
            await stream.WriteAsync(Encoding.UTF8.GetBytes(plainText), cancellationToken);
        }

        return Convert.ToBase64String(encryptedStream.ToArray());
    }

    public async Task<string> DecryptAsync(string encryptedText, CancellationToken cancellationToken)
    {
        var encryptedBytes = Convert.FromBase64String(encryptedText);

        var iv = new byte[IvSize];
        Array.Copy(encryptedBytes, 0, iv, 0, IvSize);

        using var decryptedStream = new MemoryStream();
        var decryptor = aes.Value.CreateDecryptor(configuration.Key, iv);
        await using (var stream = new CryptoStream(decryptedStream, decryptor, CryptoStreamMode.Write))
        {
            await stream.WriteAsync(encryptedBytes.AsMemory(IvSize), cancellationToken);
        }

        return Encoding.UTF8.GetString(decryptedStream.ToArray());
    }
}