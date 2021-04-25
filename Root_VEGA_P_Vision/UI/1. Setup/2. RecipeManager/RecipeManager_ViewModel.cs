using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_P_Vision
{
    public class RecipeManager_ViewModel:ObservableObject
    {
        public Setup_ViewModel m_Setup;
        public RecipeManagerPanel Main;
        private readonly EIPCoverTop_ImageViewer_ViewModel EIPcovertop_ImageViewerVM;
        public EIPCoverTop_ImageViewer_ViewModel EIPCoverTop_ImageViewerVM
        {
            get => EIPcovertop_ImageViewerVM;
        }
        

        public RecipeManager_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;
            Init();
            EIPcovertop_ImageViewerVM = new EIPCoverTop_ImageViewer_ViewModel();
            EIPcovertop_ImageViewerVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EIP_Cover.Main.Front"), GlobalObjects.Instance.Get<DialogService>());
        }
        private void Init()
        {
            Main = new RecipeManagerPanel();

            //SetPage(Main);
        }
        public enum ModifyType
        {
            None,
            LineStart,
            LineEnd,
            ScrollAll,
            Left,
            Right,
            Top,
            Bottom,
            LeftTop,
            RightTop,
            LeftBottom,
            RightBottom,
        }
    }
}
