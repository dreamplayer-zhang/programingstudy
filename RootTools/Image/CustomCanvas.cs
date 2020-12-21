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
			public double Scale;
			public Rect ScaledRect
			{
				get
				{
					Rect temp = new Rect();
					temp.Width = Rect.Width / Scale;//이건 맞음
					temp.Height = Rect.Height / Scale;//이것도 맞음
					temp.X = (Rect.X - StartPos.X) / Scale - (temp.Width / 2.0);
					temp.Y = (Rect.Y - StartPos.Y) / Scale - (temp.Height / 2.0);

					return temp;
				}
			}

			public Size Offset { get; internal set; }
			public System.Drawing.Rectangle StartPos { get; internal set; }
			public double Zoom { get; internal set; }
		}

		public ObservableCollection<CustomRect> rects = new ObservableCollection<CustomRect>();

		protected override void OnRender(System.Windows.Media.DrawingContext dc)
		{
			base.OnRender(dc);
			//StartPoint 얘는 메모리 주소가 맞음
			//rect에 잇는것도 메모리 주소가 맞음
			if (rects.Count == 0)
				return;

			var scale = ImageSize.X / this.RenderSize.Width * Zoom;//때려맞춘 비율값
			if (scale == 0)
			{
				scale = 1;
			}
			for (int i = 0; i < rects.Count; i++)
			{
				var memoryArea = new Rect(StartPoint.X, StartPoint.Y, StartPoint.Width, StartPoint.Height);
				if (memoryArea.IntersectsWith(rects[i].Rect))
				{
					//보고 있는 메모리와 동일한경우 화면상에 출력
					CustomRect mRect = rects[i];
					mRect.Scale = scale;
					mRect.StartPos = StartPoint;

					if (mRect.ScaledRect.Top > 5)
                    {
						Rect rtScaled = new Rect(mRect.ScaledRect.Left, mRect.ScaledRect.Top - 5, mRect.ScaledRect.Width, mRect.ScaledRect.Height);
						dc.DrawRectangle(null, mRect.Pen, rtScaled);
					}
                    else
                    {
						dc.DrawRectangle(null, mRect.Pen, mRect.ScaledRect);
					}
					//dc.DrawRectangle(null, mRect.Pen, mRect.ScaledRect);
				}
			}
		}
	}
}
