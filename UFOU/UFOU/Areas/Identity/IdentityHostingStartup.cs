using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UFOU.Areas.Identity.Data;
using UFOU.Models;

[assembly: HostingStartup(typeof(UFOU.Areas.Identity.IdentityHostingStartup))]
namespace UFOU.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<UsersRolesContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("UsersRoles")));

                services.AddDefaultIdentity<IdentityUser>(config =>
                {
                    config.SignIn.RequireConfirmedEmail = true;
                }).AddRoles<IdentityRole>()
                  .AddEntityFrameworkStores<UsersRolesContext>();

                //services.AddTransient<IEmailSender, EmailSender>();
            });
        }
    }
}