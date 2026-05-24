using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Publo.Abstraction.Executor;
using Publo.Postgres.Entities;
using Publo.Postgres.Environment;
using Publo.Postgres.Extensions;
using Publo.Postgres.Infrastructure.Database;
using Publo.Postgres.Infrastructure.DateTime;
using Publo.Postgres.IntegrationTests.DbHelper;
using Publo.Postgres.IntegrationTests.Shared.Fixture;
using Publo.Postgres.Options;
using Publo.Postgres.Repositories;
using Xunit;

namespace Publo.Postgres.IntegrationTests.Repositories;

[Collection(nameof(IntegrationTestCollection))]
public sealed class PostgresRepositoryTests : IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;

    private readonly PubloDbHelper _dbHelper;

    public PostgresRepositoryTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;

        _dbHelper = new PubloDbHelper(
            connectionFactory: _fixture.Services.GetRequiredService<IConnectionFactory>(),
            options: _fixture.Services.GetRequiredService<IOptions<PostgresPubloOptions>>());
    }

    [Fact]
    public async Task AddAsync_ShouldAddNewRecord()
    {
        // Arrange
        var repository = Create();

        var message = new TestEvent(Id: Random.Shared.Next(), Name: Guid.NewGuid().ToString());

        // Act
        await repository.AddAsync(message, CancellationToken.None);

        // Assert
        var rows = await _dbHelper.GetMessagesAsync<TestEvent>();

        var row = rows
            .Where(c => JsonSerializer.Deserialize<TestEvent>(c.Payload) == message)
            .Should()
            .ContainSingle()
            .Which;

        _dbHelper.Track(new MessageId(row.Id));

        row.Type.Should().Be(typeof(TestEvent).GetVersionFreeFullName());
        row.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task CreateAsync_ShouldAddClient()
    {
        // Arrange
        var repository = Create();

        var clientId = _dbHelper.Track(new ClientId(Guid.NewGuid()));

        // Act
        await repository.CreateAsync(clientId, CancellationToken.None);

        // Assert
        var row = await _dbHelper.GetClientAsync(clientId);

        row.Should().NotBeNull();
        row.Id.Should().Be(clientId.Value);
        row.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task CommitAsync_ShouldAddHandledRecord()
    {
        // Arrange
        var repository = Create();

        var clientId = _dbHelper.Track(new ClientId(Guid.NewGuid()));
        var messageId = _dbHelper.Track(new MessageId(await _dbHelper.AddMessageAsync(new TestEvent(1, Guid.NewGuid().ToString()))));

        // Act
        await repository.CommitAsync(messageId, clientId, CancellationToken.None);

        // Assert
        var row = await _dbHelper.GetHandledAsync(clientId, messageId);

        row.Should().NotBeNull();
        row.ClientId.Should().Be(clientId.Value);
        row.MessageId.Should().Be(messageId.Value);
        row.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenThereIsNoRecord()
    {
        // Arrange
        var repository = Create();

        var clientId = new ClientId(Guid.NewGuid());

        // Act
        var message = await repository.GetAsync(clientId, DateTimeOffset.UtcNow.AddDays(1), CancellationToken.None);

        // Assert
        message.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_ShouldReturnOldestUnhandledRecord()
    {
        // Arrange
        var repository = Create();

        var clientId = new ClientId(Guid.NewGuid());
        var from = DateTimeOffset.UtcNow.AddSeconds(-1);

        var firstPayload = new TestEvent(1, Guid.NewGuid().ToString());
        var secondPayload = new TestEvent(2, Guid.NewGuid().ToString());

        var firstMessageId = await _dbHelper.AddMessageAsync(firstPayload, DateTimeOffset.UtcNow);
        _ = await _dbHelper.AddMessageAsync(secondPayload, DateTimeOffset.UtcNow.AddSeconds(1));

        // Act
        var message = await repository.GetAsync(clientId, from, CancellationToken.None);

        // Assert
        message.Should().NotBeNull();
        message.Id.Should().Be(new MessageId(firstMessageId));
        message.Payload.Should().BeEquivalentTo(firstPayload);
        message.Type.Should().Be(typeof(TestEvent));
    }

    [Fact]
    public async Task GetAsync_ShouldSkipHandledRecordForClient()
    {
        // Arrange
        var repository = Create();

        var clientId = new ClientId(Guid.NewGuid());
        var from = DateTimeOffset.UtcNow.AddSeconds(-1);

        var firstMessageId = new MessageId(await _dbHelper.AddMessageAsync(new TestEvent(1, Guid.NewGuid().ToString())));
        var secondPayload = new TestEvent(2, Guid.NewGuid().ToString());
        var secondMessageId = await _dbHelper.AddMessageAsync(secondPayload, DateTimeOffset.UtcNow.AddSeconds(1));

        await _dbHelper.AddHandledAsync(clientId, firstMessageId);

        // Act
        var message = await repository.GetAsync(clientId, from, CancellationToken.None);

        // Assert
        message.Should().NotBeNull();
        message.Id.Should().Be(new MessageId(secondMessageId));
        message.Payload.Should().BeEquivalentTo(secondPayload);
    }

    [Fact]
    public async Task GetAsync_ShouldRespectFromOffset()
    {
        // Arrange
        var repository = Create();

        var clientId = new ClientId(Guid.NewGuid());
        var from = DateTimeOffset.UtcNow;

        _ = await _dbHelper.AddMessageAsync(new TestEvent(1, Guid.NewGuid().ToString()), from.AddMinutes(-1));

        var expectedPayload = new TestEvent(2, Guid.NewGuid().ToString());
        var expectedMessageId = await _dbHelper.AddMessageAsync(expectedPayload, from.AddMinutes(1));

        // Act
        var message = await repository.GetAsync(clientId, from, CancellationToken.None);

        // Assert
        message.Should().NotBeNull();
        message.Id.Should().Be(new MessageId(expectedMessageId));
        message.Payload.Should().BeEquivalentTo(expectedPayload);
        message.CreatedAt.Should().BeAfter(from);
    }

    private IPostgresRepository Create() => new PostgresRepository(
        connectionFactory: _fixture.Services.GetRequiredService<IConnectionFactory>(),
        options: _fixture.Services.GetRequiredService<IOptions<PostgresPubloOptions>>(),
        dateTimeProvider: _fixture.Services.GetRequiredService<IDateTimeProvider>(),
        logger: NullLogger<PostgresRepository>.Instance);

    public Task InitializeAsync()
        => _fixture.Services.GetRequiredService<IInfrastructureReadinessWaiter>().WaitAsync(CancellationToken.None);

    public async Task DisposeAsync() => await _dbHelper.DisposeAsync();

    public sealed record TestEvent(int Id, string Name);

    public sealed class Executor : IPubloExecutor<TestEvent>
    {
        public Task HandleAsync(TestEvent message, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
