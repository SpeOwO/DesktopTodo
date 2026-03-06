using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
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

    public enum CalendarViewMode
    {
        Monthly,
        Weekly
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly TodoManager _todoManager;
        private DateTime _currentDate;
        private CalendarViewMode _viewMode;
        
        // 크기 조절 허용 여부 상태 (기본값: false - 크기 고정됨)
        private bool _isSizeUnlocked = false;

        public ObservableCollection<CalendarDay> CalendarDays { get; set; }

        public bool IsSizeUnlocked
        {
            get => _isSizeUnlocked;
            set
            {
                _isSizeUnlocked = value;
                OnPropertyChanged();
            }
        }

        public CalendarViewMode ViewMode
        {
            get => _viewMode;
            set
            {
                _viewMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayDateText));
                GenerateCalendar(); 
            }
        }

        public DateTime CurrentDate
        {
            get => _currentDate;
            set
            {
                _currentDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayDateText));
                GenerateCalendar(); 
            }
        }

        public string DisplayDateText
        {
            get
            {
                if (ViewMode == CalendarViewMode.Monthly)
                {
                    return CurrentDate.ToString("yyyy년 MM월");
                }
                else
                {
                    Calendar cal = CultureInfo.CurrentCulture.Calendar;
                    DateTime firstDayOfMonth = new DateTime(CurrentDate.Year, CurrentDate.Month, 1);
                    
                    int currentWeek = cal.GetWeekOfYear(CurrentDate, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
                    int firstWeek = cal.GetWeekOfYear(firstDayOfMonth, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
                    
                    int weekOfMonth = currentWeek - firstWeek + 1;
                    
                    return $"{CurrentDate.ToString("yyyy년 MM월")} {weekOfMonth}주차";
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

        public void GoToPrevious()
        {
            CurrentDate = ViewMode == CalendarViewMode.Monthly ? CurrentDate.AddMonths(-1) : CurrentDate.AddDays(-7);
        }

        public void GoToNext()
        {
            CurrentDate = ViewMode == CalendarViewMode.Monthly ? CurrentDate.AddMonths(1) : CurrentDate.AddDays(7);
        }

        public void GenerateCalendar()
        {
            CalendarDays.Clear();

            if (ViewMode == CalendarViewMode.Monthly)
            {
                DateTime firstDayOfMonth = new DateTime(CurrentDate.Year, CurrentDate.Month, 1);
                int diff = (int)firstDayOfMonth.DayOfWeek;
                DateTime startDate = firstDayOfMonth.AddDays(-diff);

                for (int i = 0; i < 42; i++)
                {
                    DateTime loopDate = startDate.AddDays(i);
                    var dayObj = new CalendarDay
                    {
                        Date = loopDate,
                        IsCurrentMonth = loopDate.Month == CurrentDate.Month
                    };
                    LoadTodosForDay(dayObj);
                    CalendarDays.Add(dayObj);
                }
            }
            else
            {
                int diff = (int)CurrentDate.DayOfWeek;
                DateTime startDate = CurrentDate.AddDays(-diff);

                for (int i = 0; i < 7; i++)
                {
                    DateTime loopDate = startDate.AddDays(i);
                    var dayObj = new CalendarDay
                    {
                        Date = loopDate,
                        IsCurrentMonth = true
                    };
                    LoadTodosForDay(dayObj);
                    CalendarDays.Add(dayObj);
                }
            }
        }

        private void LoadTodosForDay(CalendarDay dayObj)
        {
            var todosForDay = _todoManager.GetTodosForDate(dayObj.Date);
            foreach (var todo in todosForDay)
            {
                dayObj.Todos.Add(todo);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}