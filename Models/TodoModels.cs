using System;
using System.Collections.Generic;

namespace DesktopTodo.Models
{
    public enum TodoState { NotStarted, InProgress, Pending, Holding, Completed }
    public enum RoutineType { Daily, Weekly, Monthly }
    public enum TaskScope { Instance, Global } 

    public abstract class TodoItemBase
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty; 
        public string Memo { get; set; } = string.Empty; 
        public TodoState State { get; set; } = TodoState.NotStarted;
        public TaskScope Scope { get; set; } = TaskScope.Instance; 
        public string ColorTag { get; set; } = "#FFFFFF"; 
        
        public bool IsRolloverEnabled { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? CompletedDate { get; set; }
    }

    public class SingleTask : TodoItemBase
    {
        public DateTime TargetDate { get; set; } // 시작일로 사용됩니다.
        public DateTime? EndDate { get; set; }   // 추가됨: 종료일. 없으면 TargetDate 하루만 진행.
        public string? ParentRoutineId { get; set; } 
    }

    public class ProjectTask : TodoItemBase
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ProgressPercentage { get; set; } = 0;
        public List<ChecklistItem> SubTasks { get; set; } = new List<ChecklistItem>();
    }

    public class ChecklistItem
    {
        public string Title { get; set; } = string.Empty;
        public bool IsDone { get; set; }
    }

    public class RoutineDefinition
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string ColorTag { get; set; } = "#FFFFFF";
        public bool IsRolloverEnabled { get; set; }

        public RoutineType Type { get; set; }
        public List<DayOfWeek> TargetDaysOfWeek { get; set; } = new List<DayOfWeek>();
        public int? MonthlyTargetDate { get; set; }
        public bool IsLastDayOfMonth { get; set; } = false;
    }

    public class AppSettings
    {
        public bool HideFutureCompletedProjects { get; set; } = true; 
        public double WindowOpacity { get; set; } = 0.8;
    }
}