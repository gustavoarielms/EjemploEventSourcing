using System;
using System.Threading.Tasks;
using EjemploEventSourcing.Application.Domain.Entities;
using EjemploEventSourcing.Application.Domain.Events.Interfaces;

namespace EjemploEventSourcing.Application.Interactors.CreateAccount
{
    public class CreateAccountInteractor : ICreateAccountInteractor
    {
        private readonly IDomainEventsPublisher _publisher;

        public CreateAccountInteractor(IDomainEventsPublisher publisher)
        {
            _publisher = publisher;
        }

        public async Task Execute(string id)
        {
            var account = Account.CreateAccount(id);

            if (account.HasChanges())
            {
                var changes = account.GetChanges();
                await _publisher.PublishEvents(changes);
                account.AcceptChanges();
            }
                
        }
    }
}
