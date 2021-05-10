using RootTools;
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
using System.Windows.Shapes;

namespace Root_CAMELLIA
{
    /// <summary>
    /// Dlg_Login.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Dlg_Login : Window, IDialog
    {
        public Dlg_Login()
        {
            InitializeComponent();
            user.Focus();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //e.Handled = true;
            try
            {
                if(e.LeftButton == MouseButtonState.Pressed)
                    this.DragMove();
            }
            catch
            {

            }
        }
    }
}
