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
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        // 창 크기 조절 이벤트
        private void ResizeCorner_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double newWidth = this.Width + e.HorizontalChange;
            double newHeight = this.Height + e.VerticalChange;
            
            if (newWidth >= this.MinWidth) this.Width = newWidth;
            if (newHeight >= this.MinHeight) this.Height = newHeight;
        }

        private void CancelTodo_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SaveTodo_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel vm)
            {
                vm.SaveTodo();
            }
            this.Close();
        }

        private void DeleteTodo_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel vm)
            {
                vm.DeleteTodo();
            }
            this.Close();
        }

        private void Color_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is string colorCode)
            {
                if (this.DataContext is MainViewModel vm)
                {
                    vm.SelectedColorTag = colorCode;
                }
            }
        }
    }
}