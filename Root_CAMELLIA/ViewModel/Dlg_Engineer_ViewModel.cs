using Root_CAMELLIA.Module;
using RootTools;
using RootTools.Control;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        public int SelectedIndex
        {
            get
            {
                return _SelectedIndex;
            }
            set
            {
                eTabAxis = (TabAxis)value;
                switch (eTabAxis)
                {
                    case TabAxis.AxisX:
                        {
                            SelectedAxis = m_AxisXY.p_axisX;
                            WorkPoint = m_AxisXY.p_axisX.m_aPos;
                            return;
                        }
                    case TabAxis.AxisY:
                        {
                            SelectedAxis = m_AxisXY.p_axisY;
                            WorkPoint = m_AxisXY.p_axisY.m_aPos;
                            return;
                        }
                    case TabAxis.AxisZ:
                        {
                            SelectedAxis = m_AxisZ;
                            WorkPoint = m_AxisZ.m_aPos;
                            return;
                        }
                }
                SetProperty(ref _SelectedIndex, value);
            }
        }
        private int _SelectedIndex = 0;

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

        public Dictionary<string, double> WorkPoint
        {
            get
            {
                return _WorkPoint;
            }
            set
            {
                SetProperty(ref _WorkPoint, value);
            }
        }
        private Dictionary<string, double> _WorkPoint;


 
        public ObservableCollection<WorkPoints> WorkPoint2
        {
            get
            {
                return _WorkPoint2;
            }
            set
            {
                SetProperty(ref _WorkPoint2, value);
            }
        }
        private ObservableCollection<WorkPoints> _WorkPoint2;
        private ObservableCollection<WorkPoints> GetWorkPoints(Dictionary<string, double> dic)
        {
            ObservableCollection<WorkPoints> data = new ObservableCollection<WorkPoints>();
            for (int i = 0; i < dic.Count; i++)
            {
                WorkPoints wp = new WorkPoints();
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
            WorkPoint = m_AxisXY.p_axisX.m_aPos;
            WorkPoint2 = GetWorkPoints(m_AxisXY.p_axisX.m_aPos);
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
        #endregion

        public class WorkPoints
        {
            public string Name
            {
                get;
                set;
            }
            public double Value
            {
                get;
                set;
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
