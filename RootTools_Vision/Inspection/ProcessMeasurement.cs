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
		string TableName = "measurement";
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

		/// <summary>
		/// EBR Measurement Data Processing
		/// </summary>
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

            int measureItem = Enum.GetValues(typeof(Measurement.EBRMeasureItem)).Length;

            for (int i = 0, index = 1; i < measureList.Count; i += measureItem, index++)
                for (int j = 0; j < measureItem; j++)
                    measureList[i + j].SetMeasureIndex(index);

			if (measureList.Count > 0)
				DatabaseManager.Instance.AddMeasurementDataList(measureList);

			#region Klarf / Defect Image 저장

			// Klarf에만 표시되는 Data
			List<Measurement> realKlarfData = new List<Measurement>();
			for (int i = 0; i < measureList.Count; i++)
			{
				if (measureList[i].m_strMeasureItem == Measurement.EBRMeasureItem.EBR.ToString())
					realKlarfData.Add(measureList[i]);
			}

			string sInspectionID = DatabaseManager.Instance.GetInspectionID();
			Settings settings = new Settings();
			SettingItem_SetupEBR settings_ebr = settings.GetItem<SettingItem_SetupEBR>();

			Tools.SaveDefectImageParallel(Path.Combine(settings_ebr.MeasureImagePath, sInspectionID), measureList, this.currentWorkplace.SharedBufferInfo, this.currentWorkplace.SharedBufferInfo.ByteCnt);

			// EBR 원형 이미지 저장
			EBRRecipe recipeParam = this.recipe.GetItem<EBRRecipe>();
			Tools.SaveCircleImage(Path.Combine(settings_ebr.MeasureImagePath, sInspectionID, (realKlarfData.Count + 1).ToString()), settings_ebr.OutputImageSizeWidth, settings_ebr.OutputImageSizeHeight
								  , this.currentWorkplace.SharedBufferInfo, recipeParam.FirstNotch, recipeParam.LastNotch);

			//if (settings_ebr.UseKlarf)
			//{
			//	KlarfData_Lot klarfData = new KlarfData_Lot();
			//	Directory.CreateDirectory(settings_ebr.KlarfSavePath);

			//	klarfData.AddSlot(recipe.WaferMap, realKlarfData, this.recipe.GetItem<OriginRecipe>());
			//	klarfData.WaferStart(recipe.WaferMap, DateTime.Now);
			//	klarfData.SetResultTimeStamp();
			//	klarfData.SaveKlarf(settings_ebr.KlarfSavePath, false);
			//}

			#endregion
            WorkEventManager.OnProcessMeasurementDone(this.currentWorkplace, new ProcessMeasurementDoneEventArgs());
        }
	}
}
