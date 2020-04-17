using System.Windows.Controls;

namespace RootTools.Light
{
    /// <summary>
    /// LightTool_12ch_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LightTool_12ch_UI : UserControl
    {
        public LightTool_12ch_UI()
        {
            InitializeComponent();
        }

        LightTool_12ch m_lightTool; 
        public void Init(LightTool_12ch lightTool)
        {
            m_lightTool = lightTool;
            this.DataContext = lightTool;
            cyUSBUI.Init(lightTool.m_usb);
            InitTabControl();
            lightTool.OnChangeTool += LightTool_OnChangeTool;
        }

        private void LightTool_OnChangeTool()
        {
            InitTabControl();
        }

        void InitTabControl()
        {
            tabControl.Items.Clear(); 
            foreach (LightBase light in m_lightTool.p_aLight)
            {
                TabItem tabItem = new TabItem();
                tabItem.Header = light.p_sID.Replace(m_lightTool.p_id + ".", "");
                tabItem.Content = light.p_ui;
                tabControl.Items.Add(tabItem); 
            }
        }
    }
}
