using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using RootTools;

namespace Root_VEGA_P_Vision
{
    public class Recipe6um_ViewModel:ObservableObject
    {
        RecipeSetting_ViewModel recipeSetting;
        RootViewer_ViewModel coverTop_ImageViewer, coverBottom_ImageViewer, baseTop_ImageViewer, baseBottom_ImageViewer;
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

        public Recipe6um_Panel Main;

        
        public Recipe6um_ViewModel(RecipeSetting_ViewModel recipeSetting)
        {
            this.recipeSetting = recipeSetting;
            Main = new Recipe6um_Panel();
            coverTop_ImageViewer = new MaskRootViewer_ViewModel("EIP_Cover.Main.Front",recipeSetting);
            coverBottom_ImageViewer = new MaskRootViewer_ViewModel("EIP_Cover.Main.Front", recipeSetting);
            baseTop_ImageViewer = new MaskRootViewer_ViewModel("EIP_Cover.Main.Front", recipeSetting);
            baseBottom_ImageViewer = new MaskRootViewer_ViewModel("EIP_Cover.Main.Front", recipeSetting);
        }
    }
}
