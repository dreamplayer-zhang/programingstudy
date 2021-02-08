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
using System.Windows.Threading;
using RootTools_Vision;

namespace Root_AOP01_Inspection
{
    class Run_ViewModel : ObservableObject
    {
        //MainWindow m_Mainwindow;
        public Run_ViewModel()
        {
            //m_Mainwindow = main;
            //m_mainVision = ((AOP01_Handler)main.m_engineer.ClassHandler()).m_mainVision;
            
            // MiniViewer
            p_miniViewerMain = new MiniViewer_ViewModel(GlobalObjects.Instance.GetNamed<ImageData>(App.MainRegName));
            p_miniViewerLeft = new MiniViewer_ViewModel(GlobalObjects.Instance.GetNamed<ImageData>(App.SideLeftRegName));
            p_miniViewerTop = new MiniViewer_ViewModel(GlobalObjects.Instance.GetNamed<ImageData>(App.SideTopRegName), true);
            p_miniViewerRight = new MiniViewer_ViewModel(GlobalObjects.Instance.GetNamed<ImageData>(App.SideRightRegName));
            p_miniViewerBottom = new MiniViewer_ViewModel(GlobalObjects.Instance.GetNamed<ImageData>(App.SideBotRegName), true);
        }
        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(2000);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            p_dSequencePercent = Math.Ceiling(((AOP01_Handler)m_Mainwindow.m_engineer.ClassHandler()).m_process.m_dSequencePercent);
        }
        #region Property
        //MainVision m_mainVision;
        //public MainVision p_mainVision
        //{
        //    get { return m_mainVision; }
        //    set { SetProperty(ref m_mainVision, value); }
        //}
        #endregion

        #region MiniViewer
        MiniViewer_ViewModel m_miniViewerMain;
        public MiniViewer_ViewModel p_miniViewerMain
        {
            get { return m_miniViewerMain; }
            set { SetProperty(ref m_miniViewerMain, value); }
        }
        public double _dSequencePercent = 0;
        public double p_dSequencePercent
        {
            get { return _dSequencePercent; }
            set
            {
                SetProperty(ref _dSequencePercent, value);
                p_sSequencePercent = _dSequencePercent.ToString() + "%";
            }
        }
        public string _sSequencePercent = "";
        public string p_sSequencePercent
        {
            get { return _sSequencePercent; }
            set
            {
                SetProperty(ref _sSequencePercent, value);
            }
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
