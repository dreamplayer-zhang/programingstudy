using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_CAMELLIA.LibSR_Met
{
    public class ConstValue
    {
        //PM
        public const string PATH_LOG = @"C:\Log\";
        public const string PATH_PM_RESULT_FOLDER = @"C:\Camellia2\PM\PM_Result\";
        public const string PATH_PM_FILE = @"C:\Camellia2\Init\PM.cpm";
        public const string PATH_PM_REFLECTANCE_FILE = @"C:\Camellia2\Init\PMReflectance.csv";
        public const string PATH_MONITORING_RESULT_FOLDER = @"C:\Camellia2\Data\Monitoring_Result\";
        public const string PATH_LAMP_INITIAL_FILE = @"C:\Camellia2\Init\Timedata.txt";
        public const int PM_REFLECTANCE_CHECK_WAVELENGTH_COUNT = 10;
        //PM-Align
        public const double PM_CAMERA_PIXEL_RESOLUTION = 1.098;
        public const double PM_STAGE_PULSE = 10;
        //
        public const int SPECTROMETER_MAX_PIXELSIZE = 5001;  
        //350nm ~ 1500nm (1nm 간격)
        //5001 값 바꾸면   SR_Background Measure 함수에서 에러 발생
        public const int RAWDATA_POINT_MAX_SIZE = 3000;
        public const int RE_SNAP_COUNT = 5;

        public const string NONE = "None";

        public const double EV_TO_WAVELENGTH_VALUE = 1239.8116;
        
       
        public const int OUT_OF_RANGE = -1111;
        //Transmittance
        public const int NUM_OF_MATERIAL_DATANUM = 400;
        public const int MULTI_THREAD_COUNT = 7;
        public const double DELTA = 0.0000000001;//미분 극소변화량
        public const double SI_INIT_THICKNESS = 7750000;
        public const int SI_AVG_OFFSET_RANGE = 80000;
        public const int SI_AVG_OFFSET_STEP = 200;
    }
}
