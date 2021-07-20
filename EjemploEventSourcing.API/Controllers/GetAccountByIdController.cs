using System.Threading.Tasks;
using EjemploEventSourcing.Application.Interactors.GetAccountById;
using Microsoft.AspNetCore.Mvc;

namespace EjemploEventSourcing.IPresenters
{
    [ApiController]
    [Route("[controller]")]
    public class GetAccountByIdController : Controller
    {
        private readonly IGetAccountByIdInteractor _interactor;

        public GetAccountByIdController(IGetAccountByIdInteractor interactor)
        {
            _interactor = interactor;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var account = await _interactor.GetById(id);
            return Ok(account);
        }
    }
}
