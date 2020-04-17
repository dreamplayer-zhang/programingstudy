using RootTools.Trees;
using System.Windows;
using System.Windows.Controls;

namespace RootTools.Camera.CognexOCR
{
    /// <summary>
    /// Camera_CognexOCR_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Camera_CognexOCR_UI : UserControl
    {
        public Camera_CognexOCR_UI()
        {
            InitializeComponent();
        }

        Camera_CognexOCR m_cam;
        public void Init(Camera_CognexOCR cam)
        {
            m_cam = cam;
            this.DataContext = cam;
            tcpipUI.Init(cam.m_tcpip);
            treeRootUI.Init(cam.p_treeRoot);
            cam.RunTree(Tree.eMode.Init); 
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            m_cam.SendReadOCR(); 
        }
    }
}
