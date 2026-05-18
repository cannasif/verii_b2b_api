using Wms.Application.Common;

namespace Wms.Infrastructure.Services.Identity;

public sealed class PasswordHasher : IPasswordHasher
{
    public string Hash(string value)
    {
        return BCrypt.Net.BCrypt.HashPassword(value);
    }

    public bool Verify(string plainText, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(plainText, hash);
    }
}
