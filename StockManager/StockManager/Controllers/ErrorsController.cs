using Microsoft.AspNetCore.Mvc;

namespace StockManager.Controllers
{



    public class ErrorsController : ControllerBase
    {
        [Route("/error")]
        public IActionResult Error()
        {
            return Problem();
        }
    }

}

