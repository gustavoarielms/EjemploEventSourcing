using System;
using System.Threading.Tasks;
using EjemploEventSourcing.Application.Domain.Events.Interfaces;

namespace EjemploEventSourcing.Application.Gateways
{
    public interface IGetAccountByIdGateway
    {
        Task<IAggregateInfoConstructor> GetAccountById(string accountId);
    }
}
