using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EjemploEventSourcing.Application.Configuration
{
    public class RabbitConfiguration
    {
        public string UserName { get; set; }
        
        public string Password { get; set; }

        public string VirtualHost { get; set; }

        public string HostName { get; set; }
    }
}
