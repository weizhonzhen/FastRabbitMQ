using FastRabbitMQ.Aop;
using FastRabbitMQ.Config;
using FastRabbitMQ.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FastRabbitMQ
{
    public static class FastRabbit
    {
        private static ConfigData config;
        private static IConnection conn;
        private static IFastRabbitAop aop;

        public static void AddMQ(Action<ConfigData> action)
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

            conn = factory.CreateConnection();

            if (config.aop == null)
                throw new Exception("FastRabbit.init aop not null");

            aop = config.aop;
        }

        public static void AddMQ(IFastRabbitAop _aop, string dbFile = "web.config", string projectName = null)
        {
            if (projectName != null)
                projectName = Assembly.GetCallingAssembly().GetName().Name;
            var config = RabbitMQConfig.GetConfig(projectName, dbFile);
            
            IConnectionFactory factory = new ConnectionFactory
            {
                HostName = config.Host,
                Port = config.Port,
                UserName = config.UserName,
                Password = config.PassWord,
                VirtualHost = config.VirtualHost,
                AutomaticRecoveryEnabled = true
            };
            conn = factory.CreateConnection();

            if (_aop == null)
                throw new Exception("FastRabbit.init aop not null");

            aop = _aop;
        }

        public static void Send(ConfigModel config, Dictionary<string, object> content)
        {
            try
            {
                using (var channe = conn.CreateModel())
                {
                    var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(content).ToString());
                    if (config.Exchange == null)
                    {
                        channe.QueueDeclare(config.QueueName, config.IsDurable, config.IsExclusive, config.IsAutoDelete, null);
                        channe.BasicPublish("", config.QueueName, null, body);
                    }
                    else
                    {
                        channe.ExchangeDeclare(config.Exchange.ExchangeName, config.Exchange.ExchangeType.ToString(),config.IsDurable,config.IsAutoDelete,null);
                        channe.BasicPublish(config.Exchange.ExchangeName, config.Exchange.RouteKey, null, body);
                    }

                    var send = new SendContext();
                    send.config = config;
                    send.content = content;
                    aop.Send(send);
                }
            }
            catch (Exception ex)
            {
                var context = new ExceptionContext();
                context.content = content;
                context.ex = ex;
                context.isSend = true;
                context.config = config;
                aop.Exception(context);
            }
        }

        public static void AddReceive(Action<ConfigModel> action)
        {
            var config = new ConfigModel();
            action(config);
            Dictionary<string, object> content = new Dictionary<string, object>();

            if (conn == null)
                throw new Exception("before AddMQ");

            try
            {
                var channe = conn.CreateModel();
                if (config.Exchange == null)
                    channe.QueueDeclare(config.QueueName, config.IsDurable, config.IsExclusive, config.IsAutoDelete, null);
                else
                {
                    channe.ExchangeDeclare(config.Exchange.ExchangeName, config.Exchange.ExchangeType.ToString());
                    config.QueueName = string.Format("{0}_{1}", config.Exchange.ExchangeName, Guid.NewGuid());
                    channe.QueueDeclare(config.QueueName, config.IsDurable, config.IsExclusive, config.IsAutoDelete, null);
                    channe.QueueBind(config.QueueName, config.Exchange.ExchangeName, config.Exchange.RouteKey);
                }

                if (!config.IsAutoAsk)
                    channe.BasicQos(0, 1, false);
                var consumer = new EventingBasicConsumer(channe);

                consumer.Received += (a, b) =>
                {
                    content = ToDic(Encoding.UTF8.GetString(b.Body.ToArray()));

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
        }

        public static void Delete(ConfigModel config)
        {
            try
            {
                using (var channe = conn.CreateModel())
                {
                    if (config.Exchange == null)
                        channe.QueueDelete(config.QueueName);
                    else
                        channe.ExchangeDelete(config.Exchange.ExchangeName);

                    var delete = new DeleteContext();
                    delete.config = config;
                    aop.Delete(delete);
                }
            }
            catch (Exception ex)
            {
                var context = new ExceptionContext();
                context.ex = ex;
                context.isDelete = true;
                context.config = config;
                aop.Exception(context);
            }
        }

        private static Dictionary<string, object> ToDic(string content)
        {
            var dic = new Dictionary<string, object>();

            if (string.IsNullOrEmpty(content))
                return dic;

            var jo = JObject.Parse(content);

            foreach (var temp in jo)
            {
                dic.Add(temp.Key, temp.Value);
            }
            return dic;
        }
    }
}
