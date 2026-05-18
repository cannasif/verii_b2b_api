namespace Wms.Application.Common;

/// <summary>
/// Şifre hashleme davranışını application katmanından ayırır.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string value);
    bool Verify(string plainText, string hash);
}
