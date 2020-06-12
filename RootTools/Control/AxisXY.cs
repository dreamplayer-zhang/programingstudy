using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace RootTools.Control
{
    public class AxisXY :ObservableObject
    {
        IAxis m_axisX = null;
        public IAxis p_axisX
        {
            get
            {
                return m_axisX;
            }
            set
            {
                SetProperty(ref m_axisX, value);
            }
        }
        IAxis m_axisY = null;
        public IAxis p_axisY
        {
            get
            {
                return m_axisY;
            }
            set
            {
                SetProperty(ref m_axisY, value);
            }
        }

        #region AxisPos
        public List<string> m_asPos = new List<string>();
        public void AddPos(params object[] aPos)
        {
            foreach (object value in aPos)
                m_asPos.Add(value.ToString());
            AddPosDone();
        }

        public void AddPosDone()
        {
            AddPosDone(p_axisX);
            AddPosDone(p_axisY); 
        }

        void AddPosDone(IAxis axis)
        {
            if (axis == null) return;
            axis.ClearPos();
            foreach (string sPos in m_asPos) axis.AddPos(sPos);
            axis.RunTree(Tree.eMode.RegRead);
            axis.RunTree(Tree.eMode.Init);
        }

        public CPoint GetPos(Enum pos)
        {
            CPoint cp = new CPoint();
            cp.X = Convert.ToInt32(p_axisX.GetPos(pos.ToString()));
            cp.Y = Convert.ToInt32(p_axisY.GetPos(pos.ToString()));
            return cp; 
        }

        public bool IsInPos(Enum pos, double posError = 10)
        {
            CPoint cp = GetPos(pos); 
            double dPos = cp.X - p_axisX.p_posCommand;
            if (Math.Abs(dPos) > posError) return false;
            dPos = cp.Y - p_axisY.p_posCommand;
            if (Math.Abs(dPos) > posError) return false;
            return true; 
        }
        #endregion

        #region Move
        public double p_vRate
        {
            get
            {
                if (p_axisX == null) return 1;
                return p_axisX.p_vRate;
            }
            set
            {
                if (p_axisX != null) p_axisX.p_vRate = value;
                if (p_axisY != null) p_axisY.p_vRate = value;
            }
        }

        public string Move(RPoint rpPos, double vMove = -1, double secAcc = -1, double secDec = -1)
        {
            if (p_axisX == null) return "AxisX == null";
            if (p_axisY == null) return "AxisY == null";
            string xInfo = p_axisX.Move(rpPos.X, vMove, secAcc, secDec);
            string yInfo = p_axisY.Move(rpPos.Y, vMove, secAcc, secDec);
            if (xInfo != "OK") return "AxisMove X Error : " + xInfo;
            if (yInfo != "OK") return "AxisMove Y Error : " + yInfo;
            return "OK";
        }

        public string Move(Enum pos, RPoint rpOffset = null, double vMove = -1, double secAcc = -1, double secDec = -1)
        {
            return Move(pos.ToString(), rpOffset, vMove, secAcc, secDec); 
        }

        public string Move(Enum pos, double xOffset, double yOffset, double vMove = -1, double secAcc = -1, double secDec = -1)
        {
            return Move(pos.ToString(), xOffset, yOffset, vMove, secAcc, secDec); 
        }

        public string Move(string pos, RPoint rpOffset = null, double vMove = -1, double secAcc = -1, double secDec = -1)
        {
            //if (rpOffset == null) rpOffset = new RPoint(0, 0);
            
            return Move(pos, 0, 0, vMove, secAcc, secDec); 
        }

        public string Move(string pos, double xOffset, double yOffset, double vMove = -1, double secAcc = -1, double secDec = -1)
        {
            if (p_axisX == null) return "AxisX == null";
            if (p_axisY == null) return "AxisY == null";
            string xInfo = p_axisX.Move(pos, xOffset, vMove, secAcc, secDec);
            string yInfo = p_axisY.Move(pos, yOffset, vMove, secAcc, secDec);
            if (xInfo != "OK") return "AxisMove X Error : " + xInfo;
            if (yInfo != "OK") return "AxisMove Y Error : " + yInfo;
            return "OK";
        }

        public string WaitReady()
        {
            string sInfo = WaitReady(p_axisX);
            if (sInfo != "OK") return sInfo;
            return WaitReady(p_axisY); 
        }

        string WaitReady(IAxis axis)
        {
            if (axis == null) return "Axis == null";
            while (axis.p_eState == Axis.eState.Move || axis.p_eState == Axis.eState.Home) 
                Thread.Sleep(10);
            switch (axis.p_eState)
            {
                case Axis.eState.Ready: return "OK";
                default: return "WaitReady Error " + axis.p_sID + " : p_eState = " + axis.p_eState.ToString();
            }
        }
        #endregion

        List<IAxis> m_toolAxis;
        string m_id;
        Log m_log;
        public AxisXY(List<IAxis> toolAxis, string id, Log log)
        {
            m_toolAxis = toolAxis;
            m_id = id;
            m_log = log;
        }

        public string RunTree(Tree tree)
        {
            string sInfo = RunTree(tree, ref m_axisX, "X");
            if (sInfo != "OK") return sInfo; 
            return RunTree(tree, ref m_axisY, "Y");
        }

        string RunTree(Tree tree, ref IAxis axis, string sName)
        {
            int nAxis0 = (axis == null) ? -1 : axis.p_nAxisID;
            int nAxis1 = tree.Set(nAxis0, -1, "Axis" + sName, "Axis Number");
            if (m_toolAxis.Count == 0)
                return "OK";
            if (nAxis0 != nAxis1)
            {
                if ((nAxis1 >= 0) && (m_toolAxis[nAxis1].p_sID.Substring(3, 4) != "Axis")) return "Can't Assign Exist Axis : " + m_id;
                if (axis != null)
                {
                    axis.p_sID = "Axis";
                    axis.ClearPos();
                    axis.RunTree(Tree.eMode.Init); 
                }
                if (nAxis1 < 0) axis = null;
                else
                {
                    axis = m_toolAxis[nAxis1];
                    axis.p_sID = m_id + "." + sName;
                    axis.p_log.m_logger = m_log.m_logger; 
                    AddPosDone(axis);
                }
            }
            return "OK";
        }
    }
}
