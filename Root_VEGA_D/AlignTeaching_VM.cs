using Root_VEGA_D.Engineer;
using Root_VEGA_D.Module;
using RootTools;
using RootTools.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_D
{
    public class AlignTeaching_VM : RootViewer_ViewModel
    {
        Vision m_vision;
        ImageData m_imgOtherPC;
        public ImageData p_imgOtherPC
        {
            get { return m_imgOtherPC; }
            set { SetProperty(ref m_imgOtherPC, value); }
        }

        public AlignTeaching_VM()
        {
            m_vision = ((VEGA_D_Handler)App.m_engineer.ClassHandler()).m_vision;

            Init();
        }
        void Init()
        {
            this.p_VisibleMenu = System.Windows.Visibility.Collapsed;

            Vision vision = App.m_engineer.m_handler.m_vision;
            if (vision != null)
            {
                MemoryTool memoryTool = App.m_engineer.ClassMemoryTool();
                p_imgOtherPC = new ImageData(vision.MemoryPool.p_id, vision.MemoryGroup.p_id, vision.MemoryOtherPC.p_id, memoryTool, vision.MemoryOtherPC.p_nCount, vision.MemoryOtherPC.p_nByte);

                init(p_imgOtherPC);
            }
        }
    }
}
