using System;

namespace EjemploEventSourcing.Application.Domain.Events.Interfaces
{
    public interface IEvent
    {
        DateTime GetDateItHappened();
        EventTypes GetEventType();
        object GetData();
    }
}