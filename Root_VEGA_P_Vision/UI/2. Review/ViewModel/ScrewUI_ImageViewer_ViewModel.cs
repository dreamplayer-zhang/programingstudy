using RootTools;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_P_Vision
{
    public class ScrewUI_ImageViewer_ViewModel: RootViewer_ViewModel
    {
        ScrewUI_ViewModel vm;
        public ScrewUI_ImageViewer_ViewModel(ScrewUI_ViewModel vm)
        {
            this.vm = vm;
            p_VisibleMenu = Visibility.Collapsed;
            p_VisibleSlider = Visibility.Collapsed;
        }
    }
}
