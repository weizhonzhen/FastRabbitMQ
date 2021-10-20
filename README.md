# FastRabbitMQ.Core
nuget url: https://www.nuget.org/packages/Fast.RabbitMQ.Core/

in Startup.cs Startup mothod
```csharp
 services.AddFastRabbitMQ(a => {
            a.Host = "127.0.0.1";
            a.PassWord = "guest";
            a.UserName = "guest";
            a.Port = 5672;
            a.VirtualHost = "/";
            a.aop = new FastRabbitAop();
    });
    
    
    public class FastRabbitAop : IFastRabbitAop
    {
        public void Exception(ExceptionContext context)
        {
            //throw new NotImplementedException();
        }

        public void Receive(ReceiveContext context)
        {
            //throw new NotImplementedException();
        }

        public void Send(SendContext context)
        {
            //throw new NotImplementedException();
        }
    }
```
Test
```csharp
 var dic = new Dictionary<string, object>();
 dic.Add("1", 1);
 
 var config = new FastRabbitMQ.Core.Model.ConfigModel();
 config.QueueName = "test1";
 //config.IsAutoAsk = true;
 FastRabbit.Send(config, dic);
 FastRabbit.Receive(config);
 
  
# FastRabbitMQ
nuget url: https://www.nuget.org/packages/Fast.RabbitMQ/
