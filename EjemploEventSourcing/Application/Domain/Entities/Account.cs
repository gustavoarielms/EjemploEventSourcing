using System;
using EjemploEventSourcing.Application.Domain.Events;
using EjemploEventSourcing.Application.Domain.Events.Interfaces;

namespace EjemploEventSourcing.Application.Domain.Entities
{
    public class Account : Aggregate
    {
        public string Id { get; set; }
        public decimal Balance { get; set; }

        public Account(string id, IEvent e = null)
        {
            Init(id);
            if (e != null)
                ApplyEvent(e);
        }

        public static Account CreateAccount(string id)
        {
            var data = new DataAccountCreatedEvent
            {
                AccountId = id,
                Balance = 0m
            };
            return new Account(id, new AccountCreatedEvent(data));
        }

        public static Account CreateEmptyAccount(string id)
        {
            return new Account(id);
        }

        public void DepositAmount(decimal amount)
        {
            var data = new DataAmountDepositedEvent
            {
                AccountId = Id,
                AmountDeposited = amount
            };
            ApplyEvent(new AmountDepositedEvent(data));
        }

        public override void ReproduceEvent(IEvent e)
        {
            switch(e.GetEventType())
            {
                case EventTypes.AccountCreated:
                    {
                        var data = (DataAccountCreatedEvent)e.GetData();
                        ReproduceAccountCreatedEvent(data.AccountId);
                    }
                    break;
                case EventTypes.AmountDeposited:
                    {
                        var data = (DataAmountDepositedEvent)e.GetData();
                        ReproduceAmountDepositedEvent(data.AmountDeposited);
                    }
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        private void ReproduceAccountCreatedEvent(string accountId)
        {
            Id = accountId;
        }

        private void ReproduceAmountDepositedEvent(decimal amountDeposited)
        {
            Balance += amountDeposited;
        }
    }
}
