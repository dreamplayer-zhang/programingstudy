using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WindII
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
            public const string RecipeRACRootPath = @"C:\Root\Setup\RAC\";

            public const string ImageRootPath = @"D:\Images\";

            public const string RootSetupPath = Root + @"Setup\";
            public const string RootSetupRACPath = RootSetupPath + @"RAC\";
            public const string RootSetupRACAlignKeyPath = RootSetupRACPath + @"AlignKey\";
        }

        public static class FilePath
        {
            public const string SettingFilePath = @"C:\Root\Setting.ini";
        }
    }
}