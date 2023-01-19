﻿using System;

namespace FastRabbitMQ.Core.Model
{
    public class Exchange
    {
        public string ExchangeName { get; set; }

        public ExchangeType ExchangeType { get; set; } = ExchangeType.direct;

        public string RouteKey { get; set; }
    }
}
