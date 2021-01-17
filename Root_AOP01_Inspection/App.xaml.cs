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
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			var input = DatabaseManager.Instance.SetDatabase(1, "localhost", "Inspections", "root", "`ati5344");
		}
	}
}
