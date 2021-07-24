using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace Root_VEGA_D.Module.Recipe
{
	/// <summary>
	/// xml형식으로된 position recipe를 parse하는 class
	/// </summary>
	public class PosRecipeData
	{
		public Dictionary<string, Point> DieInfo { get; private set; }
		/// <summary>
		/// 지정된 XML파일을 Open한다
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public bool Load(string filePath = @"D:\recipe.xml")
		{
			if (!File.Exists(filePath))
				return false;
			DieInfo = new Dictionary<string, Point>();

			var xdoc = new XmlDocument();
			xdoc.Load(filePath);
			var nodes = xdoc.SelectNodes("/Points");//root이름을 Points로 가정. 구조에 따라 변경될 수 있음
			foreach (XmlNode item in nodes[0])//root내의 모든 node를 순회하면서 Attributes를 추출. 구조에 따라 변경될 수 있음
			{
				var PointType = item.Attributes["PointType"];
				var x = item.Attributes["x"];
				var y = item.Attributes["y"];

				Debug.WriteLine(PointType.Value);
				Debug.WriteLine(x.Value);
				Debug.WriteLine(y.Value);
				DieInfo.Add(PointType.Value, new Point(double.Parse(x.Value), double.Parse(y.Value)));
			}
			return true;
		}
	}


	/*
	Sample Data
	<Points>
		<Point PointType = "FirstDieLeft" x = "12458" y = "25556"/>
		<Point PointType = "FirstDieRight" x = "30430" y = "25556"/>
		<Point PointType = "SecondDieLeft" x = "30710" y = "25556"/>
		<Point PointType = "LastDieRight" x = "139942" y = "25556"/>
		<Point PointType = "FirstDieBottom" x = "139942" y = "25556"/>
		<Point PointType = "FirstDieUp" x = "139942" y = "76060"/>
		<Point PointType = "SecondDieBottom" x = "139942" y = "76340"/>
		<Point PointType = "LastDieUp" x = "139942" y = "126844"/>
	</Points>
	 */
}
