using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace RootTools
{
	public class InformationDrawer : SimpleShapeDrawerVM
	{
		public InformationDrawer(ImageViewer_ViewModel _ImageViewer) : base(_ImageViewer)
		{
		}

		public void AddDefectInfo(DefectDataWrapper item)
		{
			AddRectInfo(item.DrawStartPoint.X, item.DrawStartPoint.Y, item.DrawWidth, item.DrawHeight, System.Windows.Media.Brushes.Red, 2);
		}

		private void AddRectInfo(int left, int top, int width, int height, System.Windows.Media.Brush brush, int thickness)
		{
			System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
			rect.Width = width;
			rect.Height = height;
			System.Windows.Controls.Canvas.SetLeft(rect, left);
			System.Windows.Controls.Canvas.SetTop(rect, top);
			rect.StrokeThickness = thickness;
			rect.Stroke = brush;

			m_ListShape.Add(rect);
			m_Element.Add(rect);
			m_ListRect.Add(new UIElementInfo(new System.Windows.Point(left, top), new System.Windows.Point(left + width, top + height)));

			//m_Element.Add(rect);
		}
		public override void Redrawing()
		{
			Shape tempShape;
			System.Windows.Point TopLeft = new System.Windows.Point();
			System.Windows.Point BottomRight = new System.Windows.Point();



			for (int i = 0; i < m_ListShape.Count; i++)
			{
				tempShape = m_ListShape[i];

				TopLeft = GetCanvasPoint(m_ListRect[i].StartPos);
				SetTopLeft(tempShape, TopLeft);
				BottomRight = GetCanvasPoint(m_ListRect[i].EndPos);
				SetBottomRight(tempShape, BottomRight);
			}
		}
	}
}
