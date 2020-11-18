using RootTools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
using System.Windows.Threading;

namespace Root_CAMELLIA
{
    /// <summary>
    /// Dlg_RecipeManger.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Dlg_RecipeManger : Window, IDialog
    {
        public Dlg_RecipeManger()
        {
            

            InitializeComponent();
       
        }
        //delegate void FHideWindow();

        //protected override void OnClosing(CancelEventArgs e)

        //{

        //    e.Cancel = true;

        //    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new

        //    FHideWindow(_HideThisWindow));

        //}


//        bool? private_dialog_result;

//        void _HideThisWindow()

//        {

//            this.Hide();
//            (typeof(Window)).GetField("_isClosing", BindingFlags.Instance |

//BindingFlags.NonPublic).SetValue(this, false);

//            (typeof(Window)).GetField("_dialogResult", BindingFlags.Instance |

//            BindingFlags.NonPublic).SetValue(this, private_dialog_result);

//            private_dialog_result = null;

//        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
