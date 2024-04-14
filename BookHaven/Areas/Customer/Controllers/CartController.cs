using BookHaven.DataAccess.Repository.IRepository;
using BookHaven.Models;
using BookHaven.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookHaven.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(ILogger<CartController> logger, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            double price;
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.shoppingCartRepository.GetAll(x => x.ApplicationUserId == userId, includeProperties: "product")
            };

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {

                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }

        public IActionResult Plus(int itemId)
        {
            try
            {
                if (itemId == 0) return NotFound();
                var cartFromDb = _unitOfWork.shoppingCartRepository.Get(x => x.Id == itemId);
                cartFromDb.Count += 1;
                _unitOfWork.shoppingCartRepository.Update(cartFromDb);
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public IActionResult Minus(int itemId)
        {
            try
            {
                if (itemId == 0) return NotFound();
                var cartFromDb = _unitOfWork.shoppingCartRepository.Get(x => x.Id == itemId);
                if (cartFromDb.Count <= 1)
                {
                    _unitOfWork.shoppingCartRepository.Remove(cartFromDb);
                }
                else
                {
                    cartFromDb.Count -= 1;
                    _unitOfWork.shoppingCartRepository.Update(cartFromDb);
                }
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        public IActionResult Remove(int itemId)
        {
            try
            {
                if (itemId == 0) return NotFound();
                var cartFromDb = _unitOfWork.shoppingCartRepository.GetAll(x => x.Id == itemId);
                if (cartFromDb != null)
                {
                    _unitOfWork.shoppingCartRepository.RemoveAll(cartFromDb);

                }
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public IActionResult Summary()
        { 
            return View(); 
        }  

        private double GetPriceBasedOnQuantity(ShoppingCart cart)
        {
            try
            {
                if (cart == null) return 0;

                if (cart.Count <= 50)
                {
                    return cart.product.Price;
                }
                else
                {
                    if (cart.Count <= 50)
                    {
                        return cart.product.Price50;
                    }
                    else
                    {
                        return cart.product.Price100;
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error calcultaing price" + e.Message);
            }

        }
    }
}
