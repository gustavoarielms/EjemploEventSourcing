using System;
using System.Threading.Tasks;
using EjemploEventSourcing.Application.DTO;

namespace EjemploEventSourcing.Application.Gateways
{
    public interface ICreateAccountGateway
    {
        Task Save(EventStoredDTO e);
    }
}
