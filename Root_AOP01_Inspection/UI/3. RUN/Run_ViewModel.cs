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


namespace Root_AOP01_Inspection
{
    class Run_ViewModel : ObservableObject
    {
        MainWindow m_Mainwindow;
        public Run_ViewModel(MainWindow main)
        {
            m_Mainwindow = main;
            m_mainVision = ((AOP01_Handler)main.m_engineer.ClassHandler()).m_mainVision;
        }

        #region Property
        MainVision m_mainVision;
        public MainVision p_mainVision
        {
            get { return m_mainVision; }
            set { SetProperty(ref m_mainVision, value); }
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
