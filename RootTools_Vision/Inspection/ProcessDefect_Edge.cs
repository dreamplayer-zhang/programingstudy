using RootTools.Database;
using RootTools_CLR;
using RootTools_Vision.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
	public class ProcessDefect_Edge : WorkBase
	{
        string TableName;
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
				SharedBufferInfo sharedBufferInfo = GetSharedBufferInfoByChipX(MergeDefectList[i].m_nChipIndexX);
				Tools.SaveDefectImage(Path.Combine(settings_edgeside.DefectImagePath, sInspectionID), MergeDefectList[i], sharedBufferInfo, i + 1);
			}

			if (settings_edgeside.UseKlarf)
			{
				KlarfData_Lot klarfData = new KlarfData_Lot();
				Directory.CreateDirectory(settings_edgeside.KlarfSavePath);

				klarfData.AddSlot(recipe.WaferMap, MergeDefectList, this.recipe.GetItem<OriginRecipe>());
				klarfData.WaferStart(recipe.WaferMap, DateTime.Now);
				klarfData.SetResultTimeStamp();

				klarfData.SaveKlarf(settings_edgeside.KlarfSavePath, false);

				//TEST(settings_edgeside.KlarfSavePath, MergeDefectList, this.currentWorkplace.SharedBufferInfo);
				//Tools.SaveTiffImage(settings_edgeside.KlarfSavePath, MergeDefectList, this.currentWorkplace.SharedBufferInfo);
			}
			//WorkEventManager.OnInspectionDone(this.currentWorkplace, new InspectionDoneEventArgs(new List<CRect>(), true));
			WorkEventManager.OnIntegratedProcessDefectDone(this.currentWorkplace, new IntegratedProcessDefectDoneEventArgs());
		}

		public System.Drawing.Bitmap testest(Defect defect)
		{
			SharedBufferInfo sharedBufferInfo_Top = GetSharedBufferInfoByChipX((int)EdgeSurface.EdgeMapPositionX.Top);
			SharedBufferInfo sharedBufferInfo_Side = GetSharedBufferInfoByChipX((int)EdgeSurface.EdgeMapPositionX.Side);
			SharedBufferInfo sharedBufferInfo_Btm = GetSharedBufferInfoByChipX((int)EdgeSurface.EdgeMapPositionX.Btm);

			// 각도로 가져오니까 필요없어
			//int posOffset_Top = this.recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseTop.PositionOffset;
			//int posOffset_Side = this.recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseSide.PositionOffset;
			//int posOffset_Btm = this.recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseBtm.PositionOffset;

			// TOP DEFECT 일 경우
			//첫 notch + (각도 * 1도당 image height)
			// side
			int imageHeightPerDegree_Side = (int)(this.recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseSide.EndNotch - this.recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseSide.StartNotch) / 360000;
			int sideY = this.recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseSide.StartNotch + (int)((defect.m_fRelY) * imageHeightPerDegree_Side);
			// bottom
			int imageHeightPerDegree_Btm = (int)(this.recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseBtm.EndNotch - this.recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseBtm.StartNotch) / 360000;
			int btmY = this.recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseBtm.StartNotch + (int)((defect.m_fRelY) * imageHeightPerDegree_Btm);


			System.Windows.Rect defectRect = defect.GetRect();
			System.Windows.Rect calcDefectRect_Side = new System.Windows.Rect(defectRect.X, defectRect.Y + sideY, defectRect.Width, defectRect.Height);
			System.Windows.Rect calcDefectRect_Btm = new System.Windows.Rect(defectRect.X, defectRect.Y + btmY, defectRect.Width, defectRect.Height);

			System.Drawing.Bitmap bitmap_Top = Tools.ConvertArrayToColorBitmap(sharedBufferInfo_Top.PtrR_GRAY, sharedBufferInfo_Top.PtrG, sharedBufferInfo_Top.PtrB, sharedBufferInfo_Top.Width, sharedBufferInfo_Top.ByteCnt, defect.GetRect());
			System.Drawing.Bitmap bitmap_Side = Tools.ConvertArrayToColorBitmap(sharedBufferInfo_Side.PtrR_GRAY, sharedBufferInfo_Side.PtrG, sharedBufferInfo_Side.PtrB, sharedBufferInfo_Side.Width, sharedBufferInfo_Side.ByteCnt, calcDefectRect_Side);
			System.Drawing.Bitmap bitmap_Btm = Tools.ConvertArrayToColorBitmap(sharedBufferInfo_Btm.PtrR_GRAY, sharedBufferInfo_Btm.PtrG, sharedBufferInfo_Btm.PtrB, sharedBufferInfo_Btm.Width, sharedBufferInfo_Btm.ByteCnt, calcDefectRect_Btm);

			//System.Drawing.Bitmap bmap = new System.Drawing.Bitmap(100, 100);
			//System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmap);
			//System.Drawing.Image img = System.Drawing.Bitmap.FromFile("d:\\test.gif");
			//System.Drawing.Image img2 = System.Drawing.Bitmap.FromFile("d:\\test2.gif");

			System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(bitmap_Top.Width + bitmap_Side.Width + bitmap_Btm.Width, bitmap_Top.Height);
			System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);
			g.DrawImage(bitmap_Top, 0, 0);
			g.DrawImage(bitmap_Side, bitmap_Top.Width, 0);
			g.DrawImage(bitmap_Btm, bitmap_Top.Width + bitmap_Side.Width, 0);
			bitmap.Save(@"D:\test.bmp");

			return bitmap;
		}


		public void TEST(string Path, List<Defect> dataList, SharedBufferInfo sharedBuffer)
		{
			Path += "\\";
			DirectoryInfo di = new DirectoryInfo(Path);
			if (!di.Exists)
				di.Create();

			ArrayList inputImage = new ArrayList();
			for (int i = 0; i < dataList.Count; i++)
			{
				MemoryStream image = new MemoryStream();
				//System.Drawing.Bitmap bitmap = Tools.ConvertArrayToColorBitmap(sharedBuffer.PtrR_GRAY, sharedBuffer.PtrG, sharedBuffer.PtrB, sharedBuffer.Width, sharedBuffer.ByteCnt, dataList[i].GetRect());
				System.Drawing.Bitmap bitmap = testest(dataList[i]);
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

		private SharedBufferInfo GetSharedBufferInfoByChipX(int chipX)
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
