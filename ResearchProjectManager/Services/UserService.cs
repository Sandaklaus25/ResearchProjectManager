using Microsoft.AspNetCore.Identity;
using ResearchProjectManager.Models;

namespace ResearchProjectManager.Services
{
    public class UserService
    {
        // We inject Identity's UserManager instead of the DbContext!
        // Note: If your app uses the default setup, change <User> to <IdentityUser>
        private readonly UserManager<User> _userManager;

        public UserService(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        // ==========================================
        // Gets a list of all users who have the "Student" role
        // ==========================================
        public async Task<List<User>> GetAllStudentsAsync()
        {
            // UserManager automatically searches the Identity tables for you!
            var students = await _userManager.GetUsersInRoleAsync("Student");

            // It returns an IList, so we just convert it to a normal List for your ViewModel
            return students.ToList();
        }

        // ==========================================
        // Gets a specific user by their ID
        // ==========================================
        public async Task<User?> GetUserByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }
    }
}