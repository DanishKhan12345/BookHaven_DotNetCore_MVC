using BookHaven.DataAccess.Repository.IRepository;
using BookHaven.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;

namespace BookHaven.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Product> productsList = _unitOfWork.productRepository.GetAll().ToList();
            return View(productsList);
        }

        public IActionResult Create()
        {
            IEnumerable<SelectListItem> CategoryList = _unitOfWork.categoryRepository.GetAll()
                .Select(x=>new SelectListItem 
                { 
                    Text = x.Name,
                    Value =x.Id.ToString()
                });
            ViewBag.CategoryList = CategoryList;
            return View("CreateProduct");
        }

        [HttpPost]
        public IActionResult Create(Product product)
        {
            try
            {
                if (product != null && ModelState.IsValid)
                {
                    _unitOfWork.productRepository.Add(product);
                    _unitOfWork.Save();
                    return RedirectToAction("Index");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error Adding Product" + e.Message);
            }

            return View("CreateProduct");
        }

        public IActionResult EditProduct(int? id)
        {
            try
            {
                if (id == null || id == 0)
                {
                    return NotFound();
                }
                else
                {
                    var productToUpdate = _unitOfWork.productRepository.Get(x => x.Id == id);
                    if (productToUpdate == null)
                    {
                        return NotFound();
                    }
                    return View(productToUpdate);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error Updating Product" + e.Message);
            }
        }

        [HttpPost]
        public IActionResult EditProduct(Product product)
        {
            try
            {
                if (product != null && ModelState.IsValid)
                {
                    _unitOfWork.productRepository.update(product);
                    _unitOfWork.Save();
                    TempData["success"] = "Category edited Successfully";
                    return RedirectToAction("Index");
                }
                return View();
            }
            catch (Exception e)
            {
                throw new Exception("Error Updating Product" + e.Message);
            }
        }

        public IActionResult DeleteProduct(int id)
        {
            try
            {
                if(id == 0) 
                {
                    return NotFound();
                }
                var productToDelete = _unitOfWork.productRepository.Get(x=>x.Id==id);
                if (productToDelete == null)
                {
                    return NotFound();
                }
                return View(productToDelete);
            }
            catch (Exception e)
            {
                throw new Exception("Error Deleting Product" + e.Message);
            }
        }

        [HttpPost,ActionName("DeleteProduct")]
        public IActionResult DeleteProductPost(Product product)
        {
            try
            {
                if (product !=null)
                {
                    _unitOfWork.productRepository.Remove(product);
                    _unitOfWork.Save();
                    TempData["success"] = "Category deleted Successfully";
                    return RedirectToAction("Index");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error Deleting Product" + e.Message);
            }
        }
    }
}
