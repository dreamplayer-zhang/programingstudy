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

namespace Root_AOP01_Packing
{
    /// <summary>
    /// Run_Panel.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Run_Panel : UserControl
    {
        public Run_Panel()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Run_ViewModel vm = this.DataContext as Run_ViewModel;
            
            //vm.p_LoadportA.p_diPlaced.btest = true;
            //vm.p_LoadportA = vm.p_LoadportA;

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Dlg_RunStep dlg = new Dlg_RunStep();
            dlg.ShowDialog();
        }
    }
}
