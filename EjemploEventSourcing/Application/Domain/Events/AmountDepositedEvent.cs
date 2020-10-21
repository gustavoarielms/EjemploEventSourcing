using System;
using EjemploEventSourcing.Application.Domain.Events.Interfaces;

namespace EjemploEventSourcing.Application.Domain.Events
{
    public class AmountDepositedEvent : IEvent
    {
        private readonly DateTime _dateItHappened;
        private readonly DataAmountDepositedEvent _data;

        public AmountDepositedEvent(DataAmountDepositedEvent data)
        {
            _dateItHappened = DateTime.Now;
            _data = data;
        }

        public object GetData()
        {
            return _data;
        }

        public DateTime GetDateItHappened()
        {
            return _dateItHappened;
        }

        public EventTypes GetEventType()
        {
            return EventTypes.AmountDeposited;
        }
    }
}