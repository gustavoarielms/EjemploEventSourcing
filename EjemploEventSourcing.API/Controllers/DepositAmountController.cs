using System.Threading.Tasks;
using EjemploEventSourcing.Application.Interactors.DepositAmount;
using Microsoft.AspNetCore.Mvc;

namespace EjemploEventSourcing.IPresenters
{
    [ApiController]
    [Route("[controller]")]
    public class DepositAmountController : Controller
    {
        private readonly IDepositAmountInteractor _interactor;

        public DepositAmountController(IDepositAmountInteractor intercator)
        {
            _interactor = intercator;
        }

        [HttpPost]
        public async Task<IActionResult> Post(DepositAmountModel model)
        {
            await _interactor.Execute(model.AccountId, model.DepositAmount);
            return Ok();
        }
    }
}
