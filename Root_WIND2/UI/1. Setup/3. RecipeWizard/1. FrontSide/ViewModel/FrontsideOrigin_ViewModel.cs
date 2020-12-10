using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Root_WIND2
{
    class FrontsideOrigin_ViewModel : ObservableObject
    {
        ImageData OriginImageData;
        TRect InspAreaBuf;
        public delegate void setOrigin(object e);
        public event setOrigin SetOrigin;
        Recipe m_Recipe;

        public void init(Setup_ViewModel setup, Recipe recipe)
        {
            m_Recipe = recipe;
            p_OriginBoxTool_VM = new FrontsideOriginBox_ViewModel();
            p_OriginBoxTool_VM.init(setup, recipe);
            p_OriginBoxTool_VM.BoxDone += P_BoxTool_VM_BoxDone;

            p_OriginTool_VM = new FrontsideOriginTool_ViewModel(recipe);
            p_OriginTool_VM.AddOrigin += P_OriginTool_VM_AddOrigin;
            p_OriginTool_VM.AddPitch += P_OriginTool_VM_AddPitch;
            p_OriginTool_VM.DelegateInspArea += P_OriginTool_VM_DelegateInspArea;
        }

        
        private FrontsideOriginBox_ViewModel m_OriginBoxTool_VM;
        public FrontsideOriginBox_ViewModel p_OriginBoxTool_VM
        {
            get
            {
                return m_OriginBoxTool_VM;
            }
            set
            {
                SetProperty(ref m_OriginBoxTool_VM, value);
            }
        }
        private FrontsideOriginTool_ViewModel m_OriginTool_VM;
        public FrontsideOriginTool_ViewModel p_OriginTool_VM
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

        public void LoadOriginData()
        {
            OriginRecipe origin = m_Recipe.GetRecipe<OriginRecipe>();
            CPoint ptOrigin = new CPoint(origin.OriginX, origin.OriginY);
            CPoint ptPitchSize = new CPoint(origin.DiePitchX, origin.DiePitchY);
            CPoint ptPadding = new CPoint(origin.InspectionBufferOffsetX, origin.InspectionBufferOffsetY);
            p_OriginTool_VM.LoadOriginData(ptOrigin, ptPitchSize, ptPadding);
            p_OriginBoxTool_VM.SetRoiRect();
            //여기서 박스 그리기?
        }



        private void P_BoxTool_VM_BoxDone(object e)
        {
            TRect BOX = e as TRect;
            int byteCnt = p_OriginBoxTool_VM.p_ImageData.p_nByte;

            ImageData BoxImageData = new ImageData(BOX.MemoryRect.Width, BOX.MemoryRect.Height, byteCnt);

            BoxImageData.m_eMode = ImageData.eMode.ImageBuffer;
            BoxImageData.SetData(p_OriginBoxTool_VM.p_ImageData
                , new CRect(BOX.MemoryRect.Left, BOX.MemoryRect.Top, BOX.MemoryRect.Right, BOX.MemoryRect.Bottom)
                , (int)p_OriginBoxTool_VM.p_ImageData.p_Stride, byteCnt);

            p_OriginTool_VM.Offset = new CPoint(BOX.MemoryRect.Left, BOX.MemoryRect.Top);
            p_OriginTool_VM.p_ImageData = BoxImageData;
            p_OriginTool_VM.SetRoiRect();
        }

        private void P_OriginTool_VM_DelegateInspArea(object e)
        {
            //여기머야
            TRect InspAreaBuf = e as TRect;           
            p_OriginBoxTool_VM.AddInspArea(InspAreaBuf);
            OriginImageData = new ImageData(InspAreaBuf.MemoryRect.Width, InspAreaBuf.MemoryRect.Height);
            OriginImageData.m_eMode = ImageData.eMode.ImageBuffer;
            OriginImageData.SetData(p_OriginBoxTool_VM.p_ImageData.GetPtr(), InspAreaBuf.MemoryRect, (int)p_OriginBoxTool_VM.p_ImageData.p_Stride);

            InspAreaBuf.Tag = OriginImageData;

            SetOrigin(InspAreaBuf);

        }
        private void P_OriginTool_VM_AddPitch(object e)
        {
            p_OriginBoxTool_VM.AddPitchPoint(e as CPoint, Brushes.Green);
        }
        private void P_OriginTool_VM_AddOrigin(object e)
        {
            p_OriginBoxTool_VM.AddOriginPoint(e as CPoint, Brushes.Red);
        }

    }
}
