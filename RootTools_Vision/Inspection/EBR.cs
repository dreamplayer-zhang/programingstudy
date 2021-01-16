﻿using RootTools;
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
		WorkplaceBundle workplaceBundle;
		Workplace workplace;

		public override WORK_TYPE Type => WORK_TYPE.INSPECTION;

		private EBRParameter parameter;
		private EBRRecipe recipe;

		public EBR() : base()
		{
			m_sName = this.GetType().Name;
		}

		public override WorkBase Clone()
		{
			return (WorkBase)this.MemberwiseClone();
		}

		public override void SetRecipe(Recipe _recipe)
		{
			this.parameter = _recipe.GetRecipe<EBRParameter>();
			this.recipe = _recipe.GetRecipe<EBRRecipe>();
		}

		public override void SetWorkplace(Workplace workplace)
		{
			this.workplace = workplace;
		}

		public override void SetWorkplaceBundle(WorkplaceBundle workplace)
		{
			this.workplaceBundle = workplace;
		}

		//public override bool DoPrework()
		//{
		//	return base.DoPrework();
		//}

		public override void DoWork()
		{
			DoInspection();
		}

		public void DoInspection()
		{
			if (this.workplace.Index == 0)
				return;

			//Inspect();

			IntPtr ptrMem = this.workplace.SharedBufferR_GRAY;

			int roiWidth = this.workplace.BufferSizeX;
			int roiHeight = this.workplace.BufferSizeY;

			int roiLeft = this.workplace.PositionX;
			int roiTop = this.workplace.PositionY;
			int roiRight = this.workplace.PositionX + roiWidth;
			int roiBtm = this.workplace.PositionY + roiHeight;

			//int[] arrAvg = new int[roiWidth];
			//int[] arrEqual = new int[roiWidth];
			int[] arrDiff = new int[roiWidth];
			//arrAvg = DoAverage(ptrMem, roiLeft, roiTop, roiWidth, roiHeight);
			//arrEqual = Equalize(arrAvg, cnt);
			//arrDiff = DoDiff(arrEqual, cnt);

			arrDiff = GetDiffArr(ptrMem, roiLeft, roiTop, roiWidth, roiHeight);
			FindEdge(arrDiff);

			WorkEventManager.OnInspectionDone(this.workplace, new InspectionDoneEventArgs(new List<CRect>())); // 나중에 ProcessDefect쪽 EVENT로...
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
					ySum += ((byte*)memory)[(y * width) + x];
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

			return arrDiff;
		}

		/*
		private unsafe int[] DoAverage(IntPtr memory, int left, int top, int width, int height)
		{
			int[] arrAvg = new int[width];
			int right = left + width;
			int btm = top + height;

			for (int x = left; x < right; x++)
			{
				int ySum = 0;
				for (int y = top; y < btm; y += 10)
				{
					ySum += ((byte*)memory)[(y * width) + x];
				}
				arrAvg[x - left] = ySum / ((btm - top) / 10);
			}

			return arrAvg;
		}

		private int[] Equalize(int[] arrAvg)
		{
			int[] arrEqual = new int[arrAvg.Length];
			int equalCnt = 10; // parameter? y축으로 평균된 애들 smoothing 할때 몇개씩 할건지....

			for (int i = 0; i < arrAvg.Length; i++)
			{
				int x0 = i - equalCnt;
				int x1 = i + equalCnt;

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

			return arrEqual;
		}

		private int[] DoDiff(int[] arrEqual)
		{
			int[] arrDiff = new int[arrEqual.Length];
			int equalCnt = 10; // parameter? y축으로 평균된 애들 smoothing 할때 몇개씩 할건지....

			for (int x = equalCnt; x < arrEqual.Length - equalCnt; x++)
			{
				arrDiff[x] = arrEqual[x + equalCnt] - arrEqual[x - equalCnt];
			}
			return arrDiff;
		}
		*/

		private void FindEdge(int[] arrDiff)
		{
			int xRange = this.parameter.XRange;
			int diffEdge = this.parameter.EdgeDiff;
			int diffBevel = this.parameter.BevelDiff;
			int diffEBR = this.parameter.EBRDiff;

			double waferEdgeX, bevelX, ebrX;

			int[] arrDiffReverse = new int[arrDiff.Length];
			for (int i = 0; i < arrDiff.Length; i++)
			{
				arrDiffReverse[i] = -arrDiff[i];
			}

			waferEdgeX = FindEdge(arrDiff, arrDiff.Length - (2 * xRange), diffEdge);
			bevelX = FindEdge(arrDiffReverse, (int)Math.Round(waferEdgeX), diffBevel);
			ebrX = FindEdge(arrDiff, (int)Math.Round(bevelX), diffEBR);
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
