using FEwS.Forums.Domain.Authentication;
using Microsoft.AspNetCore.DataProtection;

namespace FEwS.Forums.API.Authentication;

internal class DataProtectionTokenProtector(IDataProtectionProvider provider) : ISymmetricEncryptor, ISymmetricDecryptor
{
    private readonly IDataProtector protector = provider.CreateProtector("FEwS.Forums.Authentication.Session.v1");

    public Task<string> EncryptAsync(string plainText, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(protector.Protect(plainText));
    }

    public Task<string> DecryptAsync(string encryptedText, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(protector.Unprotect(encryptedText));
    }
}
