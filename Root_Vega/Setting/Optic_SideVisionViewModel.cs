using Root_Vega.Module;
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
using RootTools.Control.Ajin;
using RootTools.Control;
using System.Threading;
using System.ComponentModel;
using RootTools.Camera;
using RootTools.Memory;
using System.Windows.Media.Media3D;

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

        double m_dStageCanvasWidth;
        public double p_dStageCanvasWidth
        {
            get
            {
                return m_dStageCanvasWidth;
            }
            set
            {
                SetProperty(ref m_dStageCanvasWidth, value);
            }
        }

        double m_dStageCanvasHeight;
        public double p_dStageCanvasHeight
        {
            get
            {
                return m_dStageCanvasHeight;
            }
            set
            {
                SetProperty(ref m_dStageCanvasHeight, value);
            }
        }

        double m_dZCanvasWidth;
        public double p_dZCanvasWidth
        {
            get
            {
                return m_dZCanvasWidth;
            }
            set
            {
                SetProperty(ref m_dZCanvasWidth, value);
            }
        }

        double m_dZCanvasHeight;
        public double p_dZCanvasHeight
        {
            get
            {
                return m_dZCanvasHeight;
            }
            set
            {
                SetProperty(ref m_dZCanvasHeight, value);
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
            SideVision.Run_BevelGrab BevelGrab = (SideVision.Run_BevelGrab)Sidevision.CloneModuleRun("BevelGrab");
            var viewModel = new Dialog_SideScan_ViewModel(Sidevision, Grab, BevelGrab);
            Nullable<bool> result = m_DialogService.ShowDialog(viewModel);
            if (result.HasValue)
            {
                if (result.Value)
                {
                    if (Grab.m_grabMode == null && BevelGrab.m_grabMode == null)
                    {
                        CameraSet cameraSet = p_SideVision.m_cameraSet;
                        MemoryPool memoryPool = m_Engineer.ClassMemoryTool().GetPool("SideVision.Memory", false);

                        // SideGrab
                        SideVision.Run_SideGrab GrabSideBottom = (SideVision.Run_SideGrab)Sidevision.CloneModuleRun("SideGrab");
                        GrabMode grabModeSideBottom = new GrabMode("FullScan", cameraSet, m_LightSet, memoryPool);
                        SideVision.Run_SideGrab GrabSideLeft = (SideVision.Run_SideGrab)Sidevision.CloneModuleRun("SideGrab");
                        GrabMode grabModeSideLeft = new GrabMode("FullScan", cameraSet, m_LightSet, memoryPool);
                        SideVision.Run_SideGrab GrabSideTop = (SideVision.Run_SideGrab)Sidevision.CloneModuleRun("SideGrab");
                        GrabMode grabModeSideTop = new GrabMode("FullScan", cameraSet, m_LightSet, memoryPool);
                        SideVision.Run_SideGrab GrabSideRight = (SideVision.Run_SideGrab)Sidevision.CloneModuleRun("SideGrab");
                        GrabMode grabModeSideRight = new GrabMode("FullScan", cameraSet, m_LightSet, memoryPool);

                        GrabMode grabModeTemp = null;
                        for (int i = 0; i<4; i++)
                        {
                            switch(i)
                            {
                                case (int)eScanPos.Bottom:
                                    grabModeTemp = grabModeSideBottom;
                                    grabModeTemp.m_eScanPos = eScanPos.Bottom;
                                    break;
                                case (int)eScanPos.Left:
                                    grabModeTemp = grabModeSideLeft;
                                    grabModeTemp.m_eScanPos = eScanPos.Left;
                                    break;
                                case (int)eScanPos.Right:
                                    grabModeTemp = grabModeSideRight;
                                    grabModeTemp.m_eScanPos = eScanPos.Right;
                                    break;
                                case (int)eScanPos.Top:
                                    grabModeTemp = grabModeSideTop;
                                    grabModeTemp.m_eScanPos = eScanPos.Top;
                                    break;
                                default:
                                    return;
                                    break;
                            }
                            grabModeTemp.m_memoryGroup = memoryPool.GetGroup("Grab");
                            grabModeTemp.m_ScanStartLine = 0;
                            grabModeTemp.m_ScanLineNum = 3;
                            grabModeTemp.m_camera = p_SideVision.p_CamSide;

                        }

                        //SideVision.Run_SideGrab GrabSideBottom = (SideVision.Run_SideGrab)Sidevision.CloneModuleRun("SideGrab");
                        //GrabMode grabModeSideBottom = new GrabMode("FullScan", cameraSet, m_LightSet, memoryPool);
                        grabModeSideBottom.m_memoryGroup = memoryPool.GetGroup("Grab");
                        grabModeSideBottom.m_ScanStartLine = 0;
                        grabModeSideBottom.m_ScanLineNum = 3;
                        grabModeSideBottom.m_camera = p_SideVision.p_CamSide;
                        grabModeSideBottom.m_eScanPos = eScanPos.Bottom;
                        GrabSideBottom.m_grabMode = grabModeSideBottom;
                        Sidevision.StartRun(GrabSideBottom);

                        //SideVision.Run_SideGrab GrabSideLeft = (SideVision.Run_SideGrab)Sidevision.CloneModuleRun("SideGrab");
                        //GrabMode grabModeSideLeft = new GrabMode("FullScan", cameraSet, m_LightSet, memoryPool);
                        grabModeSideLeft.m_memoryGroup = memoryPool.GetGroup("Grab");
                        grabModeSideLeft.m_ScanStartLine = 0;
                        grabModeSideLeft.m_ScanLineNum = 3;
                        grabModeSideLeft.m_camera = p_SideVision.p_CamSide;
                        grabModeSideLeft.m_eScanPos = eScanPos.Left;
                        GrabSideLeft.m_grabMode = grabModeSideLeft;
                        Sidevision.StartRun(GrabSideLeft);

                        //SideVision.Run_SideGrab GrabSideTop = (SideVision.Run_SideGrab)Sidevision.CloneModuleRun("SideGrab");
                        //GrabMode grabModeSideTop = new GrabMode("FullScan", cameraSet, m_LightSet, memoryPool);
                        grabModeSideTop.m_memoryGroup = memoryPool.GetGroup("Grab");
                        grabModeSideTop.m_ScanStartLine = 0;
                        grabModeSideTop.m_ScanLineNum = 3;
                        grabModeSideTop.m_camera = p_SideVision.p_CamSide;
                        grabModeSideTop.m_eScanPos = eScanPos.Top;
                        GrabSideTop.m_grabMode = grabModeSideTop;
                        Sidevision.StartRun(GrabSideTop);


                        //SideVision.Run_SideGrab GrabSideRight = (SideVision.Run_SideGrab)Sidevision.CloneModuleRun("SideGrab");
                        //GrabMode grabModeSideRight = new GrabMode("FullScan", cameraSet, m_LightSet, memoryPool);
                        grabModeSideRight.m_memoryGroup = memoryPool.GetGroup("Grab");
                        grabModeSideRight.m_ScanStartLine = 0;
                        grabModeSideRight.m_ScanLineNum = 3;
                        grabModeSideRight.m_camera = p_SideVision.p_CamSide;
                        grabModeSideRight.m_eScanPos = eScanPos.Right;
                        GrabSideRight.m_grabMode = grabModeSideRight;
                        Sidevision.StartRun(GrabSideRight);

                        //// BevelGrab
                        SideVision.Run_BevelGrab GrabBevelBottom = (SideVision.Run_BevelGrab)Sidevision.CloneModuleRun("BevelGrab");
                        GrabMode grabModeBevelBottom = new GrabMode("FullScan", cameraSet, m_LightSet, memoryPool);
                        grabModeBevelBottom.m_memoryGroup = memoryPool.GetGroup("Grab");
                        grabModeBevelBottom.m_ScanStartLine = 0;
                        grabModeBevelBottom.m_ScanLineNum = 1;
                        grabModeBevelBottom.m_camera = p_SideVision.p_CamBevel;
                        grabModeBevelBottom.m_eScanPos = eScanPos.Bottom;
                        GrabBevelBottom.m_grabMode = grabModeBevelBottom;
                        Sidevision.StartRun(GrabBevelBottom);

                        SideVision.Run_BevelGrab GrabBevelLeft = (SideVision.Run_BevelGrab)Sidevision.CloneModuleRun("BevelGrab");
                        GrabMode grabModeBevelLeft = new GrabMode("FullScan", cameraSet, m_LightSet, memoryPool);
                        grabModeBevelLeft.m_memoryGroup = memoryPool.GetGroup("Grab");
                        grabModeBevelLeft.m_ScanStartLine = 0;
                        grabModeBevelLeft.m_ScanLineNum = 1;
                        grabModeBevelLeft.m_camera = p_SideVision.p_CamBevel;
                        grabModeBevelLeft.m_eScanPos = eScanPos.Left;
                        GrabBevelLeft.m_grabMode = grabModeBevelLeft;
                        Sidevision.StartRun(GrabBevelLeft);

                        SideVision.Run_BevelGrab GrabBevelTop = (SideVision.Run_BevelGrab)Sidevision.CloneModuleRun("BevelGrab");
                        GrabMode grabModeBevelTop = new GrabMode("FullScan", cameraSet, m_LightSet, memoryPool);
                        grabModeBevelTop.m_memoryGroup = memoryPool.GetGroup("Grab");
                        grabModeBevelTop.m_ScanStartLine = 0;
                        grabModeBevelTop.m_ScanLineNum = 1;
                        grabModeBevelTop.m_camera = p_SideVision.p_CamBevel;
                        grabModeBevelTop.m_eScanPos = eScanPos.Top;
                        GrabBevelTop.m_grabMode = grabModeBevelTop;
                        Sidevision.StartRun(GrabBevelTop);

                        SideVision.Run_BevelGrab GrabBevelRight = (SideVision.Run_BevelGrab)Sidevision.CloneModuleRun("BevelGrab");
                        GrabMode grabModeBevelRight = new GrabMode("FullScan", cameraSet, m_LightSet, memoryPool);
                        grabModeBevelRight.m_memoryGroup = memoryPool.GetGroup("Grab");
                        grabModeBevelRight.m_ScanStartLine = 0;
                        grabModeBevelRight.m_ScanLineNum = 1;
                        grabModeBevelRight.m_camera = p_SideVision.p_CamBevel;
                        grabModeBevelRight.m_eScanPos = eScanPos.Right;
                        GrabBevelRight.m_grabMode = grabModeBevelRight;
                        Sidevision.StartRun(GrabBevelRight);
                    }
                    else if (Grab.m_grabMode != null)
                        Sidevision.StartRun(Grab);
                    else if (BevelGrab.m_grabMode != null)
                        Sidevision.StartRun(BevelGrab);
                }
                else
                {
                    // Cancelled
                }
            }
        }

        public void AutoFocus()
        {
            EQ.p_bStop = false;
            SideVision Sidevision = ((Vega_Handler)m_Engineer.ClassHandler()).m_sideVision;
            SideVision.Run_AutoFocus af = (SideVision.Run_AutoFocus)Sidevision.CloneModuleRun("AutoFocus");
            var viewModel = new Dialog_AutoFocus_ViewModel(Sidevision, af);
            Nullable<bool> result = m_DialogService.ShowDialog(viewModel);
            if (result.HasValue)
            {
                if (result.Value)
                {
                    
                }
                else
                {
                    // Cancelled
                }
            }
            //m_DialogServiceTest.Show<Dialog_AutoFocus>(this, viewModel);
            
            return;
        }

        private void ViewModel_CloseRequested(object sender, DialogCloseRequestedEventArgs e)
        {
            throw new NotImplementedException();
        }

        #region RelayCommand
        public RelayCommand CommandAutoFocus
        {
            get
            {
                return new RelayCommand(AutoFocus);
            }
        }
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