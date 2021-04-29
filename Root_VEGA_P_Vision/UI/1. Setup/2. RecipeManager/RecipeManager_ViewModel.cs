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

        #region [ImageViewer ViewModel]
        private readonly EIPCoverTop_ImageViewer_ViewModel EIPcovertop_ImageViewerVM;
        private readonly EIPCoverBottom_ImageViewer_ViewModel EIPcoverbottom_ImageViewerVM;
        private readonly EIPBaseTop_ImageViewer_ViewModel EIPbasetop_ImageViewerVM;
        private readonly EIPBaseBottom_ImageViewer_ViewModel EIPbasebottom_ImageViewerVM;

        public EIPCoverTop_ImageViewer_ViewModel EIPCoverTop_ImageViewerVM
        {
            get => EIPcovertop_ImageViewerVM;
        }
        public EIPCoverBottom_ImageViewer_ViewModel EIPCoverBottom_ImaageViewerVM
        {
            get => EIPcoverbottom_ImageViewerVM;
        }
        public EIPBaseTop_ImageViewer_ViewModel EIPBaseTop_ImageViewerVM
        {
            get => EIPbasetop_ImageViewerVM;
        }
        public EIPBaseBottom_ImageViewer_ViewModel EIPBaseBottom_ImageViewerVM
        {
            get => EIPbasebottom_ImageViewerVM;
        }
        #endregion



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
