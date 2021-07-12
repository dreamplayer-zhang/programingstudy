using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RootTools_Vision
{
    /// <summary>
    /// MotionController.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MotionController : UserControl
    {
        public MotionController()
        {
            InitializeComponent();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!Char.IsDigit((char)KeyInterop.VirtualKeyFromKey(e.Key)) & e.Key != Key.Back & e.Key != Key.OemPeriod | e.Key == Key.Space ) 
            { 
                e.Handled = true; 
                //MessageBox.Show("숫자만 입력해주세요.\n단위는 분(min)입니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error); 
            }
        }
    }
}
