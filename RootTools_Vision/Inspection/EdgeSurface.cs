using RootTools;
using RootTools.Database;
using RootTools_CLR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools_Vision
{
	public class EdgeSurface : WorkBase
	{
		public override WORK_TYPE Type => WORK_TYPE.INSPECTION;

		private EdgeSurfaceParameter parameterEdge;
		private EdgeSurfaceRecipe recipeEdge;

		private GrabModeEdge grabModeTop;
		private GrabModeEdge grabModeSide;
		private GrabModeEdge grabModeBtm;

		public enum EdgeMapPositionX
		{
			Top = 0,
			Side = 1,
			Btm = 2
		}

		public EdgeSurface() : base()
		{
			m_sName = this.GetType().Name;
		}

		public void SetGrabMode(GrabModeBase top, GrabModeBase side, GrabModeBase btm)
		{
			grabModeTop = (GrabModeEdge)top;
			grabModeSide = (GrabModeEdge)side;
			grabModeBtm = (GrabModeEdge)btm;
		}

		public override WorkBase Clone()
		{
			return (WorkBase)this.MemberwiseClone();
		}

		protected override bool Preparation()
		{
			if (this.parameterEdge == null || this.recipeEdge == null)
			{
				this.parameterEdge = this.parameter as EdgeSurfaceParameter;
				this.recipeEdge = recipe.GetItem<EdgeSurfaceRecipe>();
			}
			return true;
		}

		protected override bool Execution()
		{
			DoInspection();
			return true;
		}

		public void DoInspection()
		{
			if (this.currentWorkplace.Index == 0)
				return;

			EdgeSurfaceParameterBase paramTop = parameterEdge.EdgeParamBaseTop;
			EdgeSurfaceParameterBase paramBottom = parameterEdge.EdgeParamBaseBtm;
			EdgeSurfaceParameterBase paramSide = parameterEdge.EdgeParamBaseSide;

			WorkEventManager.OnInspectionStart(this.currentWorkplace, new InspectionStartArgs());

			if (paramTop.ChR)
				DoColorInspection(paramTop, 0);
			if (paramTop.ChG)
				DoColorInspection(paramTop, 1);
			if (paramTop.ChB)
				DoColorInspection(paramTop, 2);
			WorkEventManager.OnInspectionDone(this.currentWorkplace, new InspectionDoneEventArgs(new List<CRect>())); // 나중에 ProcessDefect쪽 EVENT로...

			if (paramBottom.ChR)
				DoColorInspection(paramBottom, 3);
			if (paramBottom.ChG)
				DoColorInspection(paramBottom, 4);
			if (paramBottom.ChB)
				DoColorInspection(paramBottom, 5);
			WorkEventManager.OnInspectionDone(this.currentWorkplace, new InspectionDoneEventArgs(new List<CRect>())); // 나중에 ProcessDefect쪽 EVENT로...

			if (paramSide.ChR)
				DoColorInspection(paramSide, 6);
			if (paramSide.ChG)
				DoColorInspection(paramSide, 7);
			if (paramSide.ChB)
				DoColorInspection(paramSide, 8);
			WorkEventManager.OnInspectionDone(this.currentWorkplace, new InspectionDoneEventArgs(new List<CRect>())); // 나중에 ProcessDefect쪽 EVENT로...
		}

		public enum EdgeDefect
		{
			Top = 10000,
			Side = 10100,
			Btm = 10200,
		}

		#region caminfo 정해지면 test
		//private void DoColorInspection_test(EdgeSurfaceParameterBase param, int channelIndex)
		//{
		//	if (this.GetWorkplaceBufferByIndex(channelIndex) == null)
		//		return;

		//	CameraInfo cameraInfo = this.currentWorkplace.CameraInfo;
		//	int scanDegree = cameraInfo.ScanDegree;
		//	int imageH = cameraInfo.ImageHeight;
		//	int camH = cameraInfo.CameraHeight;
		//	int positionOffset = cameraInfo.CameraPositionOffset;
		//	int startPosition = 0;

		//	int heightPerDegree = imageH / scanDegree;
		//	int height360 = heightPerDegree * 360;
		//	startPosition = camH + heightPerDegree * positionOffset;

		//	OriginRecipe originRecipe = recipe.GetItem<OriginRecipe>();
		//	int width = originRecipe.OriginWidth;
		//	int height = param.ROIHeight;
			
		//	int count = (int)((originRecipe.OriginHeight - startPosition) / height); // 검사 영역 개수
		//	Parallel.For(0, count, i =>
		//	{
		//		int ptLeft = 0;
		//		int ptTop = param.StartPosition + (i * height);
		//		int ptBtm = ptTop + height;

		//		// 검사영역이 Origin Height를 넘어가는 경우
		//		if (ptBtm > originRecipe.OriginHeight)
		//		{
		//			height = originRecipe.OriginHeight - ptTop;
		//			ptBtm = originRecipe.OriginHeight;
		//		}

		//		int bufferLength = width * height;
		//		byte[] inspectionROI = new byte[bufferLength];
		//		for (int y = ptTop; y < ptBtm; y++)
		//		{
		//			int startIdx = this.currentWorkplace.SharedBufferInfo.Width * y;
		//			int dstIdx = width * (y - ptTop);

		//			Marshal.Copy(this.currentWorkplace.SharedBufferInfo.PtrList[channelIndex] + startIdx, inspectionROI, dstIdx, width);
		//		}

		//		#region [Inspection]
		//		int lastEdge = CLR_IP.Cpp_FindEdge(inspectionROI, width, height, 0, 0, (width - 1), (height - 1), 0, param.EdgeSearchLevel);
		//		int startPtX = lastEdge;    // Edge부터 검사 시작
		//		if (startPtX >= width)
		//			startPtX = 0;

		//		// profile 생성
		//		List<int> temp = new List<int>();
		//		List<int> profile = new List<int>();
		//		for (long x = 0; x < width; x++)
		//		{
		//			temp.Clear();
		//			for (long y = 0; y < bufferLength; y += width)
		//			{
		//				temp.Add(inspectionROI[x + y]);
		//			}
		//			temp.Sort();
		//			profile.Add(temp[temp.Count / 2]);  // 중앙값
		//		}

		//		// Calculate diff image (original - profile)
		//		byte[] diff = new byte[bufferLength];
		//		for (int y = 0; y < height; y++)
		//		{
		//			for (int x = startPtX; x < width; x++)
		//			{
		//				diff[(y * width) + x] = (byte)(Math.Abs(inspectionROI[(y * width) + x] - profile[x]));
		//			}
		//		}

		//		// Threshold and Labeling
		//		byte[] thresh = new byte[bufferLength];
		//		CLR_IP.Cpp_Threshold(diff, thresh, width, height, false, param.Threshold);
		//		var label = CLR_IP.Cpp_Labeling(diff, thresh, width, height, true);

		//		double resolution = 1.67; //this.currentWorkplace.CameraInfo.TargetResX;
		//		int defectSizeMin = param.DefectSizeMin;
		//		int defectSizeMax = param.DefectSizeMax;
		//		// Add defect
		//		string sInspectionID = DatabaseManager.Instance.GetInspectionID();
		//		for (int l = 0; l < label.Length; l++)
		//		{
		//			if ((label[l].area * resolution) > defectSizeMin 
		//				&& (label[l].area * resolution) < defectSizeMax)
		//			{
		//				int defectLeft = ptLeft + label[l].boundLeft;
		//				int defectTop = ptTop - label[l].boundTop;
		//				int defectWidth = Math.Abs(label[l].boundRight - label[l].boundLeft);
		//				int defectHeight = Math.Abs(label[l].boundBottom - label[l].boundTop);

		//				this.currentWorkplace.AddDefect(sInspectionID,
		//					10000 + (channelIndex * 100),
		//					(float)(label[l].area * resolution),
		//					label[l].value,
		//					0,
		//					CalcDegree(defectLeft + (defectHeight / 2), param),
		//					defectLeft,
		//					defectTop,
		//					(float)(defectWidth * resolution),
		//					(float)(defectHeight * resolution),
		//					this.currentWorkplace.MapIndexX,
		//					this.currentWorkplace.MapIndexY
		//					);
		//			}
		//		}
		//		#endregion
		//	});
		//}
		#endregion

		private void DoColorInspection(EdgeSurfaceParameterBase param, int channelIndex)
		{
			if (this.GetWorkplaceBufferByIndex(channelIndex) == null)
				return;

			OriginRecipe originRecipe = recipe.GetItem<OriginRecipe>();
			int width = originRecipe.OriginWidth;
			int height = param.ROIHeight;

			// test
			//string folderPath = @"D:\EdgeResult.csv";
			//StreamWriter swResult = new StreamWriter(folderPath);
			//swResult.WriteLine("channel index, count, start position, end position, height");

			int count = (int)((originRecipe.OriginHeight - param.StartPosition) / param.ROIHeight); // 검사 영역 개수
			for (int i = 0; i < 1; i++)
			//Parallel.For(0, count, i =>
			{
				int camEmptyBufferHeight = 500; // 210525 camera height 일단 하드코딩 <- CamInfo
				int ptLeft = 0;
				int ptTop = camEmptyBufferHeight + param.StartPosition + (i * height);	
				int ptBtm = ptTop + height;

				// 검사영역이 Origin Height를 넘어가는 경우
				if (ptBtm > originRecipe.OriginHeight)
				{
					height = originRecipe.OriginHeight - ptTop;
					ptBtm = originRecipe.OriginHeight;
				}

				int bufferLength = width * height;
				byte[] inspectionROI = new byte[bufferLength];
				for (int y = ptTop; y < ptBtm; y++)
				{
					int startIdx = this.currentWorkplace.SharedBufferInfo.Width * y;
					int dstIdx = width * (y - ptTop);

					Marshal.Copy(this.currentWorkplace.SharedBufferInfo.PtrList[channelIndex] + startIdx, inspectionROI, dstIdx, width);
					// Old
					//Array.Copy(this.GetWorkplaceBufferByIndex(channelIndex), startIdx, inspectionROI, dstIdx, width);
				}

				//swResult.WriteLine(channelIndex + "," + i + "," + ptTop + "," + ptBtm + "," + height);
				// test bitmap save 
				//System.Drawing.Bitmap bmp = Tools.CovertArrayToBitmap(inspectionROI, width, height, 1);
				//bmp.Save("D:\\test" + i + ".bmp");


				#region [Inspection]
				int lastEdge = CLR_IP.Cpp_FindEdge(inspectionROI, width, height, 0, 0, (width - 1), (height - 1), 0, param.EdgeSearchLevel);
				int startPtX = lastEdge;    // Edge부터 검사 시작
				if (startPtX >= width)
					startPtX = 0;

				// profile 생성
				List<int> temp = new List<int>();
				List<int> profile = new List<int>();
				for (long x = 0; x < width; x++)
				{
					temp.Clear();
					for (long y = 0; y < bufferLength; y += width)
					{
						temp.Add(inspectionROI[x + y]);
					}
					temp.Sort();
					profile.Add(temp[temp.Count / 2]);  // 중앙값
				}

				// Calculate diff image (original - profile)
				byte[] diff = new byte[bufferLength];
				for (int y = 0; y < height; y++)
				{
					for (int x = startPtX; x < width; x++)
					{
						diff[(y * width) + x] = (byte)(Math.Abs(inspectionROI[(y * width) + x] - profile[x]));
					}
				}

				// Threshold and Labeling
				byte[] thresh = new byte[bufferLength];
				CLR_IP.Cpp_Threshold(diff, thresh, width, height, false, param.Threshold);
				var label = CLR_IP.Cpp_Labeling(diff, thresh, width, height, true);

				double resolution = 1.67; //this.currentWorkplace.CameraInfo.TargetResX;
				int defectSizeMin = param.DefectSizeMin;
				int defectSizeMax = param.DefectSizeMax;
				// Add defect
				string sInspectionID = DatabaseManager.Instance.GetInspectionID();
				for (int l = 0; l < label.Length; l++)
				{
					if ((label[l].area * resolution) > defectSizeMin 
						&& (label[l].area * resolution) < defectSizeMax
						&& label[l].width > 50)
					{
						int defectLeft = ptLeft + label[l].boundLeft;
						int defectTop = ptTop + label[l].boundTop;
						int defectWidth = Math.Abs(label[l].boundRight - label[l].boundLeft);
						int defectHeight = Math.Abs(label[l].boundBottom - label[l].boundTop);
						
						double degree = (double)360 / (originRecipe.OriginHeight - param.StartPosition + camEmptyBufferHeight) * (defectTop + defectHeight / 2 - ptTop);

						this.currentWorkplace.AddDefect(sInspectionID,
							10000 + (channelIndex * 100),
							(float)(label[l].area * resolution),
							label[l].value,
							0,
							(float)degree,
							defectLeft,
							defectTop,
							(float)(defectWidth * resolution),
							(float)(defectHeight * resolution),
							this.currentWorkplace.MapIndexX,
							this.currentWorkplace.MapIndexY
							);
					}
				}
				#endregion
				
			}
			//);
			//swResult.Close();
		}

		public float CalcDegree(int defectY, EdgeSurfaceParameterBase param)
		{
			float degree = 0;

			//// (끝지점 - 시작지점) / defectY
			//int bufferY = (int)(360000 / this.parameterEdge.camTriggerRatio) + this.parameterEdge.camHeight;

			//float heightPerDegree = grabModeTop.m_nImageHeight / 540000;
			//float heightPerDegree = (param.EndNotch - param.StartNotch) / 540000;
			//float degree = (defectY - param.StartNotch) * heightPerDegree;
			return degree;
		}
	}
}
