using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ResearchProjectManager.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResearchProjectManager.Data
{
    // IHostedService runs automatically in the background when the app starts
    public class DbSeeder : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public DbSeeder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Create a scope to resolve our scoped Identity services
            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                await SeedRolesAsync(services);

                await SeedUsersAsync(services); //TODO: Add actual value to the function
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<DbSeeder>>();
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        // 1. SEED ROLES

        private async Task SeedRolesAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<Role>>();

            foreach (var role in Role.PredefinedRoles.AllRoles)
            {
                // role.Key is the Name 
                // role.Value is the Description 

                if (!await roleManager.RoleExistsAsync(role.Key))
                {
                    await roleManager.CreateAsync(new Role
                    {
                        Name = role.Key,
                        Description = role.Value
                    });
                }
            }
        }

        // 2. SEED USERS (Placeholder for future you)

        private async Task SeedUsersAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<User>>();

            // Your future code to create the default Admin account will go here
        }

    }
}