using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.CommonHelper;
using MyApp.DataAccessLayer.Infrastructure.IRepository;
using MyApp.Models;
using MyApp.Models.ViewModels;
using Stripe.Checkout;
using System.Security.Claims;

namespace MyAppWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private IUnitOfWork _unitOfWork;
        public CartVM cartVM;

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


       
        public IActionResult Index()
        {
            

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            cartVM = new CartVM()
            {
                ListOfItem = _unitOfWork.Cart.GetAll(m => m.ApplicationUserId == claims.Value, includeProperties: "Product"),
                OrderHeader=new MyApp.Models.OrderHeader()
            };

            foreach (var item in cartVM.ListOfItem)
            {
                cartVM.OrderHeader.OrderTotal += (item.Product.Price * item.Count);


            }
            
            return View(cartVM);
        }

        //Order Summury

        public IActionResult Summury()
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            cartVM = new CartVM()
            {
                ListOfItem = _unitOfWork.Cart.GetAll(m => m.ApplicationUserId == claims.Value, includeProperties: "Product"),
                OrderHeader = new MyApp.Models.OrderHeader()
            };

            cartVM.OrderHeader.applicationUser=_unitOfWork.ApplicationUser.GetT(m=>m.Id==claims.Value);

            cartVM.OrderHeader.Name = cartVM.OrderHeader.applicationUser.Name;
            cartVM.OrderHeader.Phone = cartVM.OrderHeader.applicationUser.PhoneNumber;
            cartVM.OrderHeader.Address = cartVM.OrderHeader.applicationUser.Address;
            cartVM.OrderHeader.City = cartVM.OrderHeader.applicationUser.City;
            cartVM.OrderHeader.State = cartVM.OrderHeader.applicationUser.State;
            cartVM.OrderHeader.PostalCode = cartVM.OrderHeader.applicationUser.Pincode;
            foreach (var item in cartVM.ListOfItem)
            {
                cartVM.OrderHeader.OrderTotal += (item.Product.Price * item.Count);


            }

            return View(cartVM);
          
        }
        //http post method
        [HttpPost]
        public IActionResult Summury(CartVM cartVM)
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            cartVM.ListOfItem = _unitOfWork.Cart.GetAll(m => m.ApplicationUserId == claims.Value, includeProperties: "Product");

            cartVM.OrderHeader.OrderStatus = OrderStatus.StatusPending;
            cartVM.OrderHeader.PaymentStatus=PaymentStatus.StatusPending;
            cartVM.OrderHeader.DateOfOrder = DateTime.Now;
            cartVM.OrderHeader.ApplicationUserId = claims.Value;

            foreach (var item in cartVM.ListOfItem)
            {
                cartVM.OrderHeader.OrderTotal += (item.Product.Price * item.Count);


            }
            _unitOfWork.OrderHeader.Add(cartVM.OrderHeader);
            _unitOfWork.Save();

            foreach (var item in cartVM.ListOfItem)
            {
                OrderDetail orderDetail = new OrderDetail()
                {
                    ProductId = item.ProductId,
                    Price = item.Product.Price,
                    OrderHeaderId = cartVM.OrderHeader.Id,
                    Count = item.Count
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();

            }
            var domain = "https://localhost:7275/";
            var options = new SessionCreateOptions
            {

                LineItems = new List<SessionLineItemOptions>(),

                Mode = "payment",
                SuccessUrl = domain+$"Customer/Cart/ordersuccess?id={cartVM.OrderHeader.Id}",
                CancelUrl = domain+$"Customer/Cart/Index",

            };

            foreach (var item in cartVM.ListOfItem)
            {
                var lineitemoptions = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Product.Price*100),
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

            _unitOfWork.OrderHeader.PaymentStatus(cartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();

            _unitOfWork.Cart.DeleteRange(cartVM.ListOfItem);
            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);


           
            return RedirectToAction("Index","Home");


        }


        public IActionResult ordersuccess(int id)
        {
            var orderheader = _unitOfWork.OrderHeader.GetT(m => m.Id == id);
            var service = new SessionService();
            Session session = service.Get(orderheader.SessionId);
            if(session.PaymentStatus.ToLower()=="paid")
            {
                _unitOfWork.OrderHeader.UpdateStatus(id,OrderStatus.StatusApproved,PaymentStatus.StatusApproved);
            }
            List<Cart> carts = _unitOfWork.Cart.GetAll(m => m.ApplicationUserId == orderheader.ApplicationUserId).ToList();
            _unitOfWork.Cart.DeleteRange(carts);
            _unitOfWork.Save();
            return View(id);
        }

        public IActionResult Plus(int id)
        {
            var plusItem = _unitOfWork.Cart.GetT(m => m.id == id);
            _unitOfWork.Cart.IncrementItemCart(plusItem, 1);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        public IActionResult Minus(int id)
        {
            var minusItem = _unitOfWork.Cart.GetT(m => m.id == id);

            if(minusItem.Count<=1)
            {
                _unitOfWork.Cart.Delete(minusItem);
            }
            else
            {
                _unitOfWork.Cart.DecrementItemCart(minusItem, 1);
            }
           
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        public IActionResult delete(int id)
        {
            var deleteItem = _unitOfWork.Cart.GetT(m => m.id == id);
            _unitOfWork.Cart.Delete(deleteItem);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }
    }
}
