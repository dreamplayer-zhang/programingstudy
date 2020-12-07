using RootTools;
using RootTools.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_AOP01_Inspection.UI._1._SETUP._3._Maintenance
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

        MemoryTool m_ToolMemory;
        ImageData m_imagedata;

        public ViewerTest_ViewModel()
        {

        }

        public void Init(MemoryTool tool)
        {
            m_ToolMemory = tool;
            p_Viewer.init(null, ProgramManager.Instance.DialogService);
            m_imagedata = new ImageData(m_ToolMemory.GetMemory("MainVision.Vision Memory", "MainVision", "Main"));
            m_imagedata.p_nByte = 3;
            p_Viewer.SetImageData(m_imagedata);
        }

        public RelayCommand btnTest
        {
            get
            {
                return new RelayCommand(() =>
                {
                    //m_ToolMemory.SendTest();
                });
            }
        }
    }
}
