using System;
using System.Collections.Generic;
using Confluent.Kafka;

namespace Publo.Kafka.Extensions;

internal static class ConsumerConfigExtensions
{
    public static ConsumerConfig AsRandom(this Dictionary<string, string> config)
    {
        var groupId = $"publo-random-{Guid.NewGuid():N}";

        return new ConsumerConfig(config)
        {
            GroupId = groupId,
            EnableAutoCommit = false,
        };
    }
}
