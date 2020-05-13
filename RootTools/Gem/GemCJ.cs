using RootTools.Trees;
using System.Collections.Generic;

namespace RootTools.Gem
{
    public class GemCJ
    {
        public enum eState
        {
            Queued,
            Selected,
            WaitingForStart,
            Excuting,
            Paused,
            Completed,
        }
        eState _eState = eState.Queued;
        public eState p_eState
        {
            get { return _eState; }
            set
            {
                if (_eState == value) return;
                m_log.Info(m_sCJobID + " CJ State Chaned : " + _eState.ToString() + " -> " + value.ToString());
                _eState = value;
            }
        }

        GemPJ.eAutoStart m_eAutoStart = GemPJ.eAutoStart.N;

        #region Tree
        public void RunTree(Tree tree)
        {
            p_eState = (eState)tree.Set(p_eState, p_eState, "State", "Control Job State", true, true);
            m_eAutoStart = (GemPJ.eAutoStart)tree.Set(m_eAutoStart, m_eAutoStart, "AutoStart", "Control Job Auto Start");
            RunTreePJ(tree.GetTree("PJ")); 
        }

        void RunTreePJ(Tree tree)
        {
            foreach (GemPJ pj in m_aPJ)
            {
                tree.Set(pj.m_sRecipeID, pj.m_sRecipeID, pj.m_sPJobID, "GemPJ RecipeID", true, true); 
            }
        }
        #endregion

        public string m_sCJobID = "";
        public List<GemPJ> m_aPJ = new List<GemPJ>();
        Log m_log;

        public GemCJ(string sCJobID, GemPJ.eAutoStart autoStart, Log log)
        {
            m_log = log;
            m_sCJobID = sCJobID;
            m_eAutoStart = autoStart;
            m_aPJ.Clear();
        }
    }
}
