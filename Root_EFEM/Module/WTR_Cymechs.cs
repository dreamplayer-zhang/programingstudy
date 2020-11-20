using RootTools;
using RootTools.Comm;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_EFEM.Module
{
    public class WTR_Cymechs : ModuleBase//, IWTR
    {
        //forget
        #region ToolBox
        RS232 m_rs232;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_rs232, this, "RS232");
            m_dicArm[eArm.A].GetTools(m_toolBox);
            m_dicArm[eArm.B].GetTools(m_toolBox);
            if (bInit)
            {
                m_rs232.OnRecieve += M_rs232_OnRecieve; 
                m_rs232.p_bConnect = true;
            }
        }
        #endregion

        //forget
        #region WTR_RND_Arm
        public enum eArm
        {
            A,
            B,
        }
        public Dictionary<eArm, Arm> m_dicArm = new Dictionary<eArm, Arm>();
        void InitArms(string id)
        {
            m_dicArm.Add(eArm.A, new Arm(id, eArm.A, this));
            m_dicArm.Add(eArm.B, new Arm(id, eArm.B, this));
        }

        public class Arm : NotifyProperty
        {
            public eArm m_eArm;
            string m_sInfoWafer = "";
            InfoWafer _infoWafer = null;
            public InfoWafer p_infoWafer
            {
                get { return _infoWafer; }
                set
                {
                    m_sInfoWafer = (value == null) ? "" : value.p_id;
                    _infoWafer = value;
                    m_reg.Write("sInfoWafer", m_sInfoWafer);
                    OnPropertyChanged();
                }
            }

            Registry m_reg;
            public void ReadInfoWafer_Registry()
            {
                m_reg = new Registry(m_id);
                m_sInfoWafer = (string)m_reg.Read("sInfoWafer", m_sInfoWafer);
                p_infoWafer = m_module.m_engineer.ClassHandler().GetGemSlot(m_sInfoWafer);
            }

            public string m_id;
            WTR_Cymechs m_module;
            InfoWafer.WaferSize m_waferSize;
            public Arm(string id, eArm arm, WTR_Cymechs module)
            {
                m_eArm = arm;
                m_module = module;
                m_id = id + "." + arm.ToString();
                m_waferSize = new InfoWafer.WaferSize(m_id, true, false);
            }

            public bool IsEnableWaferSize(InfoWafer infoWafer)
            {
                if (m_bEnable == false) return false;
                return m_waferSize.GetData(infoWafer.p_eSize).m_bEnable;
            }

            public bool m_bEnable = true;
            public void RunTree(Tree tree)
            {
                m_bEnable = tree.Set(m_bEnable, m_bEnable, "Enable", "Enable WTR Arm");
                m_waferSize.RunTree(tree.GetTree("Wafer Size", false), m_bEnable);
            }

//            public DIO_I m_diCheckVac;
//            public DIO_I m_diArmClose;
            public void GetTools(ToolBox toolBox)
            {
//                m_module.p_sInfo = toolBox.Get(ref m_diCheckVac, m_module, m_eArm.ToString() + ".CheckVac");
//                m_module.p_sInfo = toolBox.Get(ref m_diArmClose, m_module, m_eArm.ToString() + ".ArmClose");
            }

            public bool IsWaferExist(bool bIgnoreExistSensor = false)
            {
                if (bIgnoreExistSensor) return (p_infoWafer != null);
                //                return m_diCheckVac.p_bIn;
                return (p_infoWafer != null);
            }
        }
        #endregion

        #region ErrorCode
        enum eErrorMode
        {
            Operation,
            Motion,
            Motor,
            Aligner,
            Grip,
            SCARA,
            PA
        };
        enum eErrorAxis
        {
            Z1,
            T1,
            T2,
            A,
            B,
            Z2
        };
//        string[,] m_asErrorOperation = new string[,]
//        {
//            { "001",  }
//        }
        #endregion

        private void M_rs232_OnRecieve(string sRead)
        {
            throw new NotImplementedException();
        }

        public WTR_Cymechs(string id, IEngineer engineer)
        {
            InitArms(id);
            InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }
    }
}
