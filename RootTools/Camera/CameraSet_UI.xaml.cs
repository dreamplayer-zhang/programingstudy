using System.Collections.Generic;
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

        List<string> m_asCamera = new List<string>();
        void InitTabControl()
        {
            tabControl.Items.Clear();
            m_asCamera.Clear();
            comboCamera.ItemsSource = null;
            foreach (ICamera camera in m_cameraSet.m_aCamera)
            {
                TabItem tabItem = new TabItem();
                tabItem.Header = camera.p_id.Replace(m_cameraSet.m_sModule + ".", "");
                tabItem.Content = camera.p_ui;
                tabItem.Height = 0; 
                tabControl.Items.Add(tabItem);
                m_asCamera.Add(camera.p_id.Replace(m_cameraSet.m_sModule + ".", ""));
            }
            comboCamera.ItemsSource = m_asCamera;
            comboCamera.SelectedIndex = 0;
        }

        private void comboCamera_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboCamera.SelectedIndex < 0) return;
            tabControl.SelectedIndex = comboCamera.SelectedIndex;
        }
    }
}
