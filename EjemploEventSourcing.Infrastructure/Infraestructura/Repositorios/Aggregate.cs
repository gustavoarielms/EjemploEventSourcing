using System;
namespace EjemploEventSourcing.Infrastructure.Repositorios
{
    public class Aggregate
    {
        public string Id { get; set; }

        public string Type { get; set; }

        public int LastVersion { get; set; }
    }
}
