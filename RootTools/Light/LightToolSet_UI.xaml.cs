using RootTools.Trees;
using System.Windows;
using System.Windows.Controls;

namespace RootTools.Light
{
    /// <summary>
    /// LightToolSet_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LightToolSet_UI : UserControl
    {
        public LightToolSet_UI()
        {
            InitializeComponent();
        }

        LightToolSet m_lightToolSet;
        public void Init(LightToolSet lightToolSet)
        {
            m_lightToolSet = lightToolSet;
            this.DataContext = lightToolSet;
            treeRootUI.Init(lightToolSet.m_treeRoot);
            lightToolSet.RunTree(Tree.eMode.Init);
            lightToolSet.OnToolChanged += LightToolSet_OnToolChanged;
            InitTabControl(); 
        }

        private void LightToolSet_OnToolChanged()
        {
            InitTabControl();
        }

        void InitTabControl()
        {
            tabControl.Items.Clear(); 
            foreach (ILightTool lightTool in m_lightToolSet.p_aLightTool)
            {
                TabItem tabItem = new TabItem();
                tabItem.Header = lightTool.p_id;
                tabItem.Content = lightTool.p_ui;
                tabControl.Items.Add(tabItem);
            }
        }

        private void CheckBoxSetup_Click(object sender, RoutedEventArgs e)
        {
            treeRootUI.Visibility = (checkBoxSetup.IsChecked == true) ? Visibility.Visible : Visibility.Hidden; 
            tabControl.Visibility = (checkBoxSetup.IsChecked == false) ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
