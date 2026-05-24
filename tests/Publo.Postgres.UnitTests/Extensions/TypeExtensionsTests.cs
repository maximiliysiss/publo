using FluentAssertions;
using Publo.Postgres.Extensions;
using Xunit;

namespace Publo.Postgres.UnitTests.Extensions;

public class TypeExtensionsTests
{
    [Fact]
    public void GetVersionFreeFullName_ShouldReturnFullNameAndAssemblyNameWithoutVersion()
    {
        // Arrange

        // Act
        var result = typeof(string).GetVersionFreeFullName();

        // Assert
        result.Should().Be("System.String, System.Private.CoreLib");
    }
}
