using System.Threading.Tasks;
using EjemploEventSourcing.Application.Domain.Entities;
using EjemploEventSourcing.Application.Domain.Events.Interfaces;
using EjemploEventSourcing.Application.Gateways;

namespace EjemploEventSourcing.Application.Interactors.DepositAmount
{
    public class DepositAmountInteractor : IDepositAmountInteractor
    {
        private readonly IGetAccountByIdGateway _getAccountById;
        private readonly IDomainEventsPublisher _publisher;

        public DepositAmountInteractor(
            IGetAccountByIdGateway getAccountById,
            IDomainEventsPublisher publisher)
        {
            _getAccountById = getAccountById;
            _publisher = publisher;
        }

        public async Task Execute(string accountId, decimal depositAmount)
        {
            var constructor = await _getAccountById.GetAccountById(accountId);
            var account = Account.CreateEmptyAccount(constructor.AggregateId);
            account.BuildAggregate(constructor);
            account.DepositAmount(depositAmount);

            if (account.HasChanges())
            {
                var changes = account.GetChanges();
                await _publisher.PublishEvents(changes);
                account.AcceptChanges();
            }
        }
    }
}
