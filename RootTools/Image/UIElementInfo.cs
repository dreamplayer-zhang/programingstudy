using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools
{
	public class UIElementInfo
	{
		//Location : Start point (메모리 기준)
		//Width, Height (End point)


		public UIElementInfo()
		{

		}
		public UIElementInfo(System.Windows.Point start, System.Windows.Point end)
		{
			this.StartPos = start;
			this.EndPos = end;
		}
		/// <summary>
		/// UI Element의 시작점
		/// </summary>
		public System.Windows.Point StartPos;
		/// <summary>
		/// UI Element의 종착점
		/// </summary>
		public System.Windows.Point EndPos;
	}
}
