using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_VEGA_P_Vision
{
    public class MaskTools_ViewModel : ObservableObject
    {
        public MaskTools_Panel Main;
        ToolType eToolType;
        ViewerMode eviewerMode;
        bool isDraw;
        int nThickness, nThreshold, IsUp;
        public ToolType m_eToolType
        {
            get => eToolType;
            set => SetProperty(ref eToolType, value);
        }
        public ViewerMode eViewerMode
        {
            get => eviewerMode;
            set => SetProperty(ref eviewerMode, value);
        }
        public int p_nThickness
        {
            get => nThickness;
            set => SetProperty(ref nThickness, value);
        }
        public int p_nThreshold
        {
            get => nThreshold;
            set
            {
                if (value > 255)
                    SetProperty(ref nThreshold, 255);
                else if (value < 0)
                    SetProperty(ref nThreshold, 0);
                else
                    SetProperty(ref nThreshold, value);
            }
        }
        public int p_nselectedUpdown
        {
            get => IsUp;
            set => SetProperty(ref IsUp, value);
        }
        public bool IsDraw
        {
            get => isDraw;
            set => SetProperty(ref isDraw, value);
        }
        public MaskTools_ViewModel()
        {
            Main = new MaskTools_Panel();
            Main.DataContext = this;
            nThickness = 1;
            nThreshold = 0;
            IsDraw = true;
        }
        public ICommand btnDraw
        {
            get => new RelayCommand(()=> { 
                eViewerMode = ViewerMode.Mask; 
                IsDraw = true; 
            });
        }
        public ICommand btnErase
        {
            get => new RelayCommand(() => { 
                eViewerMode = ViewerMode.Mask;
                IsDraw = false; 
            });
        }
        public ICommand btnPen
        {
            get => new RelayCommand(()=> {
                eToolType = ToolType.Pen; 
            });
        }
        public ICommand btnRect
        {
            get => new RelayCommand(() => {
                eToolType = ToolType.Rect; 
            });
        }
        public ICommand btnThreshold
        {
            get => new RelayCommand(() => {
                eToolType = ToolType.Threshold; 
            });
        }
    }
}
