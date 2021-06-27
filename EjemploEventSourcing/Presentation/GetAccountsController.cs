using EjemploEventSourcing.Application.Interactors.GetAccountById;
using EjemploEventSourcing.Application.Interactors.GetAccounts;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EjemploEventSourcing.Presentation
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
