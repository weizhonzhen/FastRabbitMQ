using FastRabbitMQ.Core.Aop;
using FastRabbitMQ.Core.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace FastRabbitMQ.Core
{
    public static class FastRabbit
    {
        public static void Send(ConfigModel model, Dictionary<string, object> content)
        {
            var conn = ServiceContext.Engine.Resolve<IConnection>();
            var aop = ServiceContext.Engine.Resolve<IFastRabbitAop>();
            try
            {
                using (var channe = conn.CreateModel())
                {
                    var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(content).ToString());
                    if (model.Exchange == null)
                    {
                        channe.QueueDeclare(model.QueueName, false, false, false, null);
                        channe.BasicPublish("", model.QueueName, null, body);
                    }
                    else
                    {
                        channe.ExchangeDeclare(model.Exchange.ExchangeName, model.Exchange.ExchangeType.ToString());
                        channe.BasicPublish(model.Exchange.ExchangeName, model.Exchange.RouteKey, null, body);
                    }

                    var send = new SendContext();
                    send.config = model;
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
                context.config = model;
                aop.Exception(context);
            }
        }

        public static void Receive(ConfigModel model)
        {
            var conn = ServiceContext.Engine.Resolve<IConnection>();
            var aop = ServiceContext.Engine.Resolve<IFastRabbitAop>();
            Dictionary<string, object> content = new Dictionary<string, object>();

            try
            {
                var channe = conn.CreateModel();
                if (model.Exchange == null)
                    channe.QueueDeclare(model.QueueName, false, false, false, null);
                else
                {
                    channe.ExchangeDeclare(model.Exchange.ExchangeName, model.Exchange.ExchangeType.ToString());
                    model.QueueName = string.Format("{0}_{1}", model.Exchange.ExchangeName, Guid.NewGuid());
                    channe.QueueDeclare(model.QueueName, false, false, false, null);
                    channe.QueueBind(model.QueueName, model.Exchange.ExchangeName, model.Exchange.RouteKey);
                }

                if (!model.IsAutoAsk)
                    channe.BasicQos(0, 1, false);
                var consumer = new EventingBasicConsumer(channe);

                consumer.Received += (a, b) =>
                {
                    content = ToDic(Encoding.UTF8.GetString(b.Body.ToArray()));

                    var receive = new ReceiveContext();
                    receive.config = model;
                    receive.content = content;
                    aop.Receive(receive);

                    if (!model.IsAutoAsk)
                        channe.BasicAck(b.DeliveryTag, false);
                };
                channe.BasicConsume(model.QueueName, model.IsAutoAsk, consumer);
            }
            catch (Exception ex)
            {
                var context = new ExceptionContext();
                context.content = content;
                context.ex = ex;
                context.isReceive = true;
                context.config = model;
                aop.Exception(context);
            }
        }

        public static void Delete(ConfigModel model)
        {
            var conn = ServiceContext.Engine.Resolve<IConnection>();
            var aop = ServiceContext.Engine.Resolve<IFastRabbitAop>();
            try
            {
                using (var channe = conn.CreateModel())
                {
                    if (model.Exchange == null)                    
                        channe.QueueDelete(model.QueueName);                    
                    else
                        channe.ExchangeDelete(model.Exchange.ExchangeName);
                    
                    var delete = new DeleteContext();
                    delete.config = model;
                    aop.Delete(delete);
                }
            }
            catch(Exception ex)
            {
                var context = new ExceptionContext();
                context.ex = ex;
                context.isDelete = true;
                context.config = model;
                aop.Exception(context);
            }
        }

        internal static Dictionary<string, object> ToDic(string content)
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
