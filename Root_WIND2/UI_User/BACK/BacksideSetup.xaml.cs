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

namespace Root_WIND2.UI_User
{
    /// <summary>
    /// BacksideROI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BacksideSetup : UserControl
    {
        public BacksideSetup()
        {
            InitializeComponent();
        }

        private void Button_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox tb = sender as TextBox;
                //tb.IsReadOnly = false;
                //tb.IsReadOnly = true;
                //tb.Focusable = false;
                //tb.Focusable = true;
            }

            if (!Char.IsDigit((char)KeyInterop.VirtualKeyFromKey(e.Key)) & e.Key != Key.Back & e.Key != Key.OemPeriod | e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }
    }
}
