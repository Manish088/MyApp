using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyApp.CommonHelper;
using MyApp.DataAccessLayer;
using MyApp.DataAccessLayer.Infrastructure.IRepository;
using MyApp.Models;
using MyApp.Models.ViewModels;

namespace MyAppWeb.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = WebsiteRole.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitofwork;

        //It used to Path set into wwwroot.
        private IWebHostEnvironment _hostEnvironment;
        public ProductController(IUnitOfWork unitofwork, IWebHostEnvironment hostEnvironment)
        {
            _unitofwork = unitofwork;
            _hostEnvironment = hostEnvironment;
        }
        public IActionResult ViewProduct()
        {

            ProductVM productVM = new ProductVM();
            //IEnumerable<Category> getList=_applicatonDbContext.categories;

            productVM.Products = _unitofwork.Product.GetAll(includeProperties:"Category");
            return View(productVM);
        }

        //Create Category

        /*public IActionResult CreateCategory()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateCategory(Category ct)
        {
            if(ModelState.IsValid)
            {
                _unitofwork.Category.Add(ct);
                _unitofwork.Save();
                TempData["success"] = "Insert Successfully done";
                return RedirectToAction("ViewCategory");
            }
            return View();
           

           
        }*/

        //CreateUpdate product together
        public  IActionResult CreateUpdate(int?id)
        {
            ProductVM pvm = new ProductVM()
            {
                Product=new(),
                Categories=_unitofwork.Category.GetAll().Select(m=>new SelectListItem()
                {
                    Text=m.Name,
                    Value=m.Id.ToString()
                })

            };

            if(id== null || id==0)
            {
                //Here create page open
                return View(pvm);
            }
            else
            {

                //Here Edit page send data
                pvm.Product= _unitofwork.Product.GetT(x => x.Id == id);
                if (pvm == null)
                {
                    return NotFound();
                }
                else
                {
                    return View(pvm);
                }
            }
           
           
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUpdate(ProductVM productVM,IFormFile? file)
        {
            if(ModelState.IsValid)
            {
                string filename = string.Empty;
                if(file!=null)
                {
                    string UploadDir = Path.Combine(_hostEnvironment.WebRootPath, "ProductImage");
                    filename = Guid.NewGuid().ToString() + "-" + file.FileName;
                    string filepath=Path.Combine(UploadDir, filename);

                    if(productVM.Product.ImageUrl!=null)
                    {
                        var oldimagepath = Path.Combine(_hostEnvironment.WebRootPath, productVM.Product.ImageUrl.TrimStart('\\'));
                        if(System.IO.File.Exists(oldimagepath))
                        {
                            System.IO.File.Delete(oldimagepath);
                        }
                    }
                    using(var filestream =new FileStream(filepath,FileMode.Create))
                    {
                        file.CopyTo(filestream);
                    }
                    productVM.Product.ImageUrl = @"\ProductImage\" + filename;
                }
                if(productVM.Product.Id==0)
                {
                    _unitofwork.Product.Add(productVM.Product);
                  
                    TempData["success"] = "Product Created done";
                   
                }
                else
                {
                   _unitofwork.Product.Update(productVM.Product);
                    
                    TempData["success"] = "Product Update done";
                   
                }
                _unitofwork.Save();
                return RedirectToAction("ViewProduct");

            }
            return RedirectToAction("ViewProduct");

        }

        //Delete Category

        [HttpGet]
     /*   public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var data = _unitofwork.Category.GetT(x=>x.Id==id);
            if (data == null)
            {
                return NotFound();
            }
            return View(data);
        }*/

       /// [HttpGet,ActionName("Delete")]
        public IActionResult Delete(int? id)
        {
            
            var data = _unitofwork.Product.GetT(x=>x.Id==id);

            if(data==null)
            {
                return NotFound();
            }
            else
            {
                var oldimagepath = Path.Combine(_hostEnvironment.WebRootPath, data.ImageUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldimagepath))
                {
                    System.IO.File.Delete(oldimagepath);
                }
                _unitofwork.Product.Delete(data);
                _unitofwork.Save();
                TempData["success"] = "Delete Successfully done";
                return Json(data);
            }
            

           
           
        }

    }
}
