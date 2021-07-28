using RootTools;
using RootTools.Database;
using RootTools.Module;
using RootTools.Trees;
using RootTools_Vision;
using RootTools_Vision.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Root_WIND2.Module
{
    public class Run_InspectBackside : ModuleRunBase
    {
        BackSideVision m_module;

        string m_sRecipeName = string.Empty;

        // Grab 관련 파라매터 (이거 나중에 구조 변경 필요할듯)
        //bool m_bInvDir = false;
        public GrabModeBack m_grabMode = null;
        string m_sGrabMode = "";
        #region [Klarf]
        private static KlarfData_Lot klarfData = new KlarfData_Lot();


        private static void LotStart(string klarfPath, RecipeBase recipe, InfoWafer infoWafer, GrabModeBase grabMode)
        {
            if (klarfData == null) klarfData = new KlarfData_Lot();

            klarfData.SetModuleName("Backside");

            if (Directory.Exists(klarfPath)) Directory.CreateDirectory(klarfPath);


            klarfData.LotStart(klarfPath, infoWafer, recipe.WaferMap, grabMode);
        }

        private void CreateKlarf(RecipeBase recipe, InfoWafer infoWafer, List<Defect> defectList, bool useTDIReview = false, bool useVrsReview = false)
        {
            //klarfData.SetResolution((float)camInfo.RealResX, (float)camInfo.RealResY);
            // Product 정보 셋팅

            klarfData.WaferStart(recipe.WaferMap, infoWafer);
            klarfData.AddSlot(recipe.WaferMap, defectList, recipe.GetItem<OriginRecipe>(), useTDIReview, useVrsReview);
            klarfData.SaveKlarf();

        }

        private void LotEnd(InfoWafer infoWafer)
        {
            klarfData.CreateLotEnd();
        }
        #endregion


        #region [Getter Setter]
        public string RecipeName
        {
            get => m_sRecipeName;
            set => m_sRecipeName = value;
        }

        public string p_sGrabMode
        {
            get { return m_sGrabMode; }
            set
            {
                m_sGrabMode = value;
                m_grabMode = m_module.GetGrabMode(value);
            }
        }
        #endregion

        public Run_InspectBackside(BackSideVision module)
        {
            m_module = module;
            InitModuleRun(module);
        }

        public override ModuleRunBase Clone()
        {
            Run_InspectBackside run = new Run_InspectBackside(m_module);
            run.p_sGrabMode = p_sGrabMode;
            run.RecipeName = this.RecipeName;
            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            m_sRecipeName = tree.SetFile(m_sRecipeName, m_sRecipeName, "rcp", "Recipe", "Recipe Name", bVisible);
            // 이거 다 셋팅 되어 있는거 가져와야함
            p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);
        }

        public override string Run()
        {

            Settings settings = new Settings();
            SettingItem_SetupBackside settings_backside = settings.GetItem<SettingItem_SetupBackside>();

            InfoWafer infoWafer = m_module.GetInfoWafer(0);
            RecipeBack recipe = GlobalObjects.Instance.Get<RecipeBack>();

            GrabModeBack m_grabMode = m_module.GetGrabMode(recipe.CameraInfoIndex);

            // Check Lot Start
            if ((infoWafer != null) && (
                (infoWafer.p_eWaferOrder == InfoWafer.eWaferOrder.FirstLastWafer) ||
                (infoWafer.p_eWaferOrder == InfoWafer.eWaferOrder.FirstWafer)))
                LotStart(settings_backside.KlarfSavePath, recipe, infoWafer, m_grabMode);

            

            StopWatch inspectionTimeWatcher = new StopWatch();
            inspectionTimeWatcher.Start();

            //레시피에 GrabMode 저장하고 있어야함

            RootTools_Vision.WorkManager3.WorkManager workManager = GlobalObjects.Instance.GetNamed<RootTools_Vision.WorkManager3.WorkManager>("backInspection");
            if (workManager == null)
            {
                throw new ArgumentException("WorkManager가 초기화되지 않았습니다(null)");
            }

            workManager.Stop();


            if (m_grabMode == null) return "Grab Mode == null";

            if (EQ.IsStop() == false)
            {
                if (workManager.OpenRecipe(m_sRecipeName) == false)
                    return "Recipe Open Fail";

                workManager.Start();

            }
            else
            {
                workManager.Stop();
            }

            if (workManager.WaitWorkDone(ref EQ.m_EQ.StopToken(), 60 * 3 /* 3 minutes */) == false)
            {
                inspectionTimeWatcher.Stop();

                RootTools_Vision.TempLogger.Write("Inspection", "Time out!!!");
                return "OK";
            } // 5 minutes
            else
            {
                #region [Klarf]
                if (settings_backside.UseKlarf)
                {
                    DataTable table = DatabaseManager.Instance.SelectCurrentInspectionDefect();
                    List<Defect> defects = Tools.DataTableToDefectList(table);

                    WIND2_Engineer engineer = GlobalObjects.Instance.Get<WIND2_Engineer>();
                    CreateKlarf(recipe, infoWafer, defects, settings_backside.UseTDIReview, false);

                    if (settings_backside.UseTDIReview)
                    {
                        klarfData.SaveTiffImageOnlyTDI(defects, workManager.SharedBuffer, new Size(160, 120));
                    }

                    if(recipe.ExclusiveRegionFilePath == "")
                    {
                        recipe.ExclusiveRegionFilePath = Constants.FilePath.BacksideExclusiveRegionFilePath;
                        recipe.Save();
                    }

                    List<List<Point>> polygon = PolygonController.ReadPolygonFile(recipe.ExclusiveRegionFilePath);

                    BacksideRecipe backRecipe = recipe.GetItem<BacksideRecipe>();

                    klarfData.SaveImageJpgInterpolation(workManager.SharedBuffer,
                       new Rect(
                           settings_backside.WholeWaferImageStartX,
                           settings_backside.WholeWaferImageStartY,
                           settings_backside.WholeWaferImageEndX,
                           settings_backside.WholeWaferImageEndY),
                       (long)(settings_backside.WholeWaferImageCompressionRate * 100),
                       settings_backside.OutputImageSizeX,
                       settings_backside.OutputImageSizeY, polygon, (int)(settings_backside.SaveWaferSize * 1000 / m_grabMode.m_dRealResX_um), (int)(settings_backside.SaveWaferSize * 1000 / m_grabMode.m_dRealResY_um),
                       backRecipe.CenterX,
                       backRecipe.CenterY);

                    klarfData.SaveImageJpgMerge(settings_backside.SaveWaferSize);
                }

                #endregion
            }

            if ((infoWafer != null) && (
                (infoWafer.p_eWaferOrder == InfoWafer.eWaferOrder.FirstLastWafer) ||
                (infoWafer.p_eWaferOrder == InfoWafer.eWaferOrder.LastWafer)))
                LotEnd(infoWafer);

           
            inspectionTimeWatcher.Stop();
            RootTools_Vision.TempLogger.Write("Inspection", string.Format("{0:F3}", (double)inspectionTimeWatcher.ElapsedMilliseconds / (double)1000));

            return "OK";
        }
    }
}
