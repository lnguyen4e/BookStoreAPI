using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore_API.Data
{
    public class SeedData
    {
        public static void Seed(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            SeedRoles(roleManager);
            SeedUsers(userManager);
        }
        private static void SeedUsers(UserManager<IdentityUser> userManager)
        {

        }

        private async static Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            if(!await roleManager.RoleExistsAsync("Administrtor"))
            {
                var role = new IdentityRole
                {
                    Name = "Administrator",
                };
                var result = await roleManager.CreateAsync(role);
            }
        }
    }
}
