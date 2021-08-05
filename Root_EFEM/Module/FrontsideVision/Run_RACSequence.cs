using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_EFEM.Module
{
    public class Run_RACSequence : ModuleRunBase
    {
        Vision_Frontside m_module;
        public Run_RACSequence(Vision_Frontside module)
        {
            this.m_module = module;
        }

        public override ModuleRunBase Clone()
        {
            Run_RACSequence run = new Run_RACSequence(this.m_module);

            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            // Read RAC Data

            double waferCenterX, waferCenterY;


            // WAFER CENTERING ( Auto Focus )
            double waferEdgeLeft, waferEdgeRight, waferEdgeTop, waferEdgeBottm;


            // VRS ALIGN



            // Coarse Align ( Auto illume )



            // Fine Align



            // Refind Center



            // Recipe Teach ( Auto Illum )

            // Release Recipe

        }

        public override string Run()
        {
            return "OK";
        }
    }
}
