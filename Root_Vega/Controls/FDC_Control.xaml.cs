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

namespace Root_Vega
{
    /// <summary>
    /// TK4SControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FDC_Control : UserControl
    {
		public FDC_Control(FDC_ControlViewModel _FDC_ControlViewModel)
        {
			this.DataContext = _FDC_ControlViewModel;
            InitializeComponent();
			
        }
    }
}
