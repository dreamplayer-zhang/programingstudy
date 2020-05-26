using System.Collections.Generic;

namespace RootTools.Inspects
{
    public class InspectToolSet : IToolSet
    {
        public delegate void dgOnChangeTool();
        public event dgOnChangeTool OnChangeTool;

        #region Inspect Tool
        public List<InspectTool> m_aInspectTool = new List<InspectTool>();
        public InspectTool GetInspect(string id)
        {
            foreach (InspectTool inspectTool in m_aInspectTool)
            {
                if (inspectTool.p_id == id) return inspectTool;
            }
            InspectTool newInspectTool = new InspectTool(id, m_engineer, true);
            m_aInspectTool.Add(newInspectTool);
            if (OnChangeTool != null) OnChangeTool();
            return newInspectTool;
        }
        #endregion

        public string p_id { get; set; }
        IEngineer m_engineer;
        public InspectToolSet(string id, IEngineer engineer)
        {
            p_id = id;
            m_engineer = engineer;
        }

        public void ThreadStop()
        {
            foreach (InspectTool inspectTool in m_aInspectTool) inspectTool.ThreadStop();
        }
    }
}
