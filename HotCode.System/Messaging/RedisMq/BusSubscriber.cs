using System;
using System.Reflection;
using System.Threading.Tasks;
using HotCode.System.Messaging.interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace HotCode.System.Messaging.RedisMq
{
    public class BusSubscriber : IBusSubscriber
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly string _defaultNamespace;
        private readonly IBusPublisher _busPublisher;

        public BusSubscriber(IApplicationBuilder app)
        {
            _serviceProvider = app.ApplicationServices;
            _logger = _serviceProvider.GetRequiredService<ILogger<BusSubscriber>>();
            _connectionMultiplexer = _serviceProvider.GetRequiredService<IConnectionMultiplexer>();

            var options = _serviceProvider.GetRequiredService<RedisMqOptions>();
            _defaultNamespace = options.Namespace;

            _busPublisher = _serviceProvider.GetRequiredService<IBusPublisher>();
        }

        public IBusSubscriber SubscribeEvent<T>(Func<T, HotCodeException, IRejectedEvent> onError = null)
            where T : IEvent
        {
            var subscriber = _connectionMultiplexer.GetSubscriber();
            subscriber.SubscribeAsync(ChanelName<T>(), async (channel, redisValue) =>
            {
                var envelope = redisValue.FromJson<Envelope<T>>();
                using var scope = _serviceProvider.CreateScope();
                var eventHandler = scope.ServiceProvider.GetRequiredService<IEventHandler<T>>();

                Task Handle() => eventHandler.HandleAsync(envelope.Message, envelope.Context);
                await TryHandleAsync(envelope.Message, envelope.Context, Handle, onError);
            });

            return this;
        }

        public IBusSubscriber SubscribeCommand<T>(Func<T, HotCodeException, IRejectedEvent> onError = null)
            where T : ICommand
        {
            var subscriber = _connectionMultiplexer.GetSubscriber();
            subscriber.SubscribeAsync(ChanelName<T>(), async (channel, redisValue) =>
            {
                var envelope = redisValue.FromJson<Envelope<T>>();
                using var scope = _serviceProvider.CreateScope();
                var commandHandler = scope.ServiceProvider.GetRequiredService<ICommandHandler<T>>();

                Task Handle() => commandHandler.HandleAsync(envelope.Message, envelope.Context);
                await TryHandleAsync(envelope.Message, envelope.Context, Handle, onError);
            });

            return this;
        }

        private async Task TryHandleAsync<T>(T message, CorrelationContext context, Func<Task> handle,
            Func<T, HotCodeException, IRejectedEvent> onError = null)
        {
            var messageName = message.GetType().Name;
            try
            {
                _logger.LogInformation($"Handling a message: '{messageName}'");
                await handle();
                _logger.LogInformation($"Handled a message: '{messageName}'");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, exception.Message);

                if (exception is HotCodeException hotCodeException && onError != null)
                {
                    var rejectedEvent = onError(message, hotCodeException);
                    await _busPublisher.PublishAsync(rejectedEvent, context);
                    _logger.LogInformation(
                        $"Published a rejected event: '{rejectedEvent.GetType().Name}' for the message: '{messageName}'.");
                }

                throw new Exception($"Unable to handle a message: '{messageName}'", exception);
            }
        }

        private string ChanelName<T>()
        {
            var @namespace = typeof(T).GetCustomAttribute<MessageNamespaceAttribute>()?.Namespace ??
                             _defaultNamespace;
            var chanelName = $"{@namespace}{typeof(T).Name.Underscore()}".ToLowerInvariant();
            return chanelName;
        }
    }
}