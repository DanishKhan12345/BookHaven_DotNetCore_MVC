using BookHaven.DataAccess.Repository.IRepository;
using BookHaven.Models;
using BookHaven.Models.ViewModels;
using BookHaven.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace BookHaven.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [BindProperty]
        public OrderVM orderdetails { get; set; }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details(int id)
        {
            try
            {
                orderdetails = new OrderVM()
                {
                    orderHeader = _unitOfWork.orderHeaderRepository.Get(x => x.Id == id, includeProperties: "ApplicationUser"),
                    orderDetails = _unitOfWork.orderDetailRepository.GetAll(x => x.OrderHeaderId == id, includeProperties: "product")
                };
                return View("OrderDetails", orderdetails);
            }
            catch (Exception e)
            {
                throw new Exception("Error fetching dettails" + e.Message);
            }
        }


        [HttpPost]
        [Authorize(Roles = StaticDetails.Role_Admin + "," + StaticDetails.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            try
            {
                var orderHeaderFromDB = _unitOfWork.orderHeaderRepository.Get(x => x.Id == orderdetails.orderHeader.Id);

                orderHeaderFromDB.Name = orderdetails.orderHeader.Name;
                orderHeaderFromDB.PhoneNumber = orderdetails.orderHeader.PhoneNumber;
                orderHeaderFromDB.StreetAddress = orderdetails.orderHeader.StreetAddress;
                orderHeaderFromDB.City = orderdetails.orderHeader.City;
                orderHeaderFromDB.State = orderdetails.orderHeader.State;
                orderHeaderFromDB.PostalCode = orderdetails.orderHeader.PostalCode;

                if (!string.IsNullOrEmpty(orderdetails.orderHeader.Carrier))
                {
                    orderHeaderFromDB.Carrier = orderdetails.orderHeader.Carrier;
                }
                if (!string.IsNullOrEmpty(orderdetails.orderHeader.TrackingNumber))
                {
                    orderHeaderFromDB.TrackingNumber = orderdetails.orderHeader.TrackingNumber;
                }

                _unitOfWork.orderHeaderRepository.Update(orderHeaderFromDB);
                _unitOfWork.Save();

                TempData["success"] = "Order Details updated suuccessfully.";
                return RedirectToAction(nameof(Details), new { id = orderHeaderFromDB.Id});
            }
            catch (Exception e)
            {
                throw new Exception("Error fetching dettails" + e.Message);
            }
        }
        
        [HttpPost]
        [Authorize(Roles = StaticDetails.Role_Admin + "," + StaticDetails.Role_Employee)]
        public IActionResult StartProcessing()
        {
            try
            {
                _unitOfWork.orderHeaderRepository.UpdateStatus(orderdetails.orderHeader.Id,ShipmentStatus.Processing);
                _unitOfWork.Save();
                TempData["success"] = "Order Details updated suuccessfully.";
                return RedirectToAction(nameof(Details), new { id = orderdetails.orderHeader.Id });
            }
            catch (Exception e)
            {
                throw new Exception("Error fetching dettails" + e.Message);
            }
        }
        [HttpPost]
        [Authorize(Roles = StaticDetails.Role_Admin + "," + StaticDetails.Role_Employee)]
        public IActionResult ShipOrder()
        {
            try
            {
                var orderHeaderFromDB = _unitOfWork.orderHeaderRepository.Get(x => x.Id == orderdetails.orderHeader.Id,tracked:true);

                orderHeaderFromDB.TrackingNumber = orderdetails.orderHeader.TrackingNumber;
                orderHeaderFromDB.Carrier = orderdetails.orderHeader.Carrier;
                orderHeaderFromDB.OrderStatus = orderdetails.orderHeader.OrderStatus;
                orderHeaderFromDB.ShipDate = DateTime.Now;

                if (orderHeaderFromDB.PaymentStatus == PaymentStatus.ApprovedForDelayedPayment) 
                {
                    orderHeaderFromDB.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
                }

                _unitOfWork.orderHeaderRepository.UpdateStatus(orderHeaderFromDB.Id, ShipmentStatus.Shipped);
                _unitOfWork.orderHeaderRepository.Update(orderHeaderFromDB);
                _unitOfWork.Save();
                TempData["success"] = "Order Shipped suuccessfully.";
                return RedirectToAction(nameof(Details), new { id = orderdetails.orderHeader.Id });
            }
            catch (Exception e)
            {
                throw new Exception("Error fetching dettails" + e.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = StaticDetails.Role_Admin + "," + StaticDetails.Role_Employee)]
        public IActionResult CancelOrder() 
        {
            try
            {
                var orderheader = _unitOfWork.orderHeaderRepository.Get(x => x.Id == orderdetails.orderHeader.Id);

                if (orderheader.PaymentStatus == PaymentStatus.Approved)
                {
                    var options = new RefundCreateOptions
                    {
                        Reason = RefundReasons.RequestedByCustomer,
                        PaymentIntent = orderheader.PaymentIntentId
                    };

                    _unitOfWork.orderHeaderRepository.UpdateStatus(orderheader.Id, ShipmentStatus.Cancelled, PaymentStatus.Refunded);
                }
                else
                {
                    _unitOfWork.orderHeaderRepository.UpdateStatus(orderheader.Id, ShipmentStatus.Cancelled, PaymentStatus.Refunded);
                }

                _unitOfWork.Save();
                TempData["success"] = "Order Cancelled suuccessfully.";
                return RedirectToAction(nameof(Details), new { id = orderdetails.orderHeader.Id });
            }
            catch (Exception e)
            {
                throw new Exception("Error Cancelling Order" + e.Message);
            }
        }

        [HttpPost]
        [ActionName("Details")]
        public IActionResult DelayedPayment()
        {
            try
            {
                orderdetails.orderHeader = _unitOfWork.orderHeaderRepository.Get(x => x.Id == orderdetails.orderHeader.Id
                                            ,includeProperties:"ApplicationUser");
                orderdetails.orderDetails = _unitOfWork.orderDetailRepository.GetAll(x => x.OrderHeaderId == orderdetails.orderHeader.Id
                                            ,includeProperties:"product");

                //stripe logic to add payment
                var domain = Request.Scheme + "://" + Request.Host.Value + "/";
                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={orderdetails.orderHeader.Id}",
                    CancelUrl = domain + $"admin/order/details?orderId={orderdetails.orderHeader.Id}",
                    LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach (var item in orderdetails.orderDetails)
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
                _unitOfWork.orderHeaderRepository.UpdateStripePaymentId(orderdetails.orderHeader.Id, session.Id, session.PaymentIntentId);
                //paymentintentId is only generated when payment is successfull.
                _unitOfWork.Save();
                //Redirect Url
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303); //redirecting to new url

                _unitOfWork.Save();
                TempData["success"] = "Order Cancelled suuccessfully.";
                return RedirectToAction(nameof(Details), new { id = orderdetails.orderHeader.Id });
            }
            catch (Exception e)
            {
                throw new Exception("Error Cancelling Order" + e.Message);
            }
        }

        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            try
            {
                OrderHeader orderHeader = _unitOfWork.orderHeaderRepository.Get(x => x.Id == orderHeaderId, includeProperties: "ApplicationUser");
                if (orderHeader.PaymentStatus == PaymentStatus.ApprovedForDelayedPayment)
                {
                    //this is company order
                    var service = new SessionService();
                    Session session = service.Get(orderHeader.SessionId);

                    if (session.PaymentStatus.ToLower() == "paid")
                    {
                        _unitOfWork.orderHeaderRepository.UpdateStripePaymentId(orderHeaderId, session.Id, session.PaymentIntentId);
                        _unitOfWork.orderHeaderRepository.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, PaymentStatus.Approved);
                        _unitOfWork.Save();
                    }

                }
            }
            catch (Exception e)
            {
                throw new Exception("Error during Payment COnfirmation " + e.Message);
            }
            return View(orderHeaderId);
        }

        #region API CALL 

        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaderList ;

            if (User.IsInRole(StaticDetails.Role_Admin) || User.IsInRole(StaticDetails.Role_Employee))
            {
                orderHeaderList = _unitOfWork.orderHeaderRepository.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                orderHeaderList = _unitOfWork.orderHeaderRepository.GetAll(x => x.ApplicationUserId == userId, includeProperties: "ApplicationUser");  
            }

            switch (status)
            {
                case "pending":
                    orderHeaderList = orderHeaderList.Where(x => x.PaymentStatus == PaymentStatus.ApprovedForDelayedPayment);
                    break;
                case "inprocess":
                    orderHeaderList = orderHeaderList.Where(x => x.OrderStatus == ShipmentStatus.Processing);
                    break;
                case "completed":
                    orderHeaderList = orderHeaderList.Where(x => x.OrderStatus == ShipmentStatus.Shipped);
                    break;
                case "approved":
                    orderHeaderList = orderHeaderList.Where(x => x.OrderStatus == ShipmentStatus.Approved);
                    break;
                default:
                    break;
            }
            //return Json(new { data = productsList });
            return Json(orderHeaderList);
        }
        #endregion
    }

}
