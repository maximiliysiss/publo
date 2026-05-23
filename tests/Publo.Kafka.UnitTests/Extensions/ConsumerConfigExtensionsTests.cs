using System.Collections.Generic;
using Confluent.Kafka;
using FluentAssertions;
using Publo.Kafka.Extensions;
using Xunit;

namespace Publo.Kafka.UnitTests.Extensions;

public class ConsumerConfigExtensionsTests
{
    [Fact]
    public void AsRandom_ShouldAddGroupId()
    {
        // Arrange
        var consumerConfig = new Dictionary<string, string>();

        // Act
        var asRandom = consumerConfig.AsRandom();

        // Assert
        asRandom.GroupId.Should().Match("publo-random-*");
        asRandom.EnableAutoCommit.Should().BeFalse();
    }

    [Fact]
    public void AsRandom_ShouldOverrideGroupId_WhenThatExists()
    {
        // Arrange
        var consumerConfig = new Dictionary<string, string>
        {
            { "group.id", "test" },
            { "enable.auto.commit", "true" },
            { "auto.offset.reset", "Earliest" },
        };

        // Act
        var asRandom = consumerConfig.AsRandom();

        // Assert
        asRandom.GroupId.Should().Match("publo-random-*");
        asRandom.EnableAutoCommit.Should().BeFalse();
        asRandom.AutoOffsetReset.Should().Be(AutoOffsetReset.Earliest);
    }
}
