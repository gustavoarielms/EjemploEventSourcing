using Microsoft.AspNetCore.Mvc;

namespace EjemploEventSourcing.IPresenters
{
    [ApiController]
    [Route("[controller]")]
    public class HealthCheckController : Controller
    {
        [HttpGet]
        public ContentResult Index()
        {
            return new ContentResult
            {
                ContentType = "text/html",
                Content = "<div>Healthcheck OK</div>"
            };
        }
    }
}
