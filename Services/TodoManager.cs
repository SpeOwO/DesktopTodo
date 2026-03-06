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

            // 1. 이미 구체화된(저장된) 일반 업무들 불러오기
            var concreteTasks = Tasks.Where(t => 
                (t.EndDate.HasValue 
                    ? (t.TargetDate.Date <= targetDate && t.EndDate.Value.Date >= targetDate)
                    : t.TargetDate.Date == targetDate)
            ).ToList();
            
            result.AddRange(concreteTasks);

            // 2. [핵심] 루틴 규칙을 읽어서 메모리상에 '가상 업무'로 띄워주기 (DB 저장 안 함!)
            foreach (var routine in Routines)
            {
                // 가상 업무의 고유 ID (루틴ID + 날짜 조합)
                string transientId = $"{routine.Id}_{targetDate:yyyyMMdd}";
                
                // 만약 사용자가 이 날짜의 루틴을 이미 '완료'하거나 메모를 적어서 구체화시켰다면, 가상으로 또 띄우지 않음
                if (concreteTasks.Any(t => t.Id == transientId))
                    continue;

                bool shouldGenerate = false;
                switch (routine.Type)
                {
                    case RoutineType.Daily:
                        shouldGenerate = true;
                        break;
                    case RoutineType.Weekly:
                        shouldGenerate = routine.TargetDaysOfWeek.Contains(targetDate.DayOfWeek);
                        break;
                    case RoutineType.Monthly:
                        if (routine.IsLastDayOfMonth)
                        {
                            int daysInMonth = DateTime.DaysInMonth(targetDate.Year, targetDate.Month);
                            shouldGenerate = (targetDate.Day == daysInMonth);
                        }
                        else if (routine.MonthlyTargetDate.HasValue)
                        {
                            shouldGenerate = (targetDate.Day == routine.MonthlyTargetDate.Value);
                        }
                        break;
                }

                if (shouldGenerate)
                {
                    // 화면에 보여주기 위한 가짜(Virtual) 객체 생성
                    result.Add(new SingleTask
                    {
                        Id = transientId, // 이 특별한 ID로 나중에 가상 객체임을 식별함
                        Title = routine.Title,
                        ColorTag = routine.ColorTag,
                        TargetDate = targetDate,
                        EndDate = targetDate,
                        State = TodoState.NotStarted,
                        IsRolloverEnabled = routine.IsRolloverEnabled,
                        ParentRoutineId = routine.Id,
                        Scope = TaskScope.Instance
                    });
                }
            }

            // 3. 이월(Rollover) 처리
            var rolloverTasks = Tasks.Where(t => 
                t.IsRolloverEnabled && 
                t.State != TodoState.Completed &&
                (t.EndDate.HasValue ? t.EndDate.Value.Date < targetDate : t.TargetDate.Date < targetDate)
            ).ToList();
            
            result.AddRange(rolloverTasks);

            // 4. 프로젝트 처리
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
    }
}