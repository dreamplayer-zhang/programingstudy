using System.Windows.Controls;

namespace RootTools.Camera
{
    /// <summary>
    /// CameraSet_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CameraSet_UI : UserControl
    {
        public CameraSet_UI()
        {
            InitializeComponent();
        }

        CameraSet m_cameraSet; 
        public void Init(CameraSet cameraSet)
        {
            m_cameraSet = cameraSet;
            this.DataContext = cameraSet;
            InitTabControl();
            cameraSet.OnChangeTool += CameraSet_OnChangeTool;
        }

        private void CameraSet_OnChangeTool()
        {
            InitTabControl();
        }

        void InitTabControl()
        {
            tabControl.Items.Clear();
            foreach (ICamera camera in m_cameraSet.m_aCamera)
            {
                TabItem tabItem = new TabItem();
                tabItem.Header = camera.p_id.Replace(m_cameraSet.m_sModule + ".", "");
                tabItem.Content = camera.p_ui;
                tabControl.Items.Add(tabItem);
            }
        }
    }
}
