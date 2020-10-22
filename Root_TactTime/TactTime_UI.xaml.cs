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
            listView.ItemsSource = tactTime.m_aEvent; 
            InitModuleUI();
            treeUI.Init(tactTime.m_treeRoot);
            tactTime.RunTree(Tree.eMode.Init); 
        }

        #region Module UI
        void InitModuleUI()
        {
            foreach (Module module in m_tactTime.m_aModule) canvasTact.Children.Add(module.p_ui);
            foreach (Picker picker in m_tactTime.m_aPicker) canvasTact.Children.Add(picker.p_ui);
        }
        #endregion

        private void buttonClear_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            m_tactTime.ClearSequence(true); 
        }

        private void buttonSave_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            m_tactTime.SaveSequence(); 
        }

        private void buttonOpen_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            m_tactTime.OpenSequence();
            m_tactTime.StartSimulation();
        }

        private void buttonRun_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            m_tactTime.StartSimulation(); 
        }
    }
}
