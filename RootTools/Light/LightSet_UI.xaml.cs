using System.Windows.Controls;

namespace RootTools.Light
{
    /// <summary>
    /// LightSet_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LightSet_UI : UserControl
    {
        public LightSet_UI()
        {
            InitializeComponent();
        }

        LightSet m_LightSet;
        public void Init(LightSet lightSet)
        {
            m_LightSet = lightSet;
            this.DataContext = lightSet;
            InitTabControl();
            lightSet.OnChangeTool += LightSet_OnChangeTool;
        }
        private void LightSet_OnChangeTool()
        {
            InitTabControl();
        }

        void InitTabControl()
        {
            tabControl.Items.Clear(); 
            foreach (Light light in m_LightSet.m_aLight)
            {
                TabItem tabItem = new TabItem();
                tabItem.Header = light.m_sName;
                tabItem.Content = light.p_ui;
                tabControl.Items.Add(tabItem); 
            }
        }
    }
}
