using System;

namespace Publo.Postgres.Infrastructure.DateTime;

internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset GetNow() => DateTimeOffset.UtcNow;
}
