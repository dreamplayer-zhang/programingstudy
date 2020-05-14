using System.Collections.Generic;
using System.Windows.Controls;

namespace RootTools.Camera
{
    /// <summary>
    /// CameraToolSet_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ToolSetCamera_UI : UserControl
    {
        public ToolSetCamera_UI()
        {
            InitializeComponent();
        }

        ToolSetCamera m_toolSetCamera;
        public void Init(ToolSetCamera toolSetCamera)
        {
            m_toolSetCamera = toolSetCamera;
            this.DataContext = toolSetCamera;
            toolSetCamera.OnToolChanged += LightToolSet_OnToolChanged;
            InitTabControl();
        }

        private void LightToolSet_OnToolChanged()
        {
            InitTabControl();
        }

        void InitTabControl()
        {
            tabControl.Items.Clear();
            m_asCamera.Clear();
            comboCamera.ItemsSource = null; 
            foreach (ICamera camera in m_toolSetCamera.m_aCamera)
            {
                TabItem tabItem = new TabItem();
                tabItem.Header = camera.p_id;
                tabItem.Height = 0; 
                tabItem.Content = camera.p_ui;
                tabControl.Items.Add(tabItem);
                m_asCamera.Add(camera.p_id); 
            }
            comboCamera.ItemsSource = m_asCamera; 
        }

        List<string> m_asCamera = new List<string>();
        private void comboCamera_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboCamera.SelectedIndex < 0) return; 
            tabControl.SelectedIndex = comboCamera.SelectedIndex; 
        }
    }
}
