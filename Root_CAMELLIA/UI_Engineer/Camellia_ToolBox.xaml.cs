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

namespace Root_CAMELLIA
{
    /// <summary>
    /// Camellia_ToolBox.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Camellia_ToolBox : UserControl
    {
        public Camellia_ToolBox()
        {
            InitializeComponent();
        }
        public void Init(CAMELLIA_Engineer engineer)
        {
            loginUI.Init(engineer.m_login);
            toolBoxUI.Init(engineer.ClassToolBox());
            treeRootUI.Init(engineer.m_treeRoot);
            engineer.RunTree(Tree.eMode.Init);
        }
    }
}
