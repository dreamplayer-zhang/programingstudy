using RootTools;
using RootTools.RADS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using RootTools.Trees;
using RootTools.Camera.BaslerPylon;
using Root_Vega.Module;
using System.Threading;

namespace Root_Vega.Setting
{
    class Setting_RADSViewModel : ObservableObject
    {
        Vega_Engineer m_Engineer;

        Camera_Basler m_CamRADS;
        public Camera_Basler p_CamRADS
        {
            get
            {
                return m_CamRADS;
            }
            set
            {
                SetProperty(ref m_CamRADS, value);
            }
        }


        PatternVision m_PatternVision;
        public PatternVision p_PatternVision
        {
            get
            {
                return m_PatternVision;
            }
            set
            {
                SetProperty(ref m_PatternVision, value);
            }
        }

        RADSControl m_timerControl;
        public RADSControl p_timerControl
        {
            get
            {
                return m_timerControl;
            }
            set
            {
                SetProperty(ref m_timerControl, value);
            }
        }
        
        public Setting_RADSViewModel(Vega_Engineer engineer)
        {
            p_strLabel = "NULL";
            m_Engineer = engineer;
            p_PatternVision = ((Vega_Handler)engineer.ClassHandler()).m_patternVision;
            p_timerControl = p_PatternVision.m_RADSControl;
            //p_timerControl.SearchComplete += SetEvent;
            p_CamRADS = p_PatternVision.m_CamRADS;

            //p_timerControl.UpdateDeviceInfo();
        }

        string m_strLabel;
        public string p_strLabel
        {
            get
            {
                return m_strLabel;
            }
            set
            {
                SetProperty(ref m_strLabel, value);
            }
        }

        private void SetEvent()
        {
            if (p_timerControl != null)
            {
                var input = string.Format("Device Info :\n\nName:{0}\nMAC:{1}\nIP:{2}", m_timerControl.ControllerName, m_timerControl.ControllerMacAddress, m_timerControl.ControllerIP.ToString());
                p_strLabel = input;
            }
        }

        void GetDeviceInfo()
        {
            p_timerControl.UpdateDeviceInfo();
        }

        public RelayCommand GetDeviceInfoCommand
        {
            get
            {
                return new RelayCommand(GetDeviceInfo);
            }
        }

        void SetResetContollerPacket()
        {
            p_timerControl.ResetController();
        }

        public RelayCommand SetResetContollerPacketCommand
        {
            get
            {
                return new RelayCommand(SetResetContollerPacket);
            }
        }

        void StartADS()
        {
            p_timerControl.StartRADS();

            return;
        }

        public RelayCommand StartADSCommand
        {
            get
            {
                return new RelayCommand(StartADS);
            }
        }

        void StopADS()
        {
            p_timerControl.StopRADS();
            return;
        }

        public RelayCommand StopADSCommand
        {
            get
            {
                return new RelayCommand(StopADS);
            }
        }
    }
}
