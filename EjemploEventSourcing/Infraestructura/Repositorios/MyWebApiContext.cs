using System;
using EjemploEventSourcing.Infraestructura.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace EjemploEventSourcing.Repositorios
{
    public class EventStoreDBContext: DbContext
    {
        public DbSet<Event> Events { get; set; }
        public DbSet<Aggregate> Aggregates { get; set; }

        public EventStoreDBContext(DbContextOptions<EventStoreDBContext> options) : base(options){}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Event>().HasKey(p => new { p.AggregateId, p.AggregateVersion});
        }
    }
}