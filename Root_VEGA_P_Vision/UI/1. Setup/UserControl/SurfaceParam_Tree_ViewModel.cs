using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_VEGA_P_Vision
{ 
    public class SurfaceParam_Tree_ViewModel:ObservableObject
    {
        public SurfaceParam_Tree Main;
        EUVPodSurfaceParameterBase surfaceParameter;
        SurfaceParam_ViewModel darkParam, brightParam;
        bool darkVisibility, brightVisibility;
        public EUVPodSurfaceParameterBase SurfaceParameter
        {
            get => surfaceParameter;
            set => SetProperty(ref surfaceParameter, value);
        }
        public bool DarkVisibility
        {
            get => darkVisibility;
            set => SetProperty(ref darkVisibility, value);
        }
        public bool BrightVisibility
        {
            get => brightVisibility;
            set => SetProperty(ref brightVisibility, value);
        }
        public SurfaceParam_ViewModel DarkParam
        {
            get => darkParam;
            set => SetProperty(ref darkParam, value);
        }
        public SurfaceParam_ViewModel BrightParam
        {
            get => brightParam;
            set => SetProperty(ref brightParam, value);
        }
        public SurfaceParam_Tree_ViewModel(EUVPodSurfaceParameterBase surfaceParam)
        {
            Main = new SurfaceParam_Tree();
            Main.DataContext = this;

            surfaceParameter = surfaceParam;
            darkParam = new SurfaceParam_ViewModel(surfaceParameter.DarkParam);
            brightParam = new SurfaceParam_ViewModel(surfaceParameter.BrightParam);
            darkVisibility = brightVisibility = false;
        }
        public ICommand BrightBtn
        {
            get => new RelayCommand(() => {
                BrightVisibility = !BrightVisibility;
            });
        }
        public ICommand DarkBtn
        {
            get => new RelayCommand(() => {
                DarkVisibility = !DarkVisibility;
            });
        }
    }
}
