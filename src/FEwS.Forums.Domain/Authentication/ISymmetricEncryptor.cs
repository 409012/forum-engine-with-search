namespace FEwS.Forums.Domain.Authentication;

internal interface ISymmetricEncryptor
{
    Task<string> EncryptAsync(string plainText, CancellationToken cancellationToken);
}