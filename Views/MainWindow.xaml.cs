using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
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

        private void Prev_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel vm)
            {
                vm.GoToPrevious();
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel vm)
            {
                vm.GoToNext();
            }
        }

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

        // --- 창 크기 조절 이벤트 ---

        // 우측 가장자리 드래그
        private void ResizeRight_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double newWidth = this.Width + e.HorizontalChange;
            if (newWidth >= this.MinWidth) this.Width = newWidth;
        }

        // 아래쪽 가장자리 드래그
        private void ResizeBottom_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double newHeight = this.Height + e.VerticalChange;
            if (newHeight >= this.MinHeight) this.Height = newHeight;
        }

        // 우측 하단 모서리 드래그
        private void ResizeCorner_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double newWidth = this.Width + e.HorizontalChange;
            double newHeight = this.Height + e.VerticalChange;

            if (newWidth >= this.MinWidth) this.Width = newWidth;
            if (newHeight >= this.MinHeight) this.Height = newHeight;
        }
    }
}