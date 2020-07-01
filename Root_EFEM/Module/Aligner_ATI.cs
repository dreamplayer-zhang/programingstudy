using RootTools;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_EFEM.Module
{
    public class Aligner_ATI : ModuleBase, IWTRChild
    {
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

        public Aligner_ATI(string id, IEngineer engineer)
        {
//            m_waferSize = new WaferSize(id, false, false);
//            m_aoi = new Aligner_ATI_AOI(m_log);
            base.InitBase(id, engineer);
//            InitPosAlign();
//            InitPosOCR();
        }
    }
}
