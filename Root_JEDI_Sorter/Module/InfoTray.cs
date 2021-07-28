using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_JEDI_Sorter.Module
{
    public enum eVision
    {
        CCS,
        Top3D,
        Top2D,
        Bottom,
    }

    public enum eResult
    {
        Good,
        Reject,
        Rework,
    }

    public class InfoTray : NotifyProperty
    {
        #region Result

        Dictionary<eVision, bool> m_bInspect = new Dictionary<eVision, bool>();
        void InitInspect()
        {
            foreach (eVision eVision in Enum.GetValues(typeof(eVision))) m_bInspect.Add(eVision, false);
        }

        public void StartInspect(eVision eVision)
        {
            m_bInspect[eVision] = true;
        }

        public bool p_bInspect
        {
            get 
            {
                foreach (eVision eVision in Enum.GetValues(typeof(eVision)))
                {
                    if (m_bInspect[eVision]) return true; 
                }
                return false; 
            }
        }
        #endregion

        public string p_id { get; set; }
        public InfoTray(string id)
        {
            p_id = id; 
            InitInspect(); 
        }
    }
}
