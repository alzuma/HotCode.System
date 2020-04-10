using System.Reflection;
using System.Threading.Tasks;
using HotCode.System.Messaging.interfaces;
using StackExchange.Redis;

namespace HotCode.System.Messaging.RedisMq
{
    public class BusPublisher : IBusPublisher
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly string _defaultNamespace;

        public BusPublisher(IConnectionMultiplexer connectionMultiplexer, RedisMqOptions redisMqOptions)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _defaultNamespace = redisMqOptions.Namespace;
        }

        public async Task PublishAsync<T>(T @event, CorrelationContext context) where T : IEvent
        {
            var envelop = Envelope<T>.Create(@event, context);
            await _connectionMultiplexer.GetSubscriber().PublishAsync(ChanelName(@event), envelop.ToJson());
        }

        public async Task SendAsync<T>(T command, CorrelationContext context) where T : ICommand
        {
            var envelop = Envelope<T>.Create(command, context);
            await _connectionMultiplexer.GetSubscriber().PublishAsync(ChanelName(command), envelop.ToJson());
        }

        private string ChanelName<T>(T message)
        {
            var @namespace = message.GetType().GetCustomAttribute<MessageNamespaceAttribute>()?.Namespace ??
                             _defaultNamespace;
            var chanelName = $"{@namespace}{typeof(T).Name.Underscore()}".ToLowerInvariant();
            return chanelName;
        }
    }
}