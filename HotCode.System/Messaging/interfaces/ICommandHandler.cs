using System.Threading.Tasks;

namespace HotCode.System.Messaging.interfaces
{
    public interface ICommandHandler<in T> where T : ICommand
    {
        Task HandleAsync(T command, CorrelationContext context);
    }
}