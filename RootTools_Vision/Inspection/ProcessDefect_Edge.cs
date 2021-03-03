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
            int mergeDist = 1;  // RECIPE에서 가져와야함

			List<Defect> DefectList = CollectDefectData(mapX);
			List<Defect> MergeDefectList = Tools.MergeDefect(DefectList, mergeDist);

			foreach (Defect defect in MergeDefectList)
				this.currentWorkplace.DefectList.Add(defect);

            if (MergeDefectList.Count > 0)
                DatabaseManager.Instance.AddDefectDataList(MergeDefectList);
            
            SharedBufferInfo sharedBufferInfo = new SharedBufferInfo(currentWorkplace.SharedBufferR_GRAY,
                                                                     currentWorkplace.Width,
                                                                     currentWorkplace.Height,
                                                                     currentWorkplace.SharedBufferByteCnt,
                                                                     currentWorkplace.SharedBufferG,
                                                                     currentWorkplace.SharedBufferB);

            string sInspectionID = DatabaseManager.Instance.GetInspectionID();
            SettingItem_SetupEBR settings = GlobalObjects.Instance.Get<Settings>().GetItem<SettingItem_SetupEBR>();

            Tools.SaveDataImage(Path.Combine(settings.MeasureImagePath, sInspectionID), MergeDefectList.Cast<Data>().ToList(), sharedBufferInfo);

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
    }
}
