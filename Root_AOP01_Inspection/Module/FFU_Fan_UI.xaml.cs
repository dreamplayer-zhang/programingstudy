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

namespace Root_AOP01_Inspection.Module
{
    /// <summary>
    /// FFU_Fan_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FFU_Fan_UI : UserControl
    {
        public FFU_Fan_UI()
        {
            InitializeComponent();
        }

        FFU.Unit.Fan m_Fan;
        public void Init(FFU.Unit.Fan fan)
        {
            m_Fan = fan;
            DataContext = fan;
        }
    }
}
