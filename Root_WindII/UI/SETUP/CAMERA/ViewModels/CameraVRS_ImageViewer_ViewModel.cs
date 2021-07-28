using RootTools;
using RootTools.Control;
using RootTools_Vision;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Root_WindII
{
    public class CameraVRS_ImageViewer_ViewModel : RootViewer_ViewModel
    {
        public CameraVRS_ImageViewer_ViewModel()
        {
            this.p_VisibleMenu = Visibility.Collapsed;
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
