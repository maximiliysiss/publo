using System;
using System.Threading.Tasks;
using FluentAssertions;
using Publo.Postgres.Extensions;
using Xunit;

namespace Publo.Postgres.UnitTests.Extensions;

public class ExceptionExtensionsTests
{
    [Theory]
    [InlineData(typeof(OperationCanceledException), true)]
    [InlineData(typeof(TaskCanceledException), true)]
    [InlineData(typeof(InvalidOperationException), false)]
    public void IsCancel_ShouldReturnExpectedResult(Type exceptionType, bool expected)
    {
        // Arrange
        var exception = (Exception)Activator.CreateInstance(exceptionType)!;

        // Act
        var result = exception.IsCancel();

        // Assert
        result.Should().Be(expected);
    }
}
