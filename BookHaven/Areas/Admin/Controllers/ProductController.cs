﻿using BookHaven.DataAccess.Repository.IRepository;
using BookHaven.Models;
using BookHaven.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookHaven.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;

        }

        public IActionResult Index()
        {
            List<Product> productsList = _unitOfWork.productRepository.GetAll(includeProperties: "Category").ToList();
            return View(productsList);
        }

        public IActionResult AddUpdateProduct(int? id)
        {
            ProductVM productVM = new()
            {
                CategoryList = _unitOfWork.categoryRepository.GetAll()
                .Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }),
                Product = new Product()
            };
            if (id == null || id == 0)
            {
                //create
                return View("AddUpdateProduct", productVM);
            }
            else
            {
                //update
                productVM.Product = _unitOfWork.productRepository.Get(x => x.Id == id);
                return View("AddUpdateProduct", productVM);
            }
        }

        [HttpPost]
        public IActionResult AddUpdateProduct(ProductVM productVM, IFormFile? file)
        {
            try
            {
                if (productVM.Product != null && ModelState.IsValid)
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath; //for getting wwwroot folder path
                    if (file != null)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName); //giving new name to our file
                        string productPath = Path.Combine(wwwRootPath, @"images\product");

                        if (!(string.IsNullOrEmpty(productVM.Product.ImageUrl)))
                        {
                            //delete old image
                            var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));

                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }

                        }

                        using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }
                        productVM.Product.ImageUrl = @"\images\product\" + fileName; //for populating our viewmodel
                    }

                    if (productVM.Product.Id == 0)
                    {
                        _unitOfWork.productRepository.Add(productVM.Product);
                    }
                    else
                    {
                        _unitOfWork.productRepository.update(productVM.Product);
                    }

                    _unitOfWork.Save();
                    TempData["success"] = "Product added Successfully";
                    return RedirectToAction("Index");
                }
                else
                {
                    productVM.CategoryList = _unitOfWork.categoryRepository.GetAll().Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    });
                    return View("AddUpdateProduct", productVM);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error Adding Product" + e.Message);
            }
        }

        //public IActionResult EditProduct(int? id)
        //{
        //    try
        //    {
        //        if (id == null || id == 0)
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            var productToUpdate = _unitOfWork.productRepository.Get(x => x.Id == id);
        //            if (productToUpdate == null)
        //            {
        //                return NotFound();
        //            }
        //            return View(productToUpdate);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception("Error Updating Product" + e.Message);
        //    }
        //}

        //[HttpPost]
        //public IActionResult EditProduct(Product product)
        //{
        //    try
        //    {
        //        if (product != null && ModelState.IsValid)
        //        {
        //            _unitOfWork.productRepository.update(product);
        //            _unitOfWork.Save();
        //            TempData["success"] = "Product edited Successfully";
        //            return RedirectToAction("Index");
        //        }
        //        return View();
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception("Error Updating Product" + e.Message);
        //    }
        //}

        //public IActionResult DeleteProduct(int id)
        //{
        //    try
        //    {
        //        if (id == 0)
        //        {
        //            return NotFound();
        //        }
        //        var productToDelete = _unitOfWork.productRepository.Get(x => x.Id == id);
        //        if (productToDelete == null)
        //        {
        //            return NotFound();
        //        }
        //        return View(productToDelete);
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception("Error Deleting Product" + e.Message);
        //    }
        //}

        //[HttpPost, ActionName("DeleteProduct")]
        //public IActionResult DeleteProductPost(Product product)
        //{
        //    try
        //    {
        //        if (product != null)
        //        {
        //            _unitOfWork.productRepository.Remove(product);
        //            _unitOfWork.Save();
        //            TempData["success"] = "Product deleted Successfully";
        //            return RedirectToAction("Index");
        //        }
        //        else
        //        {
        //            return NotFound();
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception("Error Deleting Product" + e.Message);
        //    }
        //}

        #region API CALL 

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> productsList = _unitOfWork.productRepository.GetAll(includeProperties: "Category").ToList();
            //return Json(new { data = productsList });
            return Json(productsList);
        }

        [HttpDelete]
        public IActionResult DeleteProduct(int? id)
        {
            var productTobeDeleted = _unitOfWork.productRepository.Get(x => x.Id == id);
            if (productTobeDeleted == null)
            {
                return Json(new { success = false, message = "Error while Deleting" });
            }
            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productTobeDeleted.ImageUrl.TrimStart('\\'));

            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _unitOfWork.productRepository.Remove(productTobeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Deleted Successfully"});
        }

        #endregion
    }
}
