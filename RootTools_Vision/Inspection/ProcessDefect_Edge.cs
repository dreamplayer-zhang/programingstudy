﻿using RootTools;
using RootTools.Database;
using RootTools_CLR;
using RootTools_Vision.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static RootTools_Vision.Tools;

namespace RootTools_Vision
{
	public class ProcessDefect_Edge : WorkBase
	{
		public ProcessDefect_Edge()
		{

		}

		string TableName = "defect";
		public ProcessDefect_Edge(string tableName)
		{
            TableName = tableName;
        }

		public override WORK_TYPE Type => WORK_TYPE.DEFECTPROCESS_ALL;

		protected override bool Preparation()
		{
			return true;
		}

		protected override bool Execution()
		{
			ProcessDefectEdgeParameter param = this.recipe.GetItem<ProcessDefectEdgeParameter>();
			if (param.Use == false) return true;

			DoProcessDefect_Edge();
			return true;
		}

		public void DoProcessDefect_Edge()
		{
			if (this.currentWorkplace.Index != 0)
				return;

			ProcessDefectEdgeParameter processDefectParam = this.recipe.GetItem<ProcessDefectEdgeParameter>();

			List<Defect> topDefectList = CollectDefectData(EdgeSurface.EdgeDefectCode.Top);
			List<Defect> btmDefectList = CollectDefectData(EdgeSurface.EdgeDefectCode.Btm);
			List<Defect> sideDefectList = CollectDefectData(EdgeSurface.EdgeDefectCode.Side);

			List<Defect> topMergeDefectList;
			List<Defect> btmMergeDefectList;
			List<Defect> sideMergeDefectList;

			if (processDefectParam.UseMergeDefect)
			{
				// merge할 때 각도 (RelY) 값 바뀜
				topMergeDefectList = Tools.MergeDefect(topDefectList, processDefectParam.MergeDefectDistnace);
				btmMergeDefectList = Tools.MergeDefect(btmDefectList, processDefectParam.MergeDefectDistnace);
				sideMergeDefectList = Tools.MergeDefect(sideDefectList, processDefectParam.MergeDefectDistnace);
			}
			else
			{
				topMergeDefectList = topDefectList;
				btmMergeDefectList = btmDefectList;
				sideMergeDefectList = sideDefectList;
			}

			List<Defect> mergeDefectList = new List<Defect>();
			foreach (Defect defect in topMergeDefectList)
				mergeDefectList.Add(defect);
			foreach (Defect defect in btmMergeDefectList)
				mergeDefectList.Add(defect);
			foreach (Defect defect in sideMergeDefectList)
				mergeDefectList.Add(defect);

			// Top/Side/Btm 별 Defect Merge 후 Index 재정렬
			mergeDefectList = RearrangeDefectIndex(mergeDefectList);
			foreach (Defect defect in mergeDefectList)
				this.currentWorkplace.DefectList.Add(defect);

			if (mergeDefectList.Count > 0)
				DatabaseManager.Instance.AddDefectDataListNoAutoCount(mergeDefectList, TableName);

			#region Klarf / Defect Image 저장

			string sInspectionID = DatabaseManager.Instance.GetInspectionID();
			Settings settings = new Settings();
			SettingItem_SetupEdgeside settings_edgeside = settings.GetItem<SettingItem_SetupEdgeside>();
			SharedBufferInfo topSharedBufferInfo, btmSharedBufferInfo, sideSharedBufferInfo;
			topSharedBufferInfo = GetSharedBufferInfo(0);
			btmSharedBufferInfo = GetSharedBufferInfo(3);
			sideSharedBufferInfo = GetSharedBufferInfo(6);

			string path = Path.Combine(settings_edgeside.DefectImagePath, sInspectionID);
			DirectoryInfo di = new DirectoryInfo(path);
			if (!di.Exists)
				di.Create();

			for (int i = 0; i < mergeDefectList.Count; i++)
			//Parallel.For(0, mergeDefectList.Count, i =>
			{
				Bitmap mergeImage = MergeEdgesideImages(mergeDefectList[i]);
				if (mergeImage != null)
					mergeImage.Save(Path.Combine(path, mergeDefectList[i].m_nDefectIndex.ToString() + ".bmp"));
			}
			//);

			// EDGE 전체 원형 이미지 저장
			EdgeSurfaceParameter surfaceParam = this.recipe.GetItem<EdgeSurfaceParameter>();
			Tools.SaveEdgeCircleImage(Path.Combine(settings_edgeside.DefectImagePath, sInspectionID, (mergeDefectList.Count + 1).ToString()), settings_edgeside.OutputImageSizeWidth, settings_edgeside.OutputImageSizeHeight 
								  , topSharedBufferInfo, surfaceParam.EdgeParamBaseTop.StartPosition, surfaceParam.EdgeParamBaseTop.EndPosition
								  , sideSharedBufferInfo, surfaceParam.EdgeParamBaseSide.StartPosition, surfaceParam.EdgeParamBaseSide.EndPosition
								  , btmSharedBufferInfo, surfaceParam.EdgeParamBaseBtm.StartPosition, surfaceParam.EdgeParamBaseBtm.EndPosition);

			//if (settings_edgeside.UseKlarf)
			//{
			//	KlarfData_Lot klarfData = new KlarfData_Lot();
			//	Directory.CreateDirectory(settings_edgeside.KlarfSavePath);

			//	klarfData.AddSlot(recipe.WaferMap, mergeDefectList, this.recipe.GetItem<OriginRecipe>());
			//	klarfData.WaferStart(recipe.WaferMap, DateTime.Now);
			//	klarfData.SetResultTimeStamp();
			//	klarfData.SaveKlarf(settings_edgeside.KlarfSavePath, false);
			//}

			#endregion

			//WorkEventManager.OnInspectionDone(this.currentWorkplace, new InspectionDoneEventArgs(new List<CRect>(), true));
			WorkEventManager.OnIntegratedProcessDefectDone(this.currentWorkplace, new IntegratedProcessDefectDoneEventArgs());
		}

		/// <summary>
		/// Top/Side/Btm 별 SharedBufferInfo 생성 후 Return
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		private SharedBufferInfo GetSharedBufferInfo(int index)
		{
			IntPtr sharedBufferR_Gray = this.currentWorkplace.SharedBufferInfo.PtrList[index];
			IntPtr sharedBufferG = this.currentWorkplace.SharedBufferInfo.PtrList[index + 1];
			IntPtr sharedBufferB = this.currentWorkplace.SharedBufferInfo.PtrList[index + 2];
			
			SharedBufferInfo sharedBufferInfo = new SharedBufferInfo(sharedBufferR_Gray,
																	 this.currentWorkplace.SharedBufferInfo.Width,
																	 this.currentWorkplace.SharedBufferInfo.Height,
																	 this.currentWorkplace.SharedBufferByteCnt,
																	 sharedBufferG,
																	 sharedBufferB);
			return sharedBufferInfo;
		}

		private List<Defect> CollectDefectData(EdgeSurface.EdgeDefectCode defectCode)
		{
			List<Defect> DefectList = new List<Defect>();
			foreach (Workplace workplace in workplaceBundle)
			{
				foreach (Defect defect in workplace.DefectList)
				{
					if (defect.m_nDefectCode == (int)defectCode)
						DefectList.Add(defect);
				}
			}
			return DefectList;
		}

		/// <summary>
		/// Top/Btm/Side 별 Defect Data 추가.
		/// RGB 채널 Index : 
		/// Top - 0,1,2 / Btm - 3,4,5 / Side - 6,7,8
		/// </summary>
		/// <param name="min">Copied Buffer R-Channel Index</param>
		/// <param name="max">Copied Buffer B-Channel Index</param>
		/// <returns></returns>
		private List<Defect> CollectDefectData(int min, int max)
		{
			List<Defect> DefectList = new List<Defect>();
			int index = 0;

			foreach (Workplace workplace in workplaceBundle)
			{
				foreach (Defect defect in workplace.DefectList)
				{
					index = (defect.m_nDefectCode - 10000) / 100;
					if (index >= min && index <= max)
						DefectList.Add(defect);
				}
			}
			return DefectList;
		}

		private List<Defect> RearrangeDefectIndex(List<Defect> defectList)
		{
			List<Defect> MergeDefectList = new List<Defect>();
			int nDefectIndex = 1;

			for (int i = 0; i < defectList.Count; i++)
			{
				MergeDefectList.Add(defectList[i]);
				MergeDefectList[nDefectIndex - 1].SetDefectIndex(nDefectIndex++);
			}
			return MergeDefectList;
		}

		private Bitmap MergeEdgesideImages(Defect defect)
		{
			SharedBufferInfo topSharedBufferInfo, btmSharedBufferInfo, sideSharedBufferInfo;
			topSharedBufferInfo = GetSharedBufferInfo(0);
			btmSharedBufferInfo = GetSharedBufferInfo(3);
			sideSharedBufferInfo = GetSharedBufferInfo(6);

			EdgeSurfaceParameter surfaceParam = this.recipe.GetItem<EdgeSurfaceParameter>();
			int gap90 = surfaceParam.EdgeParamBaseTop.StartPosition - surfaceParam.EdgeParamBaseBtm.StartPosition;
			int gap45 = surfaceParam.EdgeParamBaseSide.StartPosition - surfaceParam.EdgeParamBaseBtm.StartPosition;

			// Defect 중심 원본 Image
			Rect defectRect = defect.GetRect();
			int imageWidth = this.recipe.GetItem<OriginRecipe>().OriginWidth;
			int imageHeight = this.recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseTop.ROIHeight;			
			int defectImageLeftPt = 0;
			int defectImageTopPt = (int)(defectRect.Top + (defectRect.Height / 2) - (imageHeight / 2)); //141520;
			
			Bitmap topImage, sideImage, btmImage;
			if (defect.m_nDefectCode == (int)EdgeSurface.EdgeDefectCode.Top)
			{
				byte[] bufferTop = Tools.ConvertBufferToArrayRect(topSharedBufferInfo, new Rect(defectImageLeftPt, defectImageTopPt, imageWidth, imageHeight));
				int edgeTop = CLR_IP.Cpp_FindEdge(bufferTop, imageWidth, imageHeight, 0, 0, (imageWidth - 1), (imageHeight - 1), 0, surfaceParam.EdgeParamBaseTop.EdgeSearchLevel);
				
				if (edgeTop < 0)
					edgeTop = 0;
				topImage = Tools.CovertBufferToBitmap(topSharedBufferInfo, new Rect(defectImageLeftPt + edgeTop, defectImageTopPt, imageWidth - edgeTop, imageHeight));

				byte[] bufferSide = Tools.ConvertBufferToArrayRect(sideSharedBufferInfo, new Rect(defectImageLeftPt, defectImageTopPt - gap45, imageWidth, imageHeight));
				int edgeSideLeft = CLR_IP.Cpp_FindEdge(bufferSide, imageWidth, imageHeight, 0, 0, (imageWidth - 1), (imageHeight - 1), 0, surfaceParam.EdgeParamBaseSide.EdgeSearchLevel);
				int edgeSideRight = CLR_IP.Cpp_FindEdge(bufferSide, imageWidth, imageHeight, 0, 0, (imageWidth - 1), (imageHeight - 1), 1, surfaceParam.EdgeParamBaseSide.EdgeSearchLevel);
				
				if (edgeSideRight - edgeSideLeft > 0)
					sideImage = Tools.CovertBufferToBitmap(sideSharedBufferInfo, new Rect(defectImageLeftPt + edgeSideLeft, defectImageTopPt - gap45, edgeSideRight - edgeSideLeft, imageHeight));
				else
					sideImage = Tools.CovertBufferToBitmap(sideSharedBufferInfo, new Rect(defectImageLeftPt + edgeSideLeft, defectImageTopPt - gap45, imageWidth, imageHeight));

				byte[] bufferBtm = Tools.ConvertBufferToArrayRect(topSharedBufferInfo, new Rect(defectImageLeftPt, defectImageTopPt - gap90, imageWidth, imageHeight));
				int edgeBtm = CLR_IP.Cpp_FindEdge(bufferBtm, imageWidth, imageHeight, 0, 0, (imageWidth - 1), (imageHeight - 1), 0, surfaceParam.EdgeParamBaseBtm.EdgeSearchLevel);
				
				if (edgeBtm < 0)
					edgeBtm = 0;
				btmImage = Tools.CovertBufferToBitmap(btmSharedBufferInfo, new Rect(defectImageLeftPt + edgeBtm, defectImageTopPt - gap90, imageWidth - edgeBtm, imageHeight));

				//topImage = Tools.CovertBufferToBitmap(topSharedBufferInfo, new Rect(imageLeftPt, imageTopPt, imageWidth, imageHeight));
				//sideImage = Tools.CovertBufferToBitmap(sideSharedBufferInfo, new Rect(imageLeftPt, imageTopPt - gap45, imageWidth, imageHeight));
				//btmImage = Tools.CovertBufferToBitmap(btmSharedBufferInfo, new Rect(imageLeftPt, imageTopPt - gap90, imageWidth, imageHeight));
				Tools.DrawBitmapRect(ref topImage, (float)(defectImageLeftPt - edgeTop + defectRect.Left), (float)(defectRect.Top - defectImageTopPt), defect.m_fWidth, defect.m_fHeight, PenColor.RED);
			}
			else if (defect.m_nDefectCode == (int)EdgeSurface.EdgeDefectCode.Side)
			{
				byte[] bufferTop = Tools.ConvertBufferToArrayRect(topSharedBufferInfo, new Rect(defectImageLeftPt, defectImageTopPt + gap45, imageWidth, imageHeight));
				int edgeTop = CLR_IP.Cpp_FindEdge(bufferTop, imageWidth, imageHeight, 0, 0, (imageWidth - 1), (imageHeight - 1), 0, surfaceParam.EdgeParamBaseTop.EdgeSearchLevel);
				
				if (edgeTop < 0)
					edgeTop = 0; 
				topImage = Tools.CovertBufferToBitmap(topSharedBufferInfo, new Rect(defectImageLeftPt + edgeTop, defectImageTopPt + gap45, imageWidth - edgeTop, imageHeight));

				byte[] bufferSide = Tools.ConvertBufferToArrayRect(sideSharedBufferInfo, new Rect(defectImageLeftPt, defectImageTopPt, imageWidth, imageHeight));
				int edgeSideLeft = CLR_IP.Cpp_FindEdge(bufferSide, imageWidth, imageHeight, 0, 0, (imageWidth - 1), (imageHeight - 1), 0, surfaceParam.EdgeParamBaseSide.EdgeSearchLevel);
				int edgeSideRight = CLR_IP.Cpp_FindEdge(bufferSide, imageWidth, imageHeight, 0, 0, (imageWidth - 1), (imageHeight - 1), 1, surfaceParam.EdgeParamBaseSide.EdgeSearchLevel);

				if (edgeSideRight - edgeSideLeft > 0)
					sideImage = Tools.CovertBufferToBitmap(sideSharedBufferInfo, new Rect(defectImageLeftPt + edgeSideLeft, defectImageTopPt, edgeSideRight - edgeSideLeft, imageHeight));
				else
					sideImage = Tools.CovertBufferToBitmap(sideSharedBufferInfo, new Rect(defectImageLeftPt + edgeSideLeft, defectImageTopPt, imageWidth, imageHeight));

				byte[] bufferBtm = Tools.ConvertBufferToArrayRect(topSharedBufferInfo, new Rect(defectImageLeftPt, defectImageTopPt - gap45, imageWidth, imageHeight));
				int edgeBtm = CLR_IP.Cpp_FindEdge(bufferBtm, imageWidth, imageHeight, 0, 0, (imageWidth - 1), (imageHeight - 1), 0, surfaceParam.EdgeParamBaseBtm.EdgeSearchLevel);

				if (edgeBtm < 0)
					edgeBtm = 0;
				btmImage = Tools.CovertBufferToBitmap(btmSharedBufferInfo, new Rect(defectImageLeftPt + edgeBtm, defectImageTopPt - gap45, imageWidth - edgeBtm, imageHeight));

				//topImage = Tools.CovertBufferToBitmap(topSharedBufferInfo, new Rect(imageLeftPt, imageTopPt + gap45, imageWidth, imageHeight));
				//sideImage = Tools.CovertBufferToBitmap(sideSharedBufferInfo, new Rect(imageLeftPt, imageTopPt, imageWidth, imageHeight));
				//btmImage = Tools.CovertBufferToBitmap(btmSharedBufferInfo, new Rect(imageLeftPt, imageTopPt - gap45, imageWidth, imageHeight));
				Tools.DrawBitmapRect(ref sideImage, (float)(defectImageLeftPt - edgeSideLeft + defectRect.Left), (float)(defectRect.Top - defectImageTopPt), defect.m_fWidth, defect.m_fHeight, PenColor.RED);
			}
			else if (defect.m_nDefectCode == (int)EdgeSurface.EdgeDefectCode.Btm)
			{
				byte[] bufferTop = Tools.ConvertBufferToArrayRect(topSharedBufferInfo, new Rect(defectImageLeftPt, defectImageTopPt + gap90, imageWidth, imageHeight));
				int edgeTop = CLR_IP.Cpp_FindEdge(bufferTop, imageWidth, imageHeight, 0, 0, (imageWidth - 1), (imageHeight - 1), 0, surfaceParam.EdgeParamBaseTop.EdgeSearchLevel);
				
				if (edgeTop < 0)
					edgeTop = 0;
				topImage = Tools.CovertBufferToBitmap(topSharedBufferInfo, new Rect(defectImageLeftPt + edgeTop, defectImageTopPt + gap90, imageWidth - edgeTop, imageHeight));

				byte[] bufferSide = Tools.ConvertBufferToArrayRect(sideSharedBufferInfo, new Rect(defectImageLeftPt, defectImageTopPt + gap45, imageWidth, imageHeight));
				int edgeSideLeft = CLR_IP.Cpp_FindEdge(bufferSide, imageWidth, imageHeight, 0, 0, (imageWidth - 1), (imageHeight - 1), 0, surfaceParam.EdgeParamBaseSide.EdgeSearchLevel);
				int edgeSideRight = CLR_IP.Cpp_FindEdge(bufferSide, imageWidth, imageHeight, 0, 0, (imageWidth - 1), (imageHeight - 1), 1, surfaceParam.EdgeParamBaseSide.EdgeSearchLevel);
				
				if (edgeSideRight - edgeSideLeft > 0)
					sideImage = Tools.CovertBufferToBitmap(sideSharedBufferInfo, new Rect(defectImageLeftPt + edgeSideLeft, defectImageTopPt + gap45, edgeSideRight - edgeSideLeft, imageHeight));
				else
					sideImage = Tools.CovertBufferToBitmap(sideSharedBufferInfo, new Rect(defectImageLeftPt + edgeSideLeft, defectImageTopPt + gap45, imageWidth, imageHeight));

				byte[] bufferBtm = Tools.ConvertBufferToArrayRect(topSharedBufferInfo, new Rect(defectImageLeftPt, defectImageTopPt, imageWidth, imageHeight));
				int edgeBtm = CLR_IP.Cpp_FindEdge(bufferBtm, imageWidth, imageHeight, 0, 0, (imageWidth - 1), (imageHeight - 1), 0, surfaceParam.EdgeParamBaseBtm.EdgeSearchLevel);

				if (edgeBtm < 0)
					edgeBtm = 0;
				btmImage = Tools.CovertBufferToBitmap(btmSharedBufferInfo, new Rect(defectImageLeftPt + edgeBtm, defectImageTopPt, imageWidth - edgeBtm, imageHeight));

				//topImage = Tools.CovertBufferToBitmap(topSharedBufferInfo, new Rect(imageLeftPt, imageTopPt + gap90, imageWidth, imageHeight));
				//sideImage = Tools.CovertBufferToBitmap(sideSharedBufferInfo, new Rect(imageLeftPt, imageTopPt + gap45, imageWidth, imageHeight));
				//btmImage = Tools.CovertBufferToBitmap(btmSharedBufferInfo, new Rect(imageLeftPt, imageTopPt, imageWidth, imageHeight));
				Tools.DrawBitmapRect(ref btmImage, (float)(defectImageLeftPt - edgeBtm + defectRect.Left), (float)(defectRect.Top - defectImageTopPt), defect.m_fWidth, defect.m_fHeight, PenColor.RED);
			}
			else
				return null;

			Bitmap filpTop = Tools.FlipXImage(topImage);
			Bitmap filpSide = Tools.FlipXImage(sideImage);

			Bitmap bitmap = new Bitmap(topImage.Width + sideImage.Width + btmImage.Width, imageHeight);
			Graphics g = Graphics.FromImage(bitmap);
			g.DrawImage(filpTop, 0, 0);
			g.DrawImage(filpSide, filpTop.Width, 0);
			g.DrawImage(btmImage, filpTop.Width + filpSide.Width, 0);

			return bitmap;
		}
	}
}
