﻿using Root_Vega.Module;
using RootTools.Camera.Dalsa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Light;
using RootTools.Module;
using System.Windows;

namespace Root_Vega
{
    public class Optic_SideVisionViewModel : ObservableObject
    {
        Vega_Engineer m_Engineer;
        SideVision m_SideVision;
        public SideVision p_SideVision
        {
            get
            {
                return m_SideVision;
            }
            set
            {
                SetProperty(ref m_SideVision, value);
            }
        }

        Camera_Basler m_CamVRS;
        public Camera_Basler p_CamVRS
        {
            get
            {
                return m_CamVRS;
            }
            set
            {
                SetProperty(ref m_CamVRS, value);
            }
        }
        LightSet m_LightSet;

        public int p_LightSideCoax
        {
            get
            {
                return GetLightByName("Side Coax");
            }
            set
            {
                SetLightByName("Side Coax",value);
            }
        }
        public int p_LightSideSide
        {
            get
            {
                return GetLightByName("Side Side");
            }
            set
            {
                SetLightByName("Side Side", value);
            }
        }
        public int p_LightBevelCoax
        {
            get
            {
                return GetLightByName("Bevel Coax");
            }
            set
            {
                SetLightByName("Bevel Coax", value);
            }
        }   
        public int p_LightBevelSide
        {
            get
            {
                return GetLightByName("Bevel Side");
            }
            set
            {
                SetLightByName("Bevel Side", value);
            }
        }
        public int p_LightSideVRSCoax
        {
            get
            {
                return GetLightByName("SideVRS Coax");
            }
            set
            {
                SetLightByName("SideVRS Coax", value);
            }
        }
        public int p_LightSideVRSSide
        {
            get
            {
                return GetLightByName("SideVRS Side");
            }
            set
            {
                SetLightByName("SideVRS Side", value);
            }
        }
        public int p_LightBevelVRSCoax
        {
            get
            {
                return GetLightByName("BevelVRS Coax");
            }
            set
            {
                SetLightByName("BevelVRS Coax", value);
            }
        }
        public int p_LightBevelVRSSide
        {
            get
            {
                return GetLightByName("BevelVRS Side");
            }
            set
            {
                SetLightByName("BevelVRS Side", value);
            }
        }
        public int p_LightAlign1
        {
            get
            {
                return GetLightByName("Align1");
            }
            set
            {
                SetLightByName("Align1", value);
            }
        }
        public int p_LightAlign2
        {
            get
            {
                return GetLightByName("Align2");
            }
            set
            {
                SetLightByName("Align2", value);
            }
        }
        
        private readonly IDialogService m_DialogService;

        public Optic_SideVisionViewModel(Vega_Engineer engineer,  IDialogService dialogService)
        {
            m_Engineer = engineer;
            m_DialogService = dialogService;
            p_SideVision = ((Vega_Handler)engineer.ClassHandler()).m_sideVision;
            m_LightSet = p_SideVision.m_lightSet;
        }

        public int GetLightByName(string str)
        {
            for (int i = 0; i < m_LightSet.m_aLight.Count; i++)
            {
                if (m_LightSet.m_aLight[i].m_sName.IndexOf(str) >= 0)
                {
                    return Convert.ToInt32(m_LightSet.m_aLight[i].p_fPower);
                }
            }
            return 0;
        }

        public void SetLightByName(string str, int nValue)
        {
            for (int i = 0; i < m_LightSet.m_aLight.Count; i++)
            {
                if (m_LightSet.m_aLight[i].m_sName.IndexOf(str) >= 0)
                {
                    m_LightSet.m_aLight[i].m_light.p_fSetPower = nValue;
                }
            }
        }

        public void HomeSideVision()
        {
            EQ.p_bStop = false;
            m_SideVision.p_eState = ModuleBase.eState.Home;
            //if (m_SideVision.m_CamMain.p_CamInfo.p_eState == eCamState.Init)
            //    m_SideVision.m_CamMain.Connect();
        }

        public void Stop()
        {
            EQ.p_bStop = true;
        }

        public void Scan()
        {
            EQ.p_bStop = false;
            SideVision Sidevision = ((Vega_Handler)m_Engineer.ClassHandler()).m_sideVision;
            //if (Sidevision.p_eState != ModuleBase.eState.Ready)
            //{
            //    MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
            //    return;
            //}
            SideVision.Run_SideGrab Grab = (SideVision.Run_SideGrab)Sidevision.CloneModuleRun("SideGrab");
            var viewModel = new Dialog_SideScan_ViewModel(Sidevision, Grab);
            Nullable<bool> result = m_DialogService.ShowDialog(viewModel);
            if (result.HasValue)
            {
                if (result.Value)
                {
                    //p_MainCamera = Grab.m_grabMode.m_camera;
                    Sidevision.StartRun(Grab);
                }
                else
                {
                    // Cancelled
                }
            }
        }

        #region RelayCommand
        public RelayCommand CommandScan
        {
            get
            {
                return new RelayCommand(Scan);
            }
        }
        
        public RelayCommand CommandStop
        {
            get
            {
                return new RelayCommand(Stop);
            }
        }

        public RelayCommand CommandHome
        {
            get
            {
                return new RelayCommand(HomeSideVision);
            }
        }
        #endregion
    }
}