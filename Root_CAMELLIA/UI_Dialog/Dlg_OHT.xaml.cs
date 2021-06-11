using Root_EFEM.Module;
using RootTools;
using RootTools.OHT.Semi;
using RootTools.OHTNew;
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

namespace Root_CAMELLIA.UI_Dialog
{
    /// <summary>
    /// Dlg_OHT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Dlg_OHT : Window
    {
        public Dlg_OHT()
        {
            InitializeComponent();
        }

        public void Init(OHT_Semi oht)
        {
            if (oht.p_id == "LoadportA.OHT") OHTA.Init(oht);
            else OHTB.Init(oht);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
