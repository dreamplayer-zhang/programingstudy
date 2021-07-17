using RootTools;
using RootTools.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_JEDI_Vision.Module
{
    public class Vision3D : ModuleBase
    {
        public eVision p_eVision { get; set; }
        public Vision3D(eVision eVision, IEngineer engineer, eRemote eRemote)
        {
            p_eVision = eVision;

        }
    }
}
