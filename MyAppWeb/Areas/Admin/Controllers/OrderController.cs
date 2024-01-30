using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.CommonHelper;
using MyApp.DataAccessLayer.Infrastructure.IRepository;
using MyApp.Models;
using MyApp.Models.ViewModels;
using Stripe;

using Stripe.Checkout;
using System.Security.Claims;



namespace MyAppWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private IUnitOfWork _unitOfWork;

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult AllOrders(string status)
        {

            IEnumerable<OrderHeader> orderHeaders;

            if(User.IsInRole("Admin") || User.IsInRole("Employee"))
            {
                orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "applicationUser");
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                orderHeaders = _unitOfWork.OrderHeader.GetAll(m=>m.ApplicationUserId==claims.Value);
            }

            switch (status)
            {
                case "pending":
                    orderHeaders = orderHeaders.Where(m => m.PaymentStatus == PaymentStatus.StatusPending);
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(m => m.PaymentStatus == PaymentStatus.StatusApproved);
                    break;
                case "underprocess":
                    orderHeaders = orderHeaders.Where(m => m.OrderStatus == OrderStatus.StatusInProcess);
                    break;
                case "shipped":
                    orderHeaders = orderHeaders.Where(m => m.OrderStatus == OrderStatus.StatusShpped);
                    break;
                default:
                    break;
            }

            return View(orderHeaders);
        }

        public IActionResult OrderDetails(int id)
        {
            OrderVM orderVm = new OrderVM()
            {
                orderDetails = _unitOfWork.OrderDetail.GetAll(m => m.OrderHeader.Id == id, includeProperties: "Product"),
                orderHeader = _unitOfWork.OrderHeader.GetT(m => m.Id == id,includeProperties: "applicationUser"),
               

            };
           
            return View(orderVm);
        }

        [Authorize(Roles = WebsiteRole.Role_Admin)]
        [HttpPost]
        public IActionResult OrderDetails(OrderVM orderVM)
        {
            var orderHeader = _unitOfWork.OrderHeader.GetT(m => m.Id == orderVM.orderHeader.Id);
            if(orderHeader != null)
            {
                orderHeader.Name=orderVM.orderHeader.Name;
                orderHeader.Phone=orderVM.orderHeader.Phone;
                orderHeader.Address=orderVM.orderHeader.Address;
                orderHeader.City = orderVM.orderHeader.City;
                orderHeader.State = orderVM.orderHeader.State;
                orderHeader.PostalCode=orderVM.orderHeader.PostalCode;

                if(orderVM.orderHeader.Carrier!=null)
                {
                    orderHeader.Carrier=orderVM.orderHeader.Carrier;
                }
                if(orderVM.orderHeader.TrackingNumber!=null)
                {
                    orderHeader.TrackingNumber=orderVM.orderHeader.TrackingNumber;
                }
                _unitOfWork.OrderHeader.Update(orderHeader);
                _unitOfWork.Save();
                TempData["success"] = "Info Updated";
                return RedirectToAction("OrderDetails", "Order", new { id = orderVM.orderHeader.Id });

            }
            else
            {
                TempData["success"] = "Info not Updated";
                return RedirectToAction("OrderDetails", "Order");
            }
           

        }

        public IActionResult InProcess(OrderVM orderVM)
        {
            _unitOfWork.OrderHeader.UpdateStatus(orderVM.orderHeader.Id, OrderStatus.StatusInProcess);

            _unitOfWork.Save();
            TempData["success"] = "Order Status Updated InProccess";
            return RedirectToAction("OrderDetails", "Order", new { id = orderVM.orderHeader.Id });
        }
        public IActionResult Shipped(OrderVM orderVM)
        {
            var orderHeader = _unitOfWork.OrderHeader.GetT(m => m.Id == orderVM.orderHeader.Id);

            if(orderHeader!=null)
            {
                orderHeader.Carrier = orderVM.orderHeader.Carrier;
                orderHeader.TrackingNumber= orderVM.orderHeader.TrackingNumber;
                orderHeader.OrderStatus = OrderStatus.StatusShpped;
                orderHeader.DateOfShipping=DateTime.Now;
                _unitOfWork.OrderHeader.Update(orderHeader);
                _unitOfWork.Save();
                TempData["success"] = "Order Status Updated-Shipped";
                return RedirectToAction("OrderDetails", "Order", new { id = orderVM.orderHeader.Id });
            }
            else
            {
                TempData["success"] = "Order Status Not Updated-Shipped";
                return RedirectToAction("OrderDetails", "Order");
            }
            
          
           
        }
         [Authorize(Roles =WebsiteRole.Role_Admin)]
        public IActionResult CancelOrder(OrderVM orderVM)
        {
            var orderHeader = _unitOfWork.OrderHeader.GetT(m => m.Id == orderVM.orderHeader.Id);

            if(orderHeader.PaymentStatus==PaymentStatus.StatusApproved)
            {
                var refund = new RefundCreateOptions()
                {
                    Reason=RefundReasons.RequestedByCustomer,
                    PaymentIntent=orderHeader.PaymentIntentId
                };
                var service = new RefundService();
                Refund Refund= service.Create(refund);
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, OrderStatus.StatusCancelled, OrderStatus.StatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, OrderStatus.StatusCancelled, OrderStatus.StatusCancelled);
            }
          
            _unitOfWork.Save();
            TempData["success"] = "Order Status Cancelled";
            return RedirectToAction("OrderDetails", "Order", new { id = orderVM.orderHeader.Id });
        
        
        }
        public IActionResult PayNow(OrderVM orderVM)
        {
            var orderDetails = _unitOfWork.OrderDetail.GetAll(m => m.OrderHeader.Id == orderVM.orderHeader.Id, includeProperties: "Product");
            var orderHeader = _unitOfWork.OrderHeader.GetT(m => m.Id == orderVM.orderHeader.Id, includeProperties: "applicationUser");

            var domain = "https://localhost:7275/";
            var options = new SessionCreateOptions
            {

                LineItems = new List<SessionLineItemOptions>(),

                Mode = "payment",
                SuccessUrl = domain + $"Customer/Cart/ordersuccess?id={orderVM.orderHeader.Id}",
                CancelUrl = domain + $"Customer/Cart/Index",

            };

            foreach (var item in orderDetails)
            {
                var lineitemoptions = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Product.Price * 100),
                        Currency = "INR",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name,
                        },

                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(lineitemoptions);


            }
            var service = new SessionService();
            Session session = service.Create(options);

            _unitOfWork.OrderHeader.PaymentStatus(orderVM.orderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();


            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);



            return RedirectToAction("Index", "Home");
        }
    }
}
