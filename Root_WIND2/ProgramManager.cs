using Microsoft.Win32;
using RootTools;
using RootTools.Memory;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace Root_WIND2
{
    public class ProgramManager
    {
        //Single ton
        private ProgramManager() 
        {

        }

        private static readonly Lazy<ProgramManager> instance = new Lazy<ProgramManager>(() => new ProgramManager());

        public static ProgramManager Instance 
        { 
            get 
            { 
                return instance.Value;
            } 
        }

        #region [Setting Parameters]

        private bool IsInitilized = false;
        //Memory 
        //private string memoryPool = "pool";
        //private string memoryGroup = "group";
        //private string memoryNameImage = "mem";
        //private string memoryNameROI = "ROI";

        private string memoryPool = "Vision.Memory";
        private string memoryGroup = "Vision";
        private string memoryNameImage = "Main";
        private string memoryNameROI = "ROI";

        //Recipe
        private string recipeFolderPath = @"C:\Root\Recipe";
        #endregion

        #region [Members]
        private IDialogService dialogService;

        private WIND2_Engineer engineer = new WIND2_Engineer();
        private MemoryTool memoryTool;
        private Recipe recipe;

        private ImageData image;
        private ImageData roiLayer;

        private ImageData imageEdge;

        InspectionManagerFrontside inspectionFront;
        InspectionManagerBackside inspectionBack;
        InspectionManagerEdge inspectionEdge;
        #endregion

        #region [Getter Setter]
        public WIND2_Engineer Engineer { get => engineer; private set => engineer = value; }
        public MemoryTool MemoryTool { get => memoryTool; private set => memoryTool = value; }
        public Recipe Recipe { get => recipe; private set => recipe = value; }
        public ImageData Image { get => image; private set => image = value; }
        public ImageData ROILayer { get => roiLayer; private set => roiLayer = value; }
        public InspectionManagerFrontside InspectionFront { get => inspectionFront; private set => inspectionFront = value; }
        public InspectionManagerBackside InspectionBack { get => inspectionBack; private set => inspectionBack = value; }
        public InspectionManagerEdge InspectionEdge { get => inspectionEdge; private set => inspectionEdge = value; }
        
        public IDialogService DialogService { get => dialogService; set => dialogService = value; }
        public ImageData ImageEdge { get => imageEdge; private set => imageEdge = value; }
        #endregion

        public bool Initialize()
        {
            bool result = true;

            try
            {
                if(IsInitilized == false)
                {
                    InitEngineer("WIND2");
                    InitMemory();
                    InitMember();
                    IsInitilized = true;
                }
            }
            catch(Exception ex)
            {
                result = false;
            }

            return result;
        }

        private bool InitEngineer(string name)
        {
            this.engineer.Init(name);

            return true;
        }

        private bool InitMemory()
        {
            memoryTool = engineer.ClassMemoryTool();

            image = new ImageData(memoryTool.GetMemory(memoryPool, memoryGroup, memoryNameImage));
            image.p_nByte = memoryTool.GetMemory(memoryPool, memoryGroup, memoryNameImage).p_nCount;

            roiLayer = new ImageData(memoryTool.GetMemory("pool", "group", memoryNameROI));
            //roiLayer.p_nByte = memoryTool.GetMemory(memoryPool, memoryGroup, memoryNameROI).p_nCount;
            if (engineer.m_eMode == WIND2_Engineer.eMode.EFEM)
            {
                imageEdge = engineer.m_handler.p_EdgeSideVision.GetMemoryData(Module.EdgeSideVision.EDGE_TYPE.EdgeTop);
                imageEdge.p_nByte = memoryTool.GetMemory("EdgeSide Vision.Memory", "EdgeSide Vision", "EdgeTop").p_nCount;                    
			}

            return true;
        }

        public ImageData GetEdgeMemory(Module.EdgeSideVision.EDGE_TYPE type)
        {
            return engineer.m_handler.p_EdgeSideVision.GetMemoryData(type);
        }

        private bool InitMember()
        {
            recipe = new Recipe();

            if (!Directory.Exists(recipeFolderPath))
                Directory.CreateDirectory(recipeFolderPath);

            // Front
            this.InspectionFront = new InspectionManagerFrontside(image.GetPtr(), image.p_Size.X, image.p_Size.Y);
            this.InspectionFront.SetColorSharedBuffer(image.GetPtr(0), image.GetPtr(1), image.GetPtr(2));


            this.Engineer.InspectionFront = this.InspectionFront;
            this.Engineer.InspectionFront.Recipe = this.recipe;

            // Back
            this.InspectionBack = new InspectionManagerBackside(image.GetPtr(), image.p_Size.X, image.p_Size.Y);

            this.Engineer.InspectionBack = this.InspectionBack;
            this.Engineer.InspectionBack.Recipe = this.recipe;

            // Edge
            if (engineer.m_eMode == WIND2_Engineer.eMode.EFEM)
            {
                if (imageEdge.p_nByte == 1)
                    this.InspectionEdge = new InspectionManagerEdge(imageEdge.GetPtr(), imageEdge.p_Size.X, imageEdge.p_Size.Y, 3);

                if (imageEdge.p_nByte == 3)
                {
                    this.InspectionEdge = new InspectionManagerEdge(imageEdge.GetPtr(), imageEdge.p_Size.X, imageEdge.p_Size.Y, 3);
                    this.InspectionEdge.SetWorkplaceBuffer(imageEdge.GetPtr(0), imageEdge.GetPtr(1), imageEdge.GetPtr(2));
                }

                this.Engineer.InspectionEdge = this.InspectionEdge;
                this.Engineer.InspectionEdge.Recipe = this.recipe;
            }
            return true;
        }

        #region [Recipe Method]
        public void NewRecipe()
        {
            try
            {
                if (MessageBox.Show("작성 중인 레시피(Recipe)를 저장하시겠습니까?", "YesOrNo", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if(this.Recipe.RecipePath == "")
                    {
                        ShowDialogSaveRecipe();
                    }
                    else
                    {
                        this.Recipe.Save(this.Recipe.RecipePath);
                    }
                }
                
                this.Recipe.Clear();
                ShowDialogSaveRecipe(true);

                WorkEventManager.OnUIRedraw(this, new UIRedrawEventArgs());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save Recipe : " + ex.Message);
            }
        }

        public void ShowDialogSaveRecipe(bool bNew = false)
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.InitialDirectory = recipeFolderPath;

                if (bNew == true) dlg.Title = "새로 만들기";

                dlg.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
                if (dlg.ShowDialog() == true)
                {
                    string sFilePath = Path.GetDirectoryName(dlg.FileName); // 디렉토리명
                    string sFileNameNoExt = Path.GetFileNameWithoutExtension(dlg.FileName); // Only 파일이름
                    string sFileName = Path.GetFileName(dlg.FileName); // 파일이름 + 확장자
                    string sFolderPath = Path.Combine(sFilePath, sFileNameNoExt); // 레시피 이름으로 된 폴더
                    string sResultFileName = Path.Combine(sFolderPath, sFileName); // 레시피 이름으 된 폴더안의 rcp 파일 경로

                    DirectoryInfo dir = new DirectoryInfo(sFolderPath);
                    if (!dir.Exists)
                        dir.Create();

                    this.SaveRecipe(sResultFileName);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Save Recipe : " + ex.Message);
            }
        }

        public void SaveRecipe(string recipePath)
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            using (StreamWriter writer = new StreamWriter(recipePath, true))
            {
                writer.WriteLine(time + " - SaveRecipe()");
            }


            WIND2EventManager.OnBeforeRecipeSave(recipe, new RecipeEventArgs());

            if (recipePath.IndexOf(".rcp") == -1) recipePath += ".rcp";
            recipe.Save(recipePath);

            WIND2EventManager.OnAfterRecipeSave(recipe, new RecipeEventArgs());
            //this.Load(sFilePath); //?
            //WorkEventManager.OnUIRedraw(this, new UIRedrawEventArgs());
        }

        public void ShowDialogLoadRecipe()
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
                if (dlg.ShowDialog() == true)
                {

                    // 레시피 전/후처리 이벤트 Recipe 클래스 안쪽에 넣어야할 수 도?
                    WIND2EventManager.OnBeforeRecipeRead(recipe, new RecipeEventArgs());

                    this.LoadRecipe(dlg.FileName);
                    WorkEventManager.OnUIRedraw(this, new UIRedrawEventArgs());

                    WIND2EventManager.OnAfterRecipeRead(recipe, new RecipeEventArgs());

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Load Recipe : " + ex.Message);
            }
        }
        public void LoadRecipe(string recipePath)
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            using (StreamWriter writer = new StreamWriter(recipePath, true))
            {
                writer.WriteLine(time + " - LoadRecipe()");
            }

            this.recipe.Read(recipePath);
        }

        #endregion
    }
}
