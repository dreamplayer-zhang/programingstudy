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

namespace RootTools.DMC
{
    /// <summary>
    /// DMCCore_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DMCCore_UI : UserControl
    {
        public DMCCore_UI()
        {
            InitializeComponent();
        }

        DMCCore m_dmc; 
        public void Init(DMCCore dmc)
        {
            m_dmc = dmc;
            DataContext = dmc;
            tcpipUI.Init(dmc.m_tcpip); 
        }
    }
}
