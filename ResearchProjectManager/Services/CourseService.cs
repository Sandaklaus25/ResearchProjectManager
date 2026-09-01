using ResearchProjectManager.Data;
using ResearchProjectManager.Models;
using Microsoft.EntityFrameworkCore;

namespace ResearchProjectManager.Services
{
    public class CourseService
    {
        private readonly ApplicationDbContext _context;

        public CourseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Course>> GetAllCoursesAsync()
        {
            return await _context.Courses.Include(c=>c.Owner).Include(c=>c.Members).ToListAsync();
        }

        public async Task<Course?> GetCourseByIdAsync(int id)
        {
            return await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Course> CreateCourseAsync(Course course)
        {
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            return course;
        }

        public async Task<Course?> UpdateCourseAsync(int courseId, Course updatedCourse)
        {
            var existingCourse = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
            if (existingCourse == null) return null;

            existingCourse.Name = updatedCourse.Name;
            existingCourse.IsPrivate = updatedCourse.IsPrivate;
            existingCourse.Color = updatedCourse.Color;
            await _context.SaveChangesAsync();
            return existingCourse;
        }

        public async Task<bool> DeleteCourseAsync(int id)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return false;
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
