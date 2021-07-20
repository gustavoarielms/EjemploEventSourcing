using EjemploEventSourcing.Application.Configuration;
using EjemploEventSourcing.Application.Gateways;
using EjemploEventSourcing.Application.Interactors.CreateAccount;
using EjemploEventSourcing.Application.Interactors.DepositAmount;
using EjemploEventSourcing.Application.Interactors.GetAccountById;
using EjemploEventSourcing.Application.Interactors.GetAccounts;
using EjemploEventSourcing.Application.services;
using EjemploEventSourcing.Application.Services;
using EjemploEventSourcing.Infrastructure.postgres;
using EjemploEventSourcing.Infrastructure.Repositorios;
using EjemploEventSourcing.Infrastructure.services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace EjemploEventSourcing.IoC
{
    public class DependencyContainer
    {
        public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            var rabbitConfiguration = configuration.GetSection("RabbitMQ").Get<RabbitConfiguration>();
            services.AddSingleton(rabbitConfiguration);

            var rabbitFactory = new ConnectionFactory
            {
                UserName = rabbitConfiguration.UserName,
                Password = rabbitConfiguration.Password,
                VirtualHost = rabbitConfiguration.VirtualHost,
                HostName = rabbitConfiguration.HostName
            };

            services.AddSingleton<IConnectionFactory>(rabbitFactory);

            services.AddDbContext<EventStoreDBContext>(opt =>
                opt.UseNpgsql(configuration.GetConnectionString("MyWebApiConnection")),
                ServiceLifetime.Singleton);

            services.AddScoped<IGetAccountByIdInteractor, GetAccountByIdInteractor>();
            services.AddScoped<IGetAccountsInteractor, GetAccountsInteractor>();
            services.AddScoped<ICreateAccountInteractor, CreateAccountInteractor>();
            services.AddScoped<IDepositAmountInteractor, DepositAmountInteractor>();
            services.AddScoped<IGetAccountByIdGateway, GetAccountByIdDAO>();
            services.AddScoped<IGetAccountsGateway, GetAccountsDAO>();
            services.AddScoped<ICreateAccountGateway, CreateAccountDAO>();
            services.AddScoped<IEventStoreService, EventStoreService>();
            services.AddScoped<CreateAccountSuscriber>();
            services.AddScoped<DepositAmountSuscriber>();
        }
    }
}
