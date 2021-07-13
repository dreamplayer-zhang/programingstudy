using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Root_VEGA_P_Vision
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        public const string mPool = "Vision.Memory";
        public const string mGroup = "Vision";
        public const string mMaskLayer = "MaskLayer";
        public const string mMainGrab = "MainGrab";
        public const string mSideGrab = "SideGrab";
        public const string mStainGrab = "StainGrab";
        public const string mZStack = "ZStackGrab";
        public const string mRotate = "Rotate";
        public const string mVisionAlign = "Vision Align";
        public const string mInspection = "Inspection";

        //FilePath

        //Folder 구조 : RootPath -> PodPK -> Recipe -> Parts Recipe
        public const string RootPath = @"C:\Root\";
        public const string RecipeRootPath = @"C:\Root\Recipe\Vega-P\";
        public const string ImageRootPath = @"D:\Images\";
        public const string SettingFilePath = @"C:\Root\Setting.ini";
        public const string RootSetupPath = RootPath + @"Setup\";

        //RecipeName
        public static readonly string[] RecipeNames = { "RecipeCoverFront", "RecipeCoverBack", "RecipePlateFront", "RecipePlateBack" };
    }
}
