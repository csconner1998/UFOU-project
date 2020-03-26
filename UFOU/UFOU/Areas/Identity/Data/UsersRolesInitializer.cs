using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UFOU.Data;
using UFOU.Models;

namespace UFOU.Areas.Identity.Data
{
    public class UsersRolesInitializer
    {
        internal async static Task Initialize(UsersRolesContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, UFOContext ufoContext)
        {
            context.Database.Migrate();

            // ROLE SEEDING
            // certain roles are required for the proper functioning of the app
            var reqRoles = new HashSet<IdentityRole>()
            {
                new IdentityRole()
                {
                    Id = "10",
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new IdentityRole()
                {
                    Id = "20",
                    Name = "UfoUser",
                    NormalizedName = "UFOUSER"
                }
            };

            // add all the required roles
            foreach (IdentityRole r in reqRoles)
            {
                if (!context.Roles.Contains(r))
                    roleManager.CreateAsync(r).Wait();
            }

            // USER/USER-ROLE SEEDING
            // if no users are in the DB, seed them here
            if (!context.Users.Any())
            {
                var users = new List<IdentityUser>()
                {
                    new IdentityUser()
                    {
                        Id = "1000",
                        NormalizedUserName = "Admin",
                        Email = "admin@ufou.com"
                    },
                    new IdentityUser()
                    {
                        Id = "2000",
                        NormalizedUserName = "Jim",
                        Email = "jim@email.com"
                    },
                    new IdentityUser()
                    {
                        Id = "2001",
                        NormalizedUserName = "Erin",
                        Email = "erin@email.com"
                    },
                    new IdentityUser()
                    {
                        Id = "2002",
                        NormalizedUserName = "Miriah",
                        Email = "miriah@email.com"
                    },
                    new IdentityUser()
                    {
                        Id = "2003",
                        NormalizedUserName = "Todd",
                        Email = "todd@email.com"
                    }

                };

                // add the user, then add their role
                foreach (IdentityUser u in users)
                {
                    // add user
                    u.UserName = u.Email;
                    u.EmailConfirmed = true;
                    u.SecurityStamp = Guid.NewGuid().ToString();
                    await userManager.CreateAsync(u, "Abc123!");

                    // add user to role based on id
                    switch (u.Id)
                    {
                        case string id when id[0].Equals('1'):
                            await userManager.AddToRoleAsync(u, "Admin");
                            break;
                        case string id when id[0].Equals('2'):
                            await userManager.AddToRoleAsync(u, "UfoUser");
                            break;
                    }
                }

            }


                context.SaveChanges();
        }
    }
}