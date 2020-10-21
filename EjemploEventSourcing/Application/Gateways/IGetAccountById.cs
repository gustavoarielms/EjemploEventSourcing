using System;
using EjemploEventSourcing.Application.Domain.Events.Interfaces;

namespace EjemploEventSourcing.Application.Gateways
{
    public interface IGetAccountByIdGateway
    {
        IAggregateInfoConstructor GetAccountById(string accountId);
    }
}
