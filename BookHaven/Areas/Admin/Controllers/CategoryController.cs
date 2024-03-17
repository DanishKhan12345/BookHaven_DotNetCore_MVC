using BookHaven.DataAccess.Data;
using BookHaven.DataAccess.Repository;
using BookHaven.DataAccess.Repository.IRepository;
using BookHaven.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookHaven.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Category> CategoryList = _unitOfWork.categoryRepository.GetAll().ToList();
            return View(CategoryList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            try
            {
                if (category != null)
                {
                    if (category.Name == category.DisplayOrder.ToString())
                    {
                        ModelState.AddModelError("name", "The Display Order cannot be same as Name");
                    }
                    if (ModelState.IsValid) //tocheck Data Annotations
                    {
                        _unitOfWork.categoryRepository.Add(category);
                        _unitOfWork.Save();
                        TempData["success"] = "Category added Successfully";
                        return RedirectToAction("Index");
                    }
                }
                return View();
            }
            catch (Exception e)
            {
                throw new Exception("Category is Invalid" + e.Message);
            }
        }

        public IActionResult EditCategory(int? id)
        {
            try
            {
                if (id == null || id == 0)
                {
                    return NotFound();
                }
                var categoryDb = _unitOfWork.categoryRepository.Get(x => x.Id == id);
                if (categoryDb == null)
                {
                    return NotFound();
                }
                return View(categoryDb);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        [HttpPost]
        public IActionResult EditCategory(Category category)
        {
            try
            {
                if (category != null && ModelState.IsValid)
                {
                    _unitOfWork.categoryRepository.Update(category); //will automatically check for id and update
                    _unitOfWork.Save();
                    TempData["success"] = "Category edited Successfully";
                    return RedirectToAction("Index");
                }
                return View();
            }
            catch (Exception e)
            {

                throw new Exception("Error while updating Category" + e.Message);
            }
        }

        public IActionResult DeleteCategory(int? id)
        {
            try
            {
                if (id == null || id == 0)
                {
                    return NotFound();
                }
                var categoryToDelete = _unitOfWork.categoryRepository.Get(x => x.Id == id);
                if (categoryToDelete == null)
                {
                    return NotFound();
                }
                return View(categoryToDelete);
            }
            catch (Exception e)
            {
                throw new Exception("Error occured while Deleting" + e.Message);
            }
        }

        [HttpPost, ActionName("DeleteCategory")]
        public IActionResult DeleteCategoryPost(int? id)
        {
            try
            {
                var categorytodelete = _unitOfWork.categoryRepository.Get(x => x.Id == id);
                if (categorytodelete != null)
                {
                    _unitOfWork.categoryRepository.Remove(categorytodelete);
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
                throw new Exception("Error occured while Deleting" + e.Message);
            }
        }
    }
}

