using FastRabbitMQ.Core.Model;
using System;
using System.Collections.Generic;

namespace FastRabbitMQ.Core.Aop
{
    public class SendContext
    {
        public Dictionary<string, object> content { get; internal set; }

        public ConfigModel config { get; set; }
    }
}
