using System.Windows;
using System.Windows.Input;
using DesktopTodoCalendar.ViewModels;

namespace DesktopTodoCalendar.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // XAML에서 바인딩할 DataContext를 코드에서도 설정할 수 있습니다.
            // 여기서는 뷰모델을 생성해서 연결해줍니다.
            this.DataContext = new MainViewModel();
        }

        // 위젯 특성상 상단 타이틀바(Title bar)가 없기 때문에,
        // 창의 아무 곳이나 잡고 드래그해서 위치를 옮길 수 있도록 해주는 필수 이벤트입니다.
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}