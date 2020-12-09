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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Root_Vega
{
    /// <summary>
    /// _6_Log.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class _6_Log : UserControl
    {
        public _6_Log()
        {
            InitializeComponent();
            logView.Init(LogView.m_logView); 
        }
        Vega_Engineer m_engineer;
        Vega_Handler m_handler;
        public void Init(Vega_Engineer engineer)
        {
            m_engineer = engineer;
            m_handler = engineer.m_handler;
            Loadport1.DataContext = m_handler.m_aLoadport[0];
            Loadport2.DataContext = m_handler.m_aLoadport[1];
        }
    }
}
