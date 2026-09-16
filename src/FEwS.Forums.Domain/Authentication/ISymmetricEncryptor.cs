namespace FEwS.Forums.Domain.Authentication;

public interface ISymmetricEncryptor
{
    Task<string> EncryptAsync(string plainText, CancellationToken cancellationToken);
}