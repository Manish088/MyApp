using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.DataAccessLayer.Infrastructure.IRepository;
using MyApp.Models;
using MyApp.Models.ViewModels;
using System.Diagnostics;
using System.Security.Claims;

namespace MyAppWeb.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IUnitOfWork _unintOfWork;
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unintOfWork )
        {
            _logger = logger;
            _unintOfWork = unintOfWork;
        }
        public void FindCartCount()
        {
            var ct = 0;
            var cartVM = new CartVM()
            {
                ListOfItem = _unintOfWork.Cart.GetAll()
            };
            var c = cartVM.ListOfItem.Select(x => x.Count);
            foreach (var item in c)
            {
                ct += item;
            }
            ViewData["cartCount"] = ct;
        }
        public IActionResult Index()
        {
            FindCartCount();
            IEnumerable<Product> getAllProduct = _unintOfWork.Product.GetAll(includeProperties:"Category");
            return View(getAllProduct);
        }

        [HttpGet]
        public IActionResult Details(int ? productid)
        {
            Cart cart = new Cart()
            {
                Product = _unintOfWork.Product.GetT(m => m.Id == productid, includeProperties: "Category"),
                ProductId = (int)productid,
                Count = 1
            };
            return View(cart);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(Cart cart)
        {

            if(ModelState.IsValid)
            { 
                    var claimsIdentity=(ClaimsIdentity)User.Identity;
                    var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    cart.ApplicationUserId = claims.Value;
                    var cartitem=_unintOfWork.Cart.GetT(m=>m.ProductId==cart.ProductId && m.ApplicationUserId==claims.Value);
           

                if(cartitem==null)
                {
                    _unintOfWork.Cart.Add(cart);
                }
                else
                {
                    _unintOfWork.Cart.IncrementItemCart(cartitem, cart.Count);
                }
                
                _unintOfWork.Save();
            }
            return RedirectToAction("Index");
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