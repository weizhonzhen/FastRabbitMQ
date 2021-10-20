using FastRabbitMQ.Core.Aop;
using System;

namespace FastRabbitMQ.Core
{
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
