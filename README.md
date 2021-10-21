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
    
     services.AddFastRabbitMQReceive(a => {
        a.QueueName = "test";
        a.IsAutoAsk = false;
        a.Exchange = new FastRabbitMQ.Core.Model.Exchange
        {
             ExchangeName = "test",
             ExchangeType = FastRabbitMQ.Core.Model.ExchangeType.direct,
             RouteKey = "key"
       };
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
        
        public void Delete(DeleteContext context)
        {
            // throw new NotImplementedException();
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
 ```
  
# FastRabbitMQ
nuget url: https://www.nuget.org/packages/Fast.RabbitMQ/

in Global.asax  Application_Start mothod
```csharp
 FastRabbit.AddMQ(a => {
         a.Host = "127.0.0.1";
         a.PassWord = "guest";
         a.UserName = "guest";
         a.Port = 5672;
         a.VirtualHost = "/";
         a.aop = new FastRabbitAop();
     });

     FastRabbit.AddReceive(a => {
          a.QueueName = "test";
          a.IsAutoAsk = false;
          a.Exchange = new FastRabbitMQ.Model.Exchange
          {
            ExchangeName = "test",
            ExchangeType = FastRabbitMQ.Model.ExchangeType.direct,
            RouteKey = "key"
        };
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
        
        public void Delete(DeleteContext context)
        {
            // throw new NotImplementedException();
        }
    }
```
Test
```csharp
 var dic = new Dictionary<string, object>();
 dic.Add("1", 1);
 
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
    
     services.AddFastRabbitMQReceive(a => {
        a.QueueName = "test";
        a.IsAutoAsk = false;
        a.Exchange = new FastRabbitMQ.Core.Model.Exchange
        {
             ExchangeName = "test",
             ExchangeType = FastRabbitMQ.Core.Model.ExchangeType.direct,
             RouteKey = "key"
       };
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
        
        public void Delete(DeleteContext context)
        {
            // throw new NotImplementedException();
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
 ```
  
# FastRabbitMQ
nuget url: https://www.nuget.org/packages/Fast.RabbitMQ/

in Global.asax  Application_Start mothod
```csharp
 FastRabbit.AddMQ(a => {
         a.Host = "127.0.0.1";
         a.PassWord = "guest";
         a.UserName = "guest";
         a.Port = 5672;
         a.VirtualHost = "/";
         a.aop = new FastRabbitAop();
     });

     FastRabbit.AddReceive(a => {
          a.QueueName = "test";
          a.IsAutoAsk = false;
          a.Exchange = new FastRabbitMQ.Model.Exchange
          {
            ExchangeName = "test",
            ExchangeType = FastRabbitMQ.Model.ExchangeType.direct,
            RouteKey = "key"
        };
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
        
        public void Delete(DeleteContext context)
        {
            // throw new NotImplementedException();
        }
    }
```
Test
```csharp
 
var dic = new Dictionary<string, object>();
dic.Add("1", 1);
var config = new FastRabbitMQ.Model.ConfigModel();
config.QueueName = "test1";
config.IsAutoAsk = false;
config.Exchange = new Exchange
{
      ExchangeName = "test",
      ExchangeType = ExchangeType.direct,
      RouteKey = "key"
 };
 FastRabbit.Send(config, dic);
 ```
    
    
