using System.Threading.Tasks;

namespace HotCode.System.Messaging.interfaces
{
    public interface IBusPublisher
    {
        Task PublishAsync<T>(T @event, CorrelationContext context) where T : IEvent;
        Task SendAsync<T>(T command, CorrelationContext context) where T : ICommand;
    }
}