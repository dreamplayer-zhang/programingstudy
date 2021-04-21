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

namespace RootTools.Weigh
{
    /// <summary>
    /// CAS_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CAS_UI : UserControl
    {
        public CAS_UI()
        {
            InitializeComponent();
        }
        CAS m_CAS;
        public void Init(CAS cas)
        {
            m_CAS = cas;
            DataContext = cas;
            rs232UI.Init(cas.p_rs232);
            treeRootUI.Init(cas.m_treeRootSetup);
            cas.RunTreeSetup(Tree.eMode.Init);
        }
    }
}
