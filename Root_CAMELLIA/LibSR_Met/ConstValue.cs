using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_CAMELLIA.LibSR_Met
{
    public class ConstValue
    {
        public const string PATH_LOG = @"C:\Log\Camellia\";
        public const string PATH_PM_RESULT_FOLDER = @"C:\Camellia\PM\PM_Result\";
        public const int SPECTROMETER_MAX_PIXELSIZE = 5001;  //350nm ~ 1500nm (1nm 간격)
        public const int RAWDATA_POINT_MAX_SIZE = 100;

        public const string NONE = "None";

        public const double EV_TO_WAVELENGTH_VALUE = 1239.8116;
        
       
        public const int OUT_OF_RANGE = -1111;
        //2020.12.23 Met.DS 
        public const int MULTI_THREAD_COUNT = 7;
        public const double DELTA = 0.0000000001;//미분 극소변화량
        public const double SI_INIT_THICKNESS = 7750000;
        public const int SI_AVG_OFFSET_RANGE = 80000;
        public const int SI_AVG_OFFSET_STEP = 200;
    }
}
