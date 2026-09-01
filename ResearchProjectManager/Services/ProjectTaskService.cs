using ResearchProjectManager.Models;
using ResearchProjectManager.Repositories;
using ResearchProjectManager.Repositories.IRepositories;
using ResearchProjectManager.ViewModels;

namespace ResearchProjectManager.Services
{
    public class ProjectTaskService
    {
        private readonly IProjectTaskRepository _projectTaskRepository;

        public ProjectTaskService(IProjectTaskRepository projectTaskRepository)
        {
            _projectTaskRepository = projectTaskRepository;
        }

        public async Task<IEnumerable<ProjectTask>> GetAllTasksAsync()
        {
            return await _projectTaskRepository.GetAllAsync();
        }

        public async Task<ProjectTask?> GetTaskByIdAsync(int id)
        {
            return await _projectTaskRepository.GetTaskWithDetailsByIdAsync(id);
        }

        public async Task<List<ProjectTask>> GetTasksByCourseIdAsync(int courseId)
        {
            return await _projectTaskRepository.GetTasksByCourseIdWithAuthorAsync(courseId);
        }

        public async Task<ProjectTask> CreateTaskAsync(ProjectTask task)
        {
            await _projectTaskRepository.AddAsync(task);
            await _projectTaskRepository.SaveChangesAsync();
            return task;
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _projectTaskRepository.GetTaskWithSubtasksAndAssignmentsAsync(id);
            if (task == null) return false;

            if (task.BaseSubtasks != null && task.BaseSubtasks.Any())
            {
                _projectTaskRepository.RemoveSubtasks(task.BaseSubtasks);
            }

            _projectTaskRepository.Remove(task);
            await _projectTaskRepository.SaveChangesAsync();
            return true;
        }

        public async Task<ProjectTask> CreateTaskFromViewModelAsync(ProjectTaskCreateViewModel model, int courseId, int userId)
        {
            var newTask = new ProjectTask
            {
                Name = model.Name,
                Description = model.Description,
                CourseId = courseId,
                AuthorId = userId,
                CreatedAt = DateTime.Now,
                BaseSubtasks = new List<Subtask>()
            };

            foreach (var subtaskDto in model.Subtasks)
            {
                if (!string.IsNullOrWhiteSpace(subtaskDto.Name))
                {
                    newTask.BaseSubtasks.Add(new Subtask
                    {
                        Name = subtaskDto.Name,
                        Description = subtaskDto.Description,
                        Status = Enums.TaskStatus.NotStarted
                    });
                }
            }

            await _projectTaskRepository.AddAsync(newTask);
            await _projectTaskRepository.SaveChangesAsync();
            return newTask;
        }
    }
}