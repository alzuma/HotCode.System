using System;
using Newtonsoft.Json;

namespace HotCode.System.Messaging
{
    public class CorrelationContext
    {
        public string Id { get; }
        public string UserId { get; }
        public string ResourceId { get; }
        public string TraceId { get; }
        public string Name { get; }
        public string Origin { get; }
        public string Culture { get; }
        public int Retries { get; set; }
        public DateTime CreatedAt { get; }

        public CorrelationContext()
        {
        }

        private CorrelationContext(string id)
        {
            Id = id;
        }

        [JsonConstructor]
        private CorrelationContext(
            string id,
            string userId,
            string resourceId,
            string traceId,
            string name,
            string origin,
            string culture,
            int retries)
        {
            Id = id;
            UserId = userId;
            ResourceId = resourceId;
            TraceId = traceId;
            Name = string.IsNullOrWhiteSpace(name) ? string.Empty : GetName(name);
            Origin = string.IsNullOrWhiteSpace(origin) ? string.Empty :
                origin.StartsWith("/") ? origin.Remove(0, 1) : origin;
            Culture = culture;
            Retries = retries;
            CreatedAt = DateTime.UtcNow;
        }

        public static CorrelationContext Create<T>(string id, string userId, string resourceId, string origin,
            string traceId, string culture)
        {
            return new CorrelationContext(
                id,
                userId,
                resourceId,
                traceId,
                typeof(T).Name,
                origin, culture,
                0);
        }

        private static string GetName(string name)
            => name.Underscore().ToLowerInvariant();
    }
}