using RootTools;
using RootTools.Database;
using RootTools.Inspects;
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

		public override WorkBase Clone()
		{
			return (WorkBase)this.MemberwiseClone();
		}

		protected override bool Preparation()
		{
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

			EdgeSurfaceParameter param = recipe.GetItem<EdgeSurfaceParameter>();
			EdgeSurfaceParameterBase paramTop = param.EdgeParamBaseTop;
			EdgeSurfaceParameterBase paramBottom = param.EdgeParamBaseBtm;
			EdgeSurfaceParameterBase paramSide = param.EdgeParamBaseSide;

			WorkEventManager.OnInspectionStart(this.currentWorkplace, new InspectionStartArgs());

			if (paramTop.ChR)
				DoColorInspection(paramTop, EdgeDefectCode.Top, 0);
			if (paramTop.ChG)
				DoColorInspection(paramTop, EdgeDefectCode.Top, 1);
			if (paramTop.ChB)
				DoColorInspection(paramTop, EdgeDefectCode.Top, 2);
			WorkEventManager.OnInspectionDone(this.currentWorkplace, new InspectionDoneEventArgs(new List<CRect>())); // 나중에 ProcessDefect쪽 EVENT로...

			if (paramBottom.ChR)
				DoColorInspection(paramBottom, EdgeDefectCode.Btm, 3);
			if (paramBottom.ChG)
				DoColorInspection(paramBottom, EdgeDefectCode.Btm, 4);
			if (paramBottom.ChB)
				DoColorInspection(paramBottom, EdgeDefectCode.Btm, 5);
			WorkEventManager.OnInspectionDone(this.currentWorkplace, new InspectionDoneEventArgs(new List<CRect>())); // 나중에 ProcessDefect쪽 EVENT로...

			if (paramSide.ChR)
				DoColorInspection(paramSide, EdgeDefectCode.Side, 6);
			if (paramSide.ChG)
				DoColorInspection(paramSide, EdgeDefectCode.Side, 7);
			if (paramSide.ChB)
				DoColorInspection(paramSide, EdgeDefectCode.Side, 8);
			WorkEventManager.OnInspectionDone(this.currentWorkplace, new InspectionDoneEventArgs(new List<CRect>())); // 나중에 ProcessDefect쪽 EVENT로...
		}

		public enum EdgeDefectCode
		{
			Top = 10000,
			Side = 10100,
			Btm = 10200,
		}
				
		private void DoColorInspection(EdgeSurfaceParameterBase param, EdgeDefectCode defectCode, int channelIndex)
		{
			//if (this.GetWorkplaceBufferByIndex(channelIndex) == null)
			//	return;

			OriginRecipe originRecipe = recipe.GetItem<OriginRecipe>();
			int width = originRecipe.OriginWidth;
			int height = param.ROIHeight;

			int notchOffset = (int)(((param.EndPosition - param.StartPosition) / 360) * param.NotchOffsetDegree);

			int startPtY = param.StartPosition + notchOffset;
			int endPtY = param.EndPosition - notchOffset;
			if (originRecipe.OriginHeight < param.EndPosition)
				endPtY = originRecipe.OriginHeight;

			int count = (int)((endPtY - startPtY) / param.ROIHeight);
			//for (int i = 1; i < 3; i++)
			Parallel.For(1, count, i =>
			{
				int ptLeft = 0;
				int ptTop = startPtY + (i * height);
				int ptBtm = ptTop + height;

				// 검사영역이 end position을 넘어가는 경우
				if (ptBtm > endPtY)
				{
					ptBtm = endPtY;
					height = ptBtm - ptTop;
				}

				int bufferLength = width * height;
				byte[] inspectionROI = new byte[bufferLength];
				for (int y = ptTop; y < ptBtm; y++)
				{
					int startIdx = this.currentWorkplace.SharedBufferInfo.Width * y;
					int dstIdx = width * (y - ptTop);

					Marshal.Copy(this.currentWorkplace.SharedBufferInfo.PtrList[channelIndex] + startIdx, inspectionROI, dstIdx, width);
				}

				// bitmap save 
				//System.Drawing.Bitmap bmp = Tools.CovertArrayToBitmap(inspectionROI, width, height, 1);
				//bmp.Save("D:\\edge " + i + ".bmp");

				#region [Inspection]

				int startPtX = 0;
				// Edge Search
				if (param.UseEdgeSearch)
				{
					int edge = CLR_IP.Cpp_FindEdge(inspectionROI, width, height, 0, 0, (width - 1), (height - 1), 0, param.EdgeSearchLevel);
					startPtX = edge;

					// edge search fail
					if (startPtX >= width)
						startPtX = 0;
				}

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

				int defectSizeMinX = param.DefectSizeMinX;
				int defectSizeMaxX = param.DefectSizeMaxX;
				int defectSizeMinY = param.DefectSizeMinY;
				int defectSizeMaxY = param.DefectSizeMaxY;

				// Add defect
				string sInspectionID = DatabaseManager.Instance.GetInspectionID();
				for (int l = 0; l < label.Length; l++)
				{
					if (label[l].width > defectSizeMinX && label[l].width < defectSizeMaxX
						&& label[l].height > defectSizeMinY && label[l].height < defectSizeMaxY)
					{
						int defectLeft = ptLeft + label[l].boundLeft;
						int defectTop = ptTop + label[l].boundTop;

						this.currentWorkplace.AddDefect(sInspectionID,
							(int)defectCode /*10000 + (channelIndex * 100)*/,
							(float)(label[l].area),
							label[l].value,
							0,
							0,
							defectLeft,
							defectTop,
							(float)label[l].width,
							(float)label[l].height,
							this.currentWorkplace.MapIndexX,
							this.currentWorkplace.MapIndexY
							);
					}
				}
				#endregion
			}
			);
		}
	}
}
