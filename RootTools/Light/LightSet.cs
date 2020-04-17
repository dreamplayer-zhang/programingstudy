using RootTools.Trees;
using System.Collections.Generic;
using System.Windows.Controls;

namespace RootTools.Light
{
    public class LightSet : ITool
    {
        public delegate void dgOnChangeTool();
        public event dgOnChangeTool OnChangeTool;

        #region List Light
        int m_lLight = 0;
        public List<Light> m_aLight = new List<Light>();
        public string RunTree(Tree tree)
        {
            m_lLight = tree.Set(m_lLight, m_lLight, "Count", "Light Count");
            while (m_aLight.Count < m_lLight)
            {
                Light light = new Light(m_lightToolSet, m_sModule + "." + m_aLight.Count.ToString("00"), m_log);
                m_aLight.Add(light);
            }
            foreach (Light light in m_aLight)
            {
                light.RunTree(tree.GetTree(light.p_id, false));
            }
            if (OnChangeTool != null) OnChangeTool();
            return "OK";
        }
        #endregion

        #region UI
        public UserControl p_ui
        {
            get
            {
                LightSet_UI ui = new LightSet_UI();
                ui.Init(this);
                return (UserControl)ui;
            }
        }
        #endregion

        public string p_id { get { return m_id; } }
        string m_id;
        string m_sModule; 
        LightToolSet m_lightToolSet;
        LogWriter m_log;
        public LightSet(LightToolSet lightToolSet, string sModule, LogWriter log)
        {
            m_sModule = sModule; 
            m_id = sModule + ".Light";
            m_lightToolSet = lightToolSet;
            m_log = log;
        }

        public void ThreadStop()
        {
            
        }
    }
}
