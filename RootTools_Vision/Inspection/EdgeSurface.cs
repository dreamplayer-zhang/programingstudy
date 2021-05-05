﻿using RootTools;
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

			EdgeSurfaceParameterBase param = parameterEdge.EdgeParamBaseTop;
			if (this.currentWorkplace.MapIndexX == (int)EdgeMapPositionX.Top)
				param = parameterEdge.EdgeParamBaseTop;
			else if (this.currentWorkplace.MapIndexX == (int)EdgeMapPositionX.Side)
				param = parameterEdge.EdgeParamBaseSide;
			else if (this.currentWorkplace.MapIndexX == (int)EdgeMapPositionX.Btm)
				param = parameterEdge.EdgeParamBaseBtm;


			OriginRecipe originRecipe = recipe.GetItem<OriginRecipe>();
			int count = (int)(originRecipe.OriginHeight / param.ROIHeight);	// 검사 영역 개수

			for (int i = 0; i < count; i++)
			{
				if (param.ChR)
					DoColorInspection(i, param, IMAGE_CHANNEL.R_GRAY);
				if (param.ChG)
					DoColorInspection(i, param, IMAGE_CHANNEL.G);
				if (param.ChB)
					DoColorInspection(i, param, IMAGE_CHANNEL.B);
			}

			//if (param.ChR)
			//	DoColorInspection(this.GetWorkplaceBuffer(IMAGE_CHANNEL.R_GRAY), param);
			//if (param.ChG)
			//	DoColorInspection(this.GetWorkplaceBuffer(IMAGE_CHANNEL.G), param);
			//if (param.ChB)
			//	DoColorInspection(this.GetWorkplaceBuffer(IMAGE_CHANNEL.B), param);

			WorkEventManager.OnInspectionDone(this.currentWorkplace, new InspectionDoneEventArgs(new List<CRect>())); // 나중에 ProcessDefect쪽 EVENT로...
		}

		private void DoColorInspection(int index, EdgeSurfaceParameterBase param, IMAGE_CHANNEL channel)
		{
			OriginRecipe originRecipe = recipe.GetItem<OriginRecipe>();

			int width = originRecipe.OriginWidth;
			int height = param.ROIHeight;
			byte[] inspectionROI = new byte[width * height];
			
			int startY = (index * height) + param.StartPosition;
			for (int i = startY; i < startY + height; i++)
			{
				int startIdx = width * i;
				int dstIdx = width * (i - startY);

				Array.Copy(this.GetWorkplaceBuffer(channel), startIdx, inspectionROI, dstIdx, width);
			}

			//System.Drawing.Bitmap bitmap = Tools.CovertArrayToBitmap(roi, width, height, 1);
			//Tools.SaveImageJpg(bitmap, @"D:\test.jpg", 50);
			DoColorInspection(inspectionROI, param); // index 같이 보내야하눈데...
		}

		private void DoColorInspection(byte[] arrSrc, EdgeSurfaceParameterBase param)
		{
			OriginRecipe originRecipe = recipe.GetItem<OriginRecipe>();

			int width = originRecipe.OriginWidth;
			int height = param.ROIHeight;
			int inspectionROI = width * height;
			
			int threshold = param.Threshold;
			int defectSizeMin = param.DefectSizeMin;
			int defectSizeMax = param.DefectSizeMax;
			int searchLevel = param.EdgeSearchLevel;
			double resolution = 1;

			// Search Wafer Edge
			int lastEdge = FindEdge(arrSrc, width, height, searchLevel);
			int startPtX = lastEdge;    // Edge부터 검사 시작

			// profile 생성
			List<int> temp = new List<int>();
			List<int> profile = new List<int>();
			for (long j = 0; j < width; j++)
			{
				temp.Clear();
				for (long i = 0; i < inspectionROI; i += width)
				{
					temp.Add(arrSrc[j + i]);
				}
				temp.Sort();
				profile.Add(temp[temp.Count / 2]);  // 중앙값
			}

			// Calculate diff image (original - profile)
			byte[] diff = new byte[inspectionROI];
			for (int j = 0; j < height; j++)
			{
				for (int i = startPtX; i < width; i++)
				{
					diff[(j * width) + i] = (byte)(Math.Abs(arrSrc[(j * width) + i] - profile[i]));
				}
			}

			// Threshold and Labeling
			byte[] thresh = new byte[inspectionROI];
			CLR_IP.Cpp_Threshold(diff, thresh, width, height, false, threshold);
			var label = CLR_IP.Cpp_Labeling(diff, thresh, width, height, true);

			// Add defect
			string sInspectionID = DatabaseManager.Instance.GetInspectionID();
			for (int i = 0; i < label.Length; i++)
			{
				if ((label[i].area * resolution) > defectSizeMin || (label[i].area * resolution) < defectSizeMax)
				{
					int defectLeft = this.currentWorkplace.PositionX + label[i].boundLeft;
					int defectTop = this.currentWorkplace.PositionY + label[i].boundTop;
					int defectWidth = Math.Abs(label[i].boundRight - label[i].boundLeft);
					int defectHeight = Math.Abs(label[i].boundBottom - label[i].boundTop);

					this.currentWorkplace.AddDefect(sInspectionID,
						10001,
						(float)(label[i].area * resolution),
						label[i].value,
						0,
						CalcDegree(defectLeft + (defectHeight/2), param),
						defectLeft,
						defectTop,
						(float)(defectWidth * resolution),
						(float)(defectHeight * resolution),
						this.currentWorkplace.MapIndexX,
						this.currentWorkplace.MapIndexY
						);
				}
			}
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
			
			avgNext = MeanOfYCoordinates(arrSrc, startPtY, startPtX, width, height);
			for (int x = startPtX + 1; x < width; x++)
			{
				avg = avgNext;
				avgNext = MeanOfYCoordinates(arrSrc, startPtY, x, width, height);

				if ((avg >= prox && prox > avgNext) || (avg <= prox && prox < avgNext))
				{
					edge = x;
					x = width + 1;
				}
			}
			return edge;
		}

		public int MeanOfYCoordinates(byte[] arrSrc, int startPtY, int findPtX, int width, int height)
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
