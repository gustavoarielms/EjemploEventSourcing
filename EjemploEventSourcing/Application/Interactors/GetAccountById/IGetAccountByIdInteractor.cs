using System;
using EjemploEventSourcing.Application.Domain.Entities;

namespace EjemploEventSourcing.Application.Interactors.GetAccountById
{
    public interface IGetAccountByIdInteractor
    {
        Account GetById(string accountId);
    }
}
