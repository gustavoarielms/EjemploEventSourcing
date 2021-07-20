using System.Threading.Tasks;
using EjemploEventSourcing.Application.Interactors.DepositAmount;
using Microsoft.AspNetCore.Mvc;
using EjemploEventSourcing.Application.Domain.Events.Services;
using EjemploEventSourcing.Application.Services;

namespace EjemploEventSourcing.IPresenters
{
    [ApiController]
    [Route("[controller]")]
    public class DepositAmountController : Controller
    {
        private readonly IDepositAmountInteractor _interactor;

        public DepositAmountController(IDepositAmountInteractor intercator, DepositAmountSuscriber suscriber)
        {
            DomainEventsPublisher.GetInstancia().ResetSuscribers();
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
