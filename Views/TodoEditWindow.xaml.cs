using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using DesktopTodo.ViewModels;

namespace DesktopTodo.Views
{
    public partial class TodoEditWindow : Window
    {
        public TodoEditWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        private void ResizeCorner_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double newWidth = this.Width + e.HorizontalChange;
            double newHeight = this.Height + e.VerticalChange;
            if (newWidth >= this.MinWidth) this.Width = newWidth;
            if (newHeight >= this.MinHeight) this.Height = newHeight;
        }

        // --- 최상단 탭 변경 이벤트 ---
        private void TabSingle_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel vm && !vm.IsEditing) vm.CurrentEditTaskType = EditTaskType.Single;
        }

        private void TabProject_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel vm && !vm.IsEditing) vm.CurrentEditTaskType = EditTaskType.Project;
        }

        private void TabRoutine_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel vm && !vm.IsEditing) vm.CurrentEditTaskType = EditTaskType.Routine;
        }

        // --- 프로젝트 하위 태스크 추가 ---
        private void AddSubTask_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel vm) vm.AddSubTask();
        }

        // --- 하단 저장/삭제/취소 ---
        private void CancelTodo_Click(object sender, RoutedEventArgs e) { this.Close(); }

        private void SaveTodo_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel vm) vm.SaveTodo();
            this.Close();
        }

        private void DeleteTodo_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel vm) vm.DeleteTodo();
            this.Close();
        }
    }
}