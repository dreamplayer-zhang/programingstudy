using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_P_Vision
{
    public class MaskViewer_ViewModel:ObservableObject
    {
        public MaskViewer_Panel Main;

        RootViewer_ViewModel coverTop_ImageViewer, coverBottom_ImageViewer, baseTop_ImageViewer, baseBottom_ImageViewer;

        public MaskViewer_ViewModel()
        {
            Main = new MaskViewer_Panel();
            Main.DataContext = this;
        }

        #region Property
        public RootViewer_ViewModel CoverTop_ImageViewer
        {
            get => coverTop_ImageViewer;
            set => SetProperty(ref coverTop_ImageViewer, value);
        }
        public RootViewer_ViewModel CoverBottom_ImageViewer
        {
            get => coverBottom_ImageViewer;
            set => SetProperty(ref coverBottom_ImageViewer, value);
        }
        public RootViewer_ViewModel BaseTop_ImageViewer
        {
            get => baseTop_ImageViewer;
            set => SetProperty(ref baseTop_ImageViewer, value);
        }
        public RootViewer_ViewModel BaseBottom_ImageViewer
        {
            get => baseBottom_ImageViewer;
            set => SetProperty(ref baseBottom_ImageViewer, value);
        }
        #endregion
    }
}
