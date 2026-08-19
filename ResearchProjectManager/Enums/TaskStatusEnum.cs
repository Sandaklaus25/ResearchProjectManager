namespace ResearchProjectManager.Enums
{
    using System;

    public enum TaskStatusEnum
    {
        NotStarted = 1,
        InProgress = 2,
        Completed = 3,
        OnHold = 4,
        Cancelled = 5
    }

    public static class TaskStatusEnumExtensions
    {
        public static string GetTaskStatusName(this TaskStatusEnum status)
        {
            return status switch
            {
                TaskStatusEnum.NotStarted => "Not Started",
                TaskStatusEnum.InProgress => "In Progress",
                TaskStatusEnum.Completed => "Completed",
                TaskStatusEnum.OnHold => "On Hold",
                TaskStatusEnum.Cancelled => "Cancelled",
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Status is not found!")
            };
        }

        public static bool IsCompleted(this TaskStatusEnum status)
        {
            return status == TaskStatusEnum.Completed;
        }

        public static bool IsInProgress(this TaskStatusEnum status)
        {
            return status == TaskStatusEnum.InProgress;
        }

        public static bool IsNotStarted(this TaskStatusEnum status)
        {
            return status == TaskStatusEnum.NotStarted;
        }

        public static bool IsOnHold(this TaskStatusEnum status)
        {
            return status == TaskStatusEnum.OnHold;
        }

        public static bool IsCancelled(this TaskStatusEnum status)
        {
            return status == TaskStatusEnum.Cancelled;
        }
    }
}

