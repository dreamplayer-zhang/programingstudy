using RootTools_Vision.UserTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools.Database;
using System.Windows;
using RootTools;
using System.ComponentModel;
using RootTools_CLR;
using System.IO;

namespace RootTools_Vision
{
    public class ProcessDefect_Wafer : WorkBase
    {
        Workplace workplace;
        WorkplaceBundle workplaceBundle;

        RecipeData recipeData;
        RecipeData_Origin recipeData_Origin;

        public ProcessDefect_Wafer()
        {
        }

        public override WORK_TYPE Type => WORK_TYPE.FINISHINGWORK;

        public override void DoWork()
        {
            DoProcessDefect_Wafer();
        }

        public override void SetWorkplace(Workplace _workplace)
        {
            this.workplace = _workplace;
        }

        public override void SetWorkplaceBundle(WorkplaceBundle _workplaceBundle)
        {
            this.workplaceBundle = _workplaceBundle;
        }

        public void DoProcessDefect_Wafer()
        {
            if (!(this.workplace.MapPositionX == -1 && this.workplace.MapPositionY == -1))
                return;

            // Option Param
            int mergeDist = 1;
            int backsideOffset = 50;
            bool isBackside = true;
            // Recipe
            int waferCenterX = recipeData_Origin.Backside_CenterX;
            int waferCenterY = recipeData_Origin.Backside_CenterY;
            int radius = recipeData_Origin.Backside_Radius;

            // 구조는 나중에 생각해봅시다...
            List<Defect> DefectList = CollectDefectData();
            if(isBackside) // Backside Option
                DeleteOutsideDefect(DefectList, waferCenterX, waferCenterY, radius, backsideOffset);

            List<Defect> MergeDefectList = MergeDefect(DefectList, mergeDist);

            foreach (Defect defect in MergeDefectList)
            {
                if (isBackside)
                    defect.CalcAbsToRelPos(waferCenterX, waferCenterY);

                else; // Frontside
            }

            Workplace displayDefect = new Workplace();
            foreach (Defect defect in MergeDefectList)
                displayDefect.DefectList.Add(defect);

            string sDefectimagePath = @"D:\DefectImage";
            string sInspectionID = DatabaseManager.Instance.GetInspectionID();    
            SaveDefectImage(Path.Combine(sDefectimagePath, sInspectionID) , MergeDefectList, this.workplace.SharedBufferByteCnt);

            //// Add Defect to DB
            if (MergeDefectList.Count > 0)
            {
                DatabaseManager.Instance.AddDefectDataList(MergeDefectList);

                //if (MergeDefectList.Count == 1)
                //{
                //    foreach (Defect defect in MergeDefectList)
                //    {
                //        DatabaseManager.Instance.AddDefectData(defect);
                //    }
                //}
                //else
                //{
                //    DatabaseManager.Instance.AddDefectDataList(MergeDefectList);
                //}
            }

            WorkEventManager.OnInspectionDone(displayDefect, new InspectionDoneEventArgs(new List<CRect>(), true));
        }

        public override void SetData(IRecipeData _recipeData, IParameterData _parameterData)
        {
            m_sName = this.GetType().Name;
            this.recipeData = _recipeData as RecipeData;
            this.recipeData_Origin = recipeData.GetRecipeData(typeof(RecipeData_Origin)) as RecipeData_Origin;

            //throw new NotImplementedException();
        }

        public override WorkBase Clone()
        {
            return (WorkBase)this.MemberwiseClone();
        }

        public List<Defect> CollectDefectData()
        {
            List<Defect> DefectList = new List<Defect>(); 

            foreach (Workplace workplace in workplaceBundle)
                foreach (Defect defect in workplace.DefectList)
                    DefectList.Add(defect);

            return DefectList;
        }

        // Wafer Backside Inspection시 WaferCenterX,Y를 기준으로 Defect의 중심이 Radius - RadiusOffset보다 먼 거리에 있다면 제거
        private void DeleteOutsideDefect(List<Defect> DefectList, int waferCenterX, int waferCenterY, int Radius, int RadiusOffset)
        {
            List<Defect> DefectList_Delete = new List<Defect>();

            for (int i = 0; i < DefectList.Count; i++) // 현재는 Defect의 중점으로 하고있으나, 잘 제거되지 않으면 Defect Bounding Box의 꼭지점들로 제거
            {
                // 좌상단
                float distX = Math.Abs(waferCenterX - (float)DefectList[i].p_rtDefectBox.Left);
                float distY = Math.Abs(waferCenterY - (float)DefectList[i].p_rtDefectBox.Top);
                double dist = Math.Sqrt(Math.Pow(distX, 2) + Math.Pow(distY, 2));

                if (dist >= (Radius - RadiusOffset))
                    continue;

                // 좌하단
                distX = Math.Abs(waferCenterX - (float)DefectList[i].p_rtDefectBox.Left);
                distY = Math.Abs(waferCenterY - (float)DefectList[i].p_rtDefectBox.Bottom);
                dist = Math.Sqrt(Math.Pow(distX, 2) + Math.Pow(distY, 2));

                if (dist >= (Radius - RadiusOffset))
                    continue;

                // 우상단
                distX = Math.Abs(waferCenterX - (float)DefectList[i].p_rtDefectBox.Right);
                distY = Math.Abs(waferCenterY - (float)DefectList[i].p_rtDefectBox.Top);
                dist = Math.Sqrt(Math.Pow(distX, 2) + Math.Pow(distY, 2));

                if (dist >= (Radius - RadiusOffset))
                    continue;

                // 우하단
                distX = Math.Abs(waferCenterX - (float)DefectList[i].p_rtDefectBox.Right);
                distY = Math.Abs(waferCenterY - (float)DefectList[i].p_rtDefectBox.Bottom);
                dist = Math.Sqrt(Math.Pow(distX, 2) + Math.Pow(distY, 2));

                if (dist >= (Radius - RadiusOffset))
                    continue;

                DefectList_Delete.Add(DefectList[i]);
            }
            DefectList.Clear();
            DefectList.AddRange(DefectList_Delete);
        }

        private List<Defect> MergeDefect(List<Defect> DefectList, int mergeDist)
        {
            string sDefectimagePath = @"D:\DefectImage";
            string sInspectionID = DatabaseManager.Instance.GetInspectionID();
            sDefectimagePath = Path.Combine(sDefectimagePath, sInspectionID);
           
            List<Defect> MergeDefectList = new List<Defect>();
            int nDefectIndex = 1;

            for (int i = 0; i < DefectList.Count; i++)
            {
                if (DefectList[i].m_nDefectCode == -123)
                    continue;

                for (int j = i + 1; j < DefectList.Count; j++)
                {
                    Rect defectRect1 = DefectList[i].p_rtDefectBox;
                    Rect defectRect2 = DefectList[j].p_rtDefectBox;
                    defectRect1.Inflate(new Size(mergeDist, mergeDist)); // Rect 가로 1 세로 1 만큼 확장

                    if (defectRect1.IntersectsWith(defectRect2) && (DefectList[i].m_nDefectCode == DefectList[j].m_nDefectCode))
                    {
                        Rect intersect = Rect.Intersect(defectRect1, defectRect2);
                        if (intersect.Height == 0 || intersect.Width == 0) // Rect가 선만 겹쳐도 Intersect True가 됨! (실제 Dist보다 +1 만큼 더 되어 merge되는 것을 방지)
                            continue;

                        // Merge Defect Info
                        int nDefectCode = DefectList[j].m_nDefectCode;
                        float fDefectSz = DefectList[i].m_fSize + DefectList[j].m_fSize;
                        int fDefectGV = (int)(DefectList[i].m_nGV * (DefectList[i].m_fSize / fDefectSz) + DefectList[j].m_nGV * (DefectList[j].m_fSize / fDefectSz));
                        float fDefectLeft = (defectRect2.Left < defectRect1.Left + mergeDist) ? (float)defectRect2.Left : (float)defectRect1.Left + mergeDist;
                        float fDefectTop = (defectRect2.Top < defectRect1.Top + mergeDist) ? (float)defectRect2.Top : (float)defectRect1.Top + mergeDist;
                        
                        float fDefectRelX = 0;
                        float fDefectRelY = 0;               

                        float fDefectRight = (defectRect2.Right > defectRect1.Right - mergeDist) ? (float)defectRect2.Right : (float)defectRect1.Right - mergeDist;
                        float fDefectBottom = (defectRect2.Bottom > defectRect1.Bottom - mergeDist) ? (float)defectRect2.Bottom : (float)defectRect1.Bottom - mergeDist;
                                   
                        DefectList[i].SetDefectInfo(sInspectionID, nDefectCode, fDefectSz, fDefectGV, fDefectRight - fDefectLeft, fDefectBottom - fDefectTop
                            , fDefectRelX, fDefectRelY, fDefectLeft, fDefectTop, DefectList[j].m_nChipIndexX, DefectList[j].m_nCHipIndexY);

                        DefectList[j].m_nDefectCode = -123; // Merge된 Defect이 중복 저장되지 않도록...
                    }
                }

                DefectList[i].SetDefectIndex(nDefectIndex++);
                MergeDefectList.Add(DefectList[i]);                    
            }

            return MergeDefectList;
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
                
                for(int i = 0; i < DefectList.Count; i++)
                {
                    Cpp_Rect rect = new Cpp_Rect();
                    rect.x = (int)DefectList[i].p_rtDefectBox.Left;
                    rect.y = (int)DefectList[i].p_rtDefectBox.Top;
                    rect.w = (int)DefectList[i].m_fWidth;
                    rect.h = (int)DefectList[i].m_fHeight;

                    defectArray[i] = rect;
                }

                CLR_IP.Cpp_SaveDefectListBMP(
                    Path,
                    (byte*)workplace.SharedBuffer.ToPointer(),
                    workplace.SharedBufferWidth,
                    workplace.SharedBufferHeight,
                    defectArray,
                    nByteCnt
                    );
            }
        }
    }
}
