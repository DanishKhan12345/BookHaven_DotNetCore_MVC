using BookHaven.DataAccess.Repository.IRepository;
using BookHaven.Models;
using BookHaven.Models.ViewModels;
using BookHaven.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace BookHaven.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
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
                ShoppingCartList = _unitOfWork.shoppingCartRepository.GetAll(x => x.ApplicationUserId == userId, includeProperties: "product"),
                OrderHeader = new()
            };

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {

                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
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
            double price;
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.shoppingCartRepository.GetAll(x => x.ApplicationUserId == userId, includeProperties: "product"),
                OrderHeader = new()
            };
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.applicationUserRepository.Get(x => x.Id == userId);

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {

                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPost()
        {
            double price;
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM.ShoppingCartList = _unitOfWork.shoppingCartRepository.GetAll(x => x.ApplicationUserId == userId, includeProperties: "product");
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;

            ApplicationUser applicationUser = _unitOfWork.applicationUserRepository.Get(x => x.Id == userId);

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //it is a normal customer, need to to capture payment
                ShoppingCartVM.OrderHeader.OrderStatus = ShipmentStatus.Pending;
                ShoppingCartVM.OrderHeader.PaymentStatus = PaymentStatus.Pending;

            }
            else
            {
                //company user has 30 days to make payment so order status will be approved
                ShoppingCartVM.OrderHeader.OrderStatus = ShipmentStatus.Approved;
                ShoppingCartVM.OrderHeader.PaymentStatus = PaymentStatus.ApprovedForDelayedPayment;
            }
            _unitOfWork.orderHeaderRepository.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new OrderDetail
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                _unitOfWork.orderDetailRepository.Add(orderDetail);
                _unitOfWork.Save();
            }
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //stripe logic to add payment
                var domain = "https://localhost:44351/";
                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + "customer/cart/index",
                    LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach (var item in ShoppingCartVM.ShoppingCartList)
                {
                    var sessionItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100),
                            Currency = "inr",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.product.Title
                            }
                        },
                        Quantity = item.Count
                    };
                options.LineItems.Add(sessionItem);
                }


                var service = new SessionService();
                Session session = service.Create(options);
                _unitOfWork.orderHeaderRepository.UpdateStripePaymentId(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                //paymentintentId is only generated when payment is successfull.
                _unitOfWork.Save();
                //Redirect Url
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303); //redirecting to new url
            }
            return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
        }

        public IActionResult OrderConfirmation(int id)
        {
            try
            {
                OrderHeader orderHeader = _unitOfWork.orderHeaderRepository.Get(x => x.Id == id, includeProperties:"ApplicationUser");
                if (orderHeader.PaymentStatus != PaymentStatus.ApprovedForDelayedPayment)
                {
                    //this is normal customer order
                    var service = new SessionService();
                    Session session = service.Get(orderHeader.SessionId);

                    if (session.PaymentStatus.ToLower() == "paid")
                    {
                        _unitOfWork.orderHeaderRepository.UpdateStripePaymentId(id,session.Id,session.PaymentIntentId);
                        _unitOfWork.orderHeaderRepository.UpdateStatus(id,ShipmentStatus.Approved,PaymentStatus.Approved);
                        _unitOfWork.Save();
                    }
                
                }
            }
            catch (Exception e )
            {
                throw new Exception("Error during Payment COnfirmation " + e.Message);
            }
            return View(id);
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
