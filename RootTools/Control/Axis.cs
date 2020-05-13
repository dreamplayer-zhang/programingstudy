using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace RootTools.Control
{
    public class Axis : ObservableObject
    {
        public enum eState
        {
            Init,
            Home,
            Ready,
            Move, 
            Jog,
            Error
        }

        #region AxisPos List
        public List<string> m_asPos = new List<string>(); 
        public void AddPos(params dynamic[] aPos)
        {
            foreach (dynamic value in aPos) m_asPos.Add(value.ToString());
        }

        public void AddPosDone()
        {
            if (p_axis == null) return;
            p_axis.ClearPos();
            foreach (string sPos in m_asPos) p_axis.AddPos(sPos);
            p_axis.RunTree(Tree.eMode.RegRead);
            p_axis.RunTree(Tree.eMode.Init); 
        }

        public int GetPos(Enum Position)
        {
            return 0;
        }
        #endregion

        #region Move
        public double p_vRate
        {
            get
            {
                if (p_axis == null) return 1;
                return p_axis.p_vRate;
            }
            set
            {
                if (p_axis != null) p_axis.p_vRate = value;
            }
        }

        public string Move(double fPos, double vMove = -1, double secAcc = -1, double secDec = -1)
        {
            if (p_axis == null) return "Axis == null";
            return p_axis.Move(fPos, vMove, secAcc, secDec); 
        }

        public string Move(string pos, double fOffset = 0, double vMove = -1, double secAcc = -1, double secDec = -1)
        {
            if (p_axis == null) return "Axis == null";
            return p_axis.Move(pos, fOffset, vMove, secAcc, secDec); 
        }

        public string Move(Enum pos, double fOffset = 0, double vMove = -1, double secAcc = -1, double secDec = -1)
        {
            return Move(pos.ToString(), fOffset, vMove, secAcc, secDec); 
        }

        public string WaitReady()
        {
            if (p_axis == null) return "Axis == null";
            while (p_axis.p_eState == eState.Move || p_axis.p_eState == eState.Home) 
                Thread.Sleep(10);
            switch (p_axis.p_eState)
            {
                case eState.Ready: return "OK";
                default: return "WaitReady Error p_eState = " + p_axis.p_eState.ToString(); 
            }
        }
        #endregion

        IAxis m_axis = null;
        public IAxis p_axis
        {
            get
            {
                return m_axis;
            }
            set
            {
                SetProperty(ref m_axis, value);
            }
        }
        
        
        List<IAxis> m_toolAxis; 
        string m_id;
        Log m_log;
        public Axis(List<IAxis> toolAxis, string id, Log log)
        {
            m_toolAxis = toolAxis; 
            m_id = id;
            m_log = log; 
        }

        public string RunTree(Tree tree)
        {
            int nAxis0 = (p_axis == null) ? -1 : p_axis.p_nAxisID; 
            int nAxis1 = tree.Set(nAxis0, -1, "Axis", "Axis Number"); 
            if (nAxis0 != nAxis1)
            {
                if (IsAssignOK(nAxis1)) return "Can't Assign Exist Axis : " + m_id;
                if (p_axis != null)
                {
                    p_axis.p_sID = "Axis";
                    p_axis.ClearPos();
                    p_axis.RunTree(Tree.eMode.Init);
                }
                if (nAxis1 < 0) p_axis = null;
                else
                {
                    p_axis = m_toolAxis[nAxis1];
                    p_axis.p_sID = m_id;
                    p_axis.p_log.m_logger = m_log.m_logger; 
                    AddPosDone();
                }
            }
            return "OK"; 
        }

        bool IsAssignOK(int nAxis1)
        {
            if (nAxis1 < 0) return false;
            if (m_toolAxis.Count <= nAxis1) return false;
            return (m_toolAxis[nAxis1].p_sID.Substring(3, 4) != "Axis");
        }
    }
}
