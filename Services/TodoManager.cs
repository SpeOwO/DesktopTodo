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
        // 데이터가 저장될 파일 경로 (내 문서 폴더 활용)
        private readonly string dataFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DesktopTodoData.json");

        // 메모리에서 관리할 데이터 리스트
        public List<SingleTask> Tasks { get; private set; } = new List<SingleTask>();
        public List<ProjectTask> Projects { get; private set; } = new List<ProjectTask>();
        public List<RoutineDefinition> Routines { get; private set; } = new List<RoutineDefinition>();
        public AppSettings Settings { get; private set; } = new AppSettings();

        public TodoManager()
        {
            LoadData();
        }

        // 1. 데이터 저장 및 불러오기 (JSON)
        public void SaveData()
        {
            var appData = new
            {
                Tasks,
                Projects,
                Routines,
                Settings
            };

            string jsonString = JsonSerializer.Serialize(appData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(dataFilePath, jsonString);
        }

        private void LoadData()
        {
            if (File.Exists(dataFilePath))
            {
                string jsonString = File.ReadAllText(dataFilePath);
                var element = JsonSerializer.Deserialize<JsonElement>(jsonString);
                
                // 경고 방지 및 안전한 데이터 파싱을 위해 TryGetProperty 사용
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

        // 2. 특정 날짜의 업무 가져오기
        public List<TodoItemBase> GetTodosForDate(DateTime date)
        {
            var targetDate = date.Date;
            var result = new List<TodoItemBase>();

            // A. 루틴 확인 및 자동 생성
            GenerateRoutineTasksForDate(targetDate);

            // B. 해당 날짜의 단발성 업무 추가
            result.AddRange(Tasks.Where(t => t.TargetDate.Date == targetDate));

            // C. 이월(Rollover)된 과거 업무 가져오기
            var rolloverTasks = Tasks.Where(t => 
                t.TargetDate.Date < targetDate && 
                t.IsRolloverEnabled && 
                (t.State != TodoState.Completed)).ToList();
            
            result.AddRange(rolloverTasks);

            // D. 해당 날짜가 포함된 공유형 업무(Project) 추가
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
                    continue; // 숨김 처리
                }
                result.Add(project);
            }

            return result;
        }

        // 3. 루틴 생성기
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