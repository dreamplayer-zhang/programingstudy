using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Root_Vega
{
	/// <summary>
	/// App.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class App : Application
	{
		public static Vega_Engineer m_engineer = new Vega_Engineer();

		public static string[] m_sideMem = new string[] { "SideTop", "SideLeft", "SideRight", "SideBottom" };
		public static string[] m_bevelMem = new string[] { "BevelTop", "BevelLeft", "BevelRight", "BevelBottom" };
		public const string sSidePool = "SideVision.Memory";
		public const string sSideGroup = "Grab";

		public const string sPatternPool = "PatternVision.Memory";
		public const string sPatternGroup = "PatternVision";
		public const string sPatternmem = "Main";

		public const string sEBRPool = "PatternVision.Memory";
		public const string sEBRGroup = "PatternVision";
		public const string sEBRmem = "Main";

		public const string sD2DPool = "sD2D.Memory";
		public const string sD2DGroup = "sD2D";

		//public static string[] sD2Dmem;
		public const string sD2Dmem = "D2dMem";
		public const string sD2DABSmem = "D2dABSMem";

		public static string MainFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

		public static string indexFilePath = @"C:\vsdb\init\SearchIndex.sqlite";
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			AppDomain.CurrentDomain.UnhandledException += (sender, arg) => ReportException(sender, arg.ExceptionObject as Exception);
		}
		public static void ReportException(object sender, Exception exception)
		{
			#region const
			const string messageFormat = @"
===========================================================
ERROR, date = {0}, sender = {1},
{2}
";
			string path = Path.Combine(MainFolder, "error.log");
			#endregion

			try
			{
				var message = string.Format(messageFormat, DateTimeOffset.Now, sender, exception);

				Debug.WriteLine(message);
				File.AppendAllText(path, message);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
		}
	}
}
