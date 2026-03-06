using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using DesktopTodo.Models;
using DesktopTodo.ViewModels;

namespace DesktopTodo.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void Prev_Click(object sender, RoutedEventArgs e) { if (this.DataContext is MainViewModel vm) vm.GoToPrevious(); }
        private void Next_Click(object sender, RoutedEventArgs e) { if (this.DataContext is MainViewModel vm) vm.GoToNext(); }

        private void ToggleView_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel vm)
            {
                if (vm.ViewMode == CalendarViewMode.Monthly)
                {
                    vm.ViewMode = CalendarViewMode.Weekly;
                    this.Height = 350;
                }
                else
                {
                    vm.ViewMode = CalendarViewMode.Monthly;
                    this.Height = 800;
                }
            }
        }

        private void ResizeRight_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double newWidth = this.Width + e.HorizontalChange;
            if (newWidth >= this.MinWidth) this.Width = newWidth;
        }

        private void ResizeBottom_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double newHeight = this.Height + e.VerticalChange;
            if (newHeight >= this.MinHeight) this.Height = newHeight;
        }

        private void ResizeCorner_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double newWidth = this.Width + e.HorizontalChange;
            double newHeight = this.Height + e.VerticalChange;
            if (newWidth >= this.MinWidth) this.Width = newWidth;
            if (newHeight >= this.MinHeight) this.Height = newHeight;
        }

        // [수정됨] 달력 빈 공간 "단일 클릭" -> 새 할 일 독립된 창으로 띄우기
        private void CalendarDay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // e.ClickCount == 2 조건을 제거하여 한 번 클릭만으로 동작하게 변경
            if (sender is FrameworkElement element && element.DataContext is CalendarDay dayObj)
            {
                if (this.DataContext is MainViewModel vm)
                {
                    vm.OpenAddPopup(dayObj.Date);
                    
                    TodoEditWindow editWindow = new TodoEditWindow();
                    editWindow.DataContext = vm;
                    editWindow.Owner = this;
                    editWindow.ShowDialog(); 
                }
            }
            e.Handled = true; // 이벤트 처리 완료 (클릭 시 창 드래그 등 중복 동작 방지)
        }

        // 기존 할 일 클릭 -> 상세/수정 독립된 창으로 띄우기
        private void TodoItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is TodoItemBase todo)
            {
                if (this.DataContext is MainViewModel vm)
                {
                    vm.OpenEditPopup(todo);
                    
                    TodoEditWindow editWindow = new TodoEditWindow();
                    editWindow.DataContext = vm;
                    editWindow.Owner = this;
                    editWindow.ShowDialog();
                }
            }
            e.Handled = true; 
        }
    }
}