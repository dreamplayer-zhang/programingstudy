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
        EUVPodSurfaceParameter surfaceParameter;
        SurfaceParam_ViewModel darkParam, brightParam;
        bool darkVisibility, brightVisibility;
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
        public SurfaceParam_Tree_ViewModel()
        {
            Main = new SurfaceParam_Tree();
            Main.DataContext = this;

            surfaceParameter = GlobalObjects.Instance.Get<RecipeVision>().GetItem<EUVPodSurfaceParameter>();
            darkParam = new SurfaceParam_ViewModel(surfaceParameter.PodStain.DarkParam);
            brightParam = new SurfaceParam_ViewModel(surfaceParameter.PodStain.BrightParam);
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
