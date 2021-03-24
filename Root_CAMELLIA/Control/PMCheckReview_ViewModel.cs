using System;
using System.Windows;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using System.Windows.Input;
using System.Threading;
using Root_CAMELLIA.Module;

namespace Root_CAMELLIA
{
    public class PMCheckReview_ViewModel : ObservableObject
    {
        public PMCheckReview_ViewModel()
        {
            Init();
        }

        public void Init()
        {
            //pointListItem = new DataTable();
            p_DataTable.Columns.Add(new DataColumn("TIme"));
            p_DataTable.Columns.Add(new DataColumn("List"));
            p_DataTable.Columns.Add(new DataColumn("Result"));
        }
            DataTable m_DataTable = new DataTable();
        public DataTable p_DataTable
        {
            get
            {
                return m_DataTable;
            }
            set
            {
                SetProperty(ref m_DataTable, value);
            }
        }
        public void UpdatePMList ()
        {
            p_DataTable.Clear();
            DataRow row;
        }
        private Module_Camellia moduleCamellia;
        public Module_Camellia ModuleCamellia
        {
            get
            {
                return moduleCamellia;
            }
            set
            {
                SetProperty(ref moduleCamellia, value);
            }
        }

        public ICommand CmdReflectanceRepeatability
        {
            get
            {
                return new RelayCommand(() =>
                {
                    PMReflectanceRepeatability();
                });
            }
        }
        public ICommand CmdThicknessRepeatbility
        {
            get
            {
                return new RelayCommand(() =>
                {
                    PMThickenssRepeatability();
                });
            }
        }

        public ICommand cmdSensorStageAlign
        {
            get
            {
                return new RelayCommand(() =>
                {
                    PMSensorStageAlign();
                });
            }
        }

        public ICommand CmdSensorCameraTilt
        {
            get
            {
                return new RelayCommand(() =>
                {
                    PMSensorCameraTilt();
                });
            }
        }

        private void PMReflectanceRepeatability ()
        {
            EQ.p_bStop = false;
            if (ModuleCamellia.p_eState != ModuleBase.eState.Ready)
            {
                MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                return;
            }
            Thread thread = new Thread(() =>
            {
                Run_PMReflectance ReflectanceRepeatability = (Run_PMReflectance)ModuleCamellia.CloneModuleRun("PMTiltAlign");
                ReflectanceRepeatability.Run();
            });
            thread.Start();
        }
        private void PMThickenssRepeatability ()
        {
            EQ.p_bStop = false;
            if (ModuleCamellia.p_eState != ModuleBase.eState.Ready)
            {
                MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                return;
            }
            Thread thread = new Thread(() =>
            {
                Run_PMThickness ThicknessRepeatability = (Run_PMThickness)ModuleCamellia.CloneModuleRun("PMTiltAlign");
                ThicknessRepeatability.Run();
            });
            thread.Start();
        }
        private void PMSensorStageAlign ()
        {
            EQ.p_bStop = false;
            if (ModuleCamellia.p_eState != ModuleBase.eState.Ready)
            {
                MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                return;
            }
            Thread thread = new Thread(() =>
            {
                Run_PMSensorStageAlign SensorStageAlign = (Run_PMSensorStageAlign)ModuleCamellia.CloneModuleRun("PMTiltAlign");
                SensorStageAlign.Run();
            });
            thread.Start();
        }
        private void PMSensorCameraTilt()
        {
            EQ.p_bStop = false;
            if (ModuleCamellia.p_eState != ModuleBase.eState.Ready)
            {
                MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                return;
            }
            Thread thread = new Thread(() =>
            {
                Run_PMSensorCameraTilt SensorCameraTilt= (Run_PMSensorCameraTilt)ModuleCamellia.CloneModuleRun("PMTiltAlign");
                SensorCameraTilt.Run();
            });
            thread.Start();
        }
    }
}
