using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Publo.Abstraction.Executor;
using Publo.Kafka.Extensions;
using Publo.Kafka.Infrastructure.Hosted;
using Publo.Kafka.Options;

namespace Publo.Kafka.Consumer;

internal sealed class KafkaConsumer : RestartableService
{
    private readonly ILogger<KafkaConsumer> _logger;

    private readonly KafkaPubloOptions _options;

    private readonly IServiceProvider _serviceProvider;

    public KafkaConsumer(
        ILogger<KafkaConsumer> logger,
        IOptions<KafkaPubloOptions> options,
        IServiceProvider serviceProvider) : base(logger)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationTokenSource reloadTokenSource)
    {
        var cancellationToken = reloadTokenSource.Token;

        var factories = new ConcurrentDictionary<Type, Func<IServiceProvider, NonGenericExecutor>>();

        var consumerConfig = _options.ConsumerConfig.AsRandom();

        using var consumer = new LocalConsumer(new ConsumerBuilder<string, string>(consumerConfig).Build());

        consumer.Subscribe(_options.Topic);

        while (!cancellationToken.IsCancellationRequested)
        {
            var message = consumer.Consume(cancellationToken);
            if (message is null)
            {
                _logger.NoMessageReceived();

                if (_options.SkipPolicy is SkipPolicy.Strict)
                    throw new InvalidOperationException("No message received");

                consumer.Commit(message);

                continue;
            }

            var type = message.Message.Key.GetMessageType();
            if (type is null)
            {
                _logger.TypeNotFound(message.Message.Key);

                if (_options.SkipPolicy is SkipPolicy.Strict)
                    throw new InvalidOperationException($"Type not found for key {message.Message.Key}");

                consumer.Commit(message);

                continue;
            }

            var value = JsonSerializer.Deserialize(message.Message.Value, type);
            if (value is null)
            {
                _logger.ValueNotFound(message.Message.Key);

                if (_options.SkipPolicy is SkipPolicy.Strict)
                    throw new InvalidOperationException($"Value not found for key {message.Message.Key}");

                consumer.Commit(message);

                continue;
            }

            using var serviceScope = _serviceProvider.CreateScope();

            var factory = factories.GetOrAdd(type, CreateExecutor);
            var nonGenericExecutor = factory(serviceScope.ServiceProvider);

            await nonGenericExecutor.ExecuteAsync(
                message: value,
                cancellationToken: cancellationToken);

            consumer.Commit(message);
        }

        return;

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

    private sealed class LocalConsumer(IConsumer<string, string> consumer) : IDisposable
    {
        public void Subscribe(string optionsTopic) => consumer.Subscribe(optionsTopic);
        public ConsumeResult<string, string>? Consume(CancellationToken cancellationToken) => consumer.Consume(cancellationToken);
        public void Commit(ConsumeResult<string, string>? message) => consumer.Commit(message);

        public void Dispose()
        {
            consumer.Close();
            consumer.Dispose();
        }
    }
}
