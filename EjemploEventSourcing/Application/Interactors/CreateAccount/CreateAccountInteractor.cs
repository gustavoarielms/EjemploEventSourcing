using System;
using System.Threading.Tasks;
using EjemploEventSourcing.Application.Domain.Entities;
using EjemploEventSourcing.Application.Domain.Events.Services;

namespace EjemploEventSourcing.Application.Interactors.CreateAccount
{
    public class CreateAccountInteractor : ICreateAccountInteractor
    {
        public void Execute(string id)
        {
            var account = Account.CreateAccount(id);

            if (account.HasChanges())
            {
                var changes = account.GetChanges();
                DomainEventsPublisher.GetInstancia().PublishEvents(changes);
                account.AcceptChanges();
            }
                
        }
    }
}
