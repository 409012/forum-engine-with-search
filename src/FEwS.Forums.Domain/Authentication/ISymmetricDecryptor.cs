namespace FEwS.Forums.Domain.Authentication;

public interface ISymmetricDecryptor
{
    Task<string> DecryptAsync(string encryptedText, CancellationToken cancellationToken);
}