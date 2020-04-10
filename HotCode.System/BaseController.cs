using System;
using System.Linq;
using System.Threading.Tasks;
using HotCode.System.Messaging;
using HotCode.System.Messaging.interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotCode.System
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class BaseController : ControllerBase
    {
        private const string AcceptLanguageHeader = "accept-language";
        private const string DefaultCulture = "en-us";
        private readonly IBusPublisher _busPublisher;

        public BaseController(IBusPublisher busPublisher)
        {
            _busPublisher = busPublisher;
        }

        private string Culture
            => Request.Headers.ContainsKey(AcceptLanguageHeader)
                ? Request.Headers[AcceptLanguageHeader].First().ToLowerInvariant()
                : DefaultCulture;

        protected async Task PublishAsync<T>(T @event) where T : IEvent
        {
            var context = CorrelationContext.Create<T>(Guid.NewGuid().ToString("N"), UserId, string.Empty,
                HttpContext.TraceIdentifier, HttpContext.Connection.Id, Culture);
            await _busPublisher.PublishAsync(@event, context);
        }

        protected async Task<IActionResult> SendAsync<T>(T command,
            string resourceId = null) where T : ICommand
        {
            var context = GetContext<T>(resourceId);
            await _busPublisher.SendAsync(command, context);
            return Accepted(context);
        }

        protected CorrelationContext GetContext<T>(string resourceId = null) where T : ICommand
        {
            return CorrelationContext.Create<T>(Guid.NewGuid().ToString("D"), UserId,
                resourceId ?? Guid.Empty.ToString("D"), HttpContext.TraceIdentifier, HttpContext.Connection.Id,
                Culture);
        }

        protected string UserId
        {
            get
            {
                var subClaim = User.Claims.FirstOrDefault(c => c.Type == "sub");
                return string.IsNullOrWhiteSpace(subClaim?.Value) ? Guid.Empty.ToString("D") : subClaim.Value;
            }
        }
    }
}