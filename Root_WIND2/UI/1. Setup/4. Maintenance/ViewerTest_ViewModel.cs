using RootTools;
using RootTools.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2.UI
{
    class ViewerTest_ViewModel : ObservableObject
    {
        RootViewer_ViewModel m_Viewer = new RootViewer_ViewModel();
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

        MemoryTool m_ToolMemory;
        ImageData m_imagedata;

        public ViewerTest_ViewModel()
        {
            
        }

        public void Init(MemoryTool tool)
        {
            m_ToolMemory = tool;

            m_imagedata = new ImageData(m_ToolMemory.GetMemory("Vision.Memory", "Vision", "Main"));
            m_imagedata.p_nByte = 3;
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
