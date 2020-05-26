using System.Collections.Generic;

namespace RootTools.ToolBoxs
{
    public class ToolSet : IToolSet
    {
        public string p_id { get; set; }

        public delegate void dgOnChangeTool();
        public event dgOnChangeTool OnChangeTool;

        #region List ITool
        public List<ITool> m_aTool = new List<ITool>();
        public void AddTool(ITool tool)
        {
            m_aTool.Add(tool);
            if (OnChangeTool != null) OnChangeTool();
        }
        #endregion

        public ToolSet(string id)
        {
            p_id = id;
        }

        public void ThreadStop()
        {
            foreach (ITool tool in m_aTool) tool.ThreadStop();
        }
    }
}
