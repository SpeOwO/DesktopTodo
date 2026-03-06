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
        
        // 날짜 바인딩을 위한 필드 추가
        private DateTime _newTodoDate;
        private DateTime _newTodoEndDate;

        public DateTime NewTodoDate
        {
            get => _newTodoDate;
            set { _newTodoDate = value; OnPropertyChanged(); }
        }
        
        public DateTime NewTodoEndDate
        {
            get => _newTodoEndDate;
            set { _newTodoEndDate = value; OnPropertyChanged(); }
        }

        public string PopupTitle => string.IsNullOrEmpty(_editingTodoId) ? "새 할 일 추가" : "할 일 상세/수정";
        public bool IsEditing => !string.IsNullOrEmpty(_editingTodoId);

        public ObservableCollection<string> AvailableColors { get; set; } = new ObservableCollection<string> 
        { "#FF6B6B", "#4DABF7", "#51CF66", "#FCC419", "#FFFFFF" };

        public ObservableCollection<TodoState> AvailableStates { get; } = new ObservableCollection<TodoState>
        { TodoState.NotStarted, TodoState.InProgress, TodoState.Pending, TodoState.Holding, TodoState.Completed };

        public ObservableCollection<TaskScope> AvailableScopes { get; } = new ObservableCollection<TaskScope>
        { TaskScope.Instance, TaskScope.Global };

        public string NewTodoTitle
        {
            get => _newTodoTitle;
            set { _newTodoTitle = value; OnPropertyChanged(); }
        }

        public string NewTodoMemo
        {
            get => _newTodoMemo;
            set { _newTodoMemo = value; OnPropertyChanged(); }
        }

        public string SelectedColorTag
        {
            get => _selectedColorTag;
            set { _selectedColorTag = value; OnPropertyChanged(); }
        }

        public TodoState NewTodoState
        {
            get => _newTodoState;
            set { _newTodoState = value; OnPropertyChanged(); }
        }

        public TaskScope NewTodoScope
        {
            get => _newTodoScope;
            set { _newTodoScope = value; OnPropertyChanged(); }
        }

        public ObservableCollection<CalendarDay> CalendarDays { get; set; }

        public bool IsSizeUnlocked
        {
            get => _isSizeUnlocked;
            set { _isSizeUnlocked = value; OnPropertyChanged(); }
        }

        public CalendarViewMode ViewMode
        {
            get => _viewMode;
            set { _viewMode = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayDateText)); GenerateCalendar(); }
        }

        public DateTime CurrentDate
        {
            get => _currentDate;
            set { _currentDate = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayDateText)); GenerateCalendar(); }
        }

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

        public void OpenAddPopup(DateTime date)
        {
            _editingTodoId = null;
            NewTodoDate = date;           // 클릭한 날짜가 기본 시작일
            NewTodoEndDate = date;        // 클릭한 날짜가 기본 종료일
            NewTodoTitle = "";
            NewTodoMemo = "";
            SelectedColorTag = "#FFFFFF";
            NewTodoState = TodoState.NotStarted;
            NewTodoScope = TaskScope.Instance; 
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
            
            if (todo is SingleTask st) 
            {
                NewTodoDate = st.TargetDate;
                NewTodoEndDate = st.EndDate ?? st.TargetDate; // EndDate가 null이면 시작일과 동일하게 표시
            }
            
            OnPropertyChanged(nameof(PopupTitle));
            OnPropertyChanged(nameof(IsEditing));
        }

        public void SaveTodo()
        {
            if (string.IsNullOrWhiteSpace(NewTodoTitle)) return;

            // 만약 종료일이 시작일보다 앞서 있다면 자동으로 맞춰줌
            if (NewTodoEndDate.Date < NewTodoDate.Date)
            {
                NewTodoEndDate = NewTodoDate;
            }

            if (string.IsNullOrEmpty(_editingTodoId))
            {
                var newTask = new SingleTask
                {
                    Title = NewTodoTitle,
                    Memo = NewTodoMemo,
                    TargetDate = NewTodoDate,
                    EndDate = NewTodoEndDate, // 설정한 기간 저장
                    ColorTag = SelectedColorTag,
                    State = NewTodoState,
                    Scope = NewTodoScope
                };
                _todoManager.Tasks.Add(newTask);
            }
            else
            {
                var taskToUpdate = _todoManager.Tasks.FirstOrDefault(t => t.Id == _editingTodoId) as SingleTask;
                if (taskToUpdate != null)
                {
                    taskToUpdate.Title = NewTodoTitle;
                    taskToUpdate.Memo = NewTodoMemo;
                    taskToUpdate.TargetDate = NewTodoDate;
                    taskToUpdate.EndDate = NewTodoEndDate; // 변경된 기간 저장
                    taskToUpdate.ColorTag = SelectedColorTag;
                    taskToUpdate.State = NewTodoState;
                    taskToUpdate.Scope = NewTodoScope; 
                }
            }

            _todoManager.SaveData();
            GenerateCalendar();
        }

        public void DeleteTodo()
        {
            if (!string.IsNullOrEmpty(_editingTodoId))
            {
                var taskToDelete = _todoManager.Tasks.FirstOrDefault(t => t.Id == _editingTodoId);
                if (taskToDelete != null)
                {
                    _todoManager.Tasks.Remove(taskToDelete);
                    _todoManager.SaveData();
                }
            }
            GenerateCalendar();
        }

        public void GoToPrevious() => CurrentDate = ViewMode == CalendarViewMode.Monthly ? CurrentDate.AddMonths(-1) : CurrentDate.AddDays(-7);
        public void GoToNext() => CurrentDate = ViewMode == CalendarViewMode.Monthly ? CurrentDate.AddMonths(1) : CurrentDate.AddDays(7);

        public void GenerateCalendar()
        {
            CalendarDays.Clear();
            DateTime startDate;

            if (ViewMode == CalendarViewMode.Monthly)
            {
                DateTime firstDayOfMonth = new DateTime(CurrentDate.Year, CurrentDate.Month, 1);
                startDate = firstDayOfMonth.AddDays(-(int)firstDayOfMonth.DayOfWeek);
                for (int i = 0; i < 42; i++) AddDayToCalendar(startDate.AddDays(i), startDate.AddDays(i).Month == CurrentDate.Month);
            }
            else
            {
                startDate = CurrentDate.AddDays(-(int)CurrentDate.DayOfWeek);
                for (int i = 0; i < 7; i++) AddDayToCalendar(startDate.AddDays(i), true);
            }
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