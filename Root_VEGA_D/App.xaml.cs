using Root_VEGA_D.Engineer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Root_VEGA_D
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
	{
		public static string MainFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
		public static VEGA_D_Engineer m_engineer = new VEGA_D_Engineer();
		static App()
        {
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
			string path = Path.Combine(MainFolder, "Log", "error.log");
			#endregion

			try
			{
				var message = string.Format(messageFormat, DateTimeOffset.Now, sender, exception);

				Debug.WriteLine(message);
				if (!Directory.Exists(Path.Combine(MainFolder, "Log")))
					Directory.CreateDirectory(Path.Combine(MainFolder, "Log"));
				File.AppendAllText(path, message);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
		}
	}
}
