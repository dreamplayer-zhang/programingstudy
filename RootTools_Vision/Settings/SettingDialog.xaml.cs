using RootTools;
using System.Windows;

namespace RootTools_Vision
{
    /// <summary>
    /// SettingDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingDialog : Window, IDialog
    {
        public SettingDialog()
        {
            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
