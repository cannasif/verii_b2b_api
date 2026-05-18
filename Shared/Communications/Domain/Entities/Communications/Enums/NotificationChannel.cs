using System;

namespace Wms.Domain.Entities.Communications.Enums;

[Flags]
public enum NotificationChannel : byte
{
    Web = 1,
    Terminal = 2,
    Email = 4,
    SMS = 8
}
