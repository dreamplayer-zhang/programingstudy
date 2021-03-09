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
			if (this.currentWorkplace.Index != 0)
				return;

			int mergeDist = 100;  // RECIPE에서 가져와야함

			List<Defect> topMergeDefectList = Tools.MergeDefect(CollectDefectData((int)EdgeSurface.EdgeMapPositionX.Top), mergeDist);
			List<Defect> sideMergeDefectList = Tools.MergeDefect(CollectDefectData((int)EdgeSurface.EdgeMapPositionX.Side), mergeDist);
			List<Defect> btmMergeDefectList = Tools.MergeDefect(CollectDefectData((int)EdgeSurface.EdgeMapPositionX.Btm), mergeDist);

			List<Defect> MergeDefectList = new List<Defect>();
			foreach (Defect defect in topMergeDefectList)
				MergeDefectList.Add(defect);
			foreach (Defect defect in sideMergeDefectList)
				MergeDefectList.Add(defect);
			foreach (Defect defect in btmMergeDefectList)
				MergeDefectList.Add(defect);

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

			//////////////////////////////////////////////

			//if (this.currentWorkplace.MapIndexY != -1)
			//	return;

			//int mapX = this.currentWorkplace.MapIndexX;
			//int mergeDist = 100;  // RECIPE에서 가져와야함
			//List<Defect> DefectList = CollectDefectData(mapX);
			//List<Defect> MergeDefectList = Tools.MergeDefect(DefectList, mergeDist);

			//foreach (Defect defect in MergeDefectList)
			//	this.currentWorkplace.DefectList.Add(defect);

			//if (MergeDefectList.Count > 0)
			//	DatabaseManager.Instance.AddDefectDataList(MergeDefectList);

			//SharedBufferInfo sharedBufferInfo = new SharedBufferInfo(currentWorkplace.SharedBufferR_GRAY,
			//														 currentWorkplace.SharedBufferWidth,
			//														 currentWorkplace.SharedBufferHeight,
			//														 currentWorkplace.SharedBufferByteCnt,
			//														 currentWorkplace.SharedBufferG,
			//														 currentWorkplace.SharedBufferB);

			//string sInspectionID = DatabaseManager.Instance.GetInspectionID();
			//SettingItem_SetupEdgeside settings = GlobalObjects.Instance.Get<Settings>().GetItem<SettingItem_SetupEdgeside>();
			//Tools.SaveDataImage(Path.Combine(settings.DefectImagePath, sInspectionID), MergeDefectList.Cast<Data>().ToList(), sharedBufferInfo);
			//////////////////////////////////////////////

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

	}
}
