using ResearchProjectManager.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResearchProjectManager.Repositories.IRepositories
{
    public interface IProjectTaskRepository : IRepository<ProjectTask>
    {
        Task<ProjectTask?> GetTaskWithDetailsByIdAsync(int id);
        Task<List<ProjectTask>> GetTasksByCourseIdWithAuthorAsync(int courseId);
        Task<ProjectTask?> GetTaskWithSubtasksAndAssignmentsAsync(int id);
        void RemoveSubtasks(List<Subtask> subtasks);
    }
}