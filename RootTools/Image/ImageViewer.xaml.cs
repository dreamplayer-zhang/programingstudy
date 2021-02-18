using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RootTools
{
	/// <summary>
	/// ImageViewer.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class ImageViewer : UserControl
	{
		public ImageViewer()
		{
			InitializeComponent();
			SetVisual(this);
		}
		public void SetVisual(Visual visual)
		{
			canvas.visual = visual;
		}
		//private void Viewer_KeyDown(object sender, KeyEventArgs e)
		//{
		//    ImageViewer_ViewModel vm = (ImageViewer_ViewModel)this.DataContext;
		//    vm.KeyEvent = e;
		//}
		public void AddBlock(double dCenterX, double dCenterY, double width, double height, Brush color,Pen lineColor)
		{
			//canvas.rects.Add(new CustomCanvas.CustomRect { Pen = lineColor, Brush = color, Rect = new Rect(left, top, width, height) });
			canvas.rects.Add(new CustomCanvas.CustomRect { Pen = lineColor, Brush = color, Rect = new Rect(dCenterX - (width / 2), dCenterY - (height / 2), width, height) });
		}

		public void ClearRect()
		{
			canvas.rects.Clear();
		}

		public void RefreshDraw()
		{
			var temp_vm = (ImageViewer_ViewModel)this.DataContext;
			canvas.StartPoint = temp_vm.p_View_Rect;//여기에 왼쪽위 메모리 주소같은게 필요함
			canvas.ImageSize = temp_vm.p_ImageData.p_Size;
			canvas.Zoom = temp_vm.p_Zoom;
			canvas.InvalidateVisual();
		}
	}
}
