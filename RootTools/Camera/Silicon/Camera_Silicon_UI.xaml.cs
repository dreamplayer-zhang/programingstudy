using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RootTools.Camera.Silicon
{
    /// <summary>
    /// Camera_Silicon_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Camera_Silicon_UI : UserControl
    {
        public Camera_Silicon_UI()
        {
            InitializeComponent();
        }

        Camera_Silicon m_cam;
        public void Init(Camera_Silicon cam)
        {
            m_cam = cam;
            this.DataContext = cam;
            treeRootUI.DataContext = cam.p_treeRoot;
        }
    }
}
