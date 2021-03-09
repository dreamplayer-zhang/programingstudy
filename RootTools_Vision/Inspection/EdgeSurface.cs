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
			//if (this.currentWorkplace.MapIndexY == -1)
			//	return;

			if (this.currentWorkplace.Index == 0)
				return;

			EdgeSurfaceParameterBase param = parameterEdge.EdgeParamBaseTop;
			if (this.currentWorkplace.MapIndexX == (int)EdgeMapPositionX.Top)
				param = parameterEdge.EdgeParamBaseTop;
			else if (this.currentWorkplace.MapIndexX == (int)EdgeMapPositionX.Side)
				param = parameterEdge.EdgeParamBaseSide;
			else if (this.currentWorkplace.MapIndexX == (int)EdgeMapPositionX.Btm)
				param = parameterEdge.EdgeParamBaseBtm;
			
			// 연구소WIND R채널만 검사
			DoColorInspection(this.GetWorkplaceBuffer(IMAGE_CHANNEL.R_GRAY), param);
			//DoColorInspection(this.GetWorkplaceBuffer(IMAGE_CHANNEL.G), param);
			//DoColorInspection(this.GetWorkplaceBuffer(IMAGE_CHANNEL.B), param);

			WorkEventManager.OnInspectionDone(this.currentWorkplace, new InspectionDoneEventArgs(new List<CRect>())); // 나중에 ProcessDefect쪽 EVENT로...
		}

		private void DoColorInspection(byte[] arrSrc, EdgeSurfaceParameterBase param)
		{
			int roiWidth = param.ROIWidth;
			int roiHeight = param.ROIHeight;
			int threshold = param.Threshold;
			int defectSize = param.DefectSizeMin;
			int searchLevel = param.EdgeSearchLevel;
			//double resolution = param.Resolution;

			if (this.currentWorkplace.Height < roiHeight)
				roiHeight = this.currentWorkplace.Height;

			int roiSize = roiWidth * roiHeight;

			// Search Wafer Edge
			int lastEdge = FindEdge(arrSrc, roiWidth, roiHeight, searchLevel);
			int startPtX = lastEdge;    // Edge부터 검사 시작

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
						Math.Abs(label[i].boundRight - label[i].boundLeft),// * resolution),
						Math.Abs(label[i].boundBottom - label[i].boundTop),// * resolution),
						this.currentWorkplace.MapIndexX,
						this.currentWorkplace.MapIndexY
						);
				}
			}
		}

		public float CalcDegree(int defectLeft)
		{
			float degree = 0;
			//// (끝지점 - 시작지점) / defectLeft
			//int bufferY = (int)(360000 / this.parameterEdge.camTriggerRatio) + this.parameterEdge.camHeight;

			//degree = () / defectLeft;

			return degree;
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
