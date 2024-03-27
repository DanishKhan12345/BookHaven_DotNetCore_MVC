using BookHaven.DataAccess.Repository.IRepository;
using BookHaven.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BookHaven.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitOfWork.productRepository.GetAll(includeProperties: "Category");
            return View(productList);
        }

        public IActionResult Details(int productId)
        {
            try
            {
                if (productId == 0)
                {
                    return NotFound();
                }
                Product product = _unitOfWork.productRepository.Get(x => x.Id == productId, includeProperties: "Category");
                return View(product);
            }
            catch (Exception e)
            {
                throw new Exception("Error finding product" + e.Message);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
