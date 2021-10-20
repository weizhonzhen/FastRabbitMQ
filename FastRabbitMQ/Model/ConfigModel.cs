using System;

namespace FastRabbitMQ.Model
{
    public class ConfigModel
    {
        public string QueueName { get; set; }

        public Exchange Exchange { get; set; }

        public bool IsAutoAsk { get; set; }

    }
}
