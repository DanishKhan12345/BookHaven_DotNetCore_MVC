using BookHaven.DataAccess.Repository.IRepository;
using BookHaven.Models.ViewModels;
using BookHaven.Models;
using BookHaven.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BookHaven.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace BookHaven.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.Role_Admin)]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDbContext _appDbContext;
        public UserController(IUnitOfWork unitOfWork, AppDbContext appDbContext)
        {
            _unitOfWork = unitOfWork;
            _appDbContext = appDbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region API CALL 

        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> usersList = _unitOfWork.applicationUserRepository.GetAll(includeProperties: "company").ToList();

            var userRoles = _appDbContext.UserRoles.ToList();
            var roles = _appDbContext.Roles.ToList();
            foreach (var user in usersList)
            {
                var roleId = userRoles.FirstOrDefault(x => x.UserId == user.Id).RoleId;
                user.Role = roles.FirstOrDefault(x => x.Id == roleId).Name;

                if (user.company == null)
                {
                    user.company = new Company() { Name = "" };
                }
            }
            return Json(usersList);
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            var userFromDb = _appDbContext.ApplicationUsers.FirstOrDefault(x => x.Id == id);
            if (userFromDb == null)
            {
                //return Json(new { success = false, message = "Error while Locking/Unlocking" });
                return Json(userFromDb);
            }
            if (userFromDb.LockoutEnd != null && userFromDb.LockoutEnd > DateTime.Now)
            {
                userFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                userFromDb.LockoutEnd = DateTime.Now.AddDays(100);
            }
            _appDbContext.SaveChanges();
            //return Json(userFromDb);
            return Json(new { success = true, message = "Lock Successfull" });
        }


        [HttpGet]
        public IActionResult RoleManagment(string userId)
        {
            string roleId = _appDbContext.UserRoles.FirstOrDefault(x=>x.UserId == userId).RoleId;

            RoleManagementVM roleManagementVM = new RoleManagementVM()
            {
                applicationUser = _appDbContext.ApplicationUsers.Include(x => x.company).FirstOrDefault(u => u.Id == userId),
                RoleList = _appDbContext.Roles.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name
                }),
                CompanyList = _appDbContext.Companies.Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Name
                })
            };
            roleManagementVM.applicationUser.Role = _appDbContext.Roles.FirstOrDefault(x=>x.Id == roleId).Name;
            return View(roleManagementVM);

        }

        [HttpPost]
        public IActionResult RoleManagment(RoleManagementVM rolemanagementvm)
        {

            return View();
        
        }


        #endregion
    }
}
