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
	public class ProcessMeasurement : WorkBase
	{
		public override WORK_TYPE Type => WORK_TYPE.DEFECTPROCESS_ALL;

		protected override bool Preparation()
		{
			return true;
		}

		protected override bool Execution()
		{
			DoProcessMeasurement();
			return true;
		}

        public void DoProcessMeasurement()
        {
            if (this.currentWorkplace.Index != 0)
                return;

            List<Measurement> measureList = new List<Measurement>();

            foreach (Workplace workplace in workplaceBundle)
                foreach (Measurement measure in workplace.MeasureList)
                    measureList.Add(measure);

            foreach (Measurement measure in measureList)
                this.currentWorkplace.MeasureList.Add(measure);

            // TO DO 이거 측정마다 다를건데 어떻게 할거임
            int measureItem = Enum.GetValues(typeof(Measurement.EBRMeasureItem)).Length;

            for (int i = 0, index = 0; i < measureList.Count; i += measureItem, index++)
                for (int j = 0; j < measureItem; j++)
                    measureList[i + j].SetMeasureIndex(index);

            DatabaseManager.Instance.AddMeasurementDataList(measureList);

            SharedBufferInfo sharedBufferInfo = new SharedBufferInfo(currentWorkplace.SharedBufferR_GRAY,
                                                                     currentWorkplace.Width,
                                                                     currentWorkplace.Height,
                                                                     currentWorkplace.SharedBufferByteCnt,
                                                                     currentWorkplace.SharedBufferG,
                                                                     currentWorkplace.SharedBufferB);
            
            SettingItem_SetupEBR settings = GlobalObjects.Instance.Get<Settings>().GetItem<SettingItem_SetupEBR>();
            string sInspectionID = DatabaseManager.Instance.GetInspectionID();
            Tools.SaveMeasurementImage(Path.Combine(settings.MeasureImagePath, sInspectionID), measureList, sharedBufferInfo);

            if (GlobalObjects.Instance.Get<KlarfData_Lot>() != null)
            {
				//GlobalObjects.Instance.Get<KlarfData_Lot>().AddSlot(recipe.WaferMap, measureList, this.recipe.GetItem<OriginRecipe>());
				//GlobalObjects.Instance.Get<KlarfData_Lot>().WaferStart(recipe.WaferMap, DateTime.Now);
				//GlobalObjects.Instance.Get<KlarfData_Lot>().SetResultTimeStamp();
				//GlobalObjects.Instance.Get<KlarfData_Lot>().SaveKlarf(settings.KlarfSavePath, false);

				string sTiffImagePath = @"D:\DefectImage";
				Tools.SaveTiffImage(sTiffImagePath, measureList, sharedBufferInfo);
            }

            WorkEventManager.OnProcessMeasurementDone(this.currentWorkplace, new ProcessMeasurementDoneEventArgs());
        }

		
	}
}
