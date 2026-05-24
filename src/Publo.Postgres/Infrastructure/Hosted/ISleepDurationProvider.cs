using System;

namespace Publo.Postgres.Infrastructure.Hosted;

internal interface ISleepDurationProvider
{
    TimeSpan GetSleepDelay(int attempt);
}
