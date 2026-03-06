using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using DesktopTodo.Models;
using DesktopTodo.Services;

namespace DesktopTodo.ViewModels
{
    public class CalendarDay
    {
        public DateTime Date { get; set; }
        public string DayNumber => Date.Day.ToString();
        public string DayColor => !IsCurrentMonth ? "#888888" : (Date.DayOfWeek == DayOfWeek.Sunday ? "#FF6B6B" : (Date.DayOfWeek == DayOfWeek.Saturday ? "#4DABF7" : "White"));
        public bool IsCurrentMonth { get; set; }
        public ObservableCollection<TodoItemBase> Todos { get; set; } = new ObservableCollection<TodoItemBase>();
    }

    public enum CalendarViewMode { Monthly, Weekly }
    public enum EditTaskType { Single, Project, Routine }

    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly TodoManager _todoManager;
        private DateTime _currentDate;
        private CalendarViewMode _viewMode;
        private bool _isSizeUnlocked = false;

        private string _newTodoTitle = string.Empty;
        private string _newTodoMemo = string.Empty;
        private string _selectedColorTag = "#FFFFFF";
        private TodoState _newTodoState = TodoState.NotStarted;
        private TaskScope _newTodoScope = TaskScope.Instance; 
        private string? _editingTodoId = null;
        private DateTime _newTodoDate;
        private DateTime _newTodoEndDate;

        private EditTaskType _currentEditTaskType = EditTaskType.Single;
        private bool _isRolloverEnabled = false;
        private RoutineType _selectedRoutineType = RoutineType.Daily;
        private string _newSubTaskTitle = string.Empty;

        public EditTaskType CurrentEditTaskType
        {
            get => _currentEditTaskType;
            set { _currentEditTaskType = value; OnPropertyChanged(); }
        }

        public bool IsRolloverEnabled
        {
            get => _isRolloverEnabled;
            set { _isRolloverEnabled = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ChecklistItem> SubTasks { get; set; } = new ObservableCollection<ChecklistItem>();
        public string NewSubTaskTitle
        {
            get => _newSubTaskTitle;
            set { _newSubTaskTitle = value; OnPropertyChanged(); }
        }

        public ObservableCollection<RoutineType> AvailableRoutineTypes { get; } = new ObservableCollection<RoutineType> { RoutineType.Daily, RoutineType.Weekly, RoutineType.Monthly };
        public RoutineType SelectedRoutineType
        {
            get => _selectedRoutineType;
            set { _selectedRoutineType = value; OnPropertyChanged(); }
        }

        public DateTime NewTodoDate { get => _newTodoDate; set { _newTodoDate = value; OnPropertyChanged(); } }
        public DateTime NewTodoEndDate { get => _newTodoEndDate; set { _newTodoEndDate = value; OnPropertyChanged(); } }
        public string PopupTitle => string.IsNullOrEmpty(_editingTodoId) ? "새 업무 추가" : "업무 상세/수정";
        public bool IsEditing => !string.IsNullOrEmpty(_editingTodoId);

        public ObservableCollection<string> AvailableColors { get; set; } = new ObservableCollection<string> 
        { "#FF6B6B", "#4DABF7", "#51CF66", "#FCC419", "#FFFFFF" };
        public ObservableCollection<TodoState> AvailableStates { get; } = new ObservableCollection<TodoState>
        { TodoState.NotStarted, TodoState.InProgress, TodoState.Pending, TodoState.Holding, TodoState.Completed };
        public ObservableCollection<TaskScope> AvailableScopes { get; } = new ObservableCollection<TaskScope>
        { TaskScope.Instance, TaskScope.Global };

        public string NewTodoTitle { get => _newTodoTitle; set { _newTodoTitle = value; OnPropertyChanged(); } }
        public string NewTodoMemo { get => _newTodoMemo; set { _newTodoMemo = value; OnPropertyChanged(); } }
        public string SelectedColorTag { get => _selectedColorTag; set { _selectedColorTag = value; OnPropertyChanged(); } }
        public TodoState NewTodoState { get => _newTodoState; set { _newTodoState = value; OnPropertyChanged(); } }
        public TaskScope NewTodoScope { get => _newTodoScope; set { _newTodoScope = value; OnPropertyChanged(); } }

        public ObservableCollection<CalendarDay> CalendarDays { get; set; }
        public bool IsSizeUnlocked { get => _isSizeUnlocked; set { _isSizeUnlocked = value; OnPropertyChanged(); } }
        public CalendarViewMode ViewMode { get => _viewMode; set { _viewMode = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayDateText)); GenerateCalendar(); } }
        public DateTime CurrentDate { get => _currentDate; set { _currentDate = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayDateText)); GenerateCalendar(); } }

        public string DisplayDateText
        {
            get
            {
                if (ViewMode == CalendarViewMode.Monthly) return CurrentDate.ToString("yyyy년 MM월");
                else
                {
                    Calendar cal = CultureInfo.CurrentCulture.Calendar;
                    DateTime firstDayOfMonth = new DateTime(CurrentDate.Year, CurrentDate.Month, 1);
                    int currentWeek = cal.GetWeekOfYear(CurrentDate, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
                    int firstWeek = cal.GetWeekOfYear(firstDayOfMonth, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
                    return $"{CurrentDate.ToString("yyyy년 MM월")} {currentWeek - firstWeek + 1}주차";
                }
            }
        }

        public MainViewModel()
        {
            _todoManager = new TodoManager();
            CalendarDays = new ObservableCollection<CalendarDay>();
            _currentDate = DateTime.Today;
            _viewMode = CalendarViewMode.Monthly;
            GenerateCalendar();
        }

        public void AddSubTask()
        {
            if (!string.IsNullOrWhiteSpace(NewSubTaskTitle))
            {
                SubTasks.Add(new ChecklistItem { Title = NewSubTaskTitle, IsDone = false });
                NewSubTaskTitle = ""; 
            }
        }

        public void OpenAddPopup(DateTime? date = null)
        {
            _editingTodoId = null;
            DateTime targetDate = date ?? DateTime.Today;
            NewTodoDate = targetDate;
            NewTodoEndDate = targetDate;
            NewTodoTitle = "";
            NewTodoMemo = "";
            SelectedColorTag = "#FFFFFF";
            NewTodoState = TodoState.NotStarted;
            NewTodoScope = TaskScope.Instance; 
            IsRolloverEnabled = false;
            
            CurrentEditTaskType = EditTaskType.Single;
            SubTasks.Clear();

            OnPropertyChanged(nameof(PopupTitle));
            OnPropertyChanged(nameof(IsEditing));
        }

        public void OpenEditPopup(TodoItemBase todo)
        {
            _editingTodoId = todo.Id;
            NewTodoTitle = todo.Title;
            NewTodoMemo = todo.Memo;
            SelectedColorTag = todo.ColorTag;
            NewTodoState = todo.State;
            NewTodoScope = todo.Scope; 
            IsRolloverEnabled = todo.IsRolloverEnabled;
            
            SubTasks.Clear();

            if (todo is SingleTask st) 
            {
                CurrentEditTaskType = EditTaskType.Single;
                NewTodoDate = st.TargetDate;
                NewTodoEndDate = st.EndDate ?? st.TargetDate;
            }
            else if (todo is ProjectTask pt)
            {
                CurrentEditTaskType = EditTaskType.Project;
                NewTodoDate = pt.StartDate;
                NewTodoEndDate = pt.EndDate;
                foreach (var sub in pt.SubTasks) SubTasks.Add(sub);
            }
            
            OnPropertyChanged(nameof(PopupTitle));
            OnPropertyChanged(nameof(IsEditing));
        }

        public void SaveTodo()
        {
            if (string.IsNullOrWhiteSpace(NewTodoTitle)) return;
            if (NewTodoEndDate.Date < NewTodoDate.Date) NewTodoEndDate = NewTodoDate;

            if (string.IsNullOrEmpty(_editingTodoId)) // [신규 추가]
            {
                if (CurrentEditTaskType == EditTaskType.Single)
                {
                    _todoManager.Tasks.Add(new SingleTask { Title = NewTodoTitle, Memo = NewTodoMemo, TargetDate = NewTodoDate, EndDate = NewTodoEndDate, ColorTag = SelectedColorTag, State = NewTodoState, Scope = NewTodoScope, IsRolloverEnabled = IsRolloverEnabled });
                }
                else if (CurrentEditTaskType == EditTaskType.Project)
                {
                    _todoManager.Projects.Add(new ProjectTask { Title = NewTodoTitle, Memo = NewTodoMemo, StartDate = NewTodoDate, EndDate = NewTodoEndDate, ColorTag = SelectedColorTag, State = NewTodoState, Scope = NewTodoScope, IsRolloverEnabled = IsRolloverEnabled, SubTasks = SubTasks.ToList() });
                }
                else if (CurrentEditTaskType == EditTaskType.Routine)
                {
                    var newRoutine = new RoutineDefinition 
                    { 
                        Title = NewTodoTitle, ColorTag = SelectedColorTag, Type = SelectedRoutineType, IsRolloverEnabled = IsRolloverEnabled 
                    };
                    if (SelectedRoutineType == RoutineType.Weekly) newRoutine.TargetDaysOfWeek.Add(NewTodoDate.DayOfWeek);
                    else if (SelectedRoutineType == RoutineType.Monthly) newRoutine.MonthlyTargetDate = NewTodoDate.Day;

                    _todoManager.Routines.Add(newRoutine);
                }
            }
            else // [수정 모드]
            {
                var taskToUpdate = _todoManager.Tasks.FirstOrDefault(t => t.Id == _editingTodoId);
                
                if (taskToUpdate is SingleTask st)
                {
                    // 일반 업무 수정
                    st.Title = NewTodoTitle; st.Memo = NewTodoMemo; st.TargetDate = NewTodoDate; st.EndDate = NewTodoEndDate; st.ColorTag = SelectedColorTag; st.State = NewTodoState; st.Scope = NewTodoScope; st.IsRolloverEnabled = IsRolloverEnabled;
                }
                else if (_editingTodoId.Contains("_"))
                {
                    // [핵심] 사용자가 가상 루틴을 클릭해서 '완료'하거나 메모를 쓴 경우 -> 실제 DB 데이터로 구체화(Materialize)하여 저장!
                    string parentRoutineId = _editingTodoId.Split('_')[0];
                    var materializedTask = new SingleTask
                    {
                        Id = _editingTodoId, // ID를 유지하여 다음번 달력을 그릴 때 가상 루틴이 아닌 이 객체를 띄우도록 덮어씀
                        Title = NewTodoTitle,
                        Memo = NewTodoMemo,
                        TargetDate = NewTodoDate,
                        EndDate = NewTodoEndDate,
                        ColorTag = SelectedColorTag,
                        State = NewTodoState,
                        Scope = NewTodoScope,
                        IsRolloverEnabled = IsRolloverEnabled,
                        ParentRoutineId = parentRoutineId
                    };
                    _todoManager.Tasks.Add(materializedTask);
                }
                else
                {
                    // 프로젝트 수정
                    var projToUpdate = _todoManager.Projects.FirstOrDefault(p => p.Id == _editingTodoId);
                    if (projToUpdate != null)
                    {
                        projToUpdate.Title = NewTodoTitle; projToUpdate.Memo = NewTodoMemo; projToUpdate.StartDate = NewTodoDate; projToUpdate.EndDate = NewTodoEndDate; projToUpdate.ColorTag = SelectedColorTag; projToUpdate.State = NewTodoState; projToUpdate.Scope = NewTodoScope; projToUpdate.IsRolloverEnabled = IsRolloverEnabled; projToUpdate.SubTasks = SubTasks.ToList();
                    }
                }
            }

            _todoManager.SaveData();
            GenerateCalendar();
        }

        public void DeleteTodo()
        {
            if (!string.IsNullOrEmpty(_editingTodoId))
            {
                var singleTask = _todoManager.Tasks.FirstOrDefault(t => t.Id == _editingTodoId);
                if (singleTask != null) 
                {
                    _todoManager.Tasks.Remove(singleTask);
                }
                else if (_editingTodoId.Contains("_"))
                {
                    // 가상 루틴 태스크 창을 열고 [삭제]를 누른 경우 -> 루틴 원본(규칙) 자체를 파괴!
                    string routineId = _editingTodoId.Split('_')[0];
                    var routineDef = _todoManager.Routines.FirstOrDefault(r => r.Id == routineId);
                    if (routineDef != null)
                    {
                        _todoManager.Routines.Remove(routineDef);
                    }
                }
                else
                {
                    var projTask = _todoManager.Projects.FirstOrDefault(p => p.Id == _editingTodoId);
                    if (projTask != null) _todoManager.Projects.Remove(projTask);
                }
                _todoManager.SaveData();
            }
            GenerateCalendar();
        }

        public void GoToPrevious() => CurrentDate = ViewMode == CalendarViewMode.Monthly ? CurrentDate.AddMonths(-1) : CurrentDate.AddDays(-7);
        public void GoToNext() => CurrentDate = ViewMode == CalendarViewMode.Monthly ? CurrentDate.AddMonths(1) : CurrentDate.AddDays(7);

        public void GenerateCalendar()
        {
            CalendarDays.Clear();
            DateTime startDate = ViewMode == CalendarViewMode.Monthly 
                ? new DateTime(CurrentDate.Year, CurrentDate.Month, 1).AddDays(-(int)new DateTime(CurrentDate.Year, CurrentDate.Month, 1).DayOfWeek)
                : CurrentDate.AddDays(-(int)CurrentDate.DayOfWeek);

            int days = ViewMode == CalendarViewMode.Monthly ? 42 : 7;
            for (int i = 0; i < days; i++) AddDayToCalendar(startDate.AddDays(i), startDate.AddDays(i).Month == CurrentDate.Month || ViewMode == CalendarViewMode.Weekly);
        }

        private void AddDayToCalendar(DateTime loopDate, bool isCurrentMonth)
        {
            var dayObj = new CalendarDay { Date = loopDate, IsCurrentMonth = isCurrentMonth };
            foreach (var todo in _todoManager.GetTodosForDate(loopDate)) dayObj.Todos.Add(todo);
            CalendarDays.Add(dayObj);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}