using RootTools;
using RootTools.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
	public class EBR : WorkBase
	{
		public override WORK_TYPE Type => WORK_TYPE.INSPECTION;

		public EBR() : base()
		{
			m_sName = this.GetType().Name;
		}

		protected override bool Preparation()
		{
			return true;
		}

		protected override bool Execution()
		{
			//DoInspection();
			DoInspection_New();
			return true;
		}

		public void DoInspection_New()
		{
			if (this.currentWorkplace.Index == 0)
				return;

			OriginRecipe originRecipe = recipe.GetItem<OriginRecipe>();
			EBRRecipe recipeEBR = recipe.GetItem<EBRRecipe>();
			EBRParameter param = recipe.GetItem<EBRParameter>();

			int firstNotch = recipeEBR.FirstNotch;
			int lastNotch = recipeEBR.LastNotch;
			int bufferHeight = lastNotch - firstNotch;
			int bufferHeightPerDegree = bufferHeight / 360;

			// new
			int width = originRecipe.OriginWidth;
			int height = param.ROIHeight;

			double stepDegree = (360 - param.NotchOffsetDegree) / param.MeasureCount;
			int stepDegreeHeight = (int)stepDegree * bufferHeightPerDegree;
			int notchOffsetHeight = (int)(param.NotchOffsetDegree * bufferHeightPerDegree);

			for (int i = 0; i < param.MeasureCount; i++)
			{
				int posY = firstNotch + notchOffsetHeight + (stepDegreeHeight * (i + 1)) - (height / 2);
				int[] arrDiff;
				arrDiff = GetDiffArr(this.currentWorkplace.SharedBufferInfo.PtrR_GRAY, 0, posY, width, height);

				double angle = stepDegree * (i + 1);
				FindEdge(arrDiff, 0, posY, width, height, angle);
			}

			// old
			//double stepDegree = 10;// parameterEBR.StepDegree;
			//int cnt = (int)(360 / stepDegree);

			//int width = 5300;//parameterEBR.ROIWidth;
			//int height = parameterEBR.ROIHeight;

			//for (int i = 0; i < cnt; i++)
			//{
			//	int posY = firstNotch + (int)((bufferHeightPerDegree * stepDegree * (i + 1)) - (height / 2));
			//	int[] arrDiff;
			//	arrDiff = GetDiffArr(this.currentWorkplace.SharedBufferInfo.PtrR_GRAY, 0, posY, width, height);
			//	FindEdge(arrDiff, 0, posY, width, height);
			//}

			WorkEventManager.OnInspectionDone(this.currentWorkplace, new InspectionDoneEventArgs(new List<CRect>())); // 나중에 ProcessDefect쪽 EVENT로...
		}

		public void DoInspection()
		{
			if (this.currentWorkplace.Index == 0)
				return;

			IntPtr ptrMem = this.currentWorkplace.SharedBufferInfo.PtrR_GRAY;
			int roiWidth = this.currentWorkplace.Width;
			int roiHeight = this.currentWorkplace.Height;
			int roiLeft = this.currentWorkplace.PositionX;
			int roiTop = this.currentWorkplace.PositionY;

			int[] arrDiff;
			arrDiff = GetDiffArr(ptrMem, roiLeft, roiTop, roiWidth, roiHeight);
			FindEdge(arrDiff);

			WorkEventManager.OnInspectionDone(this.currentWorkplace, new InspectionDoneEventArgs(new List<CRect>())); // 나중에 ProcessDefect쪽 EVENT로...
		}

		private unsafe int[] GetDiffArr(IntPtr memory, int left, int top, int width, int height)
		{
			EBRParameter param = recipe.GetItem<EBRParameter>();

			int[] arrAvg = new int[width];
			int[] arrEqual = new int[arrAvg.Length];
			int[] arrDiff = new int[arrEqual.Length];

			int right = left + width;
			int btm = top + height;

			int xRange = param.XRange;

			// average
			for (int x = left; x < right; x++)
			{
				int ySum = 0;
				for (int y = top; y < btm; y += 10)
					ySum += ((byte*)memory)[(y * this.currentWorkplace.SharedBufferInfo.Width) + x];
				arrAvg[x - left] = ySum / ((btm - top) / 10);
			}

			// equalize
			for (int i = 0; i < arrAvg.Length; i++)
			{
				int x0 = i - xRange;
				int x1 = i + xRange;

				if (x0 < 0)
					x0 = 0;
				if (x1 >= arrAvg.Length)
					x1 = arrAvg.Length - 1;

				int sum = 0;
				for (int j = x0; j <= x1; j++)
					sum += arrAvg[j];
				arrEqual[i] = sum / (x1 - x0 + 1);
			}

			// diff
			for (int x = xRange; x < arrEqual.Length - xRange; x++)
				arrDiff[x] = arrEqual[x + xRange] - arrEqual[x - xRange];

			// Raw data 저장
			string sInspectionID = DatabaseManager.Instance.GetInspectionID();
			string folderPath = @"D:\EBR RawData\";
			Directory.CreateDirectory(folderPath);
			StreamWriter swResult = new StreamWriter(folderPath + sInspectionID.ToString() + "_" + top.ToString() + "_Result.csv");
			for (int i = 0; i < arrDiff.Length; i++)
				swResult.WriteLine(arrAvg[i] + "," + arrEqual[i] + "," + arrDiff[i]);
			swResult.Close();

			return arrDiff;
		}

		private void FindEdge(int[] arrDiff, int left, int top, int width, int height, double angle)
		{
			EBRParameter param = recipe.GetItem<EBRParameter>();

			int xRange = param.XRange;
			int diffEdge = param.DiffEdge;
			int diffBevel = param.DiffBevel;
			int diffEBR = param.DiffEBR;
			double resolution = this.currentWorkplace.CameraInfo.TargetResX;

			int[] arrDiffReverse = new int[arrDiff.Length];
			for (int i = 0; i < arrDiff.Length; i++)
				arrDiffReverse[i] = -arrDiff[i];

			float waferEdgeX, bevelX, ebrX;
			waferEdgeX = FindEdge(arrDiff, arrDiff.Length - (2 * xRange), diffEdge);
			bevelX = FindEdge(arrDiffReverse, (int)Math.Round(waferEdgeX) - param.OffsetBevel, diffBevel);
			ebrX = FindEdge(arrDiff, (int)Math.Round(bevelX) - param.OffsetEBR, diffEBR);

			// Add measurement
			string sInspectionID = DatabaseManager.Instance.GetInspectionID();

			this.currentWorkplace.AddMeasurement(sInspectionID,
								"EDGE",
								Measurement.MeasureType.EBR,
								Measurement.EBRMeasureItem.Bevel,
								(float)((waferEdgeX - bevelX)),
								width,
								height,
								(float)angle,
								left,
								top,
								this.currentWorkplace.MapIndexX,
								this.currentWorkplace.MapIndexY,
								waferEdgeX);

			this.currentWorkplace.AddMeasurement(sInspectionID,
								"EDGE",
								Measurement.MeasureType.EBR,
								Measurement.EBRMeasureItem.EBR,
								(float)((waferEdgeX - ebrX)),
								width,
								height,
								(float)angle,
								left,
								top,
								this.currentWorkplace.MapIndexX,
								this.currentWorkplace.MapIndexY,
								waferEdgeX);
		}

		private void FindEdge(int[] arrDiff)
		{
			EBRParameter param = recipe.GetItem<EBRParameter>();

			int xRange = param.XRange;
			int diffEdge = param.DiffEdge;
			int diffBevel = param.DiffBevel;
			int diffEBR = param.DiffEBR;
			double resolution = this.currentWorkplace.CameraInfo.TargetResX;

			int[] arrDiffReverse = new int[arrDiff.Length];
			for (int i = 0; i < arrDiff.Length; i++)
				arrDiffReverse[i] = -arrDiff[i];

			float waferEdgeX, bevelX, ebrX;
			waferEdgeX = FindEdge(arrDiff, arrDiff.Length - (2 * xRange), diffEdge);
			bevelX = FindEdge(arrDiffReverse, (int)Math.Round(waferEdgeX) - param.OffsetBevel, diffBevel);
			ebrX = FindEdge(arrDiff, (int)Math.Round(bevelX) - param.OffsetEBR, diffEBR);

			// Add measurement
			string sInspectionID = DatabaseManager.Instance.GetInspectionID();

			this.currentWorkplace.AddMeasurement(sInspectionID,
								"EDGE",
								Measurement.MeasureType.EBR,
								Measurement.EBRMeasureItem.Bevel,
								(float)((waferEdgeX - bevelX) * resolution),
								this.currentWorkplace.Width,
								this.currentWorkplace.Height,
								CalculateAngle(this.currentWorkplace.Index),
								this.currentWorkplace.PositionX,
								this.currentWorkplace.PositionY,
								this.currentWorkplace.MapIndexX,
								this.currentWorkplace.MapIndexY);

			this.currentWorkplace.AddMeasurement(sInspectionID,
								"EDGE",
								Measurement.MeasureType.EBR,
								Measurement.EBRMeasureItem.EBR,
								(float)((waferEdgeX - ebrX) * resolution),
								this.currentWorkplace.Width,
								this.currentWorkplace.Height,
								CalculateAngle(this.currentWorkplace.Index),
								this.currentWorkplace.PositionX,
								this.currentWorkplace.PositionY,
								this.currentWorkplace.MapIndexX,
								this.currentWorkplace.MapIndexY);
		}

		private float CalculateAngle(int index)
		{
			float angle = 0;

			return angle;
		}

		private float FindEdge(int[] diff, int searchStartX, int standardDiff)
		{
			int maxValue = 0;
			int peakX = searchStartX;
			 
			while (diff[searchStartX] < standardDiff && searchStartX > 0)
				searchStartX--;

			while (diff[searchStartX] >= standardDiff && searchStartX > 0)
			{
				if (maxValue < diff[searchStartX])
				{
					maxValue = diff[searchStartX];
					peakX = searchStartX;
				}
				searchStartX--;
			}

			if (searchStartX < 0) 
				return 0;

			return FindEqualizeEdge(diff, peakX);
		}

		private float FindEqualizeEdge(int[] diff, int peakX)
		{
			EBRParameter param = recipe.GetItem<EBRParameter>();

			int xRange = param.XRange;
			double[] arrDiffSum = null;

			if (peakX < xRange)
				return 0;
			
			if ((arrDiffSum == null) || (arrDiffSum.Length < 2 * xRange))
				arrDiffSum = new double[4 * xRange];

			for (int x = peakX - xRange, ix = 0; x < peakX + xRange; x++, ix++)
				arrDiffSum[ix] = FindEdgeSum(diff, x);

			for (int x = peakX - xRange, ix = 0; x < peakX + xRange; x++, ix++)
			{
				if (arrDiffSum[ix] < 0 && ix > 0)
				{
					if (arrDiffSum[ix - 1] == arrDiffSum[ix]) return 1;

					float dx = (float)(arrDiffSum[ix - 1] / (arrDiffSum[ix - 1] - arrDiffSum[ix]));
					float maxX = x - 1 + dx;
					peakX = (int)Math.Round(maxX);

					for (int xp = peakX - (2 * xRange); xp < peakX + (2 * xRange); xp++)
					{
						if (xp < 0) xp = 0;
						
						diff[xp] = 0;
						diff[xp - 1] = 0;
					}
					return maxX;
				}
			}
			return peakX;
		}

		private double FindEdgeSum(int[] diff, int pointX)
		{
			EBRParameter param = recipe.GetItem<EBRParameter>();

			int xRange = param.XRange;
			double sum = 0;

			for (int x = pointX - xRange; x < pointX; x++)
				sum -= diff[x];
			for (int x = pointX + 1; x < pointX + xRange; x++)
				sum += diff[x];

			return sum;
		}

    }
}
