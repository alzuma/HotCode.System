using HotCode.System.Messaging.interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace HotCode.System.Messaging.RedisMq
{
    public static class Extensions
    {
        public static IBusSubscriber UseRedisMessaging(this IApplicationBuilder app)
            => new BusSubscriber(app);

        public static void AddRedisMessaging(this IServiceCollection services)
        {
            services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
            {
                var configuration = serviceProvider.GetService<IConfiguration>();
                var redisMqOptions = configuration.GetOptions<RedisMqOptions>("RedisMQ");
                var redisConfiguration = ConfigurationOptions.Parse(redisMqOptions.Host);
                return ConnectionMultiplexer.Connect(redisConfiguration);
            });

            services.AddSingleton(serviceProvider =>
            {
                var configuration = serviceProvider.GetService<IConfiguration>();
                return configuration.GetOptions<RedisMqOptions>("RedisMQ");
            });

            services.AddSingleton<IBusPublisher, BusPublisher>();
        }
    }
}