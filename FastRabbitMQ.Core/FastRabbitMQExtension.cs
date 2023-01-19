using RabbitMQ.Client;
using System;
using Microsoft.Extensions.DependencyInjection;
using FastRabbitMQ.Core;
using FastRabbitMQ.Core.Aop;
using FastRabbitMQ.Core.Model;
using RabbitMQ.Client.Events;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.Options;
using System.Threading.Channels;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FastRabbitMQExtension
    {
        private static string key = "RabbitMQ";

        public static IServiceCollection AddFastRabbitMQ(this IServiceCollection serviceCollection, IFastRabbitAop aop)
        {
            var build = new ConfigurationBuilder();
            build.SetBasePath(Directory.GetCurrentDirectory());
            build.AddJsonFile("db.json", optional: true, reloadOnChange: true);
            var config = new ServiceCollection().AddOptions().Configure<ConfigData>(build.Build().GetSection(key)).BuildServiceProvider().GetService<IOptions<ConfigData>>().Value;

            if (config.Host == null)
                throw new Exception(@"services.AddFastRabbitMQ(a => {  }); 
                                    or ( services.AddFastRabbitMQ(); and db.json add 'RabbitMQ':{'Server':'','PassWord':'','UserName':'','Port':5672,'VirtualHost':'/'} )");
            config.aop = aop;
            init(serviceCollection,config);
            return serviceCollection;
        }

        public static IServiceCollection AddFastRabbitMQ(this IServiceCollection serviceCollection, Action<ConfigData> action)
        {
            var config = new ConfigData();
            action(config);
            init(serviceCollection, config);
            return serviceCollection;
        }

        private static void init(IServiceCollection serviceCollection, ConfigData config)
        {
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
        }

        public static IServiceCollection AddFastRabbitMQReceive(this IServiceCollection serviceCollection, Action<ConfigModel> action)
        {
            if (ServiceContext.Engine == null)
                throw new Exception("before AddFastRabbitMQ");

            var conn = ServiceContext.Engine.Resolve<IConnection>();
            var aop = ServiceContext.Engine.Resolve<IFastRabbitAop>();
            Dictionary<string, object> content = new Dictionary<string, object>();

            var config = new ConfigModel();
            action(config);

            if (string.IsNullOrEmpty(config.QueueName))
                throw new Exception("QueueName is not null");

            try
            {
                var channe = conn.CreateModel();
                channe.QueueDeclare(config.QueueName, config.IsDurable, config.IsExclusive, config.IsAutoDelete, null);
                if (config.Exchange != null)
                {
                    if (string.IsNullOrEmpty(config.Exchange.ExchangeName))
                        throw new Exception("Exchange ExchangeName is not null");

                    channe.ExchangeDeclare(config.Exchange.ExchangeName, config.Exchange.ExchangeType.ToString(), config.IsDurable, config.IsAutoDelete, null);
                    channe.QueueBind(config.QueueName, config.Exchange.ExchangeName, config.Exchange.RouteKey);
                }

                if (!config.IsAutoAsk)
                    channe.BasicQos(0, 1, true);
                var consumer = new EventingBasicConsumer(channe);

                consumer.Received += (a, b) =>
                {
                    content = FastRabbit.ToDic(Encoding.UTF8.GetString(b.Body.ToArray()));

                    var receive = new ReceiveContext();
                    receive.config = config;
                    receive.content = content;
                    aop.Receive(receive);

                    if (!config.IsAutoAsk)
                        channe.BasicAck(b.DeliveryTag, false);
                };
                channe.BasicConsume(config.QueueName, config.IsAutoAsk, consumer);
            }
            catch (Exception ex)
            {
                var context = new ExceptionContext();
                context.content = content;
                context.ex = ex;
                context.isReceive = true;
                context.config = config;
                aop.Exception(context);
            }

            return serviceCollection;
        }
    }
}