using System.Collections.Generic;
using System.Windows.Controls;

namespace RootTools.Lens
{
    public class LensSet : ITool
    {
        public delegate void dgOnChangeTool();
        public event dgOnChangeTool OnChangeTool;

        #region List Lens
        public List<ILens> m_aLens = new List<ILens>();
        public List<string> p_asLens
        {
            get
            {
                List<string> asLens = new List<string>();
                foreach (ILens Lens in m_aLens) asLens.Add(Lens.p_id);
                return asLens;
            }
        }

        public void Add(ILens Lens)
        {
            ILens cam = Get(Lens.p_id);
            if (cam != null) return;
            m_aLens.Add(Lens);
            m_toolSetLens.Add(Lens);
            if (OnChangeTool != null) OnChangeTool();
        }

        public ILens Get(string id)
        {
            foreach (ILens Lens in m_aLens)
            {
                if (Lens.p_id.Contains(id)) return Lens;
            }
            return null;
        }
        #endregion

        #region UI
        public UserControl p_ui
        {
            get
            {
                LensSet_UI ui = new LensSet_UI();
                ui.Init(this);
                return (UserControl)ui;
            }
        }
        #endregion

        public string p_id { get; set; }
        public string m_sModule;
        ToolSetLens m_toolSetLens;
        Log m_log;
        public LensSet(ToolSetLens toolSetLens, string sModule, Log log)
        {
            p_id = "Lens";
            m_sModule = sModule;
            m_toolSetLens = toolSetLens;
            m_log = log;
        }

        public void ThreadStop()
        {
        }
    }
}
