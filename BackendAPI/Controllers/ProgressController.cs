using Microsoft.AspNetCore.Mvc;

namespace BackendAPI.Controllers
{
    public class ProgressController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
