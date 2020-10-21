﻿using System;
using System.Threading.Tasks;
using EjemploEventSourcing.Application.DTO;

namespace EjemploEventSourcing.Infraestructura.services
{
    public interface IEventStoreService
    {
        public Task Save(EventStoredDTO e);
        public Task Commit();
    }
}
