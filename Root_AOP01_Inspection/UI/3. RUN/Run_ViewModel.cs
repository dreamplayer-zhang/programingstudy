using Root_AOP01_Inspection.Module;
using Root_EFEM.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using RootTools.Trees;
using RootTools.GAFs;
using RootTools;

namespace Root_AOP01_Inspection
{
    class Run_ViewModel : ObservableObject
    {
        MainWindow m_Mainwindow;
        public Run_ViewModel(MainWindow main)
        {
            m_Mainwindow = main;
            m_mainVision = ((AOP01_Handler)main.m_engineer.ClassHandler()).m_mainVision;
            
            // MiniViewer
            p_miniViewerMain = new MiniViewer_ViewModel(ProgramManager.Instance.ImageMain);
            p_miniViewerLeft = new MiniViewer_ViewModel(ProgramManager.Instance.ImageSideLeft);
            p_miniViewerTop = new MiniViewer_ViewModel(ProgramManager.Instance.ImageSideTop, true);
            p_miniViewerRight = new MiniViewer_ViewModel(ProgramManager.Instance.ImageSideRight);
            p_miniViewerBottom = new MiniViewer_ViewModel(ProgramManager.Instance.ImageSideBottom, true);
        }

        #region Property
        MainVision m_mainVision;
        public MainVision p_mainVision
        {
            get { return m_mainVision; }
            set { SetProperty(ref m_mainVision, value); }
        }
        #endregion

        #region MiniViewer
        MiniViewer_ViewModel m_miniViewerMain;
        public MiniViewer_ViewModel p_miniViewerMain
        {
            get { return m_miniViewerMain; }
            set { SetProperty(ref m_miniViewerMain, value); }
        }

        MiniViewer_ViewModel m_miniViewerLeft;
        public MiniViewer_ViewModel p_miniViewerLeft
        {
            get { return m_miniViewerLeft; }
            set { SetProperty(ref m_miniViewerLeft, value); }
        }

        MiniViewer_ViewModel m_miniViewerTop;
        public MiniViewer_ViewModel p_miniViewerTop
        {
            get { return m_miniViewerTop; }
            set { SetProperty(ref m_miniViewerTop, value); }
        }

        MiniViewer_ViewModel m_miniViewerRight;
        public MiniViewer_ViewModel p_miniViewerRight
        {
            get { return m_miniViewerRight; }
            set { SetProperty(ref m_miniViewerRight, value); }
        }

        MiniViewer_ViewModel m_miniViewerBottom;
        public MiniViewer_ViewModel p_miniViewerBottom
        {
            get { return m_miniViewerBottom; }
            set { SetProperty(ref m_miniViewerBottom, value); }
        }
        #endregion

        public ICommand cmdStart
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if(!ALIDList_PopupUI.m_bShow)
                    {
                        //Dlg_Start dlg_Start = new Dlg_Start();
                        //AOP01_Handler handler = m_Mainwindow.m_engineer.m_handler;
                        //dlg_Start.Init(handler);
                        //dlg_Start.ShowDialog();
                    }
                });
            }
        }
    }
}
