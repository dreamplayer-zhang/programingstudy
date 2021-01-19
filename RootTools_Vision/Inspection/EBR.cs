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

		private EBRParameter parameter;
		private EBRRecipe recipeEBR;

		public EBR() : base()
		{
			m_sName = this.GetType().Name;
		}

		protected override bool Preparation()
		{
			this.parameter = this.recipe.GetRecipe<EBRParameter>();
			this.recipeEBR = this.recipe.GetRecipe<EBRRecipe>();

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

			IntPtr ptrMem = this.currentWorkplace.SharedBufferR_GRAY;
			int roiWidth = this.currentWorkplace.Width;
			int roiHeight = this.currentWorkplace.Height;
			int roiLeft = this.currentWorkplace.PositionX;
			int roiTop = this.currentWorkplace.PositionY;

			int[] arrDiff = new int[roiWidth];

			arrDiff = GetDiffArr(ptrMem, roiLeft, roiTop, roiWidth, roiHeight);
			FindEdge(arrDiff);

			WorkEventManager.OnInspectionDone(this.currentWorkplace, new InspectionDoneEventArgs(new List<CRect>())); // 나중에 ProcessDefect쪽 EVENT로...
		}

		private unsafe int[] GetDiffArr(IntPtr memory, int left, int top, int width, int height)
		{
			int[] arrAvg = new int[width];
			int[] arrEqual = new int[arrAvg.Length];
			int[] arrDiff = new int[arrEqual.Length];

			int right = left + width;
			int btm = top + height;

			int xRange = this.parameter.XRange;

			// average
			for (int x = left; x < right; x++)
			{
				int ySum = 0;
				for (int y = top; y < btm; y += 10)
				{
					//ySum += ((byte*)memory)[(y * width) + x];
					ySum += ((byte*)memory)[(y * this.currentWorkplace.SharedBufferWidth) + x];
				}
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
				{
					sum += arrAvg[j];
				}
				arrEqual[i] = sum / (x1 - x0 + 1);
			}

			// diff
			for (int x = xRange; x < arrEqual.Length - xRange; x++)
			{
				arrDiff[x] = arrEqual[x + xRange] - arrEqual[x - xRange];
			}

			StreamWriter sw = new StreamWriter(@"D:\EBR" + this.currentWorkplace.Index.ToString() + ".csv");
			for (int i = 0;  i < arrDiff.Length; i++)
            {
				sw.WriteLine(arrAvg[i] + "," + arrEqual[i] + "," + arrDiff[i]);
            }
			sw.Close();

			return arrDiff;
		}

		private void FindEdge(int[] arrDiff)
		{
			int xRange = this.parameter.XRange;
			int diffEdge = this.parameter.DiffEdge;
			int diffBevel = this.parameter.DiffBevel;
			int diffEBR = this.parameter.DiffEBR;

			double waferEdgeX, bevelX, ebrX;

			int[] arrDiffReverse = new int[arrDiff.Length];
			for (int i = 0; i < arrDiff.Length; i++)
			{
				arrDiffReverse[i] = -arrDiff[i];
			}

			waferEdgeX = FindEdge(arrDiff, arrDiff.Length - (2 * xRange), diffEdge);
			bevelX = FindEdge(arrDiffReverse, (int)Math.Round(waferEdgeX), diffBevel + this.parameter.OffsetBevel);
			ebrX = FindEdge(arrDiff, (int)Math.Round(bevelX), diffEBR + this.parameter.OffsetEBR);

			//StreamWriter sw = new StreamWriter(@"D:\EBRInsp" + this.currentWorkplace.Index.ToString() + ".csv");
			//sw.WriteLine(waferEdgeX + "," + bevelX + "," + ebrX);
			//sw.Close();

			// Add measurement
			string sInspectionID = DatabaseManager.Instance.GetInspectionID();
			this.currentWorkplace.AddDefect(sInspectionID,
									11111,
									0, 0,
									this.currentWorkplace.Index * this.parameter.StepDegree, 0,
									(float)(waferEdgeX - bevelX),
									(float)(waferEdgeX - ebrX),
									this.currentWorkplace.MapIndexX,
									this.currentWorkplace.MapIndexY);
		}

		private double FindEdge(int[] diff, int searchStartX, int standardDiff)
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

			if (searchStartX == 0)
				return 0;

			return FindEqualizeEdge(diff, peakX);
		}

		private double FindEqualizeEdge(int[] diff, int peakX)
		{
			int xRange = this.parameter.XRange;
			double[] arrDiffSum = null;

			if (peakX < xRange)
				return 0.0;
			
			if ((arrDiffSum == null) || (arrDiffSum.Length < 2 * xRange))
				arrDiffSum = new double[4 * xRange];

			for (int x = peakX - xRange, ix = 0; x <= peakX + xRange; x++, ix++)
				arrDiffSum[ix] = FindEdgeSum(diff, x);

			for (int x = peakX - xRange, ix = 0; x <= peakX + xRange; x++, ix++)
			{
				if (arrDiffSum[ix] < 0 && ix > 0)
				{
					if (arrDiffSum[ix - 1] == arrDiffSum[ix])
						return 1;

					double dx = arrDiffSum[ix - 1] / (arrDiffSum[ix - 1] - arrDiffSum[ix]);
					double maxX = x - 1 + dx;
					peakX = (int)Math.Round(maxX);

					for (int xp = peakX - 2 * xRange; xp <= peakX + 2 * xRange; xp++)
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
			int xRange = this.parameter.XRange;
			double sum = 0;

			for (int x = pointX - xRange; x < pointX; x++)
				sum -= diff[x];
			for (int x = pointX + 1; x <= pointX + xRange; x++)
				sum += diff[x];

			return sum;
		}

    }
}
