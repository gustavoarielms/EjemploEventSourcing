using System;
using System.Threading.Tasks;
using EjemploEventSourcing.Application.DTO;
using EjemploEventSourcing.Application.Gateways;
using EjemploEventSourcing.Infraestructura.services;

namespace EjemploEventSourcing.Infraestructura.postgres
{
    public class CreateAccountDAO : ICreateAccountGateway
    {
        private readonly IEventStoreService _service;

        public CreateAccountDAO(IEventStoreService service)
        {
            _service = service;
        }

        public async Task Save(EventStoredDTO e)
        {
            await _service.Save(e);
        }
    }
}
