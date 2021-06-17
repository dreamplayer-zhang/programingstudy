using RootTools.Trees;
using System.Windows;
using System.Windows.Controls;

namespace RootTools.Camera.CognexDM150
{
    /// <summary>
    /// Camera_CognexDM150_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Camera_CognexDM150_UI : UserControl
    {
        public Camera_CognexDM150_UI()
        {
            InitializeComponent();
        }

        Camera_CognexDM150 m_cam;
        public void Init(Camera_CognexDM150 cam)
        {
            m_cam = cam;
            DataContext = cam;
            treeRootUI.Init(cam.p_treeRoot);
            cam.RunTree(Tree.eMode.Init);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            m_cam.ReadBCD();
        }

    }
}
