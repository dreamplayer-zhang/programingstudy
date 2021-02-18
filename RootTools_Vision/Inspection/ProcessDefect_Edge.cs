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

            string sDefectimagePath = @"D:\DefectImage";
            string sInspectionID = DatabaseManager.Instance.GetInspectionID();
            SaveDefectImage(Path.Combine(sDefectimagePath, sInspectionID), MergeDefectList, this.currentWorkplace.SharedBufferByteCnt);

            //GlobalObjects.Instance.Get<KlarfData_Lot>().AddSlot(recipe.WaferMap, MergeDefectList, this.recipe.GetItem<OriginRecipe>());
            //GlobalObjects.Instance.Get<KlarfData_Lot>().WaferStart(recipe.WaferMap, DateTime.Now);
            //GlobalObjects.Instance.Get<KlarfData_Lot>().SetResultTimeStamp();
            //GlobalObjects.Instance.Get<KlarfData_Lot>().SaveKlarf(sDefectimagePath, false);

            //WorkEventManager.OnInspectionDone(this.currentWorkplace, new InspectionDoneEventArgs(new List<CRect>(), true));
            WorkEventManager.OnIntegratedProcessDefectDone(this.currentWorkplace, new IntegratedProcessDefectDoneEventArgs());
        }

		public List<Defect> CollectDefectData(int mapX)
		{
			List<Defect> DefectList = new List<Defect>();

            //foreach (Workplace workplace in workplaceBundle)
            //	foreach (Defect defect in workplace.DefectList)
            //		DefectList.Add(defect);

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
