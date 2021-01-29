using System.Collections.Generic;

namespace RootTools.Lens
{
    public class ToolSetLens : IToolSet
    {
        public delegate void dgOnToolChanged();
        public event dgOnToolChanged OnToolChanged;

        #region ILensTools
        public List<ILens> m_aLens = new List<ILens>();

        public void Add(ILens Lens)
        {
            ILens cam = GetLens(Lens.p_id);
            if (cam != null) return;
            m_aLens.Add(Lens);
            if (OnToolChanged != null) OnToolChanged();
        }

        public ILens GetLens(string sLens)
        {
            foreach (ILens Lens in m_aLens)
            {
                if (Lens.p_id == sLens) return Lens;
            }
            return null;
        }
        #endregion

        public string p_id { get; set; }

        public ToolSetLens(string id)
        {
            p_id = id;
        }

        public void ThreadStop()
        {
            foreach (ILens Lens in m_aLens) Lens.ThreadStop();
        }
    }
}
