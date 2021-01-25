using Root_EFEM.Module;
using RootTools;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_AOP01_Inspection.Engineer
{
    public class AOP01_Process : NotifyProperty
    {
        #region Locate
        /// <summary> Wafer Locate 관리용 </summary>
        public class Locate
        {
            public string m_id;
            public IWTRChild m_child = null;
            public WTRArm m_arm = null;

            /// <summary> Sequence 계산용 InfoWafer </summary>
            InfoWafer _calcWafer = null;
            public InfoWafer p_calcWafer
            {
                get { return _calcWafer; }
                set { _calcWafer = value; }
            }

            /// <summary> 실재 InfoWafer </summary>
            public InfoWafer p_infoWafer
            {
                get { return (m_child != null) ? m_child.GetInfoWafer(0) : m_arm.p_infoWafer; }
                set
                {
                    if (m_child != null) m_child.SetInfoWafer(0, value);
                    else m_arm.p_infoWafer = value;
                }
            }

            public void ClearInfoWafer()
            {
                if (p_infoWafer == null) return;
                if (IsWaferExist() == false) p_infoWafer = null;
            }

            bool IsWaferExist()
            {
                return (m_child != null) ? m_child.IsWaferExist(0) : m_arm.IsWaferExist();
            }

            public Locate(IWTRChild child)
            {
                m_id = child.p_id;
                m_child = child;
            }

            public Locate(WTRArm arm)
            {
                m_id = arm.m_id;
                m_arm = arm;
            }

            public void RunTree(Tree tree)
            {
                string sInfoWafer = (p_infoWafer == null) ? "Empty" : p_infoWafer.p_id;
                tree.GetTree("InfoWafer").Set(sInfoWafer, sInfoWafer, m_id, "InfoWafer ID", true, true);
            }
        }

        /// <summary> Wafer Locate List </summary>
        public List<Locate> m_aLocate = new List<Locate>();
        /// <summary> 프로그램 시작시 Registry 에서 Wafer 정보 읽기 </summary>
        void InitLocate()
        {
            if (m_wtr == null)
                return;
            m_aLocate.Clear();
            foreach (WTRArm arm in m_wtr.p_aArm) InitLocateArm(arm);
            foreach (IWTRChild child in m_wtr.p_aChild) InitLocateChild(child);
        }

        void InitLocateArm(WTRArm arm)
        {
            Locate locate = new Locate(arm);
            m_aLocate.Add(locate);
        }

        void InitLocateChild(IWTRChild child)
        {
            if (child.p_id.Contains("Loadport")) return;
            Locate locate = new Locate(child);
            m_aLocate.Add(locate);
        }

        public Locate GetLocate(string sLocate)
        {
            foreach (Locate locate in m_aLocate)
            {
                if (locate.m_id == sLocate) return locate;
            }
            return null;
        }
        #endregion

        public string m_id;
        IEngineer m_engineer;
        public IHandler m_handler;
        IWTR m_wtr;
        Log m_log;
        public AOP01_Process(string id, IEngineer engineer, IWTR wtr)
        {
            m_id = id;
            m_engineer = engineer;
            m_handler = engineer.ClassHandler();
            m_wtr = wtr;
            m_log = LogView.GetLog(id);
            //InitTree(id);
            InitLocate();
        }
    }
}
