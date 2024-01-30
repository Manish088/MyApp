using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.CommonHelper;
using MyApp.DataAccessLayer;
using MyApp.DataAccessLayer.Infrastructure.IRepository;
using MyApp.Models;
using MyApp.Models.ViewModels;

namespace MyAppWeb.Controllers
{
   [Area("Admin")]
   [Authorize(Roles =WebsiteRole.Role_Admin)]
   
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitofwork;
        public CategoryController(IUnitOfWork unitofwork)
        {
            _unitofwork = unitofwork;
        }
        public IActionResult ViewCategory()
        {

            
                CategoryVM categoryVM = new CategoryVM();
                //IEnumerable<Category> getList=_applicatonDbContext.categories;

                categoryVM.Categories = _unitofwork.Category.GetAll();
                return View(categoryVM);
            
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

        //CreateUpdate Category together
        public  IActionResult CreateUpdate(int?id)
        {
            CategoryVM cvm = new CategoryVM();

            if(id== null || id==0)
            {
                //Here create page open
                return View(cvm);
            }
            else
            {

                //Here Edit page send data
                cvm.Category = _unitofwork.Category.GetT(x => x.Id == id);
                if (cvm == null)
                {
                    return NotFound();
                }
                else
                {
                    return View(nameof(ViewCategory),cvm);
                }
            }
           
           
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUpdate(CategoryVM categoryVM)
        {
            if(ModelState.IsValid)
            {

                if(categoryVM.Category.Id==0)
                {
                    _unitofwork.Category.Add(categoryVM.Category);
                  
                    TempData["success"] = "Category Created done";
                   
                }
                else
                {
                    _unitofwork.Category.Update(categoryVM.Category);
                    
                    TempData["success"] = "Category Update done";
                   
                }
                _unitofwork.Save();
                return RedirectToAction("ViewCategory");

            }
            return RedirectToAction("ViewCategory");

        }

        //Delete Category

        [HttpGet]
        public IActionResult Delete(int? id)
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
        }

        [HttpPost,ActionName("Delete")]
        public IActionResult DeleteDetails(int? id)
        {
            
            var data = _unitofwork.Category.GetT(x=>x.Id==id);

            if(data==null)
            {
                return NotFound();
            }
            _unitofwork.Category.Delete(data);

            _unitofwork.Save();
            TempData["success"] = "Delete Successfully done";
            return RedirectToAction("ViewCategory");
        }

    }
}
