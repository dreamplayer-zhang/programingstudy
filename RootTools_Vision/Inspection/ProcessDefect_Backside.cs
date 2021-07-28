using RootTools;
using RootTools.Database;
using RootTools_CLR;
using RootTools_Vision.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools_Vision
{
    public class ProcessDefect_Backside : WorkBase
    {
        BacksideRecipe recipeBackside;
        string sDefectimagePath = @"D:\DefectImage";
        /// <summary>
        /// Defect Image가 저장될 Root Directory Path. 기본값 : D:\DefectImage
        /// </summary>
        public string DefectImagePath { get => sDefectimagePath; set => sDefectimagePath = value; }
        
        string TableName;

        public ProcessDefect_Backside()
        {

        }


        public ProcessDefect_Backside(string tableName)
        {
            TableName = tableName;
        }

        public override WORK_TYPE Type => WORK_TYPE.DEFECTPROCESS_ALL;


        protected override bool Preparation()
        {
            return true;
        }

        protected override bool Execution()
        {
            ProcessDefectBacksideParameter param = this.parameter as ProcessDefectBacksideParameter;
            if(param.Use == true)
                DoProcessDefect_Backside();
            else
                return true;

            return true;
        }

        public void DoProcessDefect_Backside()
        {
            if (!(this.currentWorkplace.MapIndexX == -1 && this.currentWorkplace.MapIndexY == -1))
                return;


            ProcessDefectBacksideParameter param = this.parameter as ProcessDefectBacksideParameter;

            WorkEventManager.OnProcessDefectWaferStart(this, new ProcessDefectWaferStartEventArgs());

            // Option Param
            int mergeDist = param.MergeDefectDistnace;
            int backsideOffset = 0;
            // Recipe

            // Backside
            int waferCenterX = 0;
            int waferCenterY = 0;
            int radius = 0;


            this.recipeBackside = this.recipe.GetItem<BacksideRecipe>();
            waferCenterX = recipeBackside.CenterX;
            waferCenterY = recipeBackside.CenterY;
            radius = recipeBackside.Radius;


            List<Defect> DefectList = CollectDefectData();

            Settings settings = new Settings();
            SettingItem_SetupBackside settings_backside = settings.GetItem<SettingItem_SetupBackside>();


            List<Defect> mergeDefectList;

            if (param.UseMergeDefect == true)
            {
                mergeDefectList = MergeDefect(DefectList, mergeDist);
            }
            else
            {
                mergeDefectList = DefectList;
            }

            int index = 0;
            foreach (Defect defect in mergeDefectList)
            {
                defect.m_nDefectIndex = index;
                defect.CalcAbsToRelPos(waferCenterX, waferCenterY);
                index++;
            }

            //Workplace displayDefect = new Workplace();
            foreach (Defect defect in mergeDefectList)
            {
                if (this.currentWorkplace.DefectList == null) continue;
                this.currentWorkplace.DefectList.Add(defect);
            }

            string sInspectionID = DatabaseManager.Instance.GetInspectionID();
            //Tools.SaveDefectImage(Path.Combine(DefectImagePath, sInspectionID), MergeDefectList, this.currentWorkplace.SharedBufferInfo, this.currentWorkplace.SharedBufferInfo.ByteCnt);
            

            //// Add Defect to DB
            if (mergeDefectList.Count > 0)
            {
                DatabaseManager.Instance.AddDefectDataListNoAutoCount(mergeDefectList, "defect");
            }

            //Tools.SaveDefectImage(Path.Combine(settings_backside.DefectImagePath, sInspectionID), mergeDefectList, this.currentWorkplace.SharedBufferInfo, this.currentWorkplace.SharedBufferByteCnt);
            Tools.SaveDefectImageParallel(Path.Combine(settings_backside.DefectImagePath, sInspectionID), mergeDefectList, this.currentWorkplace.SharedBufferInfo, this.currentWorkplace.SharedBufferByteCnt);


            WorkEventManager.OnInspectionDone(this.currentWorkplace, new InspectionDoneEventArgs(new List<CRect>()));
            WorkEventManager.OnIntegratedProcessDefectDone(this.currentWorkplace, new IntegratedProcessDefectDoneEventArgs());
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
            string sInspectionID = DatabaseManager.Instance.GetInspectionID();
            List<Defect> MergeDefectList = new List<Defect>();
            int nDefectIndex = 1;

            for (int i = 0; i < DefectList.Count; i++)
            {
                if (DefectList[i].m_fSize == -123)
                    continue;

                for (int j = 0; j < DefectList.Count; j++)
                {
                    Rect defectRect1 = DefectList[i].p_rtDefectBox;
                    Rect defectRect2 = DefectList[j].p_rtDefectBox;

                    if (DefectList[j].m_fSize == -123 || (i == j))
                        continue;

                    else if (defectRect1.Contains(defectRect2))
                    {
                        DefectList[j].m_fSize = -123;
                        continue;
                    }
                    else if (defectRect2.Contains(defectRect1))
                    {
                        DefectList[i].SetDefectInfo(sInspectionID, DefectList[j].m_nDefectCode, DefectList[j].m_fSize, DefectList[j].m_fGV, DefectList[j].m_fWidth, DefectList[j].m_fHeight
                            , 0, 0, (float)DefectList[j].p_rtDefectBox.Left, (float)DefectList[j].p_rtDefectBox.Top, DefectList[j].m_nChipIndexX, DefectList[j].m_nChipIndexY);
                        DefectList[j].m_fSize = -123;
                        continue;
                    }
                    // 이거 버그 있어서 주석 밑에 조건이랑 같음
                    //else if (defectRect1.IntersectsWith(defectRect2))
                    //{
                    //    Rect intersect = Rect.Intersect(defectRect1, defectRect2);
                    //    if (intersect.Height == 0 || intersect.Width == 0)
                    //    {
                    //        DefectList[j].m_fSize = -123;
                    //        continue;
                    //    }
                    //}

                    defectRect1.Inflate(new Size(mergeDist, mergeDist)); // Rect 가로/세로 mergeDist 만큼 확장
                    if (defectRect1.IntersectsWith(defectRect2) && (DefectList[i].m_nDefectCode == DefectList[j].m_nDefectCode))
                    {
                        Rect intersect = Rect.Intersect(defectRect1, defectRect2);
                        if (intersect.Height == 0 || intersect.Width == 0) // Rect가 선만 겹쳐도 Intersect True가 됨! (실제 Dist보다 +1 만큼 더 되어 merge되는 것을 방지)
                            continue;

                        // Merge Defect Info
                        int nDefectCode = DefectList[j].m_nDefectCode;

                        float fDefectGV = (float)((DefectList[i].m_fGV + DefectList[j].m_fGV) / 2.0);
                        float fDefectLeft = (defectRect2.Left < defectRect1.Left + mergeDist) ? (float)defectRect2.Left : (float)defectRect1.Left + mergeDist;
                        float fDefectTop = (defectRect2.Top < defectRect1.Top + mergeDist) ? (float)defectRect2.Top : (float)defectRect1.Top + mergeDist;
                        float fDefectRight = (defectRect2.Right > defectRect1.Right - mergeDist) ? (float)defectRect2.Right : (float)defectRect1.Right - mergeDist;
                        float fDefectBottom = (defectRect2.Bottom > defectRect1.Bottom - mergeDist) ? (float)defectRect2.Bottom : (float)defectRect1.Bottom - mergeDist;

                        float fDefectWidth = fDefectRight - fDefectLeft;
                        float fDefectHeight = fDefectBottom - fDefectTop;

                        float fDefectSz = (fDefectWidth > fDefectHeight) ? fDefectWidth : fDefectHeight;

                        float fDefectRelX = 0;
                        float fDefectRelY = 0;

                        DefectList[i].SetDefectInfo(sInspectionID, nDefectCode, fDefectSz, fDefectGV, fDefectWidth, fDefectHeight
                            , fDefectRelX, fDefectRelY, fDefectLeft, fDefectTop, DefectList[j].m_nChipIndexX, DefectList[j].m_nChipIndexY);

                        DefectList[j].m_fSize = -123; // Merge된 Defect이 중복 저장되지 않도록...
                    }
                }
            }

            for (int i = 0; i < DefectList.Count; i++)
            {
                if (DefectList[i].m_fSize != -123)
                {
                    MergeDefectList.Add(DefectList[i]);
                    MergeDefectList[nDefectIndex - 1].SetDefectIndex(nDefectIndex++);
                }
            }

            return MergeDefectList;
        }

    }
}
