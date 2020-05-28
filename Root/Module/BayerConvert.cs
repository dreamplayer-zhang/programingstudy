﻿using RootTools;
using RootTools.Camera;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;

namespace Root.Module
{
    public class BayerConvert : ModuleBase
    {
        #region ToolBox
        MemoryPool m_memoryPool;
        MemoryGroup m_memoryGroup;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory");
            m_memoryGroup = m_memoryPool.GetGroup(p_id);
        }
        #endregion

        #region Memory
        CPoint m_szROI = new CPoint(2040, 2040);
        MemoryData m_memoryBayer = null;
        MemoryData m_memoryRGB = null;
        MemoryData m_memoryBMP = null;
        void InitMemory()
        {
            m_memoryBayer = m_memoryGroup.CreateMemory("Bayer", 1, 1, m_szROI);
            m_memoryRGB = m_memoryGroup.CreateMemory("RGB", 1, 3, m_szROI);
            m_memoryBMP = m_memoryGroup.CreateMemory("BMP", 1, 3, m_szROI);
        }
        #endregion

        #region Convert
        Bayer2RGB m_converter = new Bayer2RGB();
        public void RunConvert()
        {
            StopWatch sw = new StopWatch(); 
            for (int n = 0; n < 100; n++) p_sInfo = m_converter.Convert(m_memoryBayer, 0, m_memoryRGB, 0);
            m_log.Info("Convert Time : " + sw.ElapsedMilliseconds.ToString()); 
        }
        #endregion

        public BayerConvert(string id, IEngineer engineer)
        {
            InitBase(id, engineer);
            InitMemory(); 
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Convert(this), false, "Convert Bayer -> RGB");
        }

        public class Run_Convert : ModuleRunBase
        {
            BayerConvert m_module;
            public Run_Convert(BayerConvert module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Convert run = new Run_Convert(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                m_module.RunConvert(); 
                return "OK";            
            }
        }
        #endregion
    }
}
