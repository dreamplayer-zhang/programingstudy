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
		public ProcessDefect_Edge()
		{

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
				DatabaseManager.Instance.AddDefectDataList(MergeDefectList);

			SettingItem_SetupEdgeside settings = GlobalObjects.Instance.Get<Settings>().GetItem<SettingItem_SetupEdgeside>();
			string sInspectionID = DatabaseManager.Instance.GetInspectionID();
			for (int i = 0; i < MergeDefectList.Count; i++)
			{
				SharedBufferInfo sharedBufferInfo = GetSharedBufferInfoByChipX(MergeDefectList[i].m_nChipIndexX);
				Tools.SaveDefectImage(Path.Combine(settings.DefectImagePath, sInspectionID), MergeDefectList[i], sharedBufferInfo, i + 1);
			}

			if (GlobalObjects.Instance.Get<KlarfData_Lot>() != null)
			{
				List<string> dataStringList = ConvertDataListToStringList(MergeDefectList);
				GlobalObjects.Instance.Get<KlarfData_Lot>().AddSlot(recipe.WaferMap, dataStringList, null);
				GlobalObjects.Instance.Get<KlarfData_Lot>().WaferStart(recipe.WaferMap, DateTime.Now);
				GlobalObjects.Instance.Get<KlarfData_Lot>().SetResultTimeStamp();
				GlobalObjects.Instance.Get<KlarfData_Lot>().SaveKlarf(settings.KlarfSavePath, false);

				//Tools.SaveTiffImage(settings.KlarfSavePath, MergeDefectList.Cast<Data>().ToList(), sharedBufferInfo);
			}
			//WorkEventManager.OnInspectionDone(this.currentWorkplace, new InspectionDoneEventArgs(new List<CRect>(), true));
			WorkEventManager.OnIntegratedProcessDefectDone(this.currentWorkplace, new IntegratedProcessDefectDoneEventArgs());
		}

		public void TEST(string Path/*, List<Data> dataList, SharedBufferInfo sharedBuffer*/)
		{
			Path += "\\";
			DirectoryInfo di = new DirectoryInfo(Path);
			if (!di.Exists)
				di.Create();

			Defect defect = new Defect();
			SharedBufferInfo sharedBufferInfo_Top = GetSharedBufferInfoByChipX((int)EdgeSurface.EdgeMapPositionX.Top);
			SharedBufferInfo sharedBufferInfo_Side = GetSharedBufferInfoByChipX((int)EdgeSurface.EdgeMapPositionX.Side);
			SharedBufferInfo sharedBufferInfo_Btm = GetSharedBufferInfoByChipX((int)EdgeSurface.EdgeMapPositionX.Btm);

			int posOffset_Top = this.recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseTop.PositionOffset;
			int posOffset_Side = this.recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseSide.PositionOffset;
			int posOffset_Btm = this.recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseBtm.PositionOffset;

			System.Drawing.Bitmap bitmap_Top = Tools.ConvertArrayToColorBitmap(sharedBufferInfo_Top.PtrR_GRAY, sharedBufferInfo_Top.PtrG, sharedBufferInfo_Top.PtrB, sharedBufferInfo_Top.Width, sharedBufferInfo_Top.ByteCnt, defect.GetRect());



			/////
			/*
			ArrayList inputImage = new ArrayList();
			for (int i = 0; i < dataList.Count; i++)
			{
				MemoryStream image = new MemoryStream();
				System.Drawing.Bitmap bitmap = Tools.ConvertArrayToColorBitmap(sharedBuffer.PtrR_GRAY, sharedBuffer.PtrG, sharedBuffer.PtrB, sharedBuffer.Width, sharedBuffer.ByteCnt, dataList[i].GetRect());
				//System.Drawing.Bitmap NewImg = new System.Drawing.Bitmap(bitmap);
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
			*/
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

		private List<string> ConvertDataListToStringList(List<Defect> defectList)
		{
			List<string> stringList = new List<string>();
			foreach (Defect defect in defectList)
			{
				//string str = string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} {16}");
				//stringList.Add(str);
			}
			return stringList;
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
