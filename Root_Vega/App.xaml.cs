using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
		public static string[] m_sideMem = new string[] { "SideTop", "SideLeft", "SideRight", "SideBottom" };
		public static string[] m_bevelMem = new string[] { "BevelTop", "BevelLeft", "BevelRight", "BevelBottom" };
		public const string sSidePool = "SideVision.Memory";
		public const string sSideGroup = "Grab";

		public const string sPatternPool = "pool";
		public const string sPatternGroup = "group";
		public const string sPatternmem = "mem";

		public static string MainFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
	}
}
