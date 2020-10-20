using RootTools.Trees;
using System.Windows.Controls;

namespace Root_TactTime
{
    /// <summary>
    /// TactTime_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TactTime_UI : UserControl
    {
        public TactTime_UI()
        {
            InitializeComponent();
        }

        TactTime m_tactTime; 
        public void Init(TactTime tactTime)
        {
            m_tactTime = tactTime;
            DataContext = tactTime;
            InitModuleUI();
            treeUI.Init(tactTime.m_treeRoot);
            tactTime.RunTree(Tree.eMode.Init); 
        }

        #region Module UI
        void InitModuleUI()
        {
            foreach (Module module in m_tactTime.m_aModule)
            {
                Module_UI ui = new Module_UI();
                ui.Init(module);
                canvasTact.Children.Add(ui); 
            }
        }
        #endregion

        private void buttonClear_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            m_tactTime.ClearSequence(); 
        }
    }
}
