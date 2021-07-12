using Root_VEGA_P.Engineer;
using Root_VEGA_P_Vision.Engineer;
using RootTools_Vision;
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

namespace Root_VEGA_P
{
    /// <summary>
    /// Maintenance_Panel.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Maintenance_Panel : UserControl
    {
        VEGA_P_Engineer m_engineer;

        public Maintenance_Panel()
        {
            InitializeComponent();
            m_engineer = GlobalObjects.Instance.Get<VEGA_P_Engineer>();
            engineerUI.Init(m_engineer);
        }
    }
}
