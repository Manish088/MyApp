
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.CommonHelper;

using MyApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.DataAccessLayer.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicatonDbContext _applicatonDbContext;

        public DbInitializer(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicatonDbContext applicatonDbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _applicatonDbContext = applicatonDbContext;
        }

        public void Initialize()
        {
            try
            { 
                if(_applicatonDbContext.Database.GetPendingMigrations().Count()>0)
                {
                    _applicatonDbContext.Database.Migrate();
                }

            }
            catch (Exception)
            {

                throw;
            }
            //Here check role admin not exists
            if (!_roleManager.RoleExistsAsync(WebsiteRole.Role_Admin).GetAwaiter().GetResult())
            {
                //Here define all role
                _roleManager.CreateAsync(new IdentityRole(WebsiteRole.Role_Admin)).GetAwaiter().GetResult();

                _roleManager.CreateAsync(new IdentityRole(WebsiteRole.Role_User)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(WebsiteRole.Role_Employee)).GetAwaiter().GetResult();
            }
            _userManager.CreateAsync(new ApplicationUser
            {
                UserName="admin123@gmail.com",
                Email="admin123@gmail.com",
                Name="Admin",
                Address="Surat",
                City="Surat",
                State="Gujarat",
                Pincode="394222",


            },"Admin@123").GetAwaiter().GetResult();
            ApplicationUser user=_applicatonDbContext.applicationUsers.FirstOrDefault(x=>x.Email== "admin123@gmail.com");
            _userManager.AddToRoleAsync(user, WebsiteRole.Role_Admin).GetAwaiter().GetResult();

         
        }
    }
}
