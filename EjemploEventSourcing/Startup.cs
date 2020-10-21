using EjemploEventSourcing.Application.Domain.Events.Services;
using EjemploEventSourcing.Application.Gateways;
using EjemploEventSourcing.Application.Interactors.CreateAccount;
using EjemploEventSourcing.Application.Interactors.DepositAmount;
using EjemploEventSourcing.Application.Interactors.GetAccountById;
using EjemploEventSourcing.Application.Services;
using EjemploEventSourcing.Infraestructura.postgres;
using EjemploEventSourcing.Infraestructura.services;
using EjemploEventSourcing.Repositorios;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
            services.AddDbContext<EventStoreDBContext>(opt =>
                opt.UseNpgsql(Configuration.GetConnectionString("MyWebApiConnection")));
            services.AddTransient<IGetAccountByIdInteractor, GetAccountByIdInteractor>();
            services.AddTransient<ICreateAccountInteractor, CreateAccountInteractor>();
            services.AddTransient<IDepositAmountInteractor, DepositAmountInteractor>();
            services.AddTransient<IGetAccountByIdGateway, GetAccountByIdDAO>();
            services.AddTransient<ICreateAccountGateway, CreateAccountDAO>();
            services.AddTransient<IEventStoreService, EventStoreService>();
            services.AddTransient<CreateAccountSuscriber>();
            services.AddTransient<DepositAmountSuscriber>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            IWebHostEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
