using RootTools;
using RootTools.Gem;
using RootTools.Module;
using RootTools.OHTNew;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Root_EFEM.Module
{
    public class Gem_XGem300Pro : ModuleBase
    {
        IEngineer m_engineer;
        IGem m_gem;
        ILoadport m_loadport;

        public Gem_XGem300Pro(string id, IEngineer engineer, IGem gem, ILoadport loadport)
        {
            p_id = id;
            m_engineer = engineer;
            m_gem = gem;
            m_loadport = loadport;
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_SendCarrierID(this), true, "Carrier ID Send Event");
            AddModuleRunList(new Run_SendSlotMap(this), true, "Carrier Slotmap Send Event");
        }

        public class Run_SendCarrierID : ModuleRunBase
        {
            Gem_XGem300Pro m_module;
            ILoadport m_loadport;
            public Run_SendCarrierID(Gem_XGem300Pro module)
            {
                m_module = module;
                m_loadport = m_module.m_loadport;
                m_sLotID = m_loadport.p_infoCarrier.p_sLotID;
                m_sCarrierID = m_loadport.p_infoCarrier.p_sCarrierID;
                InitModuleRun(module);
            }

            public string m_sLotID = "";
            public string m_sCarrierID = "";

            public override ModuleRunBase Clone()
            {
                Run_SendCarrierID run = new Run_SendCarrierID(m_module);
                return run;
            }
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_sLotID = tree.Set(m_sLotID, m_sLotID, "Lot ID", "Lot ID", bVisible);
                m_sCarrierID = tree.Set(m_sCarrierID, m_sCarrierID, "Carrier ID", "Carrier ID", bVisible);
            }

            public override string Run()
            {
                m_loadport.p_infoCarrier.p_sLotID = m_sLotID;
                m_loadport.p_infoCarrier.p_sCarrierID = m_sCarrierID;
                m_loadport.p_infoCarrier.SendCarrierID(m_sCarrierID);
                while (m_loadport.p_infoCarrier.p_eStateCarrierID != GemCarrierBase.eGemState.VerificationOK) 
                { 
                    Thread.Sleep(10); 
                    if(m_loadport.p_infoCarrier.p_eStateCarrierID == GemCarrierBase.eGemState.VerificationFailed) 
                        return p_sInfo + " infoCarrier.p_eStateCarrierID = " + m_loadport.p_infoCarrier.p_eStateCarrierID.ToString();
                }
                if (m_loadport.p_infoCarrier.p_eTransfer == GemCarrierBase.eTransfer.TransferBlocked) return "OK";
                else return p_sInfo + " infoCarrier.p_eTransfer = " + m_loadport.p_infoCarrier.p_eTransfer.ToString();
            }
        }

        public class Run_SendSlotMap : ModuleRunBase
        {
            Gem_XGem300Pro m_module;
            ILoadport m_loadport;

            public Run_SendSlotMap(Gem_XGem300Pro module)
            {
                m_module = module;
                m_loadport = m_module.m_loadport;
                m_sMap = m_loadport.p_infoCarrier.p_sSlotmap;
                InitModuleRun(module);
            }

            public string m_sMap = "";
            public override ModuleRunBase Clone()
            {
                Run_SendSlotMap run = new Run_SendSlotMap(m_module);
                return run;
            }
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_sMap = tree.Set(m_sMap, m_sMap, "Slotmap", "Slotmap", bVisible);
            }
            public override string Run()
            {
                m_loadport.p_infoCarrier.p_sSlotmap = m_sMap;
                m_loadport.p_infoCarrier.SendSlotMap(m_sMap);
                while (m_loadport.p_infoCarrier.p_eStateSlotMap != GemCarrierBase.eGemState.VerificationOK)
                {
                    Thread.Sleep(10);
                    if (m_loadport.p_infoCarrier.p_eStateSlotMap == GemCarrierBase.eGemState.VerificationFailed)
                        return p_sInfo + " infoCarrier.p_eStateSlotMap = " + m_loadport.p_infoCarrier.p_eStateSlotMap.ToString();
                }
                return "OK";
            }
        }

        public class Run_CreateProcessJob : ModuleRunBase
        {
            Gem_XGem300Pro m_module;
        }
    
        #endregion
    }
}
