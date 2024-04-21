using BookHaven.DataAccess.Repository.IRepository;
using BookHaven.Models;
using BookHaven.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

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
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null)
            {
				HttpContext.Session.SetInt32(StaticDetails.SessionCart,
                    _unitOfWork.shoppingCartRepository.GetAll(x => x.ApplicationUserId == claim.Value).Count());
			}

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
                ShoppingCart cart = new()
                {
                    product = _unitOfWork.productRepository.Get(x => x.Id == productId, includeProperties: "Category"),
                    Count = 1,
                    ProductId = productId
                };
                return View(cart);
            }
            catch (Exception e)
            {
                throw new Exception("Error finding product" + e.Message);
            }
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart cart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            cart.ApplicationUserId = userId;

            var cartFromDb = _unitOfWork.shoppingCartRepository.Get(x => x.ApplicationUserId == userId && x.ProductId == cart.ProductId);

            try
            {
                if (cartFromDb != null)
                {
                    cartFromDb.Count += cart.Count;
                    _unitOfWork.shoppingCartRepository.Update(cartFromDb);
                    _unitOfWork.Save();
                    TempData["success"] = "Cart updated Successfully";
                }
                else if (cart != null && cartFromDb == null)
                {
                    _unitOfWork.shoppingCartRepository.Add(cart);
                    _unitOfWork.Save();
                    HttpContext.Session.SetInt32(StaticDetails.SessionCart, 
                        _unitOfWork.shoppingCartRepository.GetAll(x => x.ApplicationUserId == userId).Count());
                    TempData["success"] = "Cart added Successfully";
                }
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
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
