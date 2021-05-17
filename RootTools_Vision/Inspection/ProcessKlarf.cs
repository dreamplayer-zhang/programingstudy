using RootTools;
using RootTools.Database;
using RootTools_Vision.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
	public class ProcessKlarf : WorkBase
	{
		public override WORK_TYPE Type => WORK_TYPE.DEFECTPROCESS_ALL;

        protected override bool Preparation()
        {
            throw new NotImplementedException();
        }
        protected override bool Execution()
		{
            DoProcessKlarf();
            return true;

        }

        public void DoProcessKlarf()
        {
            if (!(this.currentWorkplace.MapIndexX == -1 && this.currentWorkplace.MapIndexY == -1))
                return;

            List<Defect> mergeDefectList = this.currentWorkplace.DefectList;
            string sInspectionID = DatabaseManager.Instance.GetInspectionID();

            Settings settings = new Settings();
            SettingItem_SetupFrontside settings_frontside = settings.GetItem<SettingItem_SetupFrontside>();

            //Tools.SaveDefectImage(Path.Combine(settings_frontside.DefectImagePath, sInspectionID), MergeDefectList, this.currentWorkplace.SharedBufferInfo, this.currentWorkplace.SharedBufferByteCnt);
            Tools.SaveDefectImageParallel(Path.Combine(settings_frontside.DefectImagePath, sInspectionID), mergeDefectList, this.currentWorkplace.SharedBufferInfo, this.currentWorkplace.SharedBufferByteCnt);

            //MessageBox.Show(sw.ElapsedMilliseconds.ToString());

            if (settings_frontside.UseKlarf)
            {
                KlarfData_Lot klarfData = new KlarfData_Lot();
                Directory.CreateDirectory(settings_frontside.KlarfSavePath);

                klarfData.AddSlot(recipe.WaferMap, mergeDefectList, this.recipe.GetItem<OriginRecipe>());
                klarfData.WaferStart(recipe.WaferMap, DateTime.Now);
                klarfData.SetResultTimeStamp();
                klarfData.AddSlot(recipe.WaferMap, mergeDefectList, recipe.GetItem<OriginRecipe>());
                klarfData.SaveKlarf(settings_frontside.KlarfSavePath, false);

                Tools.SaveTiffImage(settings_frontside.KlarfSavePath, mergeDefectList, this.currentWorkplace.SharedBufferInfo);
            }

            WorkEventManager.OnInspectionDone(this.currentWorkplace, new InspectionDoneEventArgs(new List<CRect>(), this.currentWorkplace));
            WorkEventManager.OnIntegratedProcessDefectDone(this.currentWorkplace, new IntegratedProcessDefectDoneEventArgs());
        }
	}
}
