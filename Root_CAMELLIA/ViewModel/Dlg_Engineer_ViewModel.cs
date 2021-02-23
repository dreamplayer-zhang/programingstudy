﻿using Root_CAMELLIA.Data;
using Root_CAMELLIA.Module;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

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
                            //SelectedAxis = m_AxisXY.p_axisX;
                            //CurrentAxisWorkPoints = GetWorkPoint(m_AxisXY.p_axisX.m_aPos);
                            SelectedAxis = AxisX;
                            CurrentAxisWorkPoints = GetWorkPoint(AxisX.m_aPos);
                            return;
                        }
                    case TabAxis.AxisY:
                        {
                            //SelectedAxis = m_AxisXY.p_axisY;
                            //CurrentAxisWorkPoints = GetWorkPoint(m_AxisXY.p_axisY.m_aPos);
                            SelectedAxis = AxisY;
                            CurrentAxisWorkPoints = GetWorkPoint(AxisY.m_aPos);
                            return;
                        }
                    case TabAxis.AxisZ:
                        {
                            SelectedAxis = AxisZ;
                            CurrentAxisWorkPoints = GetWorkPoint(AxisZ.m_aPos);
                            return;
                        }
                    case TabAxis.Lifter:
                        {
                            SelectedAxis = AxisLifter;
                            CurrentAxisWorkPoints = GetWorkPoint(AxisLifter.m_aPos);
                            return;
                        }
                    case TabAxis.TiltX:
                        {
                            SelectedAxis = TiltAxisX;
                            CurrentAxisWorkPoints = GetWorkPoint(TiltAxisX.m_aPos);
                            return;
                        }
                    case TabAxis.TiltY:
                        {
                            SelectedAxis = TiltAxisY;
                            CurrentAxisWorkPoints = GetWorkPoint(TiltAxisY.m_aPos);
                            return;
                        }
                    case TabAxis.TiltZ:
                        {
                            SelectedAxis = TiltAxisZ;
                            CurrentAxisWorkPoints = GetWorkPoint(TiltAxisZ.m_aPos);
                            return;
                        }
                }
                SetProperty(ref _MotionTabIndex, value);
            }
        }
        private int _MotionTabIndex = 0;

        /// <summary>
        /// Selected Tab Axis
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
        /// Selected Axis WorkPoint Collection
        /// </summary>
        public ObservableCollection<WorkPoint> CurrentAxisWorkPoints
        {
            get
            {
                return _CurrentAxisWorkPoints;
            }
            set
            {
                SetProperty(ref _CurrentAxisWorkPoints, value);
            }
        }
        private ObservableCollection<WorkPoint> _CurrentAxisWorkPoints;

        /// <summary>
        /// Selected WorkPoint in WorkPoints Collection
        /// </summary>
        public int SelectedWorkPointIndex
        {
            get
            {
                return _SelectedWorkPointIndex;
            }
            set
            {
                SetProperty(ref _SelectedWorkPointIndex, value);
            }
        }
        private int _SelectedWorkPointIndex;

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

        public bool EnableBtn
        {
            get
            {
                return _EnableBtn;
            }
            set
            {
                SetProperty(ref _EnableBtn, value);
            }
        }
        private bool _EnableBtn = false;

        public Axis AxisX
        {
            get
            {
                return _AxisX;
            }
            set
            {
                SetProperty(ref _AxisX, value);
            }
        }
        private Axis _AxisX;

        public Axis AxisY
        {
            get
            {
                return _AxisY;
            }
            set
            {
                SetProperty(ref _AxisY, value);
            }
        }
        private Axis _AxisY;

        public Axis AxisZ
        {
            get
            {
                return _AxisZ;
            }
            set
            {
                SetProperty(ref _AxisZ, value);
            }
        }
        private Axis _AxisZ;

        public Axis AxisLifter
        {
            get
            {
                return _AxisLifter;
            }
            set
            {
                SetProperty(ref _AxisLifter, value);
            }
        }
        private Axis _AxisLifter;

        public Axis TiltAxisX
        {
            get
            {
                return _TiltAxisX;
            }
            set
            {
                SetProperty(ref _TiltAxisX, value);
            }
        }
        private Axis _TiltAxisX;

        public Axis TiltAxisY
        {
            get
            {
                return _TiltAxisY;
            }
            set
            {
                SetProperty(ref _TiltAxisY, value);
            }
        }
        private Axis _TiltAxisY;

        public Axis TiltAxisZ
        {
            get
            {
                return _TiltAxisZ;
            }
            set
            {
                SetProperty(ref _TiltAxisZ, value);
            }
        }
        private Axis _TiltAxisZ;


        //private Visibility _tttt = Visibility.Hidden;
        //public Visibility tttt
        //{
        //    get
        //    {
        //        return _tttt;
        //    }
        //    set
        //    {
        //        SetProperty(ref _tttt, value);
        //    }
        //}

        //public int asdf = 0;
        //public ICommand CmdTest
        //{
        //    get
        //    {
        //        return new RelayCommand(() =>
        //        {
        //            asdf++;

        //            if (asdf > 10 && asdf <= 20)
        //            {
        //                tttt = Visibility.Visible;
        //            }
        //            else if (asdf > 20)
        //            {
        //                tttt = Visibility.Hidden;
        //                asdf = 0;
        //            }

        //        });
        //    }
        //}

        public ICommand ConnectCommand
        {
            get
            {
                return new RelayCommand(() =>
                {

                    ModuleCamellia.p_CamVRS.FunctionConnect();

                });
            }
        }

        public ICommand GrabCommand
        {
            get
            {
                return new RelayCommand(() =>
                {

                    ModuleCamellia.p_CamVRS.GrabOneShot();

                });
            }
        }

        public ICommand ContinousGrabCommand
        {
            get
            {
                return new RelayCommand(() =>
                {

                    ModuleCamellia.p_CamVRS.GrabContinuousShot();

                });
            }
        }

        public ICommand StopGrabCommand
        {
            get
            {
                return new RelayCommand(() =>
                {

                    ModuleCamellia.p_CamVRS.GrabStop();

                });
            }
        }
        #endregion

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
        private TabAxis eTabAxis = TabAxis.AxisX;

        private RootViewer_ViewModel m_rootViewer = new RootViewer_ViewModel();
        public RootViewer_ViewModel p_rootViewer
        {
            get => this.m_rootViewer;
        }

        Dispatcher dispatcher = null;

        public Dlg_Engineer_ViewModel(MainWindow_ViewModel main)
        {
            ModuleCamellia = ((CAMELLIA_Handler)App.m_engineer.ClassHandler()).m_camellia;

            AxisX = ModuleCamellia.p_axisXY.p_axisX;
            AxisY = ModuleCamellia.p_axisXY.p_axisY;
            AxisZ = ModuleCamellia.p_axisZ;
            AxisLifter = ModuleCamellia.p_axisLifter;

            TiltAxisX = ModuleCamellia.p_tiltAxisXY.p_axisX;
            TiltAxisY = ModuleCamellia.p_tiltAxisXY.p_axisY;
            TiltAxisZ = ModuleCamellia.p_tiltAxisZ;

            ModuleCamellia.p_CamVRS.Grabed += OnGrabImageUpdate;

            p_rootViewer.p_VisibleMenu = Visibility.Collapsed;

            p_rootViewer.p_ImageData = ModuleCamellia.p_CamVRS.p_ImageViewer.p_ImageData;

            dispatcher = Application.Current.Dispatcher;
        }

        private void OnGrabImageUpdate(object sender, EventArgs e)
        {
            dispatcher.Invoke(() =>
            {
                p_rootViewer.SetImageSource();
            });

        }


        #region Motion Command
        public ICommand CmdAllHome
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Thread thread = new Thread(()=> 
                    {
                        EnableBtn = false;
                        ModuleCamellia.StateHome();
                        EnableBtn = true;
                    });
                    thread.Start();
                   //thread.Join();
                });
            }
        }
        public ICommand CmdMovePos
        {
            get
            {
                return new RelayCommand(() =>
                {
                    // 선택한 작업점 위치로 Move
                    Thread thread = new Thread(() =>
                    {
                        EnableBtn = false;
                        SelectedAxis.StartMove(CurrentAxisWorkPoints[SelectedWorkPointIndex].Value);
                        SelectedAxis.WaitReady();
                        EnableBtn = true;
                    });
                    thread.Start();
                });
            }
        }
        public ICommand CmdStop
        {
            get
            {
                return new RelayCommand(() =>
                {
                    
                    AxisX.StopAxis();
                    AxisY.StopAxis();
                    AxisZ.StopAxis();
                    AxisLifter.StopAxis();
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
                    // Regi 내보내기
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
                    Thread thread = new Thread(() =>
                    {
                        EnableBtn = false;
                        SelectedAxis.StartHome();
                        SelectedAxis.WaitReady();
                    });
                    thread.Start();
                    EnableBtn = true;
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
                    double pos = SelectedAxis.p_posActual;
                    CurrentAxisWorkPoints[SelectedWorkPointIndex].Value = pos;
                    SelectedAxis.m_aPos[SelectedAxis.m_asPos[SelectedWorkPointIndex]] = pos;
                    SelectedAxis.RunTree(RootTools.Trees.Tree.eMode.RegWrite);
                    SelectedAxis.RunTree(RootTools.Trees.Tree.eMode.Init);

                    CurrentAxisWorkPoints = GetWorkPoint(SelectedAxis.m_aPos);
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
                    // Regi 불러와서 실행
                });
            }
        }
        public ICommand CmdServoOnOff
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SelectedAxis.ServoOn(!AxisX.p_bServoOn);
                    SelectedAxis.ServoOn(!AxisY.p_bServoOn);
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
                    string str = SelectedAxis.Jog(-1);
                    if (str != "OK")
                    {
                        MessageBox.Show(str);
                    }
                });
            }
        }
        public ICommand CmdJogMinusNormal
        {
            get
            {
                return new RelayCommand(() =>
                {
                    string str = SelectedAxis.Jog(-0.31);
                    if (str != "OK")
                    {
                        MessageBox.Show(str);
                    }
                });
            }
        }
        public ICommand CmdRelMoveMinus
        {
            get
            {
                return new RelayCommand(() =>
                {

                    string str = SelectedAxis.StartMove(-PosValue);
                    if(str != "OK")
                    {
                        MessageBox.Show(str);
                    }
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
                    string str = SelectedAxis.StartMove(PosValue);
                    if (str != "OK")
                    {
                        MessageBox.Show(str);
                    }
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
                    string str = SelectedAxis.Jog(0.31);
                    if (str != "OK")
                    {
                        MessageBox.Show(str);
                    }

                });
            }
        }
        public ICommand CmdJogPlusFast
        {
            get
            {
                return new RelayCommand(() =>
                {
                    string str = SelectedAxis.Jog(1);
                    if (str != "OK")
                    {
                        MessageBox.Show(str);
                    }
                });
            }
        }
        public ICommand CmdCopyCurrentPos
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if(SelectedAxis == null)
                    {
                        return;
                    }
                    double value = SelectedAxis.p_posActual;
                    Clipboard.SetText(value.ToString());
                });
            }
        }
        #endregion

        #region SR Command
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


        #region General Command
        public ICommand CmdClose
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Run_Measure measure = (Run_Measure)ModuleCamellia.CloneModuleRun("Measure");
                    ModuleCamellia.mwvm.p_StageCenterPulse = measure.m_StageCenterPos_pulse;

                    CloseRequested(this, new DialogCloseRequestedEventArgs(true));
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
            Thread thread = new Thread(() =>
            {
                Run_Measure measure = (Run_Measure)ModuleCamellia.CloneModuleRun("Measure");
                measure.Run();
            });
            thread.Start();
        }
        private void Calibration()
        {
            EQ.p_bStop = false;
            if (ModuleCamellia.p_eState != ModuleBase.eState.Ready)
            {
                MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                return;
            }
            Thread thread = new Thread(() =>
            {
                Run_CalibrationWaferCentering calibration = (Run_CalibrationWaferCentering)ModuleCamellia.CloneModuleRun("CalibrationWaferCentering");
                calibration.m_useCal = true;
                calibration.m_useCentering = false;
                calibration.Run();
            });
            thread.Start();
        }
        private void InitCalibration()
        {
            EQ.p_bStop = false;
            if (ModuleCamellia.p_eState != ModuleBase.eState.Ready)
            {
                MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                return;
            }
            Thread thread = new Thread(() =>
            {
                Run_InitCalibration initCalibration = (Run_InitCalibration)ModuleCamellia.CloneModuleRun("InitCalWaferCentering");
                initCalibration.Run();
            });
            thread.Start();
        }
        private void Centering()
        {
            EQ.p_bStop = false;
            if (ModuleCamellia.p_eState != ModuleBase.eState.Ready)
            {
                MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                return;
            }
            Thread thread = new Thread(() =>
            {
                Run_CalibrationWaferCentering centering = (Run_CalibrationWaferCentering)ModuleCamellia.CloneModuleRun("CalibrationWaferCentering");
                centering.m_useCal = false;
                centering.m_useCentering = true;
                centering.Run();
            });
            thread.Start();
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
        #endregion

        #region Invoke Command
        public void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            int value = 0;
            if (Int32.TryParse((e.EditingElement as TextBox).Text, out value))
            {
                SelectedAxis.m_aPos[SelectedAxis.m_asPos[e.Row.GetIndex()]] = value;
                SelectedAxis.RunTree(RootTools.Trees.Tree.eMode.RegWrite);
                SelectedAxis.RunTree(RootTools.Trees.Tree.eMode.Init);
            }
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
            None,
            AxisX,
            AxisY,
            AxisZ,
            Lifter,
            TiltX,
            TiltY,
            TiltZ
        }
    }
}
