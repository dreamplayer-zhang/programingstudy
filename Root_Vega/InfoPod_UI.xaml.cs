using RootTools;
using RootTools.Trees;
using System.Windows;
using System.Windows.Controls;

namespace Root_Vega
{
    /// <summary>
    /// InfoPod_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class InfoPod_UI : UserControl
    {
        public InfoPod_UI()
        {
            InitializeComponent();
        }

        InfoPod m_infoPod;
        public void Init(InfoPod infoPod)
        {
            m_infoPod = infoPod;
            this.DataContext = infoPod;
            treeReticleUI.Init(infoPod.m_treeRootReticle);
            infoPod.RunTreeReticle(Tree.eMode.Init);
            treeUI.Init(infoPod.m_treeRoot);
            infoPod.RunTree(Tree.eMode.Init); 

        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            if (EQ.p_bSimulate == false) return; 
            m_infoPod.SetInfoReticleExist();
            m_infoPod.RunTreeReticle(Tree.eMode.Init);
        }

        private void buttonRemove_Click(object sender, RoutedEventArgs e)
        {
            if (EQ.p_bSimulate == false) return;
            m_infoPod.p_infoReticle = null;
            m_infoPod.RunTreeReticle(Tree.eMode.Init);
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            m_infoPod.StartProcess();
            m_infoPod.RunTreeReticle(Tree.eMode.Init);
        }
    }
}
