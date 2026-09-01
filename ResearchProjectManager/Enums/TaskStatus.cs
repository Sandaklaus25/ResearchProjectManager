namespace ResearchProjectManager.Enums
{
    using System;

    public enum TaskStatus
    {
        NotStarted = 1,
        InProgress = 2,
        Completed = 3,
        OnHold = 4,
        Cancelled = 5
    }

    public static class TaskStatusEnumExtensions
    {
        public static string GetTaskStatusName(this TaskStatus status)
        {
            return status switch
            {
                TaskStatus.NotStarted => "Not Started",
                TaskStatus.InProgress => "In Progress",
                TaskStatus.Completed => "Completed",
                TaskStatus.OnHold => "On Hold",
                TaskStatus.Cancelled => "Cancelled",
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Status is not found!")
            };
        }

        public static bool IsCompleted(this TaskStatus status)
        {
            return status == TaskStatus.Completed;
        }

        public static bool IsInProgress(this TaskStatus status)
        {
            return status == TaskStatus.InProgress;
        }

        public static bool IsNotStarted(this TaskStatus status)
        {
            return status == TaskStatus.NotStarted;
        }

        public static bool IsOnHold(this TaskStatus status)
        {
            return status == TaskStatus.OnHold;
        }

        public static bool IsCancelled(this TaskStatus status)
        {
            return status == TaskStatus.Cancelled;
        }
    }
}

