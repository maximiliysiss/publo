using System;
using FluentAssertions;
using Publo.Kafka.Extensions;
using Xunit;

namespace Publo.Kafka.UnitTests.Extensions;

public class TimeSpanExtensionsTests
{
    [Fact]
    public void Jitter_ShouldAddRandomMillisecondsWithinBounds()
    {
        // Arrange
        var value = TimeSpan.FromSeconds(1);

        // Act
        var result = value.Jitter(min: 10, max: 11);

        // Assert
        result.Should().Be(value + TimeSpan.FromMilliseconds(10));
    }
}
