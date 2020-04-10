using HotCode.System.Messaging.interfaces;
using Newtonsoft.Json;

namespace HotCode.System.Messaging.RedisMq
{
    public class Envelope<T> where T: IMessage
    {
        public T Message { get; }
        public CorrelationContext Context { get; }
        
        [JsonConstructor]
        public Envelope(T message, CorrelationContext context)
        {
            Message = message;
            Context = context;
        }
        
        public static Envelope<T> Create(T message, CorrelationContext context)
        {
            return new Envelope<T>(message, context);
        }
    }
}