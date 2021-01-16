using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RootTools
{
	public class CustomCanvas : Canvas
	{
		public Visual visual { get; set; }
		public double Zoom { get; set; }
		public CPoint ImageSize = new CPoint();
		/// <summary>
		/// 현재 보고있는 메모리 주소 영역
		/// </summary>
		public System.Drawing.Rectangle StartPoint
		{
			get;
			internal set;
		}
		public class CustomRect
		{
			public Rect Rect;
			public Brush Brush;
			public Pen Pen;
			public double Zoom;
			internal Rect MemorySize;

			public Rect ScaledRect
			{
				get
				{
					Rect temp = new Rect();
					var point = GetCanvasPoint(new CPoint(Rect));
					//temp.Width = Rect.Width / ;//이건 맞음
					//temp.Height = Rect.Height / Scale;//이것도 맞음
					temp.Width = 10;
					temp.Height = 10;
					temp.X = point.X;
					temp.Y = point.Y;
					temp.Width = Rect.Width * RenderSize.Width / MemorySize.Width;
					temp.Height = Rect.Height * RenderSize.Width / MemorySize.Width;

					return temp;
				}
			}

			public Size Offset { get; internal set; }
			public System.Drawing.Rectangle StartPos { get; internal set; }
			public Size RenderSize { get; internal set; }

			CPoint GetCanvasPoint(CPoint memPt)
			{
				if (MemorySize.Width > 0 && MemorySize.Height > 0)
				{
					int nX = (int)((memPt.X - MemorySize.X) * RenderSize.Width / MemorySize.Width - (RenderSize.Width / MemorySize.Width) / 2.0);
					int nY = (int)((memPt.Y - MemorySize.Y) * RenderSize.Height / MemorySize.Height - (RenderSize.Height / MemorySize.Height) / 2.0);
					return new CPoint(nX, nY);
				}
				return new CPoint(0, 0);
			}
		}

		public ObservableCollection<CustomRect> rects = new ObservableCollection<CustomRect>();

		protected override void OnRender(System.Windows.Media.DrawingContext dc)
		{
			base.OnRender(dc);
			//StartPoint 얘는 메모리 주소가 맞음
			//rect에 잇는것도 메모리 주소가 맞음
			if (rects.Count == 0)
				return;

			var bRatio_WH = (double)ImageSize.X / this.ActualWidth < (double)ImageSize.Y / this.ActualWidth;

			for (int i = 0; i < rects.Count; i++)
			{
				var memoryArea = new Rect(StartPoint.X, StartPoint.Y, StartPoint.Width, StartPoint.Height);
				if (memoryArea.IntersectsWith(rects[i].Rect))
				{
					//보고 있는 메모리와 동일한경우 화면상에 출력
					CustomRect mRect = rects[i];
					mRect.MemorySize = memoryArea;
					mRect.RenderSize = new Size(this.ActualWidth, this.ActualHeight);
					mRect.StartPos = StartPoint;
					mRect.Zoom = Zoom;

					dc.DrawRectangle(null, mRect.Pen, mRect.ScaledRect);
				}
			}
		}
	}
}
