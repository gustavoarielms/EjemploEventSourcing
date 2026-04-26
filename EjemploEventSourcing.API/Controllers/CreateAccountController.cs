using System;
using System.Threading.Tasks;
using EjemploEventSourcing.Application.Interactors.CreateAccount;
using Microsoft.AspNetCore.Mvc;

namespace EjemploEventSourcing.IPresenters
{
    [ApiController]
    [Route("[controller]")]
    public class CreateAccountController : Controller
    {
        private readonly ICreateAccountInteractor _interactor;

        public CreateAccountController(ICreateAccountInteractor intercator)
        {
            _interactor = intercator;
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var id = Guid.NewGuid();
            await _interactor.Execute(id.ToString());
            return Ok(id);
        }
    }
}
