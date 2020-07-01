using RootTools;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;

namespace Root_EFEM.Module
{
    public class Loadport_RND : ModuleBase, IWTRChild //forget
    {
        #region ToolBox
//        DIO_I m_diPlaced;
//        DIO_I m_diPresent;
//        DIO_I m_diLoad;
//        DIO_I m_diUnload;
//        DIO_I m_diDoorOpen;
//        DIO_I m_diDocked;
//        RS232 m_rs232;
        public override void GetTools(bool bInit)
        {
//            p_sInfo = m_toolBox.Get(ref m_diPlaced, this, "Place");
//            p_sInfo = m_toolBox.Get(ref m_diPresent, this, "Present");
//            p_sInfo = m_toolBox.Get(ref m_diLoad, this, "Load");
//            p_sInfo = m_toolBox.Get(ref m_diUnload, this, "Unload");
//            p_sInfo = m_toolBox.Get(ref m_diDoorOpen, this, "DoorOpen");
//            p_sInfo = m_toolBox.Get(ref m_diDocked, this, "Docked");
//            p_sInfo = m_toolBox.Get(ref m_rs232, this, "RS232");
//            if (bInit)
//            {
//                m_rs232.OnRecieve += M_rs232_OnRecieve;
//                m_rs232.p_bConnect = true;
//            }
        }
        #endregion

        public bool p_bLock { get; set; }

        public List<string> p_asChildID { get; set; }

        public string AfterGet(int nID)
        {
            throw new NotImplementedException();
        }

        public string AfterPut(int nID)
        {
            throw new NotImplementedException();
        }

        public string BeforeGet(int nID)
        {
            throw new NotImplementedException();
        }

        public string BeforePut(int nID)
        {
            throw new NotImplementedException();
        }

        public InfoWafer GetInfoWafer(int nID)
        {
            throw new NotImplementedException();
        }

        public string IsGetOK(int nID, ref int teachWTR)
        {
            throw new NotImplementedException();
        }

        public string IsPutOK(int nID, InfoWafer infoWafer, ref int teachWTR)
        {
            throw new NotImplementedException();
        }

        public bool IsWaferExist(int nID, bool bIgnoreExistSensor = false)
        {
            throw new NotImplementedException();
        }

        public void ReadInfoWafer_Registry()
        {
            throw new NotImplementedException();
        }

        public void RunTreeTeach(Tree tree)
        {
            throw new NotImplementedException();
        }

        public void SetInfoWafer(int nID, InfoWafer infoWafer)
        {
            throw new NotImplementedException();
        }

        public InfoCarrier m_infoCarrier;
        public Loadport_RND(string id, IEngineer engineer)
        {
//            InitCmd();
            p_id = id;
            m_infoCarrier = new InfoCarrier(this, id, engineer);
            m_aTool.Add(m_infoCarrier);
            base.InitBase(id, engineer);
//            InitGAF();
            if (m_gem != null) m_gem.OnGemRemoteCommand += M_gem_OnRemoteCommand;
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        private void M_gem_OnRemoteCommand(string sCmd, Dictionary<string, string> dicParam, long[] pnResult)
        {

        }


    }
}
