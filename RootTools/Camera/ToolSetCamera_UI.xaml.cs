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
            foreach (ICamera camera in m_toolSetCamera.m_aCamera)
            {
                TabItem tabItem = new TabItem();
                tabItem.Header = camera.p_id;
                tabItem.Content = camera.p_ui;
                tabControl.Items.Add(tabItem);
            }
        }
    }
}
