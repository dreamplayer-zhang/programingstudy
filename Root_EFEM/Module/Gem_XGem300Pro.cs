using RootTools;
using RootTools.Gem;
using RootTools.Module;
using RootTools.OHTNew;
using RootTools.Trees;
using System.Collections.Generic;
using System.Threading;

namespace Root_EFEM.Module
{
    public class Gem_XGem300Pro : ModuleBase
    {
        //ILoadport m_loadport;
        List<ILoadport> m_aLoadport = new List<ILoadport>();
        //public OHT_Semi[] m_OHT = new OHT_Semi[3];
        public Gem_XGem300Pro(string id, IEngineer engineer, IGem gem, List<ILoadport> loadport)
        {
            p_id = id;
            m_engineer = engineer;
            m_gem = gem;
            m_aLoadport = loadport;

            this.InitBase(id, engineer);
        }

        public override void GetTools(bool bInit)
        {
            //for(int i=0; i<m_aLoadport.Count; i++)
            //{
            //    p_sInfo = m_toolBox.Get(ref m_OHT[i], this, m_aLoadport[i].p_infoCarrier, "OHT");
            //}
        }

        #region StateHome
        public override string StateHome()
        {
            return "OK";
        }
        #endregion


        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_SendCarrierID(this), false, "Carrier ID Send Event");
            AddModuleRunList(new Run_SendSlotMap(this), false, "Carrier Slotmap Send Event");
            AddModuleRunList(new Run_SetEvent(this), true, "Set Event");
            AddModuleRunList(new Run_ReadyToLoad(this), false, "CMS ReadyToLoad");
            AddModuleRunList(new Run_ReadyToUnload(this), false, "CMS ReadyToUnload");
        }

        public class Run_SendCarrierID : ModuleRunBase
        {
            Gem_XGem300Pro m_module;
            ILoadport m_loadport;
            public Run_SendCarrierID(Gem_XGem300Pro module)
            {
                m_module = module;
                m_loadport = m_module.m_aLoadport[EQ.p_nRunLP];
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
                m_loadport = m_module.m_aLoadport[EQ.p_nRunLP];
                //m_sMap = m_loadport.p_infoCarrier.p_sSlotmap;
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
                //m_loadport.p_infoCarrier.p_sSlotmap = m_sMap;
                m_loadport.p_infoCarrier.SendSlotMap();
                while (m_loadport.p_infoCarrier.p_eStateSlotMap != GemCarrierBase.eGemState.VerificationOK)
                {
                    Thread.Sleep(10);
                    if (m_loadport.p_infoCarrier.p_eStateSlotMap == GemCarrierBase.eGemState.VerificationFailed)
                        return p_sInfo + " infoCarrier.p_eStateSlotMap = " + m_loadport.p_infoCarrier.p_eStateSlotMap.ToString();
                }
                return "OK";
            }
        }

        public class Run_SetEvent : ModuleRunBase
        {
            Gem_XGem300Pro m_module;
            IGem m_gem;
            public Run_SetEvent(Gem_XGem300Pro module)
            {
                m_module = module;
                m_gem = m_module.m_gem;
                InitModuleRun(module);
            }

            long nCEID = 0;
            public override ModuleRunBase Clone()
            {
                Run_SetEvent run = new Run_SetEvent(m_module);
                return run;
            }
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                nCEID = tree.Set(nCEID, nCEID, "Send Event", "Send Event", bVisible);
            }
            public override string Run()
            {
                if (m_gem.SetCEID(nCEID) != 0) return p_sInfo + " CEID : " + nCEID.ToString();
                return "OK";
            }
        }

        public class Run_ReadyToLoad : ModuleRunBase
        {
            Gem_XGem300Pro m_module;
            ILoadport m_loadport;
            IGem m_gem;
            public Run_ReadyToLoad(Gem_XGem300Pro module)
            {
                m_module = module;
                m_loadport = m_module.m_aLoadport[EQ.p_nRunLP];
                m_gem = m_module.m_gem;
                InitModuleRun(module);
            }
            public override ModuleRunBase Clone()
            {
                Run_ReadyToLoad run = new Run_ReadyToLoad(m_module);
                return run;
            }
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                
            }
            public override string Run()
            {
                if (m_gem.CMSSetReadyToLoad(m_loadport.p_infoCarrier) != "OK") return p_sInfo + "";
                return "OK";
            }
        }

        public class Run_ReadyToUnload : ModuleRunBase
        {
            Gem_XGem300Pro m_module;
            ILoadport m_loadport;
            IGem m_gem;

            public Run_ReadyToUnload(Gem_XGem300Pro module)
            {
                m_module = module;
                m_loadport = m_module.m_aLoadport[EQ.p_nRunLP];
                m_gem = m_module.m_gem;
                InitModuleRun(module);
            }
            public override ModuleRunBase Clone()
            {
                Run_ReadyToUnload run = new Run_ReadyToUnload(m_module);
                return run;
            }
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                
            }
            public override string Run()
            {
                if (m_gem.CMSSetReadyToUnload(m_loadport.p_infoCarrier) != "OK") return p_sInfo + "";
                return "OK";
            }
        }

        #endregion
    }
}
