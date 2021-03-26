using RootTools;
using RootTools.Memory;
using RootTools_Vision;

namespace Root_VEGA_P_Vision.UI
{
    class ViewerTest_ViewModel : ObservableObject
    {
        RootViewer_ViewModel m_Viewer = new RootViewer_ViewModel();
        #region [Property]
        public RootViewer_ViewModel p_Viewer
        {
            get
            {
                return m_Viewer;
            }
            set
            {
                SetProperty(ref m_Viewer, value);
            }
        }

        private MiniViewer_ViewModel m_MiniImageViewer;
        public MiniViewer_ViewModel p_MiniImageViewer
        {
            get
            {
                return m_MiniImageViewer;
            }
            set
            {
                SetProperty(ref m_MiniImageViewer, value);
            }
        }
        #endregion
        MemoryTool m_ToolMemory;
        ImageData m_imagedata;

        public ViewerTest_ViewModel()
        {
            
        }

        public void Init(MemoryTool tool)
        {
            m_Viewer.init(null, GlobalObjects.Instance.Get<DialogService>());
            m_ToolMemory = tool;

            m_imagedata = new ImageData(m_ToolMemory.GetMemory("Vision.Memory", "Vision", "EIP_Plate.Main.Front"));
            m_imagedata.p_nByte = 1;
            m_imagedata.p_nPlane = 1;
            p_Viewer.SetImageData(m_imagedata);
        }

        public RelayCommand btnTest
        {
            get
            {
                return new RelayCommand(() =>
                {
                    p_MiniImageViewer.SetImageSource();
                });
            }
        }
    }
}
