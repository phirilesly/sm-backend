using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace StockManager.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorsController : ControllerBase
    {
        [Route("/error")]
        public IActionResult Error()
        {
            return Problem();
        }
    }
}
