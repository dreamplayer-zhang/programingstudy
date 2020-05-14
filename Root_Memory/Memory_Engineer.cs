﻿using RootTools;
using RootTools.GAFs;
using RootTools.Gem;
using RootTools.Memory;
using RootTools.Module;
using RootTools.ToolBoxs;

namespace Root_Memory
{
    public class Memory_Engineer : IEngineer
    {
        #region IEngineer
        public Login m_login = new Login();
        public Login.User p_user
        {
            get { return m_login.p_user; }
        }

        public IGem ClassGem() { return null; }

        public GAF ClassGAF() { return null; }

        public ToolBox ClassToolBox() { return null; }

        public MemoryTool m_memoryTool;
        public MemoryTool ClassMemoryTool() { return m_memoryTool; }

        public IHandler ClassHandler() { return null; }

        public ModuleList ClassModuleList() { return null; }

        public MemoryData GetMemory(string sPool, string sGroup, string sMemory)
        {
            MemoryPool pool = m_memoryTool.GetPool(sPool);
            return (pool == null) ? null : pool.GetMemory(sGroup, sMemory);
        }

        #endregion

        public void Init(string id)
        {
            EQ.m_sModel = id;
            LogViewer.Init();
            m_login.Init();
            m_memoryTool = new MemoryTool(id, this);
        }

        public void ThreadStop()
        {
            m_memoryTool.ThreadStop();
            m_login.ThreadStop();
            LogViewer.ThreadStop();
        }
    }
}
