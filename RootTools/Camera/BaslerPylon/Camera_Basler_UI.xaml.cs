using System.Windows.Controls;

namespace RootTools.Camera.BaslerPylon
{
    /// <summary>
    /// Camera_Basler_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Camera_Basler_UI : UserControl
    {
        public Camera_Basler_UI()
        {
            InitializeComponent();
        }

        Camera_Basler m_cam;
        public void Init(Camera_Basler cam)
        {
            m_cam = cam;
            this.DataContext = cam;
            tree.DataContext = cam.p_treeRoot;
        }

    }
}
