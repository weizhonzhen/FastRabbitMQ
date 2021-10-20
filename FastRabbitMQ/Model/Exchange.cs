using System;

namespace FastRabbitMQ.Model
{
    public class Exchange
    {
        public string ExchangeName { get; set; }

        public ExchangeType ExchangeType { get; set; }

        public string RouteKey { get; set; }
    }
}
