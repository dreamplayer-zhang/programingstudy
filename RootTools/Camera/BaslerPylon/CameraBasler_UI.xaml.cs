using System.Windows.Controls;

namespace RootTools.Camera.BaslerPylon
{
    /// <summary>
    /// CameraBasler_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CameraBasler_UI : UserControl
    {
        public CameraBasler_UI()
        {
            InitializeComponent();
        }

        CameraBasler m_cameraBasler;
        public void Init(CameraBasler cameraBasler)
        {
            m_cameraBasler = cameraBasler;
            DataContext = cameraBasler;
            treeUI.Init(cameraBasler.p_treeRoot); 
        }
    }
}
