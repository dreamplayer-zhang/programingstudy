using Emgu.CV;
using Emgu.CV.Structure;
using LiveCharts;
using LiveCharts.Wpf;
using RootTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace RootTools_Vision
{
    public partial class Tools
    {
        public static List<CPoint> CircleFitting(List<CPoint> circle_points, int centerX, int centerY, int threshold)
        {
            int points_count = circle_points.Count;
            List<CPoint> newList = new List<CPoint>();
            foreach (CPoint pt in circle_points)
            {
                double radius = Math.Sqrt(Math.Pow(pt.X - centerX, 2) + Math.Pow(pt.Y - centerY, 2));
                if (radius < threshold)
                    newList.Add(pt);
            }


            return newList;
        }

        public static Point FindCircleCenterByPoints(List<Point> circle_points, int centerX, int centerY, int searchLength)
        {
            int points_count = circle_points.Count;
            

            int startX = centerX - searchLength / 2;
            int startY = centerY - searchLength / 2;
            int endX = startX + searchLength;
            int endY = startY + searchLength;

            int minX = centerX, minY = centerX;
            double minStdev = double.MaxValue;

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    double[] radiusArr = new double[points_count];

                    Parallel.For(0, points_count, (i) =>
                    {
                        double radius = Math.Sqrt(Math.Pow(circle_points[i].X - x, 2) + Math.Pow(circle_points[i].Y - y, 2));
                        radiusArr[i] = radius;
                    });

                    double avg = radiusArr.Average();
                    double stdev = Tools.CalcStdev(radiusArr, avg);
                    if(minStdev > stdev)
                    {
                        minStdev = stdev;
                        minX = x;
                        minY = y;
                    }
                }
            }


            return new Point(minX, minY);
        }

        public static double CalcStdev(double[] dataArr, double avg)
        {
            double sdSum = dataArr.Select(val => (val - avg) * (val - avg)).Sum();
            return Math.Sqrt(sdSum / (dataArr.Length - 1));
        }

		public static unsafe bool SaveCircleImage(string filepath, int saveImageWidth, int saveImageHeight, int thickness,
												  SharedBufferInfo ptrMem, int startPos, int endPos)
		{
			try
			{
				Image<Gray, byte> view = new Image<Gray, byte>(saveImageWidth, saveImageHeight);
				int pix_x = 0;
				int pix_y = 0;

				CPoint center = new CPoint(saveImageWidth / 2, saveImageHeight / 2);
				int nLen = thickness; //300;
				int nRadius = saveImageWidth < saveImageHeight ? saveImageWidth : saveImageHeight;
				int nRadius2 = nRadius - nLen;
				int nRadius3 = nRadius - nLen * 2;

				double theta = 0;
				double max = Math.Pow(nRadius / 2, 2);
				double min = Math.Pow(nRadius / 2 - nLen / 2, 2);
				double max2 = Math.Pow(nRadius2 / 2, 2);
				double min2 = Math.Pow(nRadius2 / 2 - nLen / 2, 2);
				double max3 = Math.Pow(nRadius3 / 2, 2);
				double min3 = Math.Pow(nRadius3 / 2 - nLen / 2, 2);
				double dist = 0;
				double offset = 180;

				for (int yy = 0; yy < saveImageHeight; yy++)
				{
					for (int xx = 0; xx < saveImageWidth; xx++)
					{
						dist = Math.Pow(Math.Abs(center.X - xx), 2) + Math.Pow(Math.Abs(center.Y - yy), 2);
						if (dist < max && dist > min)
						{
							theta = Math.Atan2(((double)xx - center.X), ((double)center.Y - yy)) * 180 / Math.PI + offset;
							if (theta < 0)
								theta = theta + 360;

							pix_x = Convert.ToInt32((dist - min) * ptrMem.Width / (max - min));
							pix_y = startPos + Convert.ToInt32(theta * (endPos - startPos) / 360);
							if (pix_y < 500)
								pix_y = 500;

							view.Data[yy, xx, 0] = ((byte*)ptrMem.PtrR_GRAY)[(long)pix_x + (long)pix_y * ptrMem.Width];
						}
					}
				}

				BitmapSource p_ImgSource = ImageHelper.ToBitmapSource(view);

				System.Drawing.Bitmap tempBitmap;
				tempBitmap = BitmapFromSource(p_ImgSource);
				System.Drawing.Bitmap saveBitmap = new System.Drawing.Bitmap(tempBitmap);
				tempBitmap.Dispose();
				tempBitmap = null;

				filepath += ".jpg";

				SaveImageJpg(saveBitmap, filepath, 30);
				return true;
			}
			catch (Exception ee)
			{
				//System.Windows.MessageBox.Show(ee.ToString());
				return false;
			}
		}

		public static unsafe bool SaveEdgeCircleImage(string filepath, int saveImageWidth, int saveImageHeight, int thickness,
												  SharedBufferInfo ptrMemTop, int startPosTop, int endPosTop,
												  SharedBufferInfo ptrMemSide, int startPosSide, int endPosSide,
												  SharedBufferInfo ptrMemBtm, int startPosBtm, int endPosBtm)
		{
			try
			{
				Image<Rgb, byte> view = new Image<Rgb, byte>(saveImageWidth, saveImageHeight);
				int pix_x = 0;
				int pix_y = 0;

				CPoint center = new CPoint(saveImageWidth / 2, saveImageHeight / 2);
				int nLen = thickness; //50;
				int nRadius = saveImageWidth < saveImageHeight ? saveImageWidth : saveImageHeight;
				int nRadius2 = nRadius - nLen;
				int nRadius3 = nRadius - nLen * 2;

				double theta = 0;
				double max = Math.Pow(nRadius / 2, 2);
				double min = Math.Pow(nRadius / 2 - nLen / 2, 2);
				double max2 = Math.Pow(nRadius2 / 2, 2);
				double min2 = Math.Pow(nRadius2 / 2 - nLen / 2, 2);
				double max3 = Math.Pow(nRadius3 / 2, 2);
				double min3 = Math.Pow(nRadius3 / 2 - nLen / 2, 2);
				double dist = 0;
				double offset = 180;

				for (int yy = 0; yy < saveImageHeight; yy++)
				{
					for (int xx = 0; xx < saveImageWidth; xx++)
					{
						dist = Math.Pow(Math.Abs(center.X - xx), 2) + Math.Pow(Math.Abs(center.Y - yy), 2);
						if (dist < max && dist > min)
						{
							theta = Math.Atan2(((double)xx - center.X), ((double)center.Y - yy)) * 180 / Math.PI  + offset;
							if (theta < 0)
								theta = theta + 360;

							pix_x = ptrMemTop.Width - Convert.ToInt32((dist - min) * ptrMemTop.Width / (max - min));    // 이미지 뒤집어서 붙임
							pix_y = startPosTop + Convert.ToInt32(theta * (endPosTop-startPosTop) / 360);
							if (pix_y < 500)
								pix_y = 500;

							view.Data[yy, xx, 0] = ((byte*)ptrMemTop.PtrR_GRAY)[(long)pix_x + (long)pix_y * ptrMemTop.Width];
							view.Data[yy, xx, 1] = ((byte*)ptrMemTop.PtrG)[(long)pix_x + (long)pix_y * ptrMemTop.Width];
							view.Data[yy, xx, 2] = ((byte*)ptrMemTop.PtrB)[(long)pix_x + (long)pix_y * ptrMemTop.Width];
						}

						else if (dist < max2 && dist > min2)
						{
							theta = Math.Atan2(((double)xx - center.X), ((double)center.Y - yy)) * 180 / Math.PI + offset;
							if (theta < 0)
								theta = theta + 360;

							pix_x = (Convert.ToInt32((dist - min2) * ptrMemSide.Width / (max2 - min2)) * 600) / 3000 + 1200;
							pix_y = startPosSide + Convert.ToInt32(theta * (endPosSide - startPosSide) / 360);
							if (pix_y < 500)
								pix_y = 500;

							view.Data[yy, xx, 0] = ((byte*)ptrMemSide.PtrR_GRAY)[(long)pix_x + (long)pix_y * ptrMemSide.Width];
							view.Data[yy, xx, 1] = ((byte*)ptrMemSide.PtrG)[(long)pix_x + (long)pix_y * ptrMemSide.Width];
							view.Data[yy, xx, 2] = ((byte*)ptrMemSide.PtrB)[(long)pix_x + (long)pix_y * ptrMemSide.Width];
						}

						else if (dist < max3 && dist > min3)
						{
							theta = Math.Atan2(((double)xx - center.X), ((double)center.Y - yy)) * 180 / Math.PI + offset;
							if (theta < 0)
								theta = theta + 360;

							pix_x = Convert.ToInt32((dist - min3 + 400) * ptrMemBtm.Width / (max3 - min3));
							pix_y = startPosBtm + Convert.ToInt32(theta * (endPosBtm - startPosBtm) / 360);
							if (pix_y < 500)
								pix_y = 500;

							view.Data[yy, xx, 0] = ((byte*)ptrMemBtm.PtrR_GRAY)[(long)pix_x + (long)pix_y * ptrMemBtm.Width];
							view.Data[yy, xx, 1] = ((byte*)ptrMemBtm.PtrG)[(long)pix_x + (long)pix_y * ptrMemBtm.Width];
							view.Data[yy, xx, 2] = ((byte*)ptrMemBtm.PtrB)[(long)pix_x + (long)pix_y * ptrMemBtm.Width];
						}
					}
				}

				BitmapSource p_ImgSource = ImageHelper.ToBitmapSource(view);

				System.Drawing.Bitmap tempBitmap;
				tempBitmap = BitmapFromSource(p_ImgSource);
				System.Drawing.Bitmap saveBitmap = new System.Drawing.Bitmap(tempBitmap);
				tempBitmap.Dispose();
				tempBitmap = null;

				filepath += ".jpg";

				SaveImageJpg(saveBitmap, filepath, 30);
				return true;
			}
			catch (Exception ee)
			{
				//System.Windows.MessageBox.Show(ee.ToString());
				return false;
			}
		}

		public static ChartValues<float> DataToChartValues(List<float> datas)
		{
			ChartValues<float> values = new ChartValues<float>();
			foreach (float data in datas)
			{
				values.Add(data);
			}

			return values;
		}

		public static void SaveEBRChartToImage(string filepath, List<float> ebrData, List<float> bevelData)
		{
			ChartValues<float> ebr = DataToChartValues(ebrData);
			ChartValues<float> bevel = DataToChartValues(bevelData);

			Application.Current.Dispatcher.Invoke(delegate
			{
				var chart = new LiveCharts.Wpf.CartesianChart
				{
					DisableAnimations = true,
					Width = 800,
					Height = 400,
					//AxisY = new AxesCollection
					//{
					//	new Axis
					//	{
					//		AxisLine = false,
					//		//MinValue = 0,
					//	}
					//}
				};

				chart.Series.Add(new LineSeries 
								{ 
									Values = ebr,
									Fill = System.Windows.Media.Brushes.Transparent,
									
								});
				chart.Series.Add(new LineSeries 
								{ 
									Values = bevel,
									Fill = System.Windows.Media.Brushes.Transparent,
								});

				var viewbox = new System.Windows.Controls.Viewbox();
				viewbox.Child = chart;
				viewbox.Measure(chart.RenderSize);
				viewbox.Arrange(new Rect(new System.Windows.Point(0, 0), chart.RenderSize));
				chart.Update(true, true); //force chart redraw
				viewbox.UpdateLayout();

				filepath += ".jpg";

				var encoder = new JpegBitmapEncoder();
				EncodeVisual(chart, filepath, encoder);
			});

			//chart = null;
			//viewbox = null;
			//encoder = null;
			//bitmap = null;
			//frame = null;
		}

		static void EncodeVisual(FrameworkElement visual, string fileName, BitmapEncoder encoder)
		{
			var bitmap = new RenderTargetBitmap((int)visual.ActualWidth, (int)visual.ActualHeight, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
			bitmap.Render(visual);
			var frame = BitmapFrame.Create(bitmap);
			encoder.Frames.Add(frame);
			using (var stream = File.Create(fileName)) encoder.Save(stream);
		}
	}
}
