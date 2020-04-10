using System.Threading.Tasks;

namespace HotCode.System.Messaging.interfaces
{
    public interface IEventHandler<in T> where T : IEvent
    {
        Task HandleAsync(T @event, CorrelationContext context);
    }
}