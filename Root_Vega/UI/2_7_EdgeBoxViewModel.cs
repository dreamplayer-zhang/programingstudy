using RootTools;
using RootTools.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Root_Vega
{
    class _2_7_EdgeBoxViewModel : ObservableObject
    {
        protected Dispatcher _dispatcher;
        Vega_Engineer m_Engineer;
        MemoryTool m_MemoryModule;
        

        public _2_7_EdgeBoxViewModel(Vega_Engineer engineer, IDialogService dialogService)
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            m_Engineer = engineer;
            Init(dialogService);
        }

        void Init(IDialogService dialogService)
        {
            m_MemoryModule = m_Engineer.ClassMemoryTool();
            if (m_MemoryModule != null)
            {
                p_ImageViewer_Top = new ImageViewer_ViewModel(new ImageData(m_MemoryModule.GetMemory("SideVision.Memory", "SideVision", "Top")), dialogService);
                p_ImageViewer_Left = new ImageViewer_ViewModel(new ImageData(m_MemoryModule.GetMemory("SideVision.Memory", "SideVision", "Left")), dialogService);
                p_ImageViewer_Right = new ImageViewer_ViewModel(new ImageData(m_MemoryModule.GetMemory("SideVision.Memory", "SideVision", "Right")), dialogService);
                p_ImageViewer_Bottom = new ImageViewer_ViewModel(new ImageData(m_MemoryModule.GetMemory("SideVision.Memory", "SideVision", "Bottom")), dialogService);
            }
            
            return;
        }

        private ImageViewer_ViewModel m_ImageViewer_Left;
        public ImageViewer_ViewModel p_ImageViewer_Left
        {
            get
            {
                return m_ImageViewer_Left;
            }
            set
            {
                SetProperty(ref m_ImageViewer_Left, value);
            }
        }

        private ImageViewer_ViewModel m_ImageViewer_Top;
        public ImageViewer_ViewModel p_ImageViewer_Top
        {
            get
            {
                return m_ImageViewer_Top;
            }
            set
            {
                SetProperty(ref m_ImageViewer_Top, value);
            }
        }

        private ImageViewer_ViewModel m_ImageViewer_Right;
        public ImageViewer_ViewModel p_ImageViewer_Right
        {
            get
            {
                return m_ImageViewer_Right;
            }
            set
            {
                SetProperty(ref m_ImageViewer_Right, value);
            }
        }

        private ImageViewer_ViewModel m_ImageViewer_Bottom;
        public ImageViewer_ViewModel p_ImageViewer_Bottom
        {
            get
            {
                return m_ImageViewer_Bottom;
            }
            set
            {
                SetProperty(ref m_ImageViewer_Bottom, value);
            }
        }
    }
}
