# FastRabbitMQ.Core
nuget url: https://www.nuget.org/packages/Fast.RabbitMQ.Core/

in Startup.cs Startup mothod

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
