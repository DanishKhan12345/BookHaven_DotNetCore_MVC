using BookHaven.DataAccess.Repository.IRepository;
using BookHaven.Models.ViewModels;
using BookHaven.Models;
using BookHaven.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookHaven.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Company> companiesList = _unitOfWork.companyRepository.GetAll().ToList();
            return View(companiesList);
        }

        public IActionResult AddUpdateCompany(int? id)
        {
            if (id == null || id == 0)
            {
                //create
                return View("AddUpdateCompany",new Company());
            }
            else
            {
                //update
                Company company = _unitOfWork.companyRepository.Get(x => x.Id == id);
                return View("AddUpdateCompany", company);
            }
        }

        [HttpPost]
        public IActionResult AddUpdateCompany(Company company)
        {
            try
            {
                if (company!= null && ModelState.IsValid)
                {
                    if (company.Id == 0)
                    {
                        _unitOfWork.companyRepository.Add(company);
                    }
                    else
                    {
                        _unitOfWork.companyRepository.update(company);
                    }

                    _unitOfWork.Save();
                    TempData["success"] = "Company added Successfully";
                    return RedirectToAction("Index");
                }
                else
                {
                    return View("AddUpdateCompany", company);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error Adding Company" + e.Message);
            }
        }

        #region API CALL 

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> companiesList = _unitOfWork.companyRepository.GetAll().ToList();
            //return Json(new { data = companiesList });
            return Json(companiesList);
        }

        [HttpDelete]
        public IActionResult DeleteCompany(int? id)
        {
            var companyTobeDeleted = _unitOfWork.companyRepository.Get(x => x.Id == id);
            if (companyTobeDeleted == null)
            {
                return Json(new { success = false, message = "Error while Deleting" });
            }
            _unitOfWork.companyRepository.Remove(companyTobeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Deleted Successfully" });
        }

        #endregion
    }
}
