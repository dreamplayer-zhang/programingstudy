using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Root_WIND2.UI_User
{
    public class CameraAlign_ImageViewer_ViewModel : RootViewer_ViewModel
    {

        public CameraAlign_ImageViewer_ViewModel()
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
