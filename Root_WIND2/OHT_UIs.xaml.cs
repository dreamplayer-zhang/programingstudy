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

using Root_EFEM.Module;

namespace Root_WIND2
{
    /// <summary>
    /// OHTs_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class OHTs_UI : Window
    {
        public OHTs_UI()
        {
            InitializeComponent();
        }

        public void Init(WIND2_Handler handler)
        {
            manualOHTA.Init((Loadport_RND)handler.m_aLoadport[0]);
            manualOHTB.Init((Loadport_RND)handler.m_aLoadport[1]);
        }

    }
}

