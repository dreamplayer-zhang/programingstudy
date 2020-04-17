using System.Windows.Controls;

namespace RootTools.Camera.Dalsa
{
    /// <summary>
    /// Camera_Dalsa_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Camera_Dalsa_UI : UserControl
    {
        public Camera_Dalsa_UI()
        {
            InitializeComponent();
        }

        Camera_Dalsa m_cam;
        public void Init(Camera_Dalsa cam)
        {
            m_cam = cam;
            this.DataContext = cam;
            treeRootUI.DataContext = cam.p_treeRoot;
        }
    }
}
