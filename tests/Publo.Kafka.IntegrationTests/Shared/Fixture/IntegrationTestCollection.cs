using Xunit;

namespace Publo.Kafka.IntegrationTests.Shared.Fixture;

[CollectionDefinition(nameof(IntegrationTestCollection), DisableParallelization = true)]
public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>;
