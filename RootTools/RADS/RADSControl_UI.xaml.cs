using RootTools.Trees;
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

namespace RootTools.RADS
{
    /// <summary>
    /// RADSControl_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RADSControl_UI : UserControl
    {
        public RADSControl_UI()
        {
            InitializeComponent();
        }

        RADSControl m_RADSControl;

        public void Init(RADSControl radsControl)
        {
            m_RADSControl = radsControl;
            this.DataContext = radsControl.p_connect.p_CurrentController;
            treeUI.Init(radsControl.p_connect.p_CurrentController.p_TreeRoot);
            radsControl.p_connect.p_CurrentController.RunTree(Tree.eMode.Init);
        }
    }
}
