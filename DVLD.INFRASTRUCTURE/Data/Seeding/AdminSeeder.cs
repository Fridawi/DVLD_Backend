using BCrypt.Net;
using DVLD.CORE.Constants;
using DVLD.CORE.Entities;
using DVLD.CORE.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DVLD.INFRASTRUCTURE.Data.Seeding
{
    public static class AdminSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("AdminSeeder");
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            try
            {
                var adminExists = await unitOfWork.Users.IsExistAsync(u => u.Role == UserRoles.Admin);

                if (!adminExists)
                {
                    logger.LogInformation("No Admin user found. Seeding initial Admin user via Environment Variables...");

                    string adminUserName = config["ADMIN_USERNAME"] ?? "system_admin";
                    string adminPassword = config["ADMIN_PASSWORD"] ?? "DefaultAdminPassword2026!";

                    var defaultPerson = new Person
                    {
                        NationalNo = "SYS-ADMIN-01", 
                        FirstName = "System",
                        SecondName = "Root",
                        ThirdName = "Admin", 
                        LastName = "Manager",
                        DateOfBirth = new DateTime(1990, 1, 1),
                        Gendor = 0, 
                        Address = "System Main Server Environment",
                        Phone = "0000000000",
                        Email = "admin@system.local",
                        NationalityCountryID = 80,
                        ImagePath = null
                    };

                    await unitOfWork.People.AddAsync(defaultPerson);
                    await unitOfWork.CompleteAsync();

                    var adminAccount = new User
                    {
                        UserName = adminUserName,
                        Password = BCrypt.Net.BCrypt.HashPassword(adminPassword, 12),
                        IsActive = true,
                        Role = UserRoles.Admin,
                        PersonID = defaultPerson.PersonID
                    };

                    await unitOfWork.Users.AddAsync(adminAccount);
                    await unitOfWork.CompleteAsync();

                    logger.LogInformation("Admin user '{UserName}' has been seeded successfully from environment variables.", adminUserName);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the Admin user.");
            }
        }
    }
}