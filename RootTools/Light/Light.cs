using RootTools.Trees;
using System.Windows.Controls;

namespace RootTools.Light
{
    public class Light
    {
        public LightBase m_light;

        #region Property
        public bool p_bOn
        {
            get { return m_light.p_bOn; }
            set
            {
                if (m_light.p_bOn == value) return;
                m_log.Info(p_id + " Light On : " + m_light.p_bOn.ToString() + " -> " + value.ToString());
                m_light.p_bOn = value;
            }
        }

        public double p_fPower
        {
            get {
                if (m_light == null)
                    return 0;
                return m_light.p_fSetPower; }
            set
            {
                if (m_light.p_fSetPower == value) return;
                m_log.Info(p_id + " Current Power : " + m_light.p_fSetPower.ToString() + " -> " + value.ToString());
                m_light.p_fSetPower = value;
            }
        }
        #endregion

        #region UI
        public UserControl p_ui 
        { 
            get 
            { 
                return (m_light != null) ? m_light.p_ui : null; 
            } 
        }
        #endregion

        public string p_id { get; set; }
        public string m_sName; 
        LightToolSet m_lightToolSet;
        Log m_log; 
        public Light(LightToolSet lightToolSet, string id, Log log)
        {
            p_id = id;
            m_sName = id; 
            m_lightToolSet = lightToolSet; 
            m_log = log;
        }

        public void ThreadStop()
        {
        }

        string m_sLightTool = "";
        public string RunTree(Tree tree)
        {
            m_sName = tree.Set(m_sName, m_sName, "Name", "Light Name"); 
            string sLightTool0 = m_sLightTool;
            ILightTool lightTool0 = m_lightToolSet.GetTool(sLightTool0);
            m_sLightTool = tree.Set(m_sLightTool, "", m_lightToolSet.m_asLightTool, "LightTool", "Select LightTool");
            ILightTool lightTool1 = m_lightToolSet.GetTool(m_sLightTool);
            if ((sLightTool0 != m_sLightTool) && (tree.p_treeRoot.p_eMode == Tree.eMode.Update))
            {
                tree.Set(-1, -1, "Channel", "Select Light Channel", lightTool1 != null);
                if (lightTool0 != null) Deselect(lightTool0);
                return "OK"; 
            }
            else
            {
                int nLight0 = (m_light != null) ? m_light.p_nChannel : -1;
                int nLight1 = tree.Set(nLight0, -1, "Channel", "Select Light Channel", lightTool1 != null);
                if (nLight0 == nLight1) return "OK";
                if (lightTool1 == null) return "OK";
                LightBase lightBase = lightTool1.GetLight(nLight1, p_id);
                if (lightBase == null) return "OK";
                if (lightTool0 != null) Deselect(lightTool0);
                m_light = lightBase;
                return "OK";
            }
        }

        string Deselect(ILightTool lightTool)
        {
            if (m_light == null) return "OK";
            lightTool.Deselect(m_light); 
            m_light = null; 
            return "OK"; 
        }
    }
}
