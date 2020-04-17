using Root_Vega.Module;
using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Camera.Dalsa;
using RootTools.Control.Ajin;
using RootTools.Light;
using System;
using System.Threading;

namespace Root_Vega
{
    public class _8_1_Optic_MainVisionViewModel : ObservableObject
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

        public _8_1_Optic_MainVisionViewModel(Vega_Engineer engineer)
        {
            m_Engineer = engineer;
            p_PatternVision = ((Vega_Handler)engineer.ClassHandler()).m_patternVision;
            p_CamMain = p_PatternVision.m_CamMain;
            p_CamVRS = p_PatternVision.m_CamVRS;
            p_CamAlign1 = p_PatternVision.m_CamAlign1;
            p_CamAlign2= p_PatternVision.m_CamAlign2;
            m_LightSet = p_PatternVision.m_lightSet;
        }

        
    }
}
