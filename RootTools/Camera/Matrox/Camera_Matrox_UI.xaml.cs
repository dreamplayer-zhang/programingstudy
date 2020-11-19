using System.Windows.Controls;

namespace RootTools.Camera.Matrox
{
    /// <summary>
    /// Camera_Matrox_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Camera_Matrox_UI : UserControl
    {
        public Camera_Matrox_UI()
        {
            InitializeComponent();
        }

        Camera_Matrox m_cam;
        public void Init(Camera_Matrox cam)
        {
            m_cam = cam;
            this.DataContext = cam;
            treeRootUI.DataContext = cam.p_treeRoot;
        }
    }
}
