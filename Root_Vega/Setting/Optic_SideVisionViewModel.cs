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
using System.Drawing;
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

        public int p_LightLADS
        {
            get
            {
                return GetLightByName("LADS");
            }
            set
            {
                SetLightByName("LADS", value); 
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
                        MemoryPool memoryPool = m_Engineer.ClassMemoryTool().GetPool("SideVision.Memory");

                        List<SideVision.Run_SideGrab> aSideGrabs = new List<SideVision.Run_SideGrab>();
                        List<GrabMode> aSideModes = new List<GrabMode>();
                        List<SideVision.Run_BevelGrab> aBevelGrabs = new List<SideVision.Run_BevelGrab>();
                        List<GrabMode> aBevelModes = new List<GrabMode>();
                        
                        for (int i = 0; i<4; i++)
                        {
                            aSideGrabs.Add((SideVision.Run_SideGrab)Sidevision.CloneModuleRun("SideGrab"));
                            aSideModes.Add(new GrabMode("FullScan", cameraSet, m_LightSet, memoryPool));
                            aBevelGrabs.Add((SideVision.Run_BevelGrab)Sidevision.CloneModuleRun("BevelGrab"));
                            aBevelModes.Add(new GrabMode("FullScan", cameraSet, m_LightSet, memoryPool));

                            // SideGrab
                            aSideModes[i].m_memoryGroup = memoryPool.GetGroup("Grab");
                            aSideModes[i].m_ScanStartLine = 0;
                            aSideModes[i].m_ScanLineNum = 3;
                            aSideModes[i].m_camera = p_SideVision.p_CamSide;

                            switch (i)
                            {
                                case (int)eScanPos.Bottom: aSideModes[i].m_eScanPos = eScanPos.Bottom; break;
                                case (int)eScanPos.Left: aSideModes[i].m_eScanPos = eScanPos.Left; break;
                                case (int)eScanPos.Right: aSideModes[i].m_eScanPos = eScanPos.Right; break;
                                case (int)eScanPos.Top: aSideModes[i].m_eScanPos = eScanPos.Top; break;
                            }
                            aSideGrabs[i].m_grabMode = aSideModes[i];
                            Sidevision.StartRun(aSideGrabs[i]);

                            // BevelGrab
                            aBevelModes[i].m_memoryGroup = memoryPool.GetGroup("Grab");
                            aBevelModes[i].m_ScanStartLine = 0;
                            aBevelModes[i].m_ScanLineNum = 1;
                            aBevelModes[i].m_camera = p_SideVision.p_CamBevel;

                            switch (i)
                            {
                                case (int)eScanPos.Bottom: aBevelModes[i].m_eScanPos = eScanPos.Bottom; break;
                                case (int)eScanPos.Left: aBevelModes[i].m_eScanPos = eScanPos.Left; break;
                                case (int)eScanPos.Right: aBevelModes[i].m_eScanPos = eScanPos.Right; break;
                                case (int)eScanPos.Top: aBevelModes[i].m_eScanPos = eScanPos.Top; break;
                            }
                            aBevelGrabs[i].m_grabMode = aBevelModes[i];
                            Sidevision.StartRun(aBevelGrabs[i]);
                        }
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

                }
            }
            
            return;
        }

        unsafe public void LADS()
        {
            EQ.p_bStop = false;
            SideVision Sidevision = ((Vega_Handler)m_Engineer.ClassHandler()).m_sideVision;
            SideVision.Run_LADS lads = (SideVision.Run_LADS)Sidevision.CloneModuleRun("LADS");
            var viewModel = new Dialog_LADS_ViewModel(Sidevision, lads);
            Nullable<bool> result = m_DialogService.ShowDialog(viewModel);
            if (result.HasValue)
            {
                if (result.Value)
                {

                }
                else
                {

                }
            }
        }

        unsafe double CalculatingHeight(ImageData img)
        {
            // variable
            int nImgWidth = 640;
            int nImgHeight = 102;
            double dScale = 65535.0 / nImgHeight;
            double[] daHeight = new double[nImgWidth];

            // implement
            byte* pSrc = (byte*)img.GetPtr().ToPointer();
            for (int x = 0; x < nImgWidth; x++, pSrc++)
            {
                byte* pSrcY = pSrc;
                int nSum = 0;
                int nYSum = 0;
                for (int y = 0; y < nImgHeight; y++, pSrcY += nImgWidth)
                {
                    if (*pSrcY < 70) continue;
                    nSum += *pSrcY;
                    nYSum += *pSrcY * y;
                }
                int iIndex = x;
                daHeight[iIndex] = (nSum != 0) ? ((double)nYSum / (double)nSum) : 0.0;
            }

            return GetHeightAverage(daHeight);
        }

        double GetHeightAverage(double[] daHeight)
        {
            // variable
            double dSum = 0.0;
            int nHitCount = 0;

            // implement
            for (int i = 0; i < daHeight.Length; i++)
            {
                if (daHeight[i] == 0) continue;
                nHitCount++;
                dSum += daHeight[i];
            }
            return dSum / nHitCount;
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
        public RelayCommand CommandLADS
        {
            get
            {
                return new RelayCommand(LADS);
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