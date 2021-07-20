using EjemploEventSourcing.Application.Interactors.GetAccounts;
using Microsoft.AspNetCore.Mvc;

namespace EjemploEventSourcing.IPresenters
{
    [ApiController]
    [Route("[controller]")]
    public class GetAccountsController : Controller
    {
        private readonly IGetAccountsInteractor _interactor;

        public GetAccountsController(IGetAccountsInteractor interactor)
        {
            _interactor = interactor;
        }

        // GET api/values/5
        [HttpGet()]
        public IActionResult Get()
        {
            var account = _interactor.Get();
            return Ok(account);
        }
    }
}
