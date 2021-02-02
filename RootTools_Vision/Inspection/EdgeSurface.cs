using RootTools;
using RootTools.Database;
using RootTools_CLR;
using System;
using System.Collections.Generic;
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

			//byte[] arrSrc = this.GetWorkplaceBuffer(IMAGE_CHANNEL.R_GRAY);
			//Emgu.CV.Mat mat = new Emgu.CV.Mat((int)(parameterEdge.EdgeParamBaseTop.ROIHeight), (int)(parameterEdge.EdgeParamBaseTop.ROIWidth), Emgu.CV.CvEnum.DepthType.Cv8U, 1);
			//Marshal.Copy(arrSrc, 0, mat.DataPointer, arrSrc.Length);
			//mat.Save(@"D:/" + this.currentWorkplace.Index.ToString() + ".bmp");

			DoColorInspection(this.GetWorkplaceBuffer(IMAGE_CHANNEL.R_GRAY), parameterEdge);
			DoColorInspection(this.GetWorkplaceBuffer(IMAGE_CHANNEL.G), parameterEdge);
			DoColorInspection(this.GetWorkplaceBuffer(IMAGE_CHANNEL.B), parameterEdge);

			WorkEventManager.OnInspectionDone(this.currentWorkplace, new InspectionDoneEventArgs(new List<CRect>())); // 나중에 ProcessDefect쪽 EVENT로...
		}

		private void DoColorInspection(byte[] arrSrc, EdgeSurfaceParameter param)
		{
			int roiHeight;
			int roiWidth;
			int threshold;
			int defectSize;
			int searchLevel;

			// parameter
			if (this.currentWorkplace.MapIndexX == (int)EdgeMapPositionX.Top)
			{
				roiHeight = parameterEdge.EdgeParamBaseTop.ROIHeight;
				roiWidth = parameterEdge.EdgeParamBaseTop.ROIWidth;
				threshold = parameterEdge.EdgeParamBaseTop.Threshold;
				defectSize = parameterEdge.EdgeParamBaseTop.DefectSizeMin;
				searchLevel = parameterEdge.EdgeParamBaseTop.EdgeSearchLevel;
			}
			else if (this.currentWorkplace.MapIndexX == (int)EdgeMapPositionX.Side)
			{
				roiHeight = parameterEdge.EdgeParamBaseSide.ROIHeight;
				roiWidth = parameterEdge.EdgeParamBaseSide.ROIWidth;
				threshold = parameterEdge.EdgeParamBaseSide.Threshold;
				defectSize = parameterEdge.EdgeParamBaseSide.DefectSizeMin;
				searchLevel = parameterEdge.EdgeParamBaseSide.EdgeSearchLevel;
			}
			else if (this.currentWorkplace.MapIndexX == (int)EdgeMapPositionX.Btm)
			{
				roiHeight = parameterEdge.EdgeParamBaseBtm.ROIHeight;
				roiWidth = parameterEdge.EdgeParamBaseBtm.ROIWidth;
				threshold = parameterEdge.EdgeParamBaseBtm.Threshold;
				defectSize = parameterEdge.EdgeParamBaseBtm.DefectSizeMin;
				searchLevel = parameterEdge.EdgeParamBaseBtm.EdgeSearchLevel;
			}
			else
			{
				return;
			}

			int roiSize = roiWidth * roiHeight;

			// Search Wafer Edge
			int lastEdge = FindEdge(arrSrc, roiWidth, roiHeight, searchLevel);
			int startPtX = lastEdge;	// Edge부터 검사 시작

			// profile 생성
			List<int> temp = new List<int>();
			List<int> profile = new List<int>();
			for (long j = 0; j < roiWidth; j++)
			{
				temp.Clear();
				for (long i = 0; i < roiSize; i += roiWidth)
				{
					temp.Add(arrSrc[j + i]);
				}
				temp.Sort();
				profile.Add(temp[temp.Count / 2]);  // 중앙값
			}

			// Calculate diff image (original - profile)
			byte[] diff = new byte[roiSize];
			for (int j = 0; j < roiHeight; j++)
			{
				for (int i = startPtX; i < roiWidth; i++)
				{
					diff[(j * roiWidth) + i] = (byte)(Math.Abs(arrSrc[(j * roiWidth) + i] - profile[i]));
				}
			}

			// Threshold and Labeling
			byte[] thresh = new byte[roiSize];
			CLR_IP.Cpp_Threshold(diff, thresh, roiWidth, roiHeight, false, threshold);
			var label = CLR_IP.Cpp_Labeling(diff, thresh, roiWidth, roiHeight, true);
			
			// Add defect
			string sInspectionID = DatabaseManager.Instance.GetInspectionID();
			for (int i = 0; i < label.Length; i++)
			{
				if (label[i].area > defectSize)
				{
					this.currentWorkplace.AddDefect(sInspectionID,
						10001,
						label[i].area,
						label[i].value,
						this.currentWorkplace.PositionX + label[i].boundLeft,
						this.currentWorkplace.PositionY + label[i].boundTop,
						Math.Abs(label[i].boundRight - label[i].boundLeft),
						Math.Abs(label[i].boundBottom - label[i].boundTop),
						this.currentWorkplace.MapIndexX,
						this.currentWorkplace.MapIndexY
						);
				}
			}
		}

		public int FindEdge(byte[] arrSrc, int width, int height, int searchLevel = 70)
		{
			int min = 256;
			int max = 0;
			int prox = min + (int)((max - min) * searchLevel * 0.01);

			if (searchLevel >= 100)
				prox = max;
			else if (searchLevel <= 0)
				prox = min;

			int startPtX = 0;
			int startPtY = 0;
			int avg, avgNext;
			int edge = width;
			
			avgNext = MeanForYCoordinates(arrSrc, startPtY, startPtX, width, height);
			for (int x = startPtX + 1; x < width; x++)
			{
				avg = avgNext;
				avgNext = MeanForYCoordinates(arrSrc, startPtY, x, width, height);

				if ((avg >= prox && prox > avgNext) || (avg <= prox && prox < avgNext))
				{
					edge = x;
					x = width + 1;
				}
			}
			return edge;
		}

		public int MeanForYCoordinates(byte[] arrSrc, int startPtY, int findPtX, int width, int height)
		{
			int avg = 0;

			for (int y = startPtY; y < width*height; y += width)
				avg += arrSrc[y + findPtX];
			
			if (avg != 0)
				avg /= (height - startPtY + 1);

			return avg;
		}
	}
}
