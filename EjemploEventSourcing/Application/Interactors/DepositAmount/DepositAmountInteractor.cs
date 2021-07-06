using System;
using EjemploEventSourcing.Application.Domain.Entities;
using EjemploEventSourcing.Application.Domain.Events.Services;
using EjemploEventSourcing.Application.Gateways;

namespace EjemploEventSourcing.Application.Interactors.DepositAmount
{
    public class DepositAmountInteractor : IDepositAmountInteractor
    {
        private IGetAccountByIdGateway _getAccountById;

        public DepositAmountInteractor(IGetAccountByIdGateway getAccountById)
        {
            _getAccountById = getAccountById;
        }

        public void Execute(string accountId, decimal depositAmount)
        {
            try
            {
                var constructor = _getAccountById.GetAccountById(accountId);
                var account = Account.CreateEmptyAccount(constructor.AggregateId);
                account.BuildAggregate(constructor);
                account.DepositAmount(depositAmount);

                if (account.HasChanges())
                {
                    var changes = account.GetChanges();
                    DomainEventsPublisher.GetInstancia().PublishEvents(changes);
                    account.AcceptChanges();
                }
            }
            catch(Exception e)
            {
                throw e;
            }
        }
    }
}
