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
        aes.Value.Key = configuration.Key;
        aes.Value.GenerateIV();
        byte[] iv = aes.Value.IV;  

        using var encryptedStream = new MemoryStream();
        await encryptedStream.WriteAsync(iv, cancellationToken);
        ICryptoTransform encryptor = aes.Value.CreateEncryptor();
        await using (var stream = new CryptoStream(encryptedStream, encryptor, CryptoStreamMode.Write))
        {
            await stream.WriteAsync(Encoding.UTF8.GetBytes(plainText), cancellationToken);
        }

        return Convert.ToBase64String(encryptedStream.ToArray());
    }

    public async Task<string> DecryptAsync(string encryptedText, CancellationToken cancellationToken)
    {
        byte[] encryptedBytes = Convert.FromBase64String(encryptedText);

        byte[] iv = new byte[IvSize];
        Array.Copy(encryptedBytes, 0, iv, 0, IvSize);

        using var decryptedStream = new MemoryStream();
        ICryptoTransform decryptor = aes.Value.CreateDecryptor(configuration.Key, iv);
        await using (var stream = new CryptoStream(decryptedStream, decryptor, CryptoStreamMode.Write))
        {
            await stream.WriteAsync(encryptedBytes.AsMemory(IvSize), cancellationToken);
        }

        return Encoding.UTF8.GetString(decryptedStream.ToArray());
    }
}