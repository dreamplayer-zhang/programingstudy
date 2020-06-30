using RootTools;
using RootTools.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_EFEM.Module
{
    public class WTR_RND : ModuleBase, IWTR
    {
        public void AddChild(params IWTRChild[] childs)
        {
        }

        public void ReadInfoReticle_Registry()
        {
        }

        public WTR_RND(string id, IEngineer engineer)
        {
//            InitCmd();
//            InitMotion();
//            InitArms(id);
            base.InitBase(id, engineer);
        }
    }
}
