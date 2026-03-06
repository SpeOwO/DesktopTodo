using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using DesktopTodo.Models;
using DesktopTodo.ViewModels;

namespace DesktopTodo.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow() { InitializeComponent(); this.DataContext = new MainViewModel(); }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { if (e.ChangedButton == MouseButton.Left) this.DragMove(); }
        private void Prev_Click(object sender, RoutedEventArgs e) { if (this.DataContext is MainViewModel vm) vm.GoToPrevious(); }
        private void Next_Click(object sender, RoutedEventArgs e) { if (this.DataContext is MainViewModel vm) vm.GoToNext(); }

        private void ToggleView_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel vm)
            {
                vm.ViewMode = vm.ViewMode == CalendarViewMode.Monthly ? CalendarViewMode.Weekly : CalendarViewMode.Monthly;
                this.Height = vm.ViewMode == CalendarViewMode.Weekly ? 350 : 800;
            }
        }

        private void ResizeRight_DragDelta(object sender, DragDeltaEventArgs e) { double newWidth = this.Width + e.HorizontalChange; if (newWidth >= this.MinWidth) this.Width = newWidth; }
        private void ResizeBottom_DragDelta(object sender, DragDeltaEventArgs e) { double newHeight = this.Height + e.VerticalChange; if (newHeight >= this.MinHeight) this.Height = newHeight; }
        private void ResizeCorner_DragDelta(object sender, DragDeltaEventArgs e) { double newWidth = this.Width + e.HorizontalChange; double newHeight = this.Height + e.VerticalChange; if (newWidth >= this.MinWidth) this.Width = newWidth; if (newHeight >= this.MinHeight) this.Height = newHeight; }

        // [NEW] 상단 헤더의 '+ 새 업무 추가' 버튼 클릭 이벤트
        private void AddNewTask_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel vm)
            {
                vm.OpenAddPopup(DateTime.Today); // 오늘 날짜를 기본으로 넘김
                TodoEditWindow editWindow = new TodoEditWindow { DataContext = vm, Owner = this };
                editWindow.ShowDialog(); 
            }
        }

        private void CalendarDay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is CalendarDay dayObj)
            {
                if (this.DataContext is MainViewModel vm)
                {
                    vm.OpenAddPopup(dayObj.Date);
                    TodoEditWindow editWindow = new TodoEditWindow { DataContext = vm, Owner = this };
                    editWindow.ShowDialog(); 
                }
            }
            e.Handled = true; 
        }

        private void TodoItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is TodoItemBase todo)
            {
                if (this.DataContext is MainViewModel vm)
                {
                    vm.OpenEditPopup(todo);
                    TodoEditWindow editWindow = new TodoEditWindow { DataContext = vm, Owner = this };
                    editWindow.ShowDialog();
                }
            }
            e.Handled = true; 
        }
    }
}