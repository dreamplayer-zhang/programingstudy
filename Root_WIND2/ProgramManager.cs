﻿using Microsoft.Win32;
using RootTools;
using RootTools.Memory;
using RootTools_Vision;
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
        private string memoryPool = "pool";
        private string memoryGroup = "group";
        private string memoryNameImage = "mem";
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

        InspectionManager_Vision inspectionVision;
        InspectionManager_EFEM inspectionEFEM;
        #endregion

        #region [Getter Setter]
        public WIND2_Engineer Engineer { get => engineer; private set => engineer = value; }
        public MemoryTool MemoryTool { get => memoryTool; private set => memoryTool = value; }
        public Recipe Recipe { get => recipe; private set => recipe = value; }
        public ImageData Image { get => image; private set => image = value; }
        public ImageData ROILayer { get => roiLayer; private set => roiLayer = value; }
        public InspectionManager_Vision InspectionVision { get => inspectionVision; private set => inspectionVision = value; }
        public InspectionManager_EFEM InspectionEFEM { get => inspectionEFEM; private set => inspectionEFEM = value; }
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
            roiLayer = new ImageData(memoryTool.GetMemory(memoryPool, memoryGroup, memoryNameROI));

            ImageEdge = engineer.m_handler.m_edgesideVision.GetMemoryData(Module.EdgeSideVision.EDGE_TYPE.EdgeTop);

            return true;
        }

        public ImageData GetEdgeMemory(Module.EdgeSideVision.EDGE_TYPE type)
        {
            return engineer.m_handler.m_edgesideVision.GetMemoryData(type);
        }

        private bool InitMember()
        {
            recipe = new Recipe();

            if (!Directory.Exists(recipeFolderPath))
                Directory.CreateDirectory(recipeFolderPath);

            // Vision
            this.InspectionVision = new InspectionManager_Vision(image.GetPtr(), image.p_Size.X, image.p_Size.Y);
            this.InspectionVision.Recipe = this.recipe;

            // EFEM
            this.InspectionEFEM = new InspectionManager_EFEM(ImageEdge.GetPtr(), ImageEdge.p_Size.X, ImageEdge.p_Size.Y, 3);
            this.InspectionEFEM.Recipe = this.recipe;


            this.engineer.InspectionEFEM = this.InspectionEFEM;
            this.engineer.InspectionVision = this.InspectionVision;

            return true;
        }


        #region [Recipe Method]
        public void ShowDialogSaveRecipe()
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.InitialDirectory = recipeFolderPath;

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

            recipe.Save(recipePath);
            
            //this.Load(sFilePath); //?
            WorkEventManager.OnUIRedraw(this, new UIRedrawEventArgs());
        }

        public void ShowDialogLoadRecipe()
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
                if (dlg.ShowDialog() == true)
                {
                    this.LoadRecipe(dlg.FileName);
                    WorkEventManager.OnUIRedraw(this, new UIRedrawEventArgs());
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