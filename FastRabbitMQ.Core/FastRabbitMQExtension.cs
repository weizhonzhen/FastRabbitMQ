using RabbitMQ.Client;
using System;
using Microsoft.Extensions.DependencyInjection;
using FastRabbitMQ.Core;
using FastRabbitMQ.Core.Aop;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FastRabbitMQExtension
    {
        private static ConfigData config;
        public static IServiceCollection AddFastRabbitMQ(this IServiceCollection serviceCollection, Action<ConfigData> action)
        {
            config = new ConfigData();
            action(config);

            IConnectionFactory factory = new ConnectionFactory
            {
                HostName = config.Host,
                Port = config.Port,
                UserName = config.UserName,
                Password = config.PassWord,
                VirtualHost = config.VirtualHost,
                AutomaticRecoveryEnabled = true
            };

            var conn = factory.CreateConnection();

            if (config.aop == null)
                throw new Exception("AddFastRabbitMQ aop not null");

            serviceCollection.AddSingleton<IConnection>(conn);
            serviceCollection.AddSingleton<IFastRabbitAop>(config.aop);
            ServiceContext.Init(new ServiceEngine(serviceCollection.BuildServiceProvider()));
            return serviceCollection;
        }
    }

    public class ConfigData
    {
        public string Host { get; set; }

        public int Port { get; set; } = 5672;

        public string UserName { get; set; }

        public string PassWord { get; set; }

        public string VirtualHost { get; set; } = "/";

        public IFastRabbitAop aop { get; set; }
    }
}
