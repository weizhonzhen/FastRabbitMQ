using System;

namespace FastRabbitMQ.Core.Aop
{
    public interface IFastRabbitAop
    {
        void Send(SendContext context);

        void Receive(ReceiveContext context);

        void Exception(ExceptionContext context);
    }
}
