using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Root_WIND2
{
    class Origin_ViewModel : ObservableObject
    {
        TRect Box;

        private BoxTool_ViewModel m_BOX_VM;
        public BoxTool_ViewModel p_BOX_VM
        {
            get
            {
                return m_BOX_VM;
            }
            set
            {
                SetProperty(ref m_BOX_VM, value);
            }
        }
        private OriginTool_ViewModel m_OriginTool_VM;
        public OriginTool_ViewModel p_OriginTool_VM
        {
            get
            {
                return m_OriginTool_VM;
            }
            set
            {
                SetProperty(ref m_OriginTool_VM, value);
            }
        }

        public void ViewerInit(Setup_ViewModel setup)
        {
            p_BOX_VM = new BoxTool_ViewModel(setup.m_MainWindow.m_Image, setup.m_MainWindow.dialogService);
            p_BOX_VM.BoxDone += P_BOX_VM_BoxDone;
            p_OriginTool_VM = new OriginTool_ViewModel();
            p_OriginTool_VM.AddOrigin += P_OriginTool_VM_AddOrigin;
            p_OriginTool_VM.AddPitch += P_OriginTool_VM_AddPitch;
            p_OriginTool_VM.AddArea += P_OriginTool_VM_AddInspArea;
        }

        private void P_OriginTool_VM_AddInspArea(object e)
        {
            p_BOX_VM.AddInspArea(e as TRect);
        }

        private void P_OriginTool_VM_AddPitch(object e)
        {
            p_BOX_VM.AddPitchPoint(e as CPoint, Brushes.Green);
        }
        private void P_OriginTool_VM_AddOrigin(object e)
        {
            p_BOX_VM.AddOriginPoint(e as CPoint, Brushes.Red);
        }

        private void P_BOX_VM_BoxDone(object e)
        {
            Box = e as TRect;

            ImageData BoxImageData = new ImageData(Box.MemoryRect.Width, Box.MemoryRect.Height);
            BoxImageData.m_eMode = ImageData.eMode.ImageBuffer;
            BoxImageData.SetData(p_BOX_VM.p_ImageData.GetPtr(), Box.MemoryRect, (int)p_BOX_VM.p_ImageData.p_Stride);

            p_OriginTool_VM.BoxOffset = new CPoint(Box.MemoryRect.Left, Box.MemoryRect.Top);
            p_OriginTool_VM.p_ImageData = BoxImageData;
            p_OriginTool_VM.SetRoiRect();

        }
    }
}
