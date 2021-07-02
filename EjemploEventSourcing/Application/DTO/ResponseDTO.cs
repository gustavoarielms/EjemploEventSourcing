using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EjemploEventSourcing.Application.DTO
{
    public class ResponseDTO
    {
        public string AggregateId { get; set; }

        public string AggregateData { get; set; }
    }
}
