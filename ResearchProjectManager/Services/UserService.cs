using Microsoft.AspNetCore.Identity;
using ResearchProjectManager.Models;

namespace ResearchProjectManager.Services
{
    public class UserService
    {
        private readonly UserManager<User> _userManager;

        public UserService(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<List<User>> GetAllStudentsAsync()
        {
            var students = await _userManager.GetUsersInRoleAsync("Student");

            return students.ToList();
        }

        public async Task<User?> GetUserByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }
    }
}