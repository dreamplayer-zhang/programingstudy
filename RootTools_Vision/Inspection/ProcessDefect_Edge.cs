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
			
			List<Defect> topDefectList = CollectDefectData(0, 2);
			List<Defect> btmDefectList = CollectDefectData(3, 5);
			List<Defect> sideDefectList = CollectDefectData(6, 8);

			List<Defect> topMergeDefectList = Tools.MergeDefect(topDefectList, this.recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseTop.MergeDist);
			List<Defect> btmMergeDefectList = Tools.MergeDefect(btmDefectList, this.recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseBtm.MergeDist);
			List<Defect> sideMergeDefectList = Tools.MergeDefect(sideDefectList, this.recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseSide.MergeDist);

			List<Defect> mergeDefectList = new List<Defect>();
			foreach (Defect defect in topMergeDefectList)
				mergeDefectList.Add(defect);
			foreach (Defect defect in btmMergeDefectList)
				mergeDefectList.Add(defect);
			foreach (Defect defect in sideMergeDefectList)
				mergeDefectList.Add(defect);

			topMergeDefectList.Clear();
			btmMergeDefectList.Clear();
			sideMergeDefectList.Clear();

			// Top/Side/Btm 별 Defect Merge 후 Index 재정렬
			mergeDefectList = RearrangeDefectIndex(mergeDefectList);
			foreach (Defect defect in mergeDefectList)
				this.currentWorkplace.DefectList.Add(defect);

			if (mergeDefectList.Count > 0)
				DatabaseManager.Instance.AddDefectDataList(mergeDefectList, TableName);

			int index = 0;
			foreach (Defect defect in mergeDefectList)
			{
				index = (defect.m_nDefectCode - 10000) / 100;
				if (index >= 0 && index < 3)
					topMergeDefectList.Add(defect);
				if (index >= 3 && index < 6)
					btmMergeDefectList.Add(defect);
				if (index >= 6 && index < 10)
					sideMergeDefectList.Add(defect);
			}

			#region Klarf / Defect Image 저장

			string sInspectionID = DatabaseManager.Instance.GetInspectionID();
			Settings settings = new Settings();
			SettingItem_SetupEdgeside settings_edgeside = settings.GetItem<SettingItem_SetupEdgeside>();
			SharedBufferInfo topSharedBufferInfo, btmSharedBufferInfo, sideSharedBufferInfo;

			topSharedBufferInfo = GetSharedBufferInfo(0);
			Tools.SaveDefectImageParallel(Path.Combine(settings_edgeside.DefectImagePath, sInspectionID), topMergeDefectList, topSharedBufferInfo, topSharedBufferInfo.ByteCnt);
			btmSharedBufferInfo = GetSharedBufferInfo(3);
			Tools.SaveDefectImageParallel(Path.Combine(settings_edgeside.DefectImagePath, sInspectionID), btmMergeDefectList, btmSharedBufferInfo, btmSharedBufferInfo.ByteCnt);
			sideSharedBufferInfo = GetSharedBufferInfo(6);
			Tools.SaveDefectImageParallel(Path.Combine(settings_edgeside.DefectImagePath, sInspectionID), sideMergeDefectList, sideSharedBufferInfo, sideSharedBufferInfo.ByteCnt);

			if (settings_edgeside.UseKlarf)
			{
				KlarfData_Lot klarfData = new KlarfData_Lot();
				Directory.CreateDirectory(settings_edgeside.KlarfSavePath);

				klarfData.AddSlot(recipe.WaferMap, mergeDefectList, this.recipe.GetItem<OriginRecipe>());
				klarfData.WaferStart(recipe.WaferMap, DateTime.Now);
				klarfData.SetResultTimeStamp();
				klarfData.SaveKlarf(settings_edgeside.KlarfSavePath, false);

				Tools.SaveTiffImage(settings_edgeside.KlarfSavePath, "edgetop"+ sInspectionID, topMergeDefectList, topSharedBufferInfo);
				Tools.SaveTiffImage(settings_edgeside.KlarfSavePath, "edgeBttom" + sInspectionID, btmMergeDefectList, btmSharedBufferInfo);
				Tools.SaveTiffImage(settings_edgeside.KlarfSavePath, "edgeSide" + sInspectionID, sideMergeDefectList, sideSharedBufferInfo);
			}
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

		//public System.Drawing.Bitmap MergeEdgesideImages(Defect defect)
		//{
		//	SharedBufferInfo sharedBufferInfo_Top = GetBufferInfoByChipX((int)EdgeSurface.EdgeMapPositionX.Top);
		//	SharedBufferInfo sharedBufferInfo_Side = GetBufferInfoByChipX((int)EdgeSurface.EdgeMapPositionX.Side);
		//	SharedBufferInfo sharedBufferInfo_Btm = GetBufferInfoByChipX((int)EdgeSurface.EdgeMapPositionX.Btm);

		//	// TOP DEFECT 일 경우
		//	if (defect.m_nChipIndexX == (int)EdgeSurface.EdgeMapPositionX.Top)
		//	{
		//		int heightPerDegree = (int)(grabModeTop.m_nImageHeight / grabModeTop.m_nScanDegree);
		//		int side = (int)(grabModeTop.m_nCameraPositionOffset - grabModeSide.m_nCameraPositionOffset + defect.m_fRelY) * heightPerDegree;
		//		int btm = (int)(grabModeTop.m_nCameraPositionOffset - grabModeBtm.m_nCameraPositionOffset + defect.m_fRelY) * heightPerDegree;

		//		Rect defectRect = defect.GetRect();
		//		int edge = 0; // CLR_IP.FindEdge(sharedBufferInfo_Top, new Rect(0, defectRect.Y, 500/*parameterEdge.EdgeParamBaseTop.ROIWidth*/, parameterEdge.EdgeParamBaseTop.ROIHeight));
		//		int width = 500/*parameterEdge.EdgeParamBaseTop.ROIWidth*/ - edge;

		//		Rect calcDefectRect_Top = new Rect(edge, defectRect.Y - (parameterEdge.EdgeParamBaseTop.ROIHeight / 2), width, parameterEdge.EdgeParamBaseTop.ROIHeight);
		//		//Rect calcDefectRect_Top = new Rect(0, defectRect.Y - (parameterEdge.EdgeParamBaseTop.ROIHeight /2), parameterEdge.EdgeParamBaseTop.ROIWidth, parameterEdge.EdgeParamBaseTop.ROIHeight);
		//		Rect calcDefectRect_Side = new Rect(0, side - (parameterEdge.EdgeParamBaseTop.ROIHeight / 2), 500/*parameterEdge.EdgeParamBaseTop.ROIWidth*/, parameterEdge.EdgeParamBaseTop.ROIHeight);
		//		Rect calcDefectRect_Btm = new Rect(0, btm - (parameterEdge.EdgeParamBaseTop.ROIHeight / 2), 500/*parameterEdge.EdgeParamBaseTop.ROIWidth*/, parameterEdge.EdgeParamBaseTop.ROIHeight);

		//		Bitmap bitmapTop = Tools.CovertBufferToBitmap(sharedBufferInfo_Top, calcDefectRect_Top);
		//		Bitmap bitmapSide = Tools.CovertBufferToBitmap(sharedBufferInfo_Side, calcDefectRect_Side);
		//		Bitmap bitmapBtm = Tools.CovertBufferToBitmap(sharedBufferInfo_Btm, calcDefectRect_Btm);
		//		Bitmap filpBtm = Tools.FlipYImage(bitmapBtm);

		//		Bitmap bitmap = new Bitmap(width + bitmapSide.Width + bitmapBtm.Width, bitmapTop.Height);
		//		Graphics g = Graphics.FromImage(bitmap);
		//		g.DrawImage(bitmapTop, 0, 0);
		//		g.DrawImage(bitmapSide, width, 0);
		//		g.DrawImage(filpBtm/*bitmapBtm*/, width + bitmapSide.Width, 0);
		//		bitmap.Save(@"D:\test" + defect.m_nDefectIndex.ToString() + ".bmp");

		//		return bitmap;
		//	}

		//	return null;
		//}

		//private void SaveEdgesideTiff(string Path, List<Defect> dataList, SharedBufferInfo sharedBuffer)
		//{
		//	Path += "\\";
		//	DirectoryInfo di = new DirectoryInfo(Path);
		//	if (!di.Exists)
		//		di.Create();

		//	ArrayList inputImage = new ArrayList();
		//	for (int i = 0; i < dataList.Count; i++)
		//	{
		//		MemoryStream image = new MemoryStream();
		//		//System.Drawing.Bitmap bitmap = Tools.CovertBufferToBitmap(sharedBuffer, dataList[i].GetRect());
		//		System.Drawing.Bitmap bitmap = MergeEdgesideImages(dataList[i]);
		//		bitmap.Save(image, ImageFormat.Tiff);
		//		inputImage.Add(image);
		//	}

		//	ImageCodecInfo info = null;
		//	foreach (ImageCodecInfo ice in ImageCodecInfo.GetImageEncoders())
		//	{
		//		if (ice.MimeType == "image/tiff")
		//		{
		//			info = ice;
		//			break;
		//		}
		//	}

		//	string test = "test";
		//	Path += test + ".tiff";

		//	EncoderParameters ep = new EncoderParameters(2);

		//	bool firstPage = true;

		//	System.Drawing.Image img = null;

		//	for (int i = 0; i < inputImage.Count; i++)
		//	{
		//		System.Drawing.Image img_src = System.Drawing.Image.FromStream((Stream)inputImage[i]);
		//		Guid guid = img_src.FrameDimensionsList[0];
		//		System.Drawing.Imaging.FrameDimension dimension = new System.Drawing.Imaging.FrameDimension(guid);

		//		for (int nLoopFrame = 0; nLoopFrame < img_src.GetFrameCount(dimension); nLoopFrame++)
		//		{
		//			img_src.SelectActiveFrame(dimension, nLoopFrame);

		//			ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, Convert.ToInt32(EncoderValue.CompressionLZW));

		//			if (firstPage)
		//			{
		//				img = img_src;

		//				ep.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, Convert.ToInt32(EncoderValue.MultiFrame));
		//				img.Save(Path, info, ep);

		//				firstPage = false;
		//				continue;
		//			}

		//			ep.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, Convert.ToInt32(EncoderValue.FrameDimensionPage));
		//			img.SaveAdd(img_src, ep);
		//		}
		//	}
		//	if (inputImage.Count == 0)
		//	{
		//		File.Create(Path);
		//		return;
		//	}

		//	ep.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, Convert.ToInt32(EncoderValue.Flush));
		//	img.SaveAdd(ep);
		//}

		public List<Defect> CollectDefectData()
		{
			List<Defect> DefectList = new List<Defect>();

			int index = 0;
			foreach (Workplace workplace in workplaceBundle)
			{
				if (workplace.DefectList == null) continue;

				foreach (Defect defect in workplace.DefectList)
				{
					defect.m_nDefectIndex = index++;
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
		public List<Defect> CollectDefectData(int min, int max)
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
	}
}
