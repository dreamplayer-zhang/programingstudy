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
        }

        public ICommand cmdStart
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if(!ALIDList_PopupUI.m_bShow)
                    {
                        Dlg_Start dlg_Start = new Dlg_Start();
                        AOP01_Handler handler = m_Mainwindow.m_engineer.m_handler;
                        switch (handler.LoadportType)
                        {
                            case AOP01_Handler.eLoadport.Cymechs:
                                dlg_Start.Init(handler.m_mainVision, (WTRCleanUnit)handler.m_wtr, (Loadport_Cymechs)handler.m_aLoadport[0], (Loadport_Cymechs)handler.m_aLoadport[1], m_Mainwindow.m_engineer);
                                break;
                            case AOP01_Handler.eLoadport.RND:
                            default:
                                dlg_Start.Init(handler.m_mainVision, (WTRCleanUnit)handler.m_wtr, (Loadport_RND)handler.m_aLoadport[0], (Loadport_RND)handler.m_aLoadport[1], m_Mainwindow.m_engineer);
                                break;
                        }
                        dlg_Start.ShowDialog();
                    }
                });
            }
        }
    }
}
