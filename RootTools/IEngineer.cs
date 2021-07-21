using RootTools.Control;
using RootTools.GAFs;
using RootTools.Gem;
using RootTools.Memory;
using RootTools.Module;
using RootTools.ToolBoxs;
using System.Collections.Generic;

namespace RootTools
{
    /// <summary> IEngineer : Class 참조를 위한 Interface (AutoMom 역활) </summary>
    public interface IEngineer
    {
        Login.User p_user { get; }

        IGem ClassGem();

        /// <summary> ClassGAF() : Gem/Alarm/FDC 관리, GAF m_gaf = ClassGAF(); </summary>
        GAF ClassGAF();

        IControl ClassControl(); 

        ToolBox ClassToolBox();

        MemoryTool ClassMemoryTool(); 

        IHandler ClassHandler();

        ModuleList ClassModuleList();

        MemoryData GetMemory(string sPool, string sGroup, string sMemory);

        string BuzzerOff();

        string Recovery();

        bool p_bUseXGem { get; set; }
    }
}
