using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Publo.Abstraction.Executor;
using Publo.Postgres.Extensions;
using Publo.Postgres.Entities;
using Publo.Postgres.Infrastructure.DateTime;
using Publo.Postgres.Infrastructure.Hosted;
using Publo.Postgres.Options;
using Publo.Postgres.Repositories;

namespace Publo.Postgres.Runners;

internal sealed class RunnerHostedService : RestartableService
{
    private readonly PostgresPubloOptions _options;

    private readonly IServiceProvider _serviceProvider;

    private readonly ILogger<RunnerHostedService> _logger;

    private readonly ClientId _clientId = ClientId.New();
    private readonly DateTimeOffset? _from;

    private readonly ConcurrentDictionary<Type, Func<IServiceProvider, NonGenericExecutor>> _factories = [];

    public RunnerHostedService(
        ILogger<RunnerHostedService> logger,
        IOptions<PostgresPubloOptions> options,
        IServiceProvider serviceProvider,
        IDateTimeProvider dateTimeProvider) : base(logger)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _options = options.Value;

        _from = _options.OffsetPolicy switch
        {
            OffsetPolicy.Latest => dateTimeProvider.GetNow(),
            OffsetPolicy.Earliest => null,
            _ => dateTimeProvider.GetNow(),
        };
    }

    protected override async Task ExecuteAsync(CancellationTokenSource reloadTokenSource)
    {
        var cancellationToken = reloadTokenSource.Token;

        using (var scope = _serviceProvider.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IPostgresRepository>();

            await repository.CreateAsync(_clientId, cancellationToken);
            _logger.PostgresRunnerClientRegistered(_clientId.Value);
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();

            var localRepository = scope.ServiceProvider.GetRequiredService<IPostgresRepository>();

            await HandleLoopAsync(scope, localRepository, cancellationToken);

            await Task.Delay(_options.PollingInterval, cancellationToken);
        }
    }

    private async Task HandleLoopAsync(IServiceScope scope, IPostgresRepository repository, CancellationToken cancellationToken)
    {
        bool succeededProcessing;
        do succeededProcessing = await TryProcessingMessagesAsync(scope, repository, cancellationToken);
        while (succeededProcessing && !cancellationToken.IsCancellationRequested);
    }

    private async Task<bool> TryProcessingMessagesAsync(
        IServiceScope scope,
        IPostgresRepository repository,
        CancellationToken cancellationToken)
    {
        var message = await repository.GetAsync(_clientId, _from, cancellationToken);
        if (message is null)
            return false;

        var factory = _factories.GetOrAdd(message.Type, CreateExecutor);
        var nonGenericExecutor = factory(scope.ServiceProvider);

        await nonGenericExecutor.ExecuteAsync(
            message: message.Payload,
            cancellationToken: cancellationToken);
        _logger.PostgresMessageHandled(message.Id.Value, _clientId.Value);

        await repository.CommitAsync(message.Id, _clientId, cancellationToken);

        return true;

        static Func<IServiceProvider, NonGenericExecutor> CreateExecutor(Type type)
        {
            var executorType = typeof(IPubloExecutor<>).MakeGenericType(type);

            var method = executorType.GetMethod(name: nameof(IPubloExecutor<>.HandleAsync));

            if (method is null)
            {
                var message = $"No method named {nameof(IPubloExecutor<>.HandleAsync)} found on {executorType.FullName}";
                throw new InvalidOperationException(message);
            }

            var delegateType = typeof(Func<,,>).MakeGenericType(type, typeof(CancellationToken), typeof(Task));

            return sp => NonGenericExecutor(sp, executorType, method, delegateType);
        }

        static NonGenericExecutor NonGenericExecutor(IServiceProvider sp, Type executorType, MethodInfo method, Type delegateType)
        {
            var executor = sp.GetRequiredService(executorType);

            var @delegate = method.CreateDelegate(delegateType, executor);

            return new NonGenericExecutor(@delegate);
        }
    }

    private sealed class NonGenericExecutor(Delegate executeAsync)
    {
        public Task ExecuteAsync(object message, CancellationToken cancellationToken)
            => (Task)executeAsync.DynamicInvoke(message, cancellationToken)!;
    }
}
