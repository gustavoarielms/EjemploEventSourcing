using System;
using System.Threading.Tasks;
using EjemploEventSourcing.Application.Domain.Entities;

namespace EjemploEventSourcing.Application.Interactors.GetAccountById
{
    public interface IGetAccountByIdInteractor
    {
        Task<Account> GetById(string accountId);
    }
}
