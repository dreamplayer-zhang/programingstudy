using RootTools.Database;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Root_AOP01_Inspection
{
	/// <summary>
	/// App.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class App : Application
	{
		public static AOP01_Engineer m_engineer = new AOP01_Engineer();

		public static string mPool = "MainVision.Vision Memory";
		public static string mGroup = "MainVision";
		public static string mMainMem = "Main";

		public static string mSideLeftMem = "SideLeft";
		public static string mSideRightMem = "SideRight";
		public static string mSideTopMem = "SideTop";
		public static string mSideBotMem = "SideBot";

		public static string mBackSideLeftMem = "BackSideLeft";
		public static string mBackSideRightMem = "BackSideRight";
		public static string mBackSideTopMem = "BackSideTop";
		public static string mBackSideBotMem = "BackSideBot";
		public static string m45DMem = "TDI45";
		public static string m45DGlassMem = "TDI45";

		public const string MainRegName = "imageMain";
		public const string PellRegName = "image45D";
		public const string GlassRegName = "imageGlass45D";
		public const string SideLeftRegName = "imageSideLeft";
		public const string SideTopRegName = "imageSideTop";
		public const string SideRightRegName = "imageSideRight";
		public const string SideBotRegName = "imageSideBottom";

		public const string MainRecipeRegName = "frontSurfaceRcp";
		public const string SideRecipeRegName = "sideSurfaceRcp";
		public const string PellRecipeRegName = "pellSurfaceRcp";
		public const string BackRecipeRegName = "backSurfaceRcp";

		public const string MainInspMgRegName = "InspectionFront";
		public const string MainInspLeftMgRegName = "InspectionFrontLeft";
		public const string MainInspRightMgRegName = "InspectionFrontRight";
		public const string SideLeftInspMgRegName = "InspectionSideLeft";
		public const string SideTopInspMgRegName = "InspectionSideTop";
		public const string SideRightInspMgRegName = "InspectionSideRight";
		public const string SideBotInspMgRegName = "InspectionSideBot";
		public const string PellInspMgRegName = "InspectionPell";
		public const string BackInspMgRegName = "InspectionBack";

		public static string connection = @"server=localhost;uid=root;password=`ati5344;database=inspections;port=3306;charset=utf8";

		public static string AOPImageRootPath = @"D:\DefectImage";

		public static string MainModuleName = "MainSurfaceInspection";
		public static string MainLeftModuleName = "MainLeftSurfaceInspection";
		public static string MainRightModuleName = "MainRightSurfaceInspection";
		public static string SideLeftModuleName = "SideLeftSurfaceInspection";
		public static string SideRightModuleName = "SideRightSurfaceInspection";
		public static string SideTopModuleName = "SideTopSurfaceInspection";
		public static string SideBotModuleName = "SideBotSurfaceInspection";
		public static string BackModuleName = "BackSurfaceInspection";
		public static string PellicleModuleName = "PellicleSurfaceInspection";

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			var input = DatabaseManager.Instance.SetDatabase(1, "localhost", "Inspections", "root", "`ati5344");
		}
	}
}
