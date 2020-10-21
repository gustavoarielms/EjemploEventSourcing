using System;
using System.Threading.Tasks;
using EjemploEventSourcing.Application.Domain.Events.Services;
using EjemploEventSourcing.Application.Interactors.CreateAccount;
using EjemploEventSourcing.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace EjemploEventSourcing.Presentation
{
    [ApiController]
    [Route("[controller]")]
    public class CreateAccountController : Controller
    {
        private readonly ICreateAccountInteractor _interactor;

        public CreateAccountController(ICreateAccountInteractor intercator, CreateAccountSuscriber suscriber)
        {
            DomainEventsPublisher.GetInstancia().RegisterSuscriber(suscriber);
            _interactor = intercator;
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var id = Guid.NewGuid();
            _interactor.Execute(id.ToString());
            return Ok(id);
        }
    }
}
