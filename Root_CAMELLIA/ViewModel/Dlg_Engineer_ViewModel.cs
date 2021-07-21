using Root_CAMELLIA.Data;
using Root_CAMELLIA.Draw;
using Root_CAMELLIA.Module;
using Root_CAMELLIA.ShapeDraw;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using static RootTools.Control.Axis;

namespace Root_CAMELLIA
{
    public class Dlg_Engineer_ViewModel : ObservableObject, IDialogRequestClose
    {
        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;

        #region Property
        public int TabIndex
        {
            get
            {
                return _TabIndex;
            }
            set
            {
                if(value == 0)
                {
                    Run_Measure measure = (Run_Measure)ModuleCamellia.CloneModuleRun("Measure");
                    StageCenterPos = measure.m_StageCenterPos_pulse;
                }
                SetProperty(ref _TabIndex, value);
            }
        }
        int _TabIndex = 0;

        public RPoint StageCenterPos
        {
            get
            {
                return _StageCenterPos;
            }
            set
            {
                SetProperty(ref _StageCenterPos, value);
            }
        }

        RPoint _StageCenterPos = new RPoint();

        public bool p_IsShowPoint
        {
            get
            {
                return m_IsShowPoint;
            }
            set
            {
                if (value)
                {
                    for(int i = 0; i < listSelectedPoint.Count; i++)
                    {
                        listSelectedPoint[i].CanvasEllipse.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    for (int i = 0; i < listSelectedPoint.Count; i++)
                    {
                        listSelectedPoint[i].CanvasEllipse.Visibility = Visibility.Hidden;
                    }
                }
                SetProperty(ref m_IsShowPoint, value);
            }
        }
        bool m_IsShowPoint = true;
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
                    case TabAxis.StageZ:
                        {
                            SelectedAxis = StageAxisZ;
                            CurrentAxisWorkPoints = GetWorkPoint(StageAxisZ.m_aPos);
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
        private bool _EnableBtn = true;

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

        public Axis StageAxisZ
        {
            get
            {
                return _StageAxisZ;
            }
            set
            {
                SetProperty(ref _StageAxisZ, value);
            }
        }
        private Axis _StageAxisZ;

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
            get
            {
                return m_rootViewer;
            }
            set
            {
                SetProperty(ref m_rootViewer, value);
            }
        }

        Dispatcher dispatcher = null;

        double m_alpha = 0.0;
        public double p_alpha
        {
            get
            {
                return m_alpha;
            }
            set
            {
                SetProperty(ref m_alpha, value);
            }
        }

        ObservableCollection<UIElement> m_measurePoint = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> p_measurePoint
        {
            get
            {
                return m_measurePoint;
            }
            set
            {
                SetProperty(ref m_measurePoint, value);
            }
        }

        ObservableCollection<UIElement> m_selectedPoint = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> p_selectedPoint
        {
            get
            {
                return m_selectedPoint;
            }
            set
            {
                SetProperty(ref m_selectedPoint, value);
            }
        }


        private List<ShapeEllipse> listCandidatePoint = new List<ShapeEllipse>();
        private List<ShapeEllipse> listSelectedPoint = new List<ShapeEllipse>();
        List<CCircle> listRealPos = new List<CCircle>();
        public void SetMovePoint(RecipeData rd)
        {
            double RatioX = 1000 / BaseDefine.ViewSize;
            double RatioY = 1000 / BaseDefine.ViewSize;
            int CenterX = (int)(1000 * 0.5f);
            int CenterY = (int)(1000 * 0.5f);
            DrawGeometryManager drawGeometryManager = new DrawGeometryManager();
            ObservableCollection<UIElement> temp = new ObservableCollection<UIElement>();
            listCandidatePoint.Clear();
            listSelectedPoint.Clear();
            for (int i = 0; i < rd.DataCandidatePoint.Count; i++)
            {
                ShapeManager dataPoint = new ShapeEllipse(GeneralTools.StageHoleBrush);
                ShapeEllipse dataCandidatePoint = dataPoint as ShapeEllipse;

                CCircle circle = new CCircle(rd.DataCandidatePoint[i].x, rd.DataCandidatePoint[i].y, rd.DataCandidatePoint[i].width + 4,
                    rd.DataCandidatePoint[i].height + 4, rd.DataCandidatePoint[i].MeasurementOffsetX, rd.DataCandidatePoint[i].MeasurementOffsetY);
                circle.Transform(RatioX, RatioY);

                Circle c = drawGeometryManager.GetRect(circle, CenterX, CenterY);
                dataCandidatePoint.SetData(c, (int)(circle.width), (int)(circle.height), 95);
                temp.Add(dataCandidatePoint.UIElement);
                listCandidatePoint.Add(dataCandidatePoint);
                listRealPos = new List<CCircle>(rd.DataCandidatePoint.ToArray());
            }

            p_measurePoint = temp;

            temp = new ObservableCollection<UIElement>();
            for (int i = 0; i < rd.DataSelectedPoint.Count; i++)
            {
                ShapeManager dataPoint = new ShapeEllipse(GeneralTools.GbHole);
                ShapeEllipse dataSelectedPoint = dataPoint as ShapeEllipse;

                CCircle circle = new CCircle(rd.DataSelectedPoint[i].x, rd.DataSelectedPoint[i].y, rd.DataSelectedPoint[i].width + 4,
                  rd.DataSelectedPoint[i].height + 4, rd.DataSelectedPoint[i].MeasurementOffsetX, rd.DataSelectedPoint[i].MeasurementOffsetY);
                circle.Transform(RatioX, RatioY);

                Circle c = drawGeometryManager.GetRect(circle, CenterX, CenterY);
                dataSelectedPoint.SetData(c, (int)(circle.width), (int)(circle.height), 95);
                temp.Add(dataSelectedPoint.UIElement);
                listSelectedPoint.Add(dataSelectedPoint);
            }

            p_selectedPoint = temp;
        }

        PM_SR_Parameter m_pmParameter = new PM_SR_Parameter();
        public PM_SR_Parameter p_pmParameter
        {
            get
            {
                return m_pmParameter;
            }
            set
            {
                SetProperty(ref m_pmParameter, value);
            }
        }

        PMCheckReview_ViewModel m_PMCheckReview_ViewModel = new PMCheckReview_ViewModel();
        public PMCheckReview_ViewModel p_PMCheckReview_ViewModel
        {
            get
            {
                return m_PMCheckReview_ViewModel;
            }
            set
            {
                SetProperty(ref m_PMCheckReview_ViewModel, value);
            }
        }



        #endregion
        public Dlg_Engineer_ViewModel(MainWindow_ViewModel main)
        {
            ModuleCamellia = ((CAMELLIA_Handler)App.m_engineer.ClassHandler()).m_camellia;

            // 연결 콜벡 이벤트 추가 += Connected_Callback;
            ModuleCamellia.p_CamVRS.Connected += Connected_Callback;

            //p_recipeData = DataManager.Instance.recipeDM.MeasurementRD;
            AxisX = ModuleCamellia.p_axisXY.p_axisX;
            AxisY = ModuleCamellia.p_axisXY.p_axisY;
            AxisZ = ModuleCamellia.p_axisZ;
            AxisLifter = ModuleCamellia.p_axisLifter;

            TiltAxisX = ModuleCamellia.p_tiltAxisXY.p_axisX;
            TiltAxisY = ModuleCamellia.p_tiltAxisXY.p_axisY;
            StageAxisZ = ModuleCamellia.p_stageAxisZ;

            ModuleCamellia.p_CamVRS.Grabed += OnGrabImageUpdate;

            p_rootViewer.p_VisibleMenu = Visibility.Collapsed;
            dispatcher = Application.Current.Dispatcher;

            //p_PMCheckReview_ViewModel.p_rootViewer.p_ImageData = ModuleCamellia.p_CamVRS.p_ImageViewer.p_ImageData;
            //m_task = new Task(()=>RunTask());
            //m_task.Start();
        }

        private void Connected_Callback(object sender, EventArgs e)
        {
            //ImageData sourceData = ModuleCamellia.p_CamVRS.p_ImageViewer.p_ImageData;

            //IntPtr ptr0 = sourceData.GetPtr();
            ////byte[] ptr1 = sourceData.GetByteArray();
            ////IntPtr ptr2 = sourceData.GetPtr(1);
            ////IntPtr ptr3 = sourceData.GetPtr(2);

            //byte[] buf = new byte[sourceData.p_Size.X];

            ////Array.Clear(ptr1, 0, sourceData.p_Size.X * sourceData.p_Size.Y);
            //for (int i = 0; i < sourceData.p_Size.Y; i++)
            //{

            //    Marshal.Copy(buf, 0, ptr0, sourceData.p_Size.X);


            //    ptr0 += sourceData.p_Size.X * 3;
            //}

            p_rootViewer.p_ImageData = ModuleCamellia.p_CamVRS.p_ImageViewer.p_ImageData;
            //ModuleCamellia.p_CamVRS.p_ImageViewer.SetImageSource();


        }

        //private bool m_isStart = false;
        //void RunTask()
        //{
        //    m_isStart = true;
        //    while (m_isStart)
        //    {
        //        Thread.Sleep(1);
        //    }
        //}

        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                CloseWindow();
            }
        }

        public void OnMouseLeave(object sender, MouseEventArgs e)
        {
            foreach (ShapeEllipse se in listCandidatePoint)
            {
                se.SetBrush(GeneralTools.StageHoleBrush);
            }
            foreach (ShapeEllipse se in listSelectedPoint)
            {
                se.SetBrush(GeneralTools.GbHole);
            }
            nMinIndex = -1;
        }

        public void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (listRealPos.Count <= 0)
            {
                return;
            }

            if (ModuleCamellia.p_eState != ModuleBase.eState.Ready)
            {
                MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                EnableBtn = true;
                return;
            }

            if (!EnableBtn)
            {
                return;
            }

            nSelectIndex = nMinIndex;
            double centerX;
            double centerY;
            if (DataManager.Instance.m_waferCentering.m_ptCenter.X == 0 && DataManager.Instance.m_waferCentering.m_ptCenter.Y == 0)
            {
                centerX = StageCenterPos.X;
                centerY = StageCenterPos.Y;
            }
            else // 나중에 centering 값 추가 테스트 진행 예정
            {
                centerX = DataManager.Instance.m_waferCentering.m_ptCenter.X;
                centerY = DataManager.Instance.m_waferCentering.m_ptCenter.Y;
            }

            double x = listRealPos[nMinIndex].x;
            double y = listRealPos[nMinIndex].y;
            double dX = centerX - x * 10000;
            double dY = centerY - y * 10000;
            Thread thread = new Thread(() =>
            {
                EnableBtn = false;
                string str;
                str = ModuleCamellia.p_axisXY.StartMove(new RPoint(dX, dY));
                if (str != "OK")
                {
                    MessageBox.Show(str);
                    return;
                }
                ModuleCamellia.p_axisXY.WaitReady();
                EnableBtn = true;
            });
            thread.Start();
            //MessageBox.Show(listRealPos[nMinIndex].x.ToString() + " " + listRealPos[nMinIndex].y.ToString());
        }


        int nMinIndex = -1;
        int nSelectIndex = -1;
        public void OnMouseMove(object sender, MouseEventArgs e)
        {
            Point pt = e.GetPosition((UIElement)sender);

            double dMin = 9999;
            int nIndex = 0;
            

            foreach (ShapeEllipse se in listCandidatePoint)
            {
                double dDistance = GetDistance(se, new System.Windows.Point(pt.X, pt.Y));

                if (dDistance < dMin)
                {
                    dMin = dDistance;
                    nMinIndex = nIndex;
                }
                nIndex++;
            }

            foreach (ShapeEllipse se in listCandidatePoint)
            {
                if (se.Equals(listCandidatePoint[nMinIndex]))
                {
                    //bSelected = true;
                    se.SetBrush(GeneralTools.GbSelect);
                }
                else
                {
                    se.SetBrush(GeneralTools.StageHoleBrush);
                }
            }

            nIndex = 0;

            foreach (ShapeEllipse se in listSelectedPoint)
            {
                if (se.CenterX == listCandidatePoint[nMinIndex].CenterX && se.CenterY == listCandidatePoint[nMinIndex].CenterY)
                {
                    se.SetBrush(GeneralTools.SelectedOverBrush);
                }
                else
                {
                    se.SetBrush(GeneralTools.GbHole);
                }
            }
        }

        private double GetDistance(ShapeEllipse eg, Point pt)
        {
            double dResult = Math.Sqrt(Math.Pow(eg.CenterX - pt.X, 2) + Math.Pow(eg.CenterY - pt.Y, 2));

            return Math.Round(dResult, 3);
        }

        public void GrabStop()
        {
            if (ModuleCamellia.p_CamVRS.p_CamInfo._IsGrabbing)
                ModuleCamellia.p_CamVRS.GrabStop();
        }

        #region Cameara Command
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
                    GrabStop();
                });
            }
        }
        #endregion

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
                    if (!CheckAxis())
                    {
                        return;
                    }
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
                    SelectedAxis.StopAxis();
                    //AxisX.StopAxis();
                    //AxisY.StopAxis();
                    //AxisZ.StopAxis();
                    //AxisLifter.StopAxis();
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
                    if (!CheckAxis())
                    {
                        return;
                    }
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
                    if (!CheckAxis())
                    {
                        return;
                    }
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
                    if (!CheckAxis())
                    {
                        return;
                    }
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
                    if (!CheckAxis())
                    {
                        return;
                    }
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
                    if (!CheckAxis())
                    {
                        return;
                    }
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
                    if (!CheckAxis())
                    {
                        return;
                    }
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
                    if (!CheckAxis())
                    {
                        return;
                    }
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
                    if (!CheckAxis())
                    {
                        return;
                    }
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
                    if (!CheckAxis())
                    {
                        return;
                    }
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
                    if (!CheckAxis())
                    {
                        return;
                    }
                    double value = SelectedAxis.p_posActual;
                    Clipboard.SetText(value.ToString());
                    MessageBox.Show("Copied Value!");
                });
            }
        }
        #endregion

        private bool CheckAxis()
        {
            if(SelectedAxis == null)
            {
                MessageBox.Show("Please Select Axis", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

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

        public ICommand CmdPointMeasure
        {
            get
            {
                return new RelayCommand(() =>
                {
                    PointMeasureSample();
                });
            }
        }
        #endregion


        #region General Command

        public ICommand CmdTest
        {
            get
            {
                return new RelayCommand(() =>
                {
                    //if (!ModuleCamellia.p_CamVRS.p_CamInfo.OpenStatus)
                    //{
                    //    ModuleCamellia.p_CamVRS.Connect();
                    //}
                    //while (!ModuleCamellia.p_CamVRS.m_ConnectDone) ;

                    //// if(연결되있을 경우만)
                    //p_rootViewer.p_ImageData = ModuleCamellia.p_CamVRS.p_ImageViewer.p_ImageData;
                    Thread thread = new Thread(() =>
                    {
                        Run_Delay delay = (Run_Delay)ModuleCamellia.CloneModuleRun("Delay");
                        ModuleCamellia.StartRun(delay);
                    });
                    thread.Start();
                    //StageCenterPos = measure.m_StageCenterPos_pulse;
                });
            }
        }

        public ICommand LoadedCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    //if (!ModuleCamellia.p_CamVRS.p_CamInfo.OpenStatus)
                    //{
                    //    ModuleCamellia.p_CamVRS.Connect();
                    //}
                    //while (!ModuleCamellia.p_CamVRS.m_ConnectDone) ;

                    //// if(연결되있을 경우만)
                    //p_rootViewer.p_ImageData = ModuleCamellia.p_CamVRS.p_ImageViewer.p_ImageData;
                    RaisePropertyChanged("p_pmParameter");

                    Run_Measure measure = (Run_Measure)ModuleCamellia.CloneModuleRun("Measure");
                    StageCenterPos = measure.m_StageCenterPos_pulse;
                });
            }
        }

        public ICommand UnloadedCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    GrabStop();
                });
            }
        }

        public ICommand CmdClose
        {
            get
            {
                return new RelayCommand(() =>
                {
                    CloseWindow();
                });
            }
        }

        public ICommand CmdPM
        {
            get
            {
                return new RelayCommand(() =>
                {

                });
            }
        }

        public ICommand CmdInit
        {
            get
            {
                return new RelayCommand(() =>
                {
                    UpdateParameter();
                });
            }
        }
        #endregion

        #region Function

        void CloseWindow()
        {
            Run_Measure measure = (Run_Measure)ModuleCamellia.CloneModuleRun("Measure");
            ModuleCamellia.mwvm.p_StageCenterPulse = measure.m_StageCenterPos_pulse;

            CloseRequested(this, new DialogCloseRequestedEventArgs(true));
        }

        public void UpdateParameter()
        {
            LibSR_Met.DataManager dataManager = LibSR_Met.DataManager.GetInstance();
            dataManager.m_SettngData.nAverage_NIR = p_pmParameter.p_NIRAverage;
            dataManager.m_SettngData.nAverage_VIS = p_pmParameter.p_VISAverage;
            dataManager.m_SettngData.nInitCalIntTime_NIR = p_pmParameter.p_InitNIRIntegrationTime;
            dataManager.m_SettngData.nInitCalIntTime_VIS = p_pmParameter.p_InitVISIntegrationTime;
            dataManager.m_SettngData.nBoxcar_NIR = p_pmParameter.p_NIRBoxcar;
            dataManager.m_SettngData.nBoxcar_VIS = p_pmParameter.p_VISBoxcar;
            dataManager.m_SettngData.nBGIntTime_NIR = p_pmParameter.p_BGNIRIntegrationTime;
            dataManager.m_SettngData.nBGIntTime_VIS = p_pmParameter.p_BGVISIntegrationTime;
            dataManager.m_SettngData.nMeasureIntTime_NIR = p_pmParameter.p_NIRIntegrationTime;
            dataManager.m_SettngData.nMeasureIntTime_VIS = p_pmParameter.p_VISIntegrationTime;
            dataManager.m_SettngData.dAlphaFit = p_pmParameter.p_alpha1;
        }
        private void OnGrabImageUpdate(object sender, EventArgs e)
        {
            dispatcher.Invoke(() =>
            {
                p_rootViewer.SetImageSource();
                
                //p_rootViewer.p_ImageData.SaveImageSync(@"C:\Users\cgkim\Desktop\image\test.bmp");
            });

        }

        private void PointMeasureSample()
        {
            if (!EnableBtn)
            {
                return;
            }
            EQ.p_bStop = false;
            if (ModuleCamellia.p_eState != ModuleBase.eState.Ready)
            {
                MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                return;
            }

            if(nSelectIndex == -1)
            {
                MessageBox.Show("Measure Point가 선택되지 않았습니다.");
                return;
            }
            Run_Measure measure = (Run_Measure)ModuleCamellia.CloneModuleRun("Measure");
            UpdateParameter();
            measure.m_isPM = true;
            measure.m_isAlphaFit = p_pmParameter.p_isAlpha1;
            measure.m_isPointMeasure = true;
            measure.m_ptMeasure = new RPoint(listRealPos[nSelectIndex].x, listRealPos[nSelectIndex].y);
            ModuleCamellia.StartRun(measure);
        }
        private void MeasureSample()
        {
            if (!EnableBtn)
            {
                return;
            }
            EQ.p_bStop = false;
            if (ModuleCamellia.p_eState != ModuleBase.eState.Ready)
            {
                MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                return;
            }
            //Thread thread = new Thread(() =>
            //{
                Run_Measure measure = (Run_Measure)ModuleCamellia.CloneModuleRun("Measure");
                UpdateParameter();
                measure.m_isPM = true;
                measure.m_isAlphaFit = p_pmParameter.p_isAlpha1;
                ModuleCamellia.StartRun(measure);
                //measure.Run();
            //});
            //thread.Start();
        }
        private void Calibration()
        {
            if (!EnableBtn)
            {
                return;
            }
            EQ.p_bStop = false;
            if (ModuleCamellia.p_eState != ModuleBase.eState.Ready)
            {
                MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                return;
            }
            Thread thread = new Thread(() =>
            {
                Run_CalibrationWaferCentering calibration = (Run_CalibrationWaferCentering)ModuleCamellia.CloneModuleRun("CalibrationWaferCentering");
                UpdateParameter();
                calibration.m_isPM = true;
                calibration.m_useCal = true;
                calibration.m_useCentering = false;
                //calibration.Run();
                ModuleCamellia.StartRun(calibration);
            });
            thread.Start();
        }
        private void InitCalibration()
        {
            if (!EnableBtn)
            {
                return;
            }
            EQ.p_bStop = false;
            if (ModuleCamellia.p_eState != ModuleBase.eState.Ready)
            {
                MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                return;
            }
            Thread thread = new Thread(() =>
            {
                Run_InitCalibration initCalibration = (Run_InitCalibration)ModuleCamellia.CloneModuleRun("InitCalibration");
                initCalibration.m_isPM = true;
                UpdateParameter();
                //initCalibration.Run();
                ModuleCamellia.StartRun(initCalibration);
            });
            thread.Start();
        }
        private void Centering()
        {
            if (!EnableBtn)
            {
                return;
            }
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
                //centering.Run();
                ModuleCamellia.StartRun(centering);
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

        public class PM_SR_Parameter : ObservableObject
        {
            #region Property

            double m_alpha1 = 1.0;
            public double p_alpha1
            {
                get
                {
                    return m_alpha1;
                }
                set
                {
                    SetProperty(ref m_alpha1, value);
                }
            }
            public bool p_isAlpha1 { get; set; } = false;
            public int p_VISIntegrationTime { get; set; } = 25;
            public int p_NIRIntegrationTime { get; set; } = 150;
            public int p_InitVISIntegrationTime { get; set; } = 25;
            public int p_InitNIRIntegrationTime { get; set; } = 150;
            public int p_BGVISIntegrationTime { get; set; } = 50;
            public int p_BGNIRIntegrationTime { get; set; } = 150;
            public int p_VISBoxcar { get; set; } = 2;
            public int p_NIRBoxcar { get; set; } = 2;
            public int p_VISAverage { get; set; } = 3;
            public int p_NIRAverage { get; set; } = 3;
            public int p_repeat { get; set; } = 1;
            public int p_delay { get; set; } = 0;
            #endregion

            private void LoadSRParameter()
            {
                (LibSR_Met.SettingData, LibSR_Met.Nanoview.ERRORCODE_NANOVIEW) m_SettingDataWithErrorCode = App.m_nanoView.LoadSettingParameters();
                if (m_SettingDataWithErrorCode.Item2 == LibSR_Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                {
                    p_NIRAverage = m_SettingDataWithErrorCode.Item1.nAverage_NIR;
                    p_VISAverage = m_SettingDataWithErrorCode.Item1.nAverage_VIS;
                    p_InitVISIntegrationTime = m_SettingDataWithErrorCode.Item1.nInitCalIntTime_VIS;
                    p_InitNIRIntegrationTime = m_SettingDataWithErrorCode.Item1.nInitCalIntTime_NIR;
                    p_BGNIRIntegrationTime = m_SettingDataWithErrorCode.Item1.nBGIntTime_NIR;
                    p_BGVISIntegrationTime = m_SettingDataWithErrorCode.Item1.nBGIntTime_VIS;
                    p_VISBoxcar = m_SettingDataWithErrorCode.Item1.nBoxcar_VIS;
                    p_NIRBoxcar = m_SettingDataWithErrorCode.Item1.nBoxcar_NIR;

                }
            }

            public void SetRecipeData(RecipeData rd)
            {
                p_VISIntegrationTime = rd.VISIntegrationTime;
                p_NIRIntegrationTime = rd.NIRIntegrationTime;

                LoadSRParameter();
            }
        }
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
            StageZ
        }
    }
}
