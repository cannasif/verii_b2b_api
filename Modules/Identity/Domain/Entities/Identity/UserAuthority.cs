using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.Identity;

/// <summary>
/// `_old` Role/authority kaydının pragmatik karşılığıdır.
/// AccessControl batch'inde genişletilecek, bu aşamada auth claim üretimine yeterlidir.
/// </summary>
public sealed class UserAuthority : BaseEntity
{
    public string Title { get; set; } = string.Empty;
}
