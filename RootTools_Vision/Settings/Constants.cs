using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public static class Constants
    {
        public static class Memory
        {
            public const string Name = "AAAA";
        }

        public static class RootPath
        {
            public const string RecipeRootPath = @"C:\Root\\Recipe\";
            public const string RecipeFrontRootPath = @"C:\Root\Recipe\Front\";
            public const string RecipeBackRootPath = @"C:\Root\Recipe\Back\";
            public const string RecipeEdgeRootPath = @"C:\Root\Recipe\Edge\";
            public const string RecipeEBRRootPath = @"C:\Root\Recipe\EBR\";

            public const string ImageRootPath = @"D:\Images\";

        }

        public static class FilePath
        {
            public const string SettingFilePath = @"C:\Root\Setting.ini";
            public const string DefectCodeFilePath = @"C:\Root\DefectCode.ini";
            public const string KlarfSettingFilePath = @"C:\Root\KlarfSetting.xml";
        }

        public static class ASCWaferMap
        {
            public const int WAFER_MAP_FLATZONE_LEFT = 1;
            public const int WAFER_MAP_FLATZONE_RIGHT = 2;
            public const int WAFER_MAP_FLATZONE_TOP = 3;
            public const int WAFER_MAP_FLATZONE_BOTTOM = 4;

            public const int WAFER_MAP_CHIP_GOOD_1 = 1;
            public const int WAFER_MAP_CHIP_DIE_1 = 2;
            public const int WAFER_MAP_CHIP_DIE_2 = 3;
            public const int WAFER_MAP_CHIP_FLATZONE = 4;
            public const int WAFER_MAP_CHIP_GOOD_2 = 5;

            public const int WAFER_MAP_CHIP_NOCHIP = 255;
            public const int WAFER_MAP_CHIP_DEFECT = 254;
            public const int WAFER_MAP_CHIP_ALIGN = 253;
            public const int WAFER_MAP_CHIP_REFERENCE = 252;
            public const int WAFER_MAP_CHIP_PSMARK = 251;
            public const int WAFER_MAP_CHIP_INVALID = 250;
            public const int WAFER_MAP_CHIP_EXT_0 = 0;
            public const int WAFER_MAP_CHIP_EXT_6 = 6;
            public const int WAFER_MAP_CHIP_EXT_7 = 7;
            public const int WAFER_MAP_CHIP_EXT_8 = 8;
            public const int WAFER_MAP_CHIP_EXT_9 = 9;

            //public const int WAFER_MAP_CHIP_EXT_B = 11;
            public const int WAFER_MAP_CHIP_BEF_DEFECT = 11;
            public const int WAFER_MAP_CHIP_EXT_C = 12;
            public const int WAFER_MAP_CHIP_EXT_E = 13;
            public const int WAFER_MAP_CHIP_EXT_F = 14;
            public const int WAFER_MAP_CHIP_EXT_G = 15;
            public const int WAFER_MAP_CHIP_EXT_H = 16;
            public const int WAFER_MAP_CHIP_EXT_I = 17;
            public const int WAFER_MAP_CHIP_EXT_J = 18;
            public const int WAFER_MAP_CHIP_EXT_K = 19;
            public const int WAFER_MAP_CHIP_EXT_L = 20;
            public const int WAFER_MAP_CHIP_EXT_M = 21;
            public const int WAFER_MAP_CHIP_EXT_N = 22;
            public const int WAFER_MAP_CHIP_EXT_O = 23;
            public const int WAFER_MAP_CHIP_EXT_P = 24;
            public const int WAFER_MAP_CHIP_EXT_Q = 25;
            public const int WAFER_MAP_CHIP_EXT_S = 26;
            public const int WAFER_MAP_CHIP_EXT_T = 27;
            public const int WAFER_MAP_CHIP_EXT_U = 28;
            public const int WAFER_MAP_CHIP_EXT_V = 29;
            public const int WAFER_MAP_CHIP_EXT_W = 30;
            public const int WAFER_MAP_CHIP_EXT_X = 31;
            public const int WAFER_MAP_CHIP_EXT_Y = 32;
            public const int WAFER_MAP_CHIP_EXT_Z = 33;

            public const int SAMPLE_MAP_CHIP_UNSELECTED = 0;
            public const int SAMPLE_MAP_CHIP_SELECTED = 1;
            public const int SAMPLE_MAP_CHIP_INVALID = 2;
            public const int SAMPLE_MAP_CHIP_DISCOLOR = 3;
        }
    }
}
