using RootTools.Database;
using RootTools_CLR;
using RootTools_Vision.Utility;
using System;
using System.Collections.Generic;
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
			if (this.currentWorkplace.MapIndexY != -1)
				return;

			int mapX = this.currentWorkplace.MapIndexX;
			int mergeDist = 100;  // RECIPE에서 가져와야함

			List<Defect> DefectList = CollectDefectData(mapX);
			List<Defect> MergeDefectList = Tools.MergeDefect(DefectList, mergeDist);

			foreach (Defect defect in MergeDefectList)
				this.currentWorkplace.DefectList.Add(defect);

			if (MergeDefectList.Count > 0)
				DatabaseManager.Instance.AddDefectDataList(MergeDefectList);

			SharedBufferInfo sharedBufferInfo = new SharedBufferInfo(currentWorkplace.SharedBufferR_GRAY,
																	 currentWorkplace.SharedBufferWidth,
																	 currentWorkplace.SharedBufferHeight,
																	 currentWorkplace.SharedBufferByteCnt,
																	 currentWorkplace.SharedBufferG,
																	 currentWorkplace.SharedBufferB);

			string sInspectionID = DatabaseManager.Instance.GetInspectionID();
			SettingItem_SetupEdgeside settings = GlobalObjects.Instance.Get<Settings>().GetItem<SettingItem_SetupEdgeside>();

			Tools.SaveDataImage(Path.Combine(settings.DefectImagePath, sInspectionID), MergeDefectList.Cast<Data>().ToList(), sharedBufferInfo);
			SaveDefectImage(Path.Combine(settings.DefectImagePath, sInspectionID), MergeDefectList, this.currentWorkplace.SharedBufferByteCnt);

			if (GlobalObjects.Instance.Get<KlarfData_Lot>() != null)
			{
				List<string> dataStringList = GlobalObjects.Instance.Get<KlarfData_Lot>().DefectDataToStringList(MergeDefectList);
				GlobalObjects.Instance.Get<KlarfData_Lot>().AddSlot(recipe.WaferMap, dataStringList, null);
				GlobalObjects.Instance.Get<KlarfData_Lot>().WaferStart(recipe.WaferMap, DateTime.Now);
				GlobalObjects.Instance.Get<KlarfData_Lot>().SetResultTimeStamp();
				GlobalObjects.Instance.Get<KlarfData_Lot>().SaveKlarf(settings.KlarfSavePath, false);

				Tools.SaveTiffImage(settings.KlarfSavePath, MergeDefectList.Cast<Data>().ToList(), sharedBufferInfo);
			}

			//WorkEventManager.OnInspectionDone(this.currentWorkplace, new InspectionDoneEventArgs(new List<CRect>(), true));
			WorkEventManager.OnIntegratedProcessDefectDone(this.currentWorkplace, new IntegratedProcessDefectDoneEventArgs());
		}

		public List<Defect> CollectDefectData(int mapX)
		{
			List<Defect> DefectList = new List<Defect>();

			foreach (Workplace workplace in workplaceBundle)
			{
				if (workplace.MapIndexX == mapX)
				{
					foreach (Defect defect in workplace.DefectList)
						DefectList.Add(defect);
				}
			}
			return DefectList;
		}

		private void SaveDefectImage(String Path, List<Defect> DefectList, int nByteCnt)
		{
			Path += "\\";
			DirectoryInfo di = new DirectoryInfo(Path);
			if (!di.Exists)
				di.Create();

			if (DefectList.Count < 1)
				return;

			unsafe
			{
				Cpp_Rect[] defectArray = new Cpp_Rect[DefectList.Count];

				for (int i = 0; i < DefectList.Count; i++)
				{
					Cpp_Rect rect = new Cpp_Rect();
					rect.x = (int)DefectList[i].p_rtDefectBox.Left;
					rect.y = (int)DefectList[i].p_rtDefectBox.Top;
					rect.w = (int)DefectList[i].m_fWidth;
					rect.h = (int)DefectList[i].m_fHeight;

					defectArray[i] = rect;
				}

				if (nByteCnt == 1)
				{
					CLR_IP.Cpp_SaveDefectListBMP(
					   Path,
					   (byte*)currentWorkplace.SharedBufferR_GRAY.ToPointer(),
					   currentWorkplace.SharedBufferWidth,
					   currentWorkplace.SharedBufferHeight,
					   defectArray
					   );
				}

				else if (nByteCnt == 3)
				{
					CLR_IP.Cpp_SaveDefectListBMP_Color(
						Path,
					   (byte*)currentWorkplace.SharedBufferR_GRAY.ToPointer(),
					   (byte*)currentWorkplace.SharedBufferG.ToPointer(),
					   (byte*)currentWorkplace.SharedBufferB.ToPointer(),
					   currentWorkplace.SharedBufferWidth,
					   currentWorkplace.SharedBufferHeight,
					   defectArray);


				}
			}
		}
	}
}
