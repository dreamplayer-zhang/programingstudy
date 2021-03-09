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
using System.Collections;
using System.Drawing.Imaging;
using RootTools_Vision.Utility;

namespace RootTools_Vision
{
    public class ProcessDefect_Wafer : WorkBase
    {
        public ProcessDefect_Wafer()
        {
        }

        public override WORK_TYPE Type => WORK_TYPE.DEFECTPROCESS_ALL;


        protected override bool Preparation()
        {
            return true;
        }

        protected override bool Execution()
        {
            DoProcessDefect_Wafer();

            return true;
        }

        public void DoProcessDefect_Wafer()
        {
            if (!(this.currentWorkplace.MapIndexX == -1 && this.currentWorkplace.MapIndexY == -1))
                return;

            WorkEventManager.OnProcessDefectWaferStart(this, new ProcessDefectWaferStartEventArgs());

            // Option Param
            int mergeDist = 1;


            List<Defect> DefectList = CollectDefectData();


            TempLogger.Write("Defect", string.Format("Total : {0}", DefectList.Count));

            List<Defect> MergeDefectList = Tools.MergeDefect(DefectList, mergeDist);

            TempLogger.Write("Defect", string.Format("Merge : {0}", MergeDefectList.Count));

            foreach (Defect defect in MergeDefectList)
            {
                OriginRecipe originRecipe = this.recipe.GetItem<OriginRecipe>();
                defect.CalcAbsToRelPos(originRecipe.OriginX, originRecipe.OriginY); // Frontside
            }

            //Workplace displayDefect = new Workplace();
            foreach (Defect defect in MergeDefectList)
                this.currentWorkplace.DefectList.Add(defect);

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

            SettingItem_SetupFrontside settings = GlobalObjects.Instance.Get<Settings>().GetItem<SettingItem_SetupFrontside>();
            string sInspectionID = DatabaseManager.Instance.GetInspectionID();

			Tools.SaveDataImage(Path.Combine(settings.DefectImagePath, sInspectionID), MergeDefectList.Cast<Data>().ToList(), currentWorkplace.SharedBufferInfo);

            if (GlobalObjects.Instance.Get<KlarfData_Lot>() != null)
            {
                List<string> dataStringList = DefectDataToStringList(MergeDefectList);
                GlobalObjects.Instance.Get<KlarfData_Lot>().AddSlot(recipe.WaferMap, dataStringList, null);
                GlobalObjects.Instance.Get<KlarfData_Lot>().WaferStart(recipe.WaferMap, DateTime.Now);
                GlobalObjects.Instance.Get<KlarfData_Lot>().SetResultTimeStamp();
                GlobalObjects.Instance.Get<KlarfData_Lot>().SaveKlarf(settings.KlarfSavePath, false);
                Tools.SaveTiffImage(settings.KlarfSavePath, MergeDefectList.Cast<Data>().ToList(), currentWorkplace.SharedBufferInfo);
            }

            // 기존 210302 지울거야
            /*
            SettingItem_SetupFrontside settings = GlobalObjects.Instance.Get<Settings>().GetItem<SettingItem_SetupFrontside>();
            SaveDefectImage(Path.Combine(settings.DefectImagePath, sInspectionID), MergeDefectList, this.currentWorkplace.SharedBufferByteCnt);

            if (settings.UseKlarf)
            {
                Directory.CreateDirectory(settings.KlarfSavePath);

                GlobalObjects.Instance.Get<KlarfData_Lot>().AddSlot(recipe.WaferMap, MergeDefectList, this.recipe.GetItem<OriginRecipe>());
                GlobalObjects.Instance.Get<KlarfData_Lot>().WaferStart(recipe.WaferMap, DateTime.Now);
                GlobalObjects.Instance.Get<KlarfData_Lot>().SetResultTimeStamp();

                GlobalObjects.Instance.Get<KlarfData_Lot>().SaveKlarf(settings.KlarfSavePath, false);

                SaveTiffImage(settings.KlarfSavePath, MergeDefectList, 3);
            }
            */
            
            WorkEventManager.OnInspectionDone(this.currentWorkplace, new InspectionDoneEventArgs(new List<CRect>(), true));
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
                // 좌상단^
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

        private List<string> DefectDataToStringList(List<Defect> defectList)
        {
            List<string> stringList = new List<string>();
            foreach (Defect defect in defectList)
            {
                //string str = string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} {16}");
                //stringList.Add(str);
            }
            return stringList;
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
                    else if(defectRect2.Contains(defectRect1))
                    {
                        DefectList[i].SetDefectInfo(sInspectionID, DefectList[j].m_nDefectCode, DefectList[j].m_fSize, DefectList[j].m_fGV, DefectList[j].m_fWidth, DefectList[j].m_fHeight
                            , 0, 0, (float)DefectList[j].p_rtDefectBox.Left, (float)DefectList[j].p_rtDefectBox.Top, DefectList[j].m_nChipIndexX, DefectList[j].m_nCHipIndexY);
                        DefectList[j].m_fSize = -123;
                        continue;
                    }
                    else if(defectRect1.IntersectsWith(defectRect2))
                    {
                        Rect intersect = Rect.Intersect(defectRect1, defectRect2);
                        if (intersect.Height == 0 || intersect.Width == 0)
                        {
                            DefectList[j].m_fSize = -123;
                            continue;
                        }
                    }

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
                            , fDefectRelX, fDefectRelY, fDefectLeft, fDefectTop, DefectList[j].m_nChipIndexX, DefectList[j].m_nCHipIndexY);

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
        
        // 지울거야
        /*
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

                if (nByteCnt == 1) { 
                    CLR_IP.Cpp_SaveDefectListBMP(
                       Path,
                       (byte*)currentWorkplace.SharedBufferR_GRAY.ToPointer(),
                       currentWorkplace.SharedBufferWidth,
                       currentWorkplace.SharedBufferHeight,
                       defectArray
                       );
                }
                
                else if (nByteCnt == 3) { 
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

        ArrayList inputImage = new ArrayList();
        private void SaveTiffImage(string Path, List<Defect> DefectList, int nByteCnt)
        {
            Path += "\\";
            DirectoryInfo di = new DirectoryInfo(Path);
            if (!di.Exists)
                di.Create();

            for (int i = 0; i < DefectList.Count; i++)
            {
                MemoryStream image = new MemoryStream();
                System.Drawing.Bitmap bitmap = Tools.ConvertArrayToColorBitmap(currentWorkplace.SharedBufferR_GRAY, currentWorkplace.SharedBufferG, currentWorkplace.SharedBufferB,currentWorkplace.SharedBufferWidth, 3, DefectList[i].GetRect());
                //System.Drawing.Bitmap NewImg = new System.Drawing.Bitmap(bitmap);
                bitmap.Save(image, ImageFormat.Tiff);
                inputImage.Add(image);
            }

            ImageCodecInfo info = null;
            foreach (ImageCodecInfo ice in ImageCodecInfo.GetImageEncoders())
            {
                if (ice.MimeType == "image/tiff")
                {
                    info = ice;
                    break;
                }
            }

            string test = "test";
            Path += test +".tiff";

            EncoderParameters ep = new EncoderParameters(2);

            bool firstPage = true;

            System.Drawing.Image img = null;

            for(int i = 0; i < inputImage.Count; i++)
            {
                System.Drawing.Image img_src = System.Drawing.Image.FromStream((Stream)inputImage[i]);
                Guid guid = img_src.FrameDimensionsList[0];
                System.Drawing.Imaging.FrameDimension dimension = new System.Drawing.Imaging.FrameDimension(guid);

                for (int nLoopFrame = 0; nLoopFrame < img_src.GetFrameCount(dimension); nLoopFrame++)
                {
                    img_src.SelectActiveFrame(dimension, nLoopFrame);

                    ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, Convert.ToInt32(EncoderValue.CompressionLZW));

                    if (firstPage)
                    {
                        img = img_src;

                        ep.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, Convert.ToInt32(EncoderValue.MultiFrame));
                        img.Save(Path, info, ep);

                        firstPage = false;
                        continue;
                    }

                    ep.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, Convert.ToInt32(EncoderValue.FrameDimensionPage));
                    img.SaveAdd(img_src, ep);
                }
            }
            if(inputImage.Count == 0)
            {
                File.Create(Path);
                return;
            }

            ep.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, Convert.ToInt32(EncoderValue.Flush));
            img.SaveAdd(ep);
        }
        */
    }
}
