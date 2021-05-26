using Root_WIND2.Module;
using RootTools;
using RootTools.Control;
using RootTools_Vision;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Root_WIND2.UI_User
{
    public class CameraVRS_ImageViewer_ViewModel : RootViewer_ViewModel
    {
        public CameraVRS_ImageViewer_ViewModel()
        {

        }

        public void OnUpdateImage(Object sender, EventArgs args)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.SetImageSource();
            });
        }
    }
}
