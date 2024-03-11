using BookHaven.Data;
using BookHaven.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookHaven.Controllers
{
    public class CategoryController : Controller
    {
        private readonly AppDbContext _appDbprovider;
        public CategoryController(AppDbContext appDbprovider)
        {
            _appDbprovider = appDbprovider;
        }
        public IActionResult Index()
        {
            List<Category> CategoryList = _appDbprovider.Categories.ToList();
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
                        _appDbprovider.Categories.Add(category);
                        _appDbprovider.SaveChanges();
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
                var categoryDb = _appDbprovider.Categories.FirstOrDefault(x => x.Id == id);
                if (categoryDb == null)
                {
                    return NotFound();
                }
                return View(categoryDb);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        public IActionResult EditCategory(Category category)
        {
            try
            {
                if (category != null)
                {
                    if (ModelState.IsValid)
                    {
                        _appDbprovider.Update(category); //will automatically check for id and update
                        _appDbprovider.SaveChanges();
                        TempData["success"] = "Category edited Successfully";
                        return RedirectToAction("Index");
                    }
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
                var categoryToDelete = _appDbprovider.Categories.FirstOrDefault(x => x.Id == id);
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

        [HttpPost,ActionName("DeleteCategory")]
        public IActionResult DeleteCategoryPost(int? id)
        {
            try
            {
                var categorytodelete=_appDbprovider.Categories.FirstOrDefault(x=>x.Id==id);
                if (categorytodelete!=null)
                {
                    _appDbprovider.Categories.Remove(categorytodelete);
                    _appDbprovider.SaveChanges();
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

