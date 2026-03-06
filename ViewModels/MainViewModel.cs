using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DesktopTodo.Models;
using DesktopTodo.Services;

namespace DesktopTodo.ViewModels
{
    // INotifyPropertyChanged: 데이터가 바뀌면 화면(UI)도 자동으로 바뀌게(새로고침) 해주는 마법의 인터페이스입니다.
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly TodoManager _todoManager;
        private DateTime _selectedDate;

        // 화면의 리스트뷰(ListView)와 직접 연결될 투두 리스트
        // ObservableCollection을 쓰면 리스트에 항목이 추가/삭제될 때 화면이 즉시 업데이트됩니다.
        public ObservableCollection<TodoItemBase> CurrentTodos { get; set; }

        // 사용자가 달력에서 선택한 날짜
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged(); // 화면에 "날짜 바뀌었어!"라고 알려줌
                RefreshTodos();      // 날짜가 바뀌었으니 해당 날짜의 투두 목록을 다시 불러옴
            }
        }

        public MainViewModel()
        {
            _todoManager = new TodoManager();
            CurrentTodos = new ObservableCollection<TodoItemBase>();
            
            // 앱을 처음 켜면 기본적으로 '오늘' 날짜를 선택하도록 설정
            SelectedDate = DateTime.Today; 
        }

        // 선택된 날짜에 맞춰 투두 리스트를 새로고침하는 메서드
        public void RefreshTodos()
        {
            CurrentTodos.Clear();
            
            // TodoManager에서 앞서 만든 멋진 로직(루틴 생성, 이월, Project 숨김 등)을 거친 데이터들을 가져옵니다.
            var todos = _todoManager.GetTodosForDate(SelectedDate);
            
            foreach (var todo in todos)
            {
                CurrentTodos.Add(todo);
            }
        }

        // --- INotifyPropertyChanged 구현부 ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}