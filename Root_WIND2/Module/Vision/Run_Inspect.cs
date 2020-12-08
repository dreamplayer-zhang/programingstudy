using RootTools;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2.Module
{
    public class Run_Inspect : ModuleRunBase
    {
        Vision m_module;

        string m_sRecipeName = string.Empty;

        #region [Getter Setter]
        public string RecipeName { get => m_sRecipeName; set => m_sRecipeName = value; }
        #endregion

        public Run_Inspect(Vision module)
        {
            m_module = module;
            InitModuleRun(module);
        }

        public override ModuleRunBase Clone()
        {
            Run_Inspect run = new Run_Inspect(m_module);
            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            m_sRecipeName = tree.SetFile(m_sRecipeName, m_sRecipeName, "rcp", "Recipe", "Recipe Name", bVisible);
        }

        public override string Run()
        {
            return "OK";
        }
    }
}
