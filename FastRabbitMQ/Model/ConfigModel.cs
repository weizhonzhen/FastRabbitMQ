﻿namespace FastRabbitMQ.Model
{
    public class ConfigModel
    {
        public string QueueName { get; set; }

        public Exchange Exchange { get; set; }

        public bool IsAutoAsk { get; set; }

        public bool IsDurable { get; set; } = true;

        public bool IsExclusive { get; set; }

        public bool IsAutoDelete { get; set; }

        public bool IsUnused { get; set; }

        public bool IsEmpty { get; set; }

    }
}
