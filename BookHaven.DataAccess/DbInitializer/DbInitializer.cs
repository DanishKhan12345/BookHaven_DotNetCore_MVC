using BookHaven.DataAccess.Data;
using BookHaven.Models;
using BookHaven.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookHaven.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _db;

        public DbInitializer(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        public void Initialize()
        {
            //migrations if they are not applied
            try
            {
                if (_db.Database.GetPendingMigrations().Count()>0)
                {
                   _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
            }

            //create roles if not created
            if (!_roleManager.RoleExistsAsync(StaticDetails.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(StaticDetails.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(StaticDetails.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(StaticDetails.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(StaticDetails.Role_Company)).GetAwaiter().GetResult();

                //if roles are not created , then we will create admin user as well.
                _userManager.CreateAsync(new ApplicationUser { 
                UserName = "admindanish@bookhaven.com",
                Email = "admindanish@bookhaven.com",
                Name = "Danish Khan",
                PhoneNumber = "9811262446",
                StreetAddress = "Vikas Puri",
                State ="Delhi",
                PostalCode = "110018",
                City = "New Delhi"
                },"Dani1998@").GetAwaiter().GetResult();

                ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(x=>x.Email == "admindanish@bookhaven.com");
                _userManager.AddToRoleAsync(user,StaticDetails.Role_Admin);
            }
            return;
        }
    }
}
