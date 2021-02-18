using Root_WIND2.Module;
using RootTools;
using RootTools.Control;
using RootTools_Vision;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Root_WIND2.UI_User
{
    public class CameraVRS_ImageViewer_ViewModel : ObservableObject
    {
        Vision m_Vision;
        public Vision p_Vision
        {
            get
            {
                return m_Vision;
            }
            set
            {
                SetProperty(ref m_Vision, value);
            }
        }

        Axis m_axisX;
        public Axis p_axisX
        {
            get
            {
                return m_axisX;
            }
            set
            {
                SetProperty(ref m_axisX, value);
            }
        }

        Axis m_axisY;
        public Axis p_axisY
        {
            get
            {
                return m_axisY;
            }
            set
            {
                SetProperty(ref m_axisY, value);
            }
        }

        Axis m_axisZ;
        public Axis p_axisZ
        {
            get
            {
                return m_axisZ;
            }
            set
            {
                SetProperty(ref m_axisZ, value);
            }
        }

        Axis m_axisRotate;
        public Axis p_axisRotate
        {
            get
            {
                return m_axisRotate;
            }
            set
            {
                SetProperty(ref m_axisRotate, value);
            }
        }

        RootViewer_ViewModel m_RootViewer = new RootViewer_ViewModel();
        public RootViewer_ViewModel p_RootViewer
        {
            get
            {
                return m_RootViewer;
            }
            set
            {
                SetProperty(ref m_RootViewer, value);
            }
        }

        Dispatcher dispatcher = null;
        public CameraVRS_ImageViewer_ViewModel()
        {
            
            p_Vision = GlobalObjects.Instance.Get<WIND2_Engineer>().m_handler.p_Vision;

            p_axisX = p_Vision.AxisXY.p_axisX;
            p_axisY = p_Vision.AxisXY.p_axisY;
            p_axisZ = p_Vision.AxisZ;
            p_axisRotate = p_Vision.AxisRotate;

            p_Vision.p_CamAutoFocus.Grabed += OnUpdateImage;
            p_RootViewer.p_VisibleMenu = System.Windows.Visibility.Collapsed;
            p_RootViewer.p_ImageData = p_Vision.p_CamAutoFocus.p_ImageViewer.p_ImageData;
            dispatcher = Application.Current.Dispatcher;
        }

        public void OnUpdateImage(object obj, EventArgs args)
        {
            dispatcher.Invoke(() =>
            {
                p_RootViewer.SetImageSource();
            });
        }
    }
}
