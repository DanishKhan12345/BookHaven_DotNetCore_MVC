using Microsoft.AspNetCore.Mvc;

namespace BookHaven.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
