using System;
using System.Threading.Tasks;
using EjemploEventSourcing.Application.Domain.Entities;
using EjemploEventSourcing.Application.Gateways;

namespace EjemploEventSourcing.Application.Interactors.GetAccountById
{
    public class GetAccountByIdInteractor : IGetAccountByIdInteractor
    {
        private readonly IGetAccountByIdGateway _gateway;

        public GetAccountByIdInteractor(IGetAccountByIdGateway gateway)
        {
            _gateway = gateway;
        }

        public async Task<Account> GetById(string accountId)
        {
            var constructor = await _gateway.GetAccountById(accountId);
            var account = Account.CreateEmptyAccount(constructor.AggregateId);
            account.BuildAggregate(constructor);
            return account;
        }
    }
}
