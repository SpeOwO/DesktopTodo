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
                
                // 실제 개발 시에는 Json 타입 변환을 더 엄격하게 처리합니다.
                // 여기서는 구조를 보여주기 위해 간략화했습니다.
                Tasks = JsonSerializer.Deserialize<List<SingleTask>>(element.GetProperty("Tasks").GetRawText()) ?? new List<SingleTask>();
                Projects = JsonSerializer.Deserialize<List<ProjectTask>>(element.GetProperty("Projects").GetRawText()) ?? new List<ProjectTask>();
                Routines = JsonSerializer.Deserialize<List<RoutineDefinition>>(element.GetProperty("Routines").GetRawText()) ?? new List<RoutineDefinition>();
                Settings = JsonSerializer.Deserialize<AppSettings>(element.GetProperty("Settings").GetRawText()) ?? new AppSettings();
            }
        }

        // 2. 특정 날짜의 업무 가져오기 (핵심 로직)
        public List<TodoItemBase> GetTodosForDate(DateTime date)
        {
            var targetDate = date.Date;
            var result = new List<TodoItemBase>();

            // A. 루틴 확인 및 자동 생성 (해당 날짜에 루틴 업무가 아직 안 만들어졌다면 생성)
            GenerateRoutineTasksForDate(targetDate);

            // B. 해당 날짜의 단발성 업무(Task) 및 생성된 루틴 업무 추가
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
                // Q2 옵션: 미래 날짜 숨김 처리 로직
                if (Settings.HideFutureCompletedProjects && 
                    project.State == TodoState.Completed && 
                    project.CompletedDate.HasValue && 
                    targetDate > project.CompletedDate.Value.Date)
                {
                    continue; // 완료일 다음날부터는 캘린더에 포함시키지 않음 (숨김)
                }
                result.Add(project);
            }

            return result;
        }

        // 3. 루틴 생성기 (공장 역할)
        private void GenerateRoutineTasksForDate(DateTime date)
        {
            foreach (var routine in Routines)
            {
                // 이미 이 루틴으로 생성된 오늘자 업무가 있는지 확인
                bool alreadyExists = Tasks.Any(t => t.ParentRoutineId == routine.Id && t.TargetDate.Date == date.Date);
                if (alreadyExists) continue;

                bool shouldGenerate = false;

                // 루틴 조건 검사
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

                // 조건에 맞으면 새로운 SingleTask 인스턴스 생성
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