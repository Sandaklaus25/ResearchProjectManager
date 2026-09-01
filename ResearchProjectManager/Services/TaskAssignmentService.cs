using Microsoft.AspNetCore.Identity;
using ResearchProjectManager.Enums;
using ResearchProjectManager.Models;
using ResearchProjectManager.Repositories;
using ResearchProjectManager.Repositories.IRepositories;

namespace ResearchProjectManager.Services
{
    public class TaskAssignmentService
    {
        private readonly ITaskAssignmentRepository _assignmentRepository;
        private readonly IProjectTaskRepository _projectTaskRepository;
        private readonly UserManager<User> _userManager;

        public TaskAssignmentService(
            ITaskAssignmentRepository assignmentRepository,
            IProjectTaskRepository projectTaskRepository,
            UserManager<User> userManager)
        {
            _assignmentRepository = assignmentRepository;
            _projectTaskRepository = projectTaskRepository;
            _userManager = userManager;
        }

        public async Task<List<TaskAssignment>> GetTaskAssignmentsForTeamAsync(int teamId)
        {
            return await _assignmentRepository.GetAssignmentsForTeamAsync(teamId);
        }

        public async Task<List<TaskAssignment>> GetAllAssignmentsAsync()
        {
            return await _assignmentRepository.GetAllAssignmentsWithDetailsAsync();
        }

        public async Task<List<TaskAssignment>> GetTaskAssignmentsByCourseIdAsync(int courseId)
        {
            return await _assignmentRepository.GetAssignmentsByCourseIdWithDetailsAsync(courseId);
        }

        public async Task UpdateAssignmentStatusAsync(int assignmentId, Enums.TaskStatus newStatus)
        {
            var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
            if (assignment != null)
            {
                assignment.TaskStatus = newStatus;
                if (newStatus == Enums.TaskStatus.Completed) assignment.CompletedAt = DateTime.Now;
                _assignmentRepository.Update(assignment);
                await _assignmentRepository.SaveChangesAsync();
            }
        }

        public async Task UpdateDeadLineAsync(int assignmentId, DateTime newDeadline)
        {
            var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
            if (assignment != null)
            {
                assignment.Deadline = newDeadline;
                _assignmentRepository.Update(assignment);
                await _assignmentRepository.SaveChangesAsync();
            }
        }

        private async Task<TaskAssignment> BuildAssignmentAsync(int projectTaskId, int teamId, DateTime deadline, string? assignmentName = null)
        {
            var projectTask = await _projectTaskRepository.GetTaskWithSubtasksAndAssignmentsAsync(projectTaskId);

            var assignment = new TaskAssignment
            {
                Name = !string.IsNullOrWhiteSpace(assignmentName)
                    ? assignmentName
                    : projectTask?.Name ?? "Unnamed Assignment",
                TaskId = projectTaskId,
                TeamId = teamId,
                Deadline = deadline,
                TaskStatus = Enums.TaskStatus.NotStarted,
                CompletedAt = null,
                Subtasks = new List<Subtask>()
            };

            if (projectTask?.BaseSubtasks != null)
            {
                foreach (var baseTask in projectTask.BaseSubtasks)
                {
                    assignment.Subtasks.Add(new Subtask
                    {
                        Name = baseTask.Name,
                        Description = baseTask.Description,
                        Status = Enums.TaskStatus.NotStarted,
                        IsBaseTask = true
                    });
                }
            }

            return assignment;
        }

        public async Task AssignToNewGroupAsync(int projectTaskId, int courseId, List<int> selectedStudentIds, DateTime deadline, string? assignmentName = null)
        {
            var newTeam = new Team
            {
                Name = !string.IsNullOrWhiteSpace(assignmentName) ? assignmentName : $"Group Project - Task #{projectTaskId}",
                CourseId = courseId
            };

            await _assignmentRepository.AddTeamAsync(newTeam);
            await _assignmentRepository.SaveChangesAsync();

            foreach (var studentId in selectedStudentIds)
            {
                await _assignmentRepository.AddUserTeamAsync(new UserTeam
                {
                    UserId = studentId,
                    TeamId = newTeam.Id
                });
            }

            var assignment = await BuildAssignmentAsync(projectTaskId, newTeam.Id, deadline, assignmentName);

            await _assignmentRepository.AddAsync(assignment);
            await _assignmentRepository.SaveChangesAsync();
        }

        public async Task<TaskAssignment> CreateAssignmentAsync(int projectTaskId, int teamId, DateTime deadline, string? assignmentName = null)
        {
            var assignment = await BuildAssignmentAsync(projectTaskId, teamId, deadline, assignmentName);

            await _assignmentRepository.AddAsync(assignment);
            await _assignmentRepository.SaveChangesAsync();

            return assignment;
        }

        public async Task<bool> DeleteAssignmentAsync(int assignmentId)
        {
            var assignment = await _assignmentRepository.GetAssignmentWithSubtasksAndTurnInsAsync(assignmentId);
            if (assignment == null) return false;

            if (assignment.Subtasks != null && assignment.Subtasks.Any())
            {
                _assignmentRepository.RemoveSubtasks(assignment.Subtasks);
            }

            _assignmentRepository.Remove(assignment);
            await _assignmentRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> TurnInAssignmentAsync(int assignmentId, List<(string FileName, string FileWebPath)> files)
        {
            var assignment = await _assignmentRepository.GetAssignmentWithSubtasksAndTurnInsAsync(assignmentId);
            if (assignment == null) return false;

            bool allDone = !assignment.Subtasks.Any() || assignment.Subtasks.All(s => s.Status == Enums.TaskStatus.Completed || s.Status == Enums.TaskStatus.Cancelled);

            if (allDone)
            {
                assignment.TaskStatus = Enums.TaskStatus.Completed;
                assignment.CompletedAt = DateTime.Now;

                if (files != null && files.Any())
                {
                    foreach (var file in files)
                    {
                        assignment.TurnInAttachments.Add(new Attachment { FileName = file.FileName, FilePath = file.FileWebPath });
                    }
                }
                _assignmentRepository.Update(assignment);
                await _assignmentRepository.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> UnsubmitAssignmentAsync(int assignmentId)
        {
            var assignment = await _assignmentRepository.GetAssignmentWithTurnInsAsync(assignmentId);

            if (assignment != null && assignment.TaskStatus == Enums.TaskStatus.Completed)
            {
                assignment.TaskStatus = Enums.TaskStatus.InProgress;
                assignment.CompletedAt = null;

                if (assignment.TurnInAttachments != null && assignment.TurnInAttachments.Any())
                {
                    _assignmentRepository.RemoveAttachments(assignment.TurnInAttachments);
                }

                _assignmentRepository.Update(assignment);
                await _assignmentRepository.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task AddCommentAsync(int assignmentId, int userId, string commentText, List<(string FileName, string FileWebPath)> files)
        {
            var comment = new Comment
            {
                TaskAssignmentId = assignmentId,
                UserId = userId,
                CommentText = commentText ?? "",
                CreatedAt = DateTime.Now,
                Attachments = new List<Attachment>()
            };

            if (files != null && files.Any())
            {
                foreach (var file in files)
                {
                    comment.Attachments.Add(new Attachment
                    {
                        FileName = file.FileName,
                        FilePath = file.FileWebPath
                    });
                }
            }

            await _assignmentRepository.AddCommentAsync(comment);
            await _assignmentRepository.SaveChangesAsync();
        }

        public async Task<List<TaskAssignment>> GetMyAssignmentsAsync(int userId)
        {
            return await _assignmentRepository.GetAssignmentsForStudentAsync(userId);
        }

        public async Task<List<TaskAssignment>> GetMyCourseAssignmentsAsync(int userId, int courseId)
        {
            return await _assignmentRepository.GetCourseAssignmentsForStudentAsync(userId, courseId);
        }

        public async Task AddSubtaskToAssignmentAsync(int assignmentId, int userId, string name, string? description)
        {
            var assignment = await _assignmentRepository.GetAssignmentWithSubtasksAndTurnInsAsync(assignmentId);
            if (assignment == null) return;

            assignment.Subtasks.Add(new Subtask
            {
                Name = name,
                Description = description,
                Status = Enums.TaskStatus.NotStarted,
                IsBaseTask = false,
                CreatorId = userId
            });
            _assignmentRepository.Update(assignment);
            await _assignmentRepository.SaveChangesAsync();
        }

        public async Task UpdateSubtaskStatusAsync(int subtaskId, int userId, Enums.TaskStatus newStatus)
        {
            var subtask = await _assignmentRepository.GetSubtaskWithAssignmentAsync(subtaskId);

            if (subtask != null)
            {
                subtask.Status = newStatus;
                subtask.LastStatusUpdaterId = userId;

                if (subtask.TaskAssignment != null && subtask.TaskAssignment.TaskStatus == Enums.TaskStatus.NotStarted)
                {
                    if (newStatus == Enums.TaskStatus.InProgress || newStatus == Enums.TaskStatus.Completed)
                    {
                        subtask.TaskAssignment.TaskStatus = Enums.TaskStatus.InProgress;
                    }
                }
                await _assignmentRepository.SaveChangesAsync();
            }
        }

        public async Task<bool> DeleteSubtaskAsync(int subtaskId)
        {
            var subtask = await _assignmentRepository.GetSubtaskByIdAsync(subtaskId);
            if (subtask != null && !subtask.IsBaseTask)
            {
                await _assignmentRepository.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<TaskAssignment?> GetTaskAssignmentForSubtaskBoardAsync(int assignmentId)
        {
            return await _assignmentRepository.GetAssignmentForSubtaskBoardAsync(assignmentId);
        }

        public async Task<bool> GradeAssignmentAsync(int assignmentId, int grade)
        {
            if (grade < 0 || grade > 100) return false;

            var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);

            if (assignment != null)
            {
                assignment.Grade = grade;
                _assignmentRepository.Update(assignment);
                await _assignmentRepository.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> RemoveGradeAsync(int assignmentId)
        {
            var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
            if (assignment != null)
            {
                assignment.Grade = null;
                _assignmentRepository.Update(assignment);
                await _assignmentRepository.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> IsStudentInAssignmentFreePassStaffTeamAsync(int assignmentId, int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null && !await _userManager.IsInRoleAsync(user, "Student"))
            {
                return true;
            }
            return await _assignmentRepository.IsStudentInTeamAsync(assignmentId, userId);
        }

        public async Task<bool> TurnInAssignmentWithFilesAsync(int assignmentId, List<IFormFile>? turnInFiles)
        {
            var assignment = await _assignmentRepository.GetAssignmentWithSubtasksAndTurnInsAsync(assignmentId);
            if (assignment == null) return false;

            bool allDone = !assignment.Subtasks.Any() || assignment.Subtasks.All(s => s.Status == Enums.TaskStatus.Completed || s.Status == Enums.TaskStatus.Cancelled);
            if (!allDone) return false;

            var uploadedFiles = await ProcessUploadedFilesAsync(turnInFiles);

            assignment.TaskStatus = Enums.TaskStatus.Completed;
            assignment.CompletedAt = DateTime.Now;

            foreach (var file in uploadedFiles)
            {
                assignment.TurnInAttachments.Add(new Attachment { FileName = file.FileName, FilePath = file.FileWebPath });
            }

            _assignmentRepository.Update(assignment);
            await _assignmentRepository.SaveChangesAsync();
            return true;
        }

        public async Task AddCommentWithFilesAsync(int assignmentId, int userId, string commentText, List<IFormFile>? attachmentFiles)
        {
            var uploadedFiles = await ProcessUploadedFilesAsync(attachmentFiles);

            var comment = new Comment
            {
                TaskAssignmentId = assignmentId,
                UserId = userId,
                CommentText = commentText ?? "",
                CreatedAt = DateTime.Now,
                Attachments = uploadedFiles.Select(f => new Attachment { FileName = f.FileName, FilePath = f.FileWebPath }).ToList()
            };

            await _assignmentRepository.AddCommentAsync(comment);
            await _assignmentRepository.SaveChangesAsync();
        }

        private async Task<List<(string FileName, string FileWebPath)>> ProcessUploadedFilesAsync(List<IFormFile>? files)
        {
            var uploadedFiles = new List<(string FileName, string FileWebPath)>();
            if (files == null || !files.Any()) return uploadedFiles;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            foreach (var file in files.Take(3))
            {
                if (file.Length > 0)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                    var physicalPath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(physicalPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    uploadedFiles.Add((file.FileName, "/uploads/" + uniqueFileName));
                }
            }
            return uploadedFiles;
        }
    }
}