using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EjemploEventSourcing.Application.Interactors.DepositAmount;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using EjemploEventSourcing.Application.Domain.Events.Services;
using EjemploEventSourcing.Application.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EjemploEventSourcing.Presentation
{
    [ApiController]
    [Route("[controller]")]
    public class DepositAmountController : Controller
    {
        private readonly IDepositAmountInteractor _interactor;

        public DepositAmountController(IDepositAmountInteractor intercator, DepositAmountSuscriber suscriber)
        {
            DomainEventsPublisher.GetInstancia().RegisterSuscriber(suscriber);
            _interactor = intercator;
        }

        [HttpPost]
        public async Task<IActionResult> Post(DepositAmountModel model)
        {
            _interactor.Execute(model.AccountId, model.DepositAmount);
            return Ok();
        }
    }
}
