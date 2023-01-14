using System;

namespace FastRabbitMQ.Core.Model
{
    public class ConfigModel
    {
        public string QueueName { get; set; }

        public Exchange Exchange { get; set; }

        public bool IsAutoAsk { get; set; }

        public bool Durable { get; set; }

        public bool Exclusive { get; set; }

        public bool AutoDelete { get; set; }

        public bool IfUnused { get; set; }

        public bool IfEmpty { get; set; }
    }
}
