using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using DesktopTodo.Models;

namespace DesktopTodo.Services
{
    public class TodoManager
    {
        private readonly string dataFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DesktopTodoData.json");

        public List<SingleTask> Tasks { get; private set; } = new List<SingleTask>();
        public List<ProjectTask> Projects { get; private set; } = new List<ProjectTask>();
        public List<RoutineDefinition> Routines { get; private set; } = new List<RoutineDefinition>();
        public AppSettings Settings { get; private set; } = new AppSettings();

        public TodoManager()
        {
            LoadData();
        }

        public void SaveData()
        {
            var appData = new { Tasks, Projects, Routines, Settings };
            string jsonString = JsonSerializer.Serialize(appData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(dataFilePath, jsonString);
        }

        private void LoadData()
        {
            if (File.Exists(dataFilePath))
            {
                string jsonString = File.ReadAllText(dataFilePath);
                var element = JsonSerializer.Deserialize<JsonElement>(jsonString);
                
                if (element.TryGetProperty("Tasks", out var tasksProp))
                    Tasks = JsonSerializer.Deserialize<List<SingleTask>>(tasksProp.GetRawText()) ?? new List<SingleTask>();
                    
                if (element.TryGetProperty("Projects", out var projectsProp))
                    Projects = JsonSerializer.Deserialize<List<ProjectTask>>(projectsProp.GetRawText()) ?? new List<ProjectTask>();
                    
                if (element.TryGetProperty("Routines", out var routinesProp))
                    Routines = JsonSerializer.Deserialize<List<RoutineDefinition>>(routinesProp.GetRawText()) ?? new List<RoutineDefinition>();
                    
                if (element.TryGetProperty("Settings", out var settingsProp))
                    Settings = JsonSerializer.Deserialize<AppSettings>(settingsProp.GetRawText()) ?? new AppSettings();
            }
        }

        public List<TodoItemBase> GetTodosForDate(DateTime date)
        {
            var targetDate = date.Date;
            var result = new List<TodoItemBase>();

            GenerateRoutineTasksForDate(targetDate);

            // [수정됨] 종료일(EndDate)이 설정된 경우 시작일~종료일 사이에 모두 표시되도록 변경
            result.AddRange(Tasks.Where(t => 
                (t.EndDate.HasValue 
                    ? (t.TargetDate.Date <= targetDate && t.EndDate.Value.Date >= targetDate)
                    : t.TargetDate.Date == targetDate)
            ));

            // 이월(Rollover) 처리: 종료일이 지났는데 완료되지 않은 항목
            var rolloverTasks = Tasks.Where(t => 
                t.IsRolloverEnabled && 
                t.State != TodoState.Completed &&
                (t.EndDate.HasValue ? t.EndDate.Value.Date < targetDate : t.TargetDate.Date < targetDate)
            ).ToList();
            
            result.AddRange(rolloverTasks);

            var activeProjects = Projects.Where(p => 
                p.StartDate.Date <= targetDate && 
                p.EndDate.Date >= targetDate).ToList();

            foreach (var project in activeProjects)
            {
                if (Settings.HideFutureCompletedProjects && 
                    project.State == TodoState.Completed && 
                    project.CompletedDate.HasValue && 
                    targetDate > project.CompletedDate.Value.Date)
                {
                    continue; 
                }
                result.Add(project);
            }

            return result;
        }

        private void GenerateRoutineTasksForDate(DateTime date)
        {
            foreach (var routine in Routines)
            {
                bool alreadyExists = Tasks.Any(t => t.ParentRoutineId == routine.Id && t.TargetDate.Date == date.Date);
                if (alreadyExists) continue;

                bool shouldGenerate = false;

                switch (routine.Type)
                {
                    case RoutineType.Daily:
                        shouldGenerate = true;
                        break;
                    case RoutineType.Weekly:
                        shouldGenerate = routine.TargetDaysOfWeek.Contains(date.DayOfWeek);
                        break;
                    case RoutineType.Monthly:
                        if (routine.IsLastDayOfMonth)
                        {
                            int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
                            shouldGenerate = (date.Day == daysInMonth);
                        }
                        else if (routine.MonthlyTargetDate.HasValue)
                        {
                            shouldGenerate = (date.Day == routine.MonthlyTargetDate.Value);
                        }
                        break;
                }

                if (shouldGenerate)
                {
                    var newTask = new SingleTask
                    {
                        Title = routine.Title,
                        ColorTag = routine.ColorTag,
                        IsRolloverEnabled = routine.IsRolloverEnabled,
                        TargetDate = date.Date,
                        ParentRoutineId = routine.Id,
                        State = TodoState.NotStarted
                    };
                    Tasks.Add(newTask);
                }
            }
        }
    }
}