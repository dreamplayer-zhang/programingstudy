using RootTools;
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

		private EdgeSurfaceParameter parameterEdge;
		private EdgeSurfaceRecipe recipeEdge;

		private GrabModeEdge grabModeTop;
		private GrabModeEdge grabModeSide;
		private GrabModeEdge grabModeBtm;

		public void SetGrabMode(GrabModeBase top, GrabModeBase side, GrabModeBase btm)
		{
			grabModeTop = (GrabModeEdge)top;
			grabModeSide = (GrabModeEdge)side;
			grabModeBtm = (GrabModeEdge)btm;
		}

		public override WORK_TYPE Type => WORK_TYPE.DEFECTPROCESS_ALL;

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
			DoProcessDefect_Edge();
			return true;
		}

		public void DoProcessDefect_Edge()
		{
			if (this.currentWorkplace.Index != 0)
				return;

			List<Defect> topMergeDefectList = Tools.MergeDefect(CollectDefectData((int)EdgeSurface.EdgeMapPositionX.Top), this.recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseTop.MergeDist);
			List<Defect> sideMergeDefectList = Tools.MergeDefect(CollectDefectData((int)EdgeSurface.EdgeMapPositionX.Side), this.recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseSide.MergeDist);
			List<Defect> btmMergeDefectList = Tools.MergeDefect(CollectDefectData((int)EdgeSurface.EdgeMapPositionX.Btm), this.recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseBtm.MergeDist);

			List<Defect> MergeDefectList = new List<Defect>();
			foreach (Defect defect in topMergeDefectList)
				MergeDefectList.Add(defect);
			foreach (Defect defect in sideMergeDefectList)
				MergeDefectList.Add(defect);
			foreach (Defect defect in btmMergeDefectList)
				MergeDefectList.Add(defect);

			// Top/Side/Btm 별 Defect Merge 후 Index 재정렬
			MergeDefectList = RearrangeDefectIndex(MergeDefectList);

			foreach (Defect defect in MergeDefectList)
				this.currentWorkplace.DefectList.Add(defect);

			if (MergeDefectList.Count > 0)
				DatabaseManager.Instance.AddDefectDataList(MergeDefectList, TableName);

			string sInspectionID = DatabaseManager.Instance.GetInspectionID();
			Settings settings = new Settings();
			SettingItem_SetupEdgeside settings_edgeside = settings.GetItem<SettingItem_SetupEdgeside>();

			for (int i = 0; i < MergeDefectList.Count; i++)
			{
				SharedBufferInfo sharedBufferInfo = GetBufferInfoByChipX(MergeDefectList[i].m_nChipIndexX);
				Tools.SaveDefectImage(Path.Combine(settings_edgeside.DefectImagePath, sInspectionID), MergeDefectList[i], sharedBufferInfo, i + 1);
			}

			/*
			if (settings_edgeside.UseKlarf)
			{
				KlarfData_Lot klarfData = new KlarfData_Lot();
				Directory.CreateDirectory(settings_edgeside.KlarfSavePath);

				klarfData.AddSlot(recipe.WaferMap, MergeDefectList, this.recipe.GetItem<OriginRecipe>());
				klarfData.WaferStart(recipe.WaferMap, DateTime.Now);
				klarfData.SetResultTimeStamp();
				klarfData.SaveKlarf(settings_edgeside.KlarfSavePath, false);

				SaveEdgesideTiff(settings_edgeside.KlarfSavePath, MergeDefectList, this.currentWorkplace.SharedBufferInfo);
				Tools.SaveTiffImage(settings_edgeside.KlarfSavePath, MergeDefectList, this.currentWorkplace.SharedBufferInfo);
			}
			*/

			//WorkEventManager.OnInspectionDone(this.currentWorkplace, new InspectionDoneEventArgs(new List<CRect>(), true));
			WorkEventManager.OnIntegratedProcessDefectDone(this.currentWorkplace, new IntegratedProcessDefectDoneEventArgs());
		}

		public System.Drawing.Bitmap MergeEdgesideImages(Defect defect)
		{
			SharedBufferInfo sharedBufferInfo_Top = GetBufferInfoByChipX((int)EdgeSurface.EdgeMapPositionX.Top);
			SharedBufferInfo sharedBufferInfo_Side = GetBufferInfoByChipX((int)EdgeSurface.EdgeMapPositionX.Side);
			SharedBufferInfo sharedBufferInfo_Btm = GetBufferInfoByChipX((int)EdgeSurface.EdgeMapPositionX.Btm);

			// TOP DEFECT 일 경우
			if (defect.m_nChipIndexX == (int)EdgeSurface.EdgeMapPositionX.Top)
			{
				int heightPerDegree = (int)(grabModeTop.m_nImageHeight / grabModeTop.m_nScanDegree);
				int side = (int)(grabModeTop.m_nCameraPositionOffset - grabModeSide.m_nCameraPositionOffset + defect.m_fRelY) * heightPerDegree;
				int btm = (int)(grabModeTop.m_nCameraPositionOffset - grabModeBtm.m_nCameraPositionOffset + defect.m_fRelY) * heightPerDegree;

				Rect defectRect = defect.GetRect();
				int edge = FindEdge(sharedBufferInfo_Top, new Rect(0, defectRect.Y, 500/*parameterEdge.EdgeParamBaseTop.ROIWidth*/, parameterEdge.EdgeParamBaseTop.ROIHeight));
				int width = 500/*parameterEdge.EdgeParamBaseTop.ROIWidth*/ - edge;

				Rect calcDefectRect_Top = new Rect(edge, defectRect.Y - (parameterEdge.EdgeParamBaseTop.ROIHeight / 2), width, parameterEdge.EdgeParamBaseTop.ROIHeight);
				//Rect calcDefectRect_Top = new Rect(0, defectRect.Y - (parameterEdge.EdgeParamBaseTop.ROIHeight /2), parameterEdge.EdgeParamBaseTop.ROIWidth, parameterEdge.EdgeParamBaseTop.ROIHeight);
				Rect calcDefectRect_Side = new Rect(0, side - (parameterEdge.EdgeParamBaseTop.ROIHeight / 2), 500/*parameterEdge.EdgeParamBaseTop.ROIWidth*/, parameterEdge.EdgeParamBaseTop.ROIHeight);
				Rect calcDefectRect_Btm = new Rect(0, btm - (parameterEdge.EdgeParamBaseTop.ROIHeight / 2), 500/*parameterEdge.EdgeParamBaseTop.ROIWidth*/, parameterEdge.EdgeParamBaseTop.ROIHeight);

				Bitmap bitmapTop = Tools.CovertBufferToBitmap(sharedBufferInfo_Top, calcDefectRect_Top);
				Bitmap bitmapSide = Tools.CovertBufferToBitmap(sharedBufferInfo_Side, calcDefectRect_Side);
				Bitmap bitmapBtm = Tools.CovertBufferToBitmap(sharedBufferInfo_Btm, calcDefectRect_Btm);
				Bitmap filpBtm = Tools.FlipYImage(bitmapBtm);

				Bitmap bitmap = new Bitmap(width + bitmapSide.Width + bitmapBtm.Width, bitmapTop.Height);
				Graphics g = Graphics.FromImage(bitmap);
				g.DrawImage(bitmapTop, 0, 0);
				g.DrawImage(bitmapSide, width, 0);
				g.DrawImage(filpBtm/*bitmapBtm*/, width + bitmapSide.Width, 0);
				bitmap.Save(@"D:\test" + defect.m_nDefectIndex.ToString() + ".bmp");

				return bitmap;
			}

			return null;
		}

		private int FindEdge(SharedBufferInfo info, Rect rect)
		{
			byte[] buffer = Tools.ConvertBufferToArrayRect(info, rect);
			CRect cRect = new CRect((int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Bottom);

			Bitmap bitmap = Tools.ConvertArrayToBitmapRect(buffer, cRect.Width, cRect.Height, 1, cRect);
			bitmap.Save(@"D:\1.bmp");

			int edge = FindEdge(buffer, (int)rect.Width, (int)rect.Height, 20);

			return edge;
		}

		private int FindEdge(byte[] arrSrc, int width, int height, int searchLevel = 70)
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

		private int MeanOfYCoordinates(byte[] arrSrc, int startPtY, int findPtX, int width, int height)
		{
			int avg = 0;

			for (int y = startPtY; y < width * height; y += width)
				avg += arrSrc[y + findPtX];

			if (avg != 0)
				avg /= (height - startPtY + 1);

			return avg;
		}

		private void SaveEdgesideTiff(string Path, List<Defect> dataList, SharedBufferInfo sharedBuffer)
		{
			Path += "\\";
			DirectoryInfo di = new DirectoryInfo(Path);
			if (!di.Exists)
				di.Create();

			ArrayList inputImage = new ArrayList();
			for (int i = 0; i < dataList.Count; i++)
			{
				MemoryStream image = new MemoryStream();
				//System.Drawing.Bitmap bitmap = Tools.CovertBufferToBitmap(sharedBuffer, dataList[i].GetRect());
				System.Drawing.Bitmap bitmap = MergeEdgesideImages(dataList[i]);
				bitmap.Save(image, ImageFormat.Tiff);
				inputImage.Add(image);
			}

			ImageCodecInfo info = null;
			foreach (ImageCodecInfo ice in ImageCodecInfo.GetImageEncoders())
			{
				if (ice.MimeType == "image/tiff")
				{
					info = ice;
					break;
				}
			}

			string test = "test";
			Path += test + ".tiff";

			EncoderParameters ep = new EncoderParameters(2);

			bool firstPage = true;

			System.Drawing.Image img = null;

			for (int i = 0; i < inputImage.Count; i++)
			{
				System.Drawing.Image img_src = System.Drawing.Image.FromStream((Stream)inputImage[i]);
				Guid guid = img_src.FrameDimensionsList[0];
				System.Drawing.Imaging.FrameDimension dimension = new System.Drawing.Imaging.FrameDimension(guid);

				for (int nLoopFrame = 0; nLoopFrame < img_src.GetFrameCount(dimension); nLoopFrame++)
				{
					img_src.SelectActiveFrame(dimension, nLoopFrame);

					ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, Convert.ToInt32(EncoderValue.CompressionLZW));

					if (firstPage)
					{
						img = img_src;

						ep.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, Convert.ToInt32(EncoderValue.MultiFrame));
						img.Save(Path, info, ep);

						firstPage = false;
						continue;
					}

					ep.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, Convert.ToInt32(EncoderValue.FrameDimensionPage));
					img.SaveAdd(img_src, ep);
				}
			}
			if (inputImage.Count == 0)
			{
				File.Create(Path);
				return;
			}

			ep.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, Convert.ToInt32(EncoderValue.Flush));
			img.SaveAdd(ep);
		}

		public List<Defect> CollectDefectData(int chipX)
		{
			List<Defect> DefectList = new List<Defect>();

			foreach (Workplace workplace in workplaceBundle)
			{
				if (workplace.MapIndexX == chipX)
				{
					foreach (Defect defect in workplace.DefectList)
						DefectList.Add(defect);
				}
			}
			return DefectList;
		}

		private SharedBufferInfo GetBufferInfoByChipX(int chipX)
		{
			foreach (Workplace workplace in workplaceBundle)
			{
				if (workplace.MapIndexX == chipX)
					return workplace.SharedBufferInfo;
			}

			return currentWorkplace.SharedBufferInfo;
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
	}
}
