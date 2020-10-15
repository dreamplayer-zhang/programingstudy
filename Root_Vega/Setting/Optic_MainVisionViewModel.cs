using Root_Vega.Module;
using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Camera.Dalsa;
using RootTools.Control.Ajin;
using RootTools.Inspects;
using RootTools.Light;
using RootTools.Module;
using System;
using System.Threading;
using System.Windows;

namespace Root_Vega
{
    public class Optic_MainVisionViewModel : ObservableObject
    {
        Vega_Engineer m_Engineer;
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
        Camera_Dalsa m_CamMain;
        public Camera_Dalsa p_CamMain
        {
            get
            {
                return m_CamMain;
            }
            set
            {
                SetProperty(ref m_CamMain, value);
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
        Camera_Basler m_CamAlign1;
        public Camera_Basler p_CamAlign1
        {
            get
            {
                return m_CamAlign1;
            }
            set
            {
                SetProperty(ref m_CamAlign1, value);
            }
        }
        Camera_Basler m_CamAlign2;
        public Camera_Basler p_CamAlign2
        {
            get
            {
                return m_CamAlign2;
            }
            set
            {
                SetProperty(ref m_CamAlign2, value);
            }
        }
        public int p_LightMain
        {
            get
            {
                for (int i = 0; i < m_LightSet.m_aLight.Count; i++)
                {
                    if (m_LightSet.m_aLight[i].m_sName.IndexOf("Main Coax") >= 0)
                    {
                        return Convert.ToInt32(m_LightSet.m_aLight[i].p_fPower);
                    }
                }
                return 0;
            }
            set
            {
                for (int i = 0; i < m_LightSet.m_aLight.Count; i++)
                {
                    if (m_LightSet.m_aLight[i].m_sName.IndexOf("Main Coax") >= 0)
                    {
                        m_LightSet.m_aLight[i].m_light.p_fSetPower = value;
                    }
                }
            }
        }
        public int p_LightVRS
        {
            get
            {
                for (int i = 0; i < m_LightSet.m_aLight.Count; i++)
                {
                    if (m_LightSet.m_aLight[i].m_sName.IndexOf("VRS") >= 0)
                    {
                        return Convert.ToInt32(m_LightSet.m_aLight[i].p_fPower);
                    }
                }
                return 0;
            }
            set
            {
                for (int i = 0; i < m_LightSet.m_aLight.Count; i++)
                {
                    if (m_LightSet.m_aLight[i].m_sName.IndexOf("VRS") >= 0)
                    {
                        m_LightSet.m_aLight[i].m_light.p_fSetPower = value;
                    }
                }
            }
        }
        public int p_LightAlign1
        {
            get
            {
                for (int i = 0; i < m_LightSet.m_aLight.Count; i++)
                {
                    if (m_LightSet.m_aLight[i].m_sName.IndexOf("Align1") >= 0)
                    {
                        return Convert.ToInt32(m_LightSet.m_aLight[i].p_fPower);
                    }
                }
                return 0;
            }
            set
            {
                for (int i = 0; i < m_LightSet.m_aLight.Count; i++)
                {
                    if (m_LightSet.m_aLight[i].m_sName.IndexOf("Align1") >= 0)
                    {
                        m_LightSet.m_aLight[i].m_light.p_fSetPower = value;
                    }
                }
            }
        }
        public int p_LightAlign2
        {
            get
            {
                for (int i = 0; i < m_LightSet.m_aLight.Count; i++)
                {
                    if (m_LightSet.m_aLight[i].m_sName.IndexOf("Align2") >= 0)
                    {
                        return Convert.ToInt32(m_LightSet.m_aLight[i].p_fPower);
                    }
                }
                return 0;
            }
            set
            {
                for (int i = 0; i < m_LightSet.m_aLight.Count; i++)
                {
                    if (m_LightSet.m_aLight[i].m_sName.IndexOf("Align2") >= 0)
                    {
                        m_LightSet.m_aLight[i].m_light.p_fSetPower = value;
                    }
                }
            }
        }

        LightSet m_LightSet;
        private readonly IDialogService m_DialogService;

        public Optic_MainVisionViewModel(Vega_Engineer engineer, IDialogService dialogService)
        {
            m_Engineer = engineer;
            p_PatternVision = ((Vega_Handler)engineer.ClassHandler()).m_patternVision;
            m_DialogService = dialogService;
            p_CamMain = p_PatternVision.m_CamMain;
            p_CamVRS = p_PatternVision.m_CamVRS;
            p_CamAlign1 = p_PatternVision.m_CamAlign1;
            p_CamAlign2= p_PatternVision.m_CamAlign2;
            m_LightSet = p_PatternVision.m_lightSet;
        }

        public void HomePatternVision()
        {
            EQ.p_bStop = false;
            m_PatternVision.p_eState = ModuleBase.eState.Home;
            if (m_PatternVision.p_axisClamp.p_sensorHome == false) return;    // 8번축(Clamp축)이 Home위치가 아니면 Home 시퀀스 동작하지 않도록 Interlock 추가
            //if (m_PatternVision.m_CamMain.p_CamInfo.p_eState == eCamState.Init)
            //    m_PatternVision.m_CamMain.Connect();
        }

        void Stop()
        {
            EQ.p_bStop = true;
        }

        public void WholeLineScan()
        {
            EQ.p_bStop = false;
            //Vision vision = ((Vega_Handler)m_Engineer.ClassHandler()).p_vision;
            //Vision.Run_Grab Grab = (Vision.Run_Grab)vision.CloneModuleRun("Grab");
            //if (vision.m_aGrabMode.Count == 0)
            //    return;
            //GrabMode mode = vision.m_aGrabMode[0];

            //int ScanWholeLine = (int)Math.Ceiling(Grab.m_yLine * 1000 / (mode.m_camera.GetRoiSize().X * Grab.m_fRes));

            //mode.m_ScanLineNum = ScanWholeLine;
            //mode.m_ScanStartLine = 0;
            //Grab.m_grabMode = mode;
            //p_MainCamera = Grab.m_grabMode.m_camera;
            //vision.StartRun(Grab);
            //Worker_ViewerUpdate.RunWorkerAsync();
        }

        public void Scan()
        {
            EQ.p_bStop = false;
            PatternVision Patternvision = ((Vega_Handler)m_Engineer.ClassHandler()).m_patternVision;
            if (Patternvision.p_eState != ModuleBase.eState.Ready)
            {
                MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                return;
            }
            PatternVision.Run_Grab Grab = (PatternVision.Run_Grab)Patternvision.CloneModuleRun("Grab");
            var viewModel = new Dialog_Scan_ViewModel(Patternvision, Grab);
            Nullable<bool> result = m_DialogService.ShowDialog(viewModel);
            if (result.HasValue)
            {
                if (result.Value)
                {   
                    Patternvision.StartRun(Grab);
                    // Worker_ViewerUpdate.RunWorkerAsync();
                    //ModuleList modulelist = ((Wind_Handler)m_Engineer.ClassHandler()).m_moduleList;
                    //ModuleRunList runlist = ((Wind_Handler)m_Engineer.ClassHandler()).m_moduleList.m_moduleRunList;
                    //runlist.Clear();
                    //runlist.Add(vision, Grab);
                    //EQ.p_eState = EQ.eState.Ready;
                    //modulelist.StartModuleRuns(); 
                }
                else
                {
                    // Cancelled
                }
            }
        }

        public void Test()
        {
            // VRSReviewImageCapture 테스트
            //PatternVision.Run_VRSReviewImagCapture sequence = new PatternVision.Run_VRSReviewImagCapture(m_PatternVision);
            //sequence.m_grabMode = m_PatternVision.m_aGrabMode[0];
            //sequence.m_dResX_um = 0.4995;
            //sequence.m_dResY_um = 0.5;
            //sequence.m_dReticleSize_mm = 150;
            //sequence.m_rpReticleCenterPos = new RPoint(-811154.0, 2020000);
            //RPoint rpReturn = sequence.GetAxisPosFromMemoryPos(new CPoint());
            //return;
        }

        public RelayCommand CommandTest
        {
            get
            {
                return new RelayCommand(Test);
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
                return new RelayCommand(HomePatternVision);
            }
        }
        
    }
}
