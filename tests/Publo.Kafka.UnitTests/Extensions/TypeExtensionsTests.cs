using System;
using FluentAssertions;
using Publo.Kafka.Extensions;
using Xunit;

namespace Publo.Kafka.UnitTests.Extensions;

public class TypeExtensionsTests
{
    [Fact]
    public void GetMessageKey_ShouldReturnKey()
    {
        // Arrange

        // Act
        var messageKey = typeof(string).GetMessageKey();

        // Assert
        messageKey.Should().Match("*:System.String, System.Private.CoreLib");
    }

    [Fact]
    public void GetMessageType_ShouldReturnType()
    {
        // Arrange
        var key = $"{Guid.NewGuid():N}:System.String, System.Private.CoreLib";

        // Act
        var type = key.GetMessageType();

        // Assert
        type.Should().Be(typeof(string));
    }
}
