using Emgu.CV;
using RootTools.Database;
using RootTools.Memory;
using RootTools_CLR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2.Module
{
	public class SideVision_Inspection
	{
		List<Defect> defect = new List<Defect>();

		public void Inspection(IntPtr ptrSrc, int width, int height, int imageCnt, int defectSize)
		{
			int memW = width;
			int memSize = memW * height;

			byte[] arrSrc = new byte[memSize];
			Marshal.Copy(ptrSrc, arrSrc, 0, arrSrc.Length);

			// profile 생성
			List<int> temp = new List<int>();
			List<int> profile = new List<int>();
			for (long j = 0; j < memW; j++)
			{
				temp.Clear();
				for (long i = 0; i < memSize; i += memW)
				{
					temp.Add(arrSrc[j + i]);
				}
				temp.Sort();
				profile.Add(temp[temp.Count / 2]);  // 중앙값
			}
			//for (long j = 0; j < memW; j++)
			//{
			//	temp.Clear();
			//	Parallel.For(0, height, (i) => 
			//	{ 
			//		temp.Add(arrSrc[j + i * memW]); 
			//	});
			//	temp.Sort();
			//	profile.Add(temp[temp.Count / 2]);  // 중앙값
			//}

			// 평균 영상
			//byte[] avg = new byte[memSize];
			//for (int j = 0; j < height; j++)
			//{
			//	for (int i = 0; i < memW; i++)
			//	{
			//		avg[(j * memW) + i] = (byte)(profile[i]);
			//	}
			//}

			// 차영상 구하기 diff = original - profile 
			byte[] diff = new byte[memSize];
			for (int j = 0; j < height; j++)
			{
				for (int i = 0; i < memW; i++)
				{
					diff[(j * memW) + i] = (byte)(Math.Abs(arrSrc[(j * memW) + i] - profile[i]));
				}
			}

			//byte[] hist = new byte[memSize];
			//int channels = 0;
			//int dims = 1; 
			//int histSize = 256;
			//float[] ranges = new float[] { 0, 255 };
			//CLR_IP.Cpp_Histogram(diff, hist, memW, height, channels, dims, histSize, ranges);
			
			byte[] thresh = new byte[memSize];
			CLR_IP.Cpp_Threshold(diff, thresh, memW, height, false, 15);
			var label = CLR_IP.Cpp_Labeling(diff, thresh, memW, height, true);

			for (int i = 0; i < label.Length; i++)
			{
				if (label[i].area > defectSize)
				{
					Defect d = new Defect(DatabaseManager.Instance.GetInspectionID(),
						10001,
						label[i].area,
						label[i].value,
						label[i].boundLeft,
						(height * imageCnt) + label[i].boundTop,
						Math.Abs(label[i].boundRight - label[i].boundLeft),
						Math.Abs(label[i].boundBottom - label[i].boundTop),
						0,0
						);
					defect.Add(d);
				}
			}

			//test
			//var matThresh = new Mat((int)height, (int)memW, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
			//Marshal.Copy(thresh, 0, matThresh.DataPointer, thresh.Length);
			//matThresh.Save(@"D:\matThresh.bmp");
		}

		public List<Defect> GetDefectList()
		{
			return defect;
		}

		/*
		public void Insp(MemoryData memory, long height)
		{
			long memW = memory.W;
			long memH = height; //memory.p_sz.Y;
			long memSize = memW * height;

			IntPtr ptrSrc = (IntPtr)((long)memory.GetPtr() + memW);
			byte[] arrSrc = new byte[memSize];
			Marshal.Copy(ptrSrc, arrSrc, 0, arrSrc.Length);
			//var matOrigin = new Mat((int)memH, (int)memW, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
			//Marshal.Copy(arrSrc, 0, matOrigin.DataPointer, arrSrc.Length);
			//matOrigin.Save(@"D:\matOrigin.bmp");

			// profile 생성
			List<int> temp = new List<int>();
			List<int> profile = new List<int>();

			for (long j = 0; j < memW; j++)
			{
				temp.Clear();
				for (long i = 0; i < memSize; i += memW)
				{
					temp.Add(arrSrc[j + i]);
				}
				temp.Sort();
				profile.Add(temp[temp.Count / 2]);  // 중앙값
													//profile.Add((int)temp.Average());	// 평균
			}

			// 평균 영상
			//byte[] avg = new byte[memSize];
			//for (int j = 0; j < memH; j++)
			//{
			//	for (int i = 0; i < memW; i++)
			//	{
			//		avg[(j * memW) + i] = (byte)(profile[i]);
			//	}
			//}
			//var matAvg = new Mat((int)memH, (int)memW, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
			//Marshal.Copy(avg, 0, matAvg.DataPointer, avg.Length);
			//matAvg.Save(@"D:\matAvg.bmp");

			// 차영상 구하기 diff = original - profile 
			byte[] diff = new byte[memSize];
			for (int j = 0; j < memH; j++)
			{
				for (int i = 0; i < memW; i++)
				{
					diff[(j * memW) + i] = (byte)(Math.Abs(arrSrc[(j * memW) + i] - profile[i]));
				}
			}
			var matDiff = new Mat((int)memH, (int)memW, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
			Marshal.Copy(diff, 0, matDiff.DataPointer, diff.Length);
			//matDiff.Save(@"D:\matDiff.bmp");

			// 차영상의 histogram 생성
			int[] channels = { 0 };
			int[] histSize = new int[] { 256 };
			float[] ranges = new float[] { 0, 255 };
			Mat matHist = new Mat();
			using (Emgu.CV.Util.VectorOfMat vm = new Emgu.CV.Util.VectorOfMat())
			{
				vm.Push(matDiff);
				CvInvoke.CalcHist(vm, channels, null, matHist, histSize, ranges, false);
			}
			//double minval = 0, maxval = 0;
			//Point minloc = new Point(), maxloc = new Point();
			//CvInvoke.MinMaxLoc(matHist, ref minval, ref maxval, ref minloc, ref maxloc);

			// threshold
			int threshold = 12;
			//for (int i = 0; i < matHist.Rows; i++)
			//{
			//	if ((float)matHist.Row(i).GetData().GetValue(0, 0) == 0)
			//	{ 
			//		threshold = i;
			//		break;
			//	}
			//}
			Mat matThreshold = new Mat();
			CvInvoke.Threshold(matDiff, matThreshold, threshold, 256, Emgu.CV.CvEnum.ThresholdType.Binary);

			// defect 표시
			Mat matColor = new Mat(matThreshold.Height, matThreshold.Width, Emgu.CV.CvEnum.DepthType.Cv8U, 3);
			CvInvoke.CvtColor(matThreshold, matColor, Emgu.CV.CvEnum.ColorConversion.Gray2Rgb);
			Array arrThreshold = matThreshold.GetData();
			for (int j = 0; j < matColor.Height; j++)
			{
				for (int i = 0; i < matColor.Width; i++)
				{
					double val = Convert.ToDouble(arrThreshold.GetValue(j, i));
					if (val != 0.0)
					{
						System.Drawing.Point pt = new System.Drawing.Point(i, j);
						DrawCross(matColor, pt);
					}
				}
			}
			matColor.Save(@"D:\matColor.bmp");
			byte[] arrResult = new byte[memSize];
			CLR_IP.ContourFitEllipse(matThreshold.GetRawData(), arrResult, matThreshold.Width, matThreshold.Height);
		}

		void DrawCross(IInputOutputArray img, System.Drawing.Point point)
		{
			MCvScalar color = new MCvScalar(0, 0, 255); // red
			CvInvoke.Line(img, new System.Drawing.Point(point.X - 1, point.Y), new System.Drawing.Point(point.X + 1, point.Y), color);
			CvInvoke.Line(img, new System.Drawing.Point(point.X, point.Y - 1), new System.Drawing.Point(point.X, point.Y + 1), color);
		}

		void CalcDefectSize()
		{

		}
		*/
	}
}
