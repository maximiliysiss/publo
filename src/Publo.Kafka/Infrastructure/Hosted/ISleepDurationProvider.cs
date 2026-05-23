using System;

namespace Publo.Kafka.Infrastructure.Hosted;

internal interface ISleepDurationProvider
{
    TimeSpan GetSleepDelay(int attempt);
}
