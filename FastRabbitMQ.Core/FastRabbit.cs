using FastRabbitMQ.Core.Aop;
using FastRabbitMQ.Core.Model;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Channels;

namespace FastRabbitMQ.Core
{
    public static class FastRabbit
    {
        public static void Send(ConfigModel config, Dictionary<string, object> content)
        {
            var jsonOption = new JsonSerializerOptions() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            var conn = ServiceContext.Engine.Resolve<IConnection>();
            var aop = ServiceContext.Engine.Resolve<IFastRabbitAop>();
            try
            {
                using (var channe = conn.CreateModel())
                {
                    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(content, jsonOption));
                    channe.QueueDeclare(config.QueueName, config.IsDurable, config.IsExclusive, config.IsAutoDelete, null);
                    if (config.Exchange == null)
                        channe.BasicPublish("", config.QueueName, null, body);
                    else
                    {
                        channe.ExchangeDeclare(config.Exchange.ExchangeName, config.Exchange.ExchangeType.ToString(), config.IsDurable, config.IsAutoDelete, null);
                        channe.QueueBind(config.QueueName, config.Exchange.ExchangeName, config.Exchange.RouteKey);
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

        public static void Delete(ConfigModel config)
        {
            var conn = ServiceContext.Engine.Resolve<IConnection>();
            var aop = ServiceContext.Engine.Resolve<IFastRabbitAop>();
            try
            {
                using (var channe = conn.CreateModel())
                {
                    if (config.Exchange == null)
                        channe.QueueDelete(config.QueueName, config.IsUnused, config.IsEmpty);
                    else
                        channe.ExchangeDelete(config.Exchange.ExchangeName,config.IsUnused);

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

        internal static Dictionary<string, object> ToDic(string content)
        {
            var jsonOption = new JsonSerializerOptions() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            var dic = new Dictionary<string, object>();

            if (string.IsNullOrEmpty(content))
                return dic;

            using (var document = JsonDocument.Parse(content))
            {
                foreach (var element in document.RootElement.EnumerateObject())
                {
                    dic.Add(element.Name, element.Value.GetRawText());
                }
            }
            return dic;
        }
    }
}