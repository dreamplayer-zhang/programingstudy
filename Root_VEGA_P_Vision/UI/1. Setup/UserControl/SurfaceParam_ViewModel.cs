using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_P_Vision
{
    public class SurfaceParam_ViewModel:ObservableObject
    {
        SurfaceParam surfaceParam;
        public SurfaceParam_Panel Main;

        #region Property

        public SurfaceParam Param
        {
            get => surfaceParam;
            set => SetProperty(ref surfaceParam, value);
        }
        public SurfaceParam_ViewModel(SurfaceParam surfaceParam)
        {
            this.surfaceParam = surfaceParam;
            Main = new SurfaceParam_Panel();
            Main.DataContext = this;
        }
        #endregion

    }
}
