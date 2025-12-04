using Microsoft.AspNetCore.Mvc;

namespace COMP2138_ICE.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            switch (statusCode)
            {
                case 404:
                    ViewBag.ErrorMessage = "Sorry, the resource you requested could not be found.";
                    return View("NotFound");
                case 500:
                    ViewBag.ErrorMessage = "Sorry, something went wrong on the server.";
                    return View("ServerError");
            }

            return View("Error");
        }

        [Route("Error")]
        public IActionResult Error()
        {
            return View("Error");
        }
    }
}
