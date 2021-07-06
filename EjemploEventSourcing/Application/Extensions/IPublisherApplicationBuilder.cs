using EjemploEventSourcing.Application.Domain.Events.Interfaces;
using EjemploEventSourcing.Application.Domain.Events.Services;
using Microsoft.AspNetCore.Builder;
using System;

namespace EjemploEventSourcing.Application.Extensions
{
    public static class PublisherExtensions
    {
        public static IApplicationBuilder UsePublisher(
            this IApplicationBuilder app, Type publisherType, Type[] subscribersTypes)
        {

            var publisher = (DomainEventsPublisher) app.ApplicationServices.GetService(publisherType);

            foreach (var subscriberType in subscribersTypes)
            {
                publisher.RegisterSuscriber((IDomainEventsSuscriber)app.ApplicationServices.GetService(subscriberType));
            }

            return app;
        }
    }
}
