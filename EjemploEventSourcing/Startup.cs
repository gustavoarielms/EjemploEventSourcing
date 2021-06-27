using EjemploEventSourcing.Application.Configuration;
using EjemploEventSourcing.Application.Domain.Events.Services;
using EjemploEventSourcing.Application.Gateways;
using EjemploEventSourcing.Application.Interactors.CreateAccount;
using EjemploEventSourcing.Application.Interactors.DepositAmount;
using EjemploEventSourcing.Application.Interactors.GetAccountById;
using EjemploEventSourcing.Application.Interactors.GetAccounts;
using EjemploEventSourcing.Application.Services;
using EjemploEventSourcing.Infraestructura.postgres;
using EjemploEventSourcing.Infraestructura.services;
using EjemploEventSourcing.Presentation;
using EjemploEventSourcing.Repositorios;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace EjemploEventSourcing
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var rabbitConfiguration = Configuration.GetSection("RabbitMQ").Get<RabbitConfiguration>();
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
                opt.UseNpgsql(Configuration.GetConnectionString("MyWebApiConnection")));
            services.AddScoped<IGetAccountByIdInteractor, GetAccountByIdInteractor>();
            services.AddScoped<IGetAccountsInteractor, GetAccountsInteractor>();
            services.AddScoped<ICreateAccountInteractor, CreateAccountInteractor>();
            services.AddScoped<IDepositAmountInteractor, DepositAmountInteractor>();
            services.AddScoped<IGetAccountByIdGateway, GetAccountByIdDAO>();
            services.AddScoped<IGetAccountsGateway, GetAccountsDAO>();
            services.AddScoped<ICreateAccountGateway, CreateAccountDAO>();
            services.AddScoped<IEventStoreService, EventStoreService>();
            services.AddScoped<IAccountCreatedPresenter, AccountCreatedPresenter>();
            services.AddScoped<CreateAccountSuscriber>();
            services.AddScoped<DepositAmountSuscriber>();
            services.AddControllers();
            services.AddSwaggerGen();
            services.AddSwaggerGenNewtonsoftSupport();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            IWebHostEnvironment env)
        {
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tenencia Cliente API v1");
                c.RoutePrefix = string.Empty;
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
