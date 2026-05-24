using System;

namespace Publo.Postgres.Infrastructure.DateTime;

internal interface IDateTimeProvider
{
    DateTimeOffset GetNow();
}
