﻿using Root_CAMELLIA.Module;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_CAMELLIA
{
    public class Dlg_Engineer_ViewModel : ObservableObject, IDialogRequestClose
    {
        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;

        #region Property
        /// <summary>
        /// Selected Tab Axis Index
        /// </summary>
        public int MotionTabIndex
        {
            get
            {
                return _MotionTabIndex;
            }
            set
            {
                eTabAxis = (TabAxis)value;
                switch (eTabAxis)
                {
                    case TabAxis.AxisX:
                        {
                            SelectedAxis = m_AxisXY.p_axisX;
                            SelectedAxisWorkPoint = GetWorkPoint(m_AxisXY.p_axisX.m_aPos);
                            return;
                        }
                    case TabAxis.AxisY:
                        {
                            SelectedAxis = m_AxisXY.p_axisY;
                            SelectedAxisWorkPoint = GetWorkPoint(m_AxisXY.p_axisY.m_aPos);
                            return;
                        }
                    case TabAxis.AxisZ:
                        {
                            SelectedAxis = m_AxisZ;
                            SelectedAxisWorkPoint = GetWorkPoint(m_AxisZ.m_aPos);
                            return;
                        }
                }
                SetProperty(ref _MotionTabIndex, value);
            }
        }
        private int _MotionTabIndex = 6;

        /// <summary>
        /// Selected Axis Tab
        /// </summary>
        public Axis SelectedAxis
        {
            get
            {
                return _SelectedAxis;
            }
            set
            {
                SetProperty(ref _SelectedAxis, value);
            }
        }
        private Axis _SelectedAxis;

        /// <summary>
        /// SelectedAxis WorkPoint
        /// </summary>
        public ObservableCollection<WorkPoint> SelectedAxisWorkPoint
        {
            get
            {
                return _SelectedAxisWorkPoint;
            }
            set
            {
                SetProperty(ref _SelectedAxisWorkPoint, value);
            }
        }
        private ObservableCollection<WorkPoint> _SelectedAxisWorkPoint;

        /// <summary>
        /// Input Position Value
        /// </summary>
        public int PosValue
        {
            get
            {
                return _PosValue;
            }
            set
            {
                SetProperty(ref _PosValue, value);
            }
        }
        private int _PosValue = 0;

        /// <summary>
        /// Module Camellia
        /// </summary>
        public Module_Camellia ModuleCamellia
        {
            get
            {
                return _ModuleCamellia;
            }
            set
            {
                SetProperty(ref _ModuleCamellia, value);
            }
        }
        private Module_Camellia _ModuleCamellia;
        #endregion

        private AxisXY m_AxisXY;
        private Axis m_AxisZ;
        private TabAxis eTabAxis = TabAxis.AxisX;


        public void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            int value = 0;
            if (Int32.TryParse((e.EditingElement as TextBox).Text, out value))
            {
                m_AxisXY.p_axisX.m_aPos[m_AxisXY.p_axisX.m_asPos[e.Row.GetIndex()]] = value;
                m_AxisXY.p_axisX.RunTree(RootTools.Trees.Tree.eMode.RegWrite);
                m_AxisXY.p_axisX.RunTree(RootTools.Trees.Tree.eMode.Init);
            }
        }
        private ObservableCollection<WorkPoint> GetWorkPoint(Dictionary<string, double> dic)
        {
            ObservableCollection<WorkPoint> data = new ObservableCollection<WorkPoint>();
            for (int i = 0; i < dic.Count; i++)
            {
                WorkPoint wp = new WorkPoint();
                wp.Name = dic.Keys.ToArray()[i];
                wp.Value = dic.Values.ToArray()[i];
                data.Add(wp);
            }
            return data;
        }

        public Dlg_Engineer_ViewModel(MainWindow_ViewModel main)
        {
            ModuleCamellia = ((CAMELLIA_Handler)App.m_engineer.ClassHandler()).m_camellia;
            m_AxisXY = ModuleCamellia.p_axisXY;
            m_AxisZ = ModuleCamellia.p_axisZ;
            MotionTabIndex = 0;
            //SelectedAxisWorkPoint = GetWorkPoint(m_AxisXY.p_axisX.m_aPos);
        }

        #region ICommand
        public ICommand CmdAllHome
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_AxisXY.p_axisX.StartHome();
                    m_AxisXY.p_axisY.StartHome();
                    m_AxisZ.StartHome();
                    m_AxisXY.WaitReady();
                    m_AxisZ.WaitReady();
                });
            }
        }
        public ICommand CmdMovePos
        {
            get
            {
                return new RelayCommand(() =>
                {
                    
                    // 선택한 축 위치로 Move
                });
            }
        }
        public ICommand CmdStop
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_AxisXY.p_axisX.StopAxis();
                    m_AxisXY.p_axisY.StopAxis();
                    m_AxisZ.StopAxis();
                });
            }
        }
        public ICommand CmdSave
        {
            get
            {
                return new RelayCommand(() =>
                {
                    // Save 작업점 List
                });
            }
        }
        public ICommand CmdAxisHome
        {
            get
            {
                return new RelayCommand(() =>
                {
                    // Select Axis Home
                    SelectedAxis.StartHome();
                    SelectedAxis.WaitReady();
                });
            }
        }
        public ICommand CmdSetPos
        {
            get
            {
                return new RelayCommand(() =>
                {
                    // 지금 축 위치 propertygrid에 set
                });
            }
        }
        public ICommand CmdRepeat
        {
            get
            {
                return new RelayCommand(() =>
                {
                    //패스
                });
            }
        }
        public ICommand CmdLoad
        {
            get
            {
                return new RelayCommand(() =>
                {
                    // Load 작업점 List
                });
            }
        }
        public ICommand CmdServoOnOff
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_AxisXY.ServoOn(!m_AxisXY.p_axisX.p_bSeroOn);
                    m_AxisZ.ServoOn(!m_AxisZ.p_bSeroOn);
                });
            }
        }
        public ICommand CmdJogMinusFast
        {
            get
            {
                return new RelayCommand(() =>
                {
                    //select Axis jog
                    SelectedAxis.Jog(-1);
                });
            }
        }
        public ICommand CmdJogMinusNormal
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SelectedAxis.Jog(-0.31);
                });
            }
        }
        public ICommand CmdRelMoveMinus
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SelectedAxis.StartMove(-PosValue);
                    SelectedAxis.WaitReady();
                });
            }
        }
        public ICommand CmdRelMovePlus
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SelectedAxis.StartMove(PosValue);
                    SelectedAxis.WaitReady();
                });
            }
        }
        public ICommand CmdJogPlusNormal
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SelectedAxis.Jog(0.31);

                });
            }
        }
        public ICommand CmdJogPlusFast
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SelectedAxis.Jog(1);
                });
            }
        }
        public ICommand CmdClose
        {
            get
            {
                return new RelayCommand(() =>
                {
                    CloseRequested(this, new DialogCloseRequestedEventArgs(true));
                });
            }
        }
        public ICommand CmdCentering
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Centering();
                });
            }
        }
        public ICommand CmdInitCal
        {
            get
            {
                return new RelayCommand(() =>
                {
                    InitCalibration();
                });
            }
        }
        public ICommand CmdMeasureBack
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Calibration();
                });
            }
        }
        public ICommand CmdMeasureSample
        {
            get
            {
                return new RelayCommand(() =>
                {
                    MeasureSample();
                });
            }
        }
        public ICommand CmdRepeatMeasure
        {
            get
            {
                return new RelayCommand(() =>
                {

                });
            }
        }

        #endregion

        #region Function
        private void MeasureSample()
        {
            EQ.p_bStop = false;
            if (ModuleCamellia.p_eState != ModuleBase.eState.Ready)
            {
                MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                return;
            }
            Module_Camellia.Run_Measure measure = (Module_Camellia.Run_Measure)ModuleCamellia.CloneModuleRun("Measure");
            measure.Run();
        }
        private void Calibration()
        {
            EQ.p_bStop = false;
            if (ModuleCamellia.p_eState != ModuleBase.eState.Ready)
            {
                MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                return;
            }
            Module_Camellia.Run_Calibration calibration = (Module_Camellia.Run_Calibration)ModuleCamellia.CloneModuleRun("Calibration");
            calibration.Run();
        }
        private void InitCalibration()
        {
            EQ.p_bStop = false;
            if (ModuleCamellia.p_eState != ModuleBase.eState.Ready)
            {
                MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                return;
            }
            Module_Camellia.Run_InitCalWaferCentering initCalibration = (Module_Camellia.Run_InitCalWaferCentering)ModuleCamellia.CloneModuleRun("InitCalWaferCentering");
            initCalibration.Run();
        }
        private void Centering()
        {
            EQ.p_bStop = false;
            if (ModuleCamellia.p_eState != ModuleBase.eState.Ready)
            {
                MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                return;
            }
            Module_Camellia.Run_WaferCentering centering = (Module_Camellia.Run_WaferCentering)ModuleCamellia.CloneModuleRun("WaferCentering");
            centering.Run();
        }
        #endregion

        public class WorkPoint
        {
            private string _Name;
            public string Name
            {
                get
                {
                    return _Name;
                }
                set
                {
                    _Name = value;
                }
            }
            private double _Value;
            public double Value
            {
                get
                {
                    return _Value;
                }
                set
                {
                    _Value = value;
                }
            }
        }
        private enum TabAxis
        {
            AxisX,
            AxisY,
            AxisZ,
            Lifter,
            StageZ,
            TiltX,
            TiltY
        }
    }
}
