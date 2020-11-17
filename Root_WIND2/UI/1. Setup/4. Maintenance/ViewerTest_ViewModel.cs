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

        MemoryTool m_ToolMemory;

        public ViewerTest_ViewModel()
        {
            
        }

        public void Init(MemoryTool tool)
        {
            m_ToolMemory = tool;
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
