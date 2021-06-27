using System;
using System.Collections.Generic;
using System.Linq;
using EjemploEventSourcing.Application.Gateways;
using EjemploEventSourcing.Repositorios;

namespace EjemploEventSourcing.Infraestructura.postgres
{
    public class GetAccountsDAO : IGetAccountsGateway
    {
        private readonly EventStoreDBContext _context;

        public GetAccountsDAO(EventStoreDBContext context)
        {
            _context = context;
        }

        public IList<string> GetAccounts()
        {
            if(!_context.Aggregates.Any())
                throw new InvalidOperationException();

            var accounts = _context.Aggregates.Select(b => b.Id).ToList();
            
            if (accounts == null)
                throw new InvalidOperationException();

            return accounts;
        }
    }
}
