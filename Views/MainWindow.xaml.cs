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

        // 달력 빈 공간 더블클릭 -> 새 할 일 독립된 창으로 띄우기
        private void CalendarDay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (sender is FrameworkElement element && element.DataContext is CalendarDay dayObj)
                {
                    if (this.DataContext is MainViewModel vm)
                    {
                        vm.OpenAddPopup(dayObj.Date);
                        
                        // 새로운 윈도우 생성 및 띄우기
                        TodoEditWindow editWindow = new TodoEditWindow();
                        editWindow.DataContext = vm;
                        editWindow.Owner = this; // 메인 윈도우 중앙에 뜨게 만듦
                        editWindow.ShowDialog(); // 모달 창으로 표시 (입력 중 바탕 창 클릭 방지)
                    }
                }
                e.Handled = true;
            }
        }

        // 기존 할 일 클릭 -> 상세/수정 독립된 창으로 띄우기
        private void TodoItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is TodoItemBase todo)
            {
                if (this.DataContext is MainViewModel vm)
                {
                    vm.OpenEditPopup(todo);
                    
                    // 새로운 윈도우 생성 및 띄우기
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