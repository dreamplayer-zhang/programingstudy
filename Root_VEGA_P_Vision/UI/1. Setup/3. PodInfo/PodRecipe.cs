using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_P_Vision
{ 
    public class PodRecipe
    {
    }
    public class Condition
    {
        bool doinspection;
        int defectcode;
        string defectName;
        #region Property

        public bool DoInspection
        {
            get => doinspection;
            set => doinspection = value;
        }
        public int DefectCode
        {
            get => defectcode;
            set => defectcode = value;
        }
        public string DefectName
        {
            get => defectName;
            set => defectName = value;
        }
        #endregion
    }
}
