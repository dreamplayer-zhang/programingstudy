using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2
{
    public static class Constants
    {
        public static class Memory
        {
            public const string Name = "AAAA";
        }

        public static class RootPath
        {
            public const string Root = @"C:\Root\";
            public const string RecipeRootPath = @"C:\Root\Recipe\";
            public const string RecipeFrontRootPath = @"C:\Root\Recipe\Front\";
            public const string RecipeBackRootPath = @"C:\Root\Recipe\Back\";
            public const string RecipeEdgeRootPath = @"C:\Root\Recipe\Edge\";
            public const string RecipeEBRRootPath = @"C:\Root\Recipe\EBR\";
            public const string RecipeAlignRootPath = @"C:\Root\Recipe\Align\";

            public const string ImageRootPath = @"D:\Images\";

            public const string RootSetupPath = Root + @"Setup\";
        }

        public static class FileName
        {
            public const string BacksideExclusiveRegionFileName = "Backside_ExclusivePolygonList.xml";
            public const string BacksideCenterPointFileName = "Backside_CenterPoint.xml";
            public const string SettingFilePath = @"C:\Root\Setting.ini";
            public const string DefectCodeFilePath = @"C:\Root\DefectCode.ini";
        }

        public static class FilePath
        {
            public const string SettingFilePath = @"C:\Root\Setting.ini";
            public const string BacksideExclusiveRegionFilePath = Constants.RootPath.RootSetupPath + FileName.BacksideExclusiveRegionFileName;
            public const string BacksideCenterPointFilePath = Constants.RootPath.RootSetupPath + FileName.BacksideCenterPointFileName;
        }


    }
}
