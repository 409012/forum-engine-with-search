namespace FEwS.Forums.Domain.Authentication;

internal interface ISymmetricDecryptor
{
    Task<string> DecryptAsync(string encryptedText, CancellationToken cancellationToken);
}