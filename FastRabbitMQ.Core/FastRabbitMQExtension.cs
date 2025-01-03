﻿using FastRabbitMQ.Core;
using FastRabbitMQ.Core.Aop;
using FastRabbitMQ.Core.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FastRabbitMQExtension
    {
        private static string key = "RabbitMQ";
        private static string receiveQueueName = "RabbitMQReceive";

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
            init(serviceCollection, config);
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

            var receiveConfig = new ConfigModel()
            {
                QueueName = receiveQueueName,
                IsAutoAsk = true,
                IsDurable = true,
                Exchange = new Exchange { ExchangeType = FastRabbitMQ.Core.Model.ExchangeType.direct }                
            };

            var content = new Dictionary<string, object>();
            try
            {
                MqReceive(conn, config.aop, receiveConfig, true);
            }
            catch (Exception ex)
            {
                var context = new ExceptionContext();
                context.content = content;
                context.ex = ex;
                context.isReceive = true;
                context.config = receiveConfig;
                config.aop.Exception(context);
            }

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
            var content = new Dictionary<string, object>();

            var config = new ConfigModel();
            action(config);

            if (string.IsNullOrEmpty(config.Exchange?.ExchangeName))
                throw new Exception("Exchange ExchangeName is not null");

            if (string.IsNullOrEmpty(config.QueueName))
                throw new Exception("QueueName is not null");

            try
            {
                MqReceive(conn, aop, config);
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

        private static void MqReceive(IConnection conn, IFastRabbitAop aop, ConfigModel config, bool isSendReceive = false)
        {
            var content = new Dictionary<string, object>();
            var channe = conn.CreateModel();
            Dictionary<string, object> arguments = null;

            if (config.MaxPriority != null)
                arguments = new Dictionary<string, object>
                        {
                            { "x-max-priority", config.MaxPriority.Value }
                        };

            channe.QueueDeclare(config.QueueName, config.IsDurable, config.IsExclusive, config.IsAutoDelete, arguments);
            channe.ExchangeDeclare(config.Exchange.ExchangeName, config.Exchange.ExchangeType.ToString(), config.IsDurable, config.IsAutoDelete);
            channe.QueueBind(config.QueueName, config.Exchange.ExchangeName, config.Exchange.RouteKey);

            if (!config.IsAutoAsk)
                channe.BasicQos(0, 1, true);
            var consumer = new EventingBasicConsumer(channe);
            consumer.Received += (a, b) =>
            {
                content = FastRabbit.ToDic(Encoding.UTF8.GetString(b.Body.ToArray()));

                var receive = new ReceiveContext();
                receive.config = config;
                receive.content = content;

                if (isSendReceive && aop != null)
                    aop.Receive(new ReceiveContext { content = content, config = config });

                if (!isSendReceive)
                    aop.Receive(receive);

                if (!config.IsAutoAsk)
                    channe.BasicAck(b.DeliveryTag, false);
            };
            channe.BasicConsume(config.QueueName, config.IsAutoAsk, consumer);
        }
    }
}