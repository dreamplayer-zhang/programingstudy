using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2
{
    public class EdgePanel_ViewModel : ObservableObject
    {
        Setup_ViewModel m_Setup;
        private DrawTool_ViewModel m_DrawTool_VM;
        public DrawTool_ViewModel p_DrawTool_VM
        {
            get
            {
                return m_DrawTool_VM;
            }
            set
            {
                SetProperty(ref m_DrawTool_VM, value);
            }
        }


        public EdgePanel_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;
        }

        public void Init()
        {
            p_DrawTool_VM = new DrawTool_ViewModel(m_Setup.m_MainWindow.m_engineer.m_handler.m_sideVision.GetMemoryData(Module.SideVision.eMemData.EdgeTop), m_Setup.m_MainWindow.dialogService);
        }
    }
}
