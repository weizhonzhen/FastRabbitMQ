using FastRabbitMQ.Model;
using System;
using System.Collections.Generic;

namespace FastRabbitMQ.Aop
{
    public class SendContext
    {
        public Dictionary<string, object> content { get; internal set; }

        public ConfigModel config { get; set; }
    }
}
