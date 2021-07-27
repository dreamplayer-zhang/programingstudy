using RootTools;
using RootTools.Database;
using RootTools_Vision.DataTypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RootTools_Vision
{

    public class DefectData
    {
        Ellipse uiElement;
        Defect defectData;
        
        public bool IsSelected
        {
            get;
            set;
        }

        public Ellipse UIElement
        {
            get => this.uiElement;
            set => this.uiElement = value;
        }

        public Defect Defect
        {
            get => this.defectData;
            set => this.defectData = value;
        }

        public DefectData(Defect data, Ellipse uiElement)
        {
            this.defectData = data;
            this.uiElement = uiElement;
        }
    }


    public class DefectViewer_ViewModel : ObservableObject
    {

        int[] defectCountMap = null;

        private RecipeBase recipeFront = new RecipeBase();
        private RecipeBase recipeBack = new RecipeBase();
        private RecipeBase recipeEdge = new RecipeBase();
        private RecipeBase recipeEBR = new RecipeBase();

        private List<Defect> defectList;

        private List<DefectData> frontDefectList;
        private List<DefectData> backDefectList;
        private List<DefectData> edgeDefectList;
        private List<DefectData> ebrDefectList;

        public DefectViewer_ViewModel()
        {
            frontDefectList = new List<DefectData>();
            backDefectList = new List<DefectData>();
            edgeDefectList = new List<DefectData>();
            ebrDefectList = new List<DefectData>();

            this.frontItems = new ObservableCollection<Ellipse>();
            this.backItems = new ObservableCollection<Ellipse>();
            this.edgeItems = new ObservableCollection<Ellipse>();
            this.ebrItems = new ObservableCollection<Ellipse>();

            InitBaseElements();
        }


        #region [Properties]

        public int[] DefectCountMap
        {
            get => this.defectCountMap;
        }

        private bool isCheckedDisplayWafer = true;
        public bool IsCheckedDisplayWafer
        {
            get => this.isCheckedDisplayWafer;
            set
            {
                SetProperty(ref this.isCheckedDisplayWafer, value);

                if(this.isCheckedDisplayWafer == true)
                {
                    if(!this.BaseElements.Contains(this.baseWafer))
                    {
                        this.BaseElements.Add(this.baseWafer);
                    }
                    RedrawBaseWafer();
                }
                else
                {
                    if (this.BaseElements.Contains(this.baseWafer))
                    {
                        this.BaseElements.Remove(this.baseWafer);
                    }
                }
            }
        }

        private bool isCheckedDisplayChip = true;
        public bool IsCheckedDisplayChip
        {
            get => this.isCheckedDisplayChip;
            set
            {
                SetProperty(ref this.isCheckedDisplayChip, value);

                RedrawMap();
            }
        }

        private bool isCheckedDisplayTrendMap = false;
        public bool IsCheckedDisplayTrendMap
        {
            get => this.isCheckedDisplayTrendMap;
            set
            {
                SetProperty(ref this.isCheckedDisplayTrendMap, value);
                
                RedrawTrendMap();
            }
        }

        private int frontDefectCount = 0;
        public int FrontDefectCount
        {
            get => this.frontDefectCount;
            set
            {
                SetProperty(ref this.frontDefectCount, value);
            }
        }

        private int backDefectCount = 0;
        public int BackDefectCount
        {
            get => this.backDefectCount;
            set
            {
                SetProperty(ref this.backDefectCount, value);
            }
        }

        private int edgeDefectCount = 0;
        public int EdgeDefectCount
        {
            get => this.edgeDefectCount;
            set
            {
                SetProperty(ref this.edgeDefectCount, value);
            }
        }

        private int ebrDefectCount = 0;
        public int EBRDefectCount
        {
            get => this.ebrDefectCount;
            set
            {
                SetProperty(ref this.ebrDefectCount, value);
            }
        }

        private string recipeID = "";
        public string RecipeID
        {
            get => this.recipeID;
            set
            {
                SetProperty(ref this.recipeID, value);
            }
        }

        private string waferID = "";
        public string WaferID
        {
            get => this.waferID;
            set
            {
                SetProperty(ref this.waferID, value);
            }
        }

        private string mapSizeX = "";
        public string MapSizeX
        {
            get => this.mapSizeX;
            set
            {
                SetProperty(ref this.mapSizeX, value);
            }
        }

        private string mapSizeY = "";
        public string MapSizeY
        {
            get => this.mapSizeY;
            set
            {
                SetProperty(ref this.mapSizeY, value);
            }
        }

        private string grossDie = "";
        public string GrossDie
        {
            get => this.grossDie;
            set
            {
                SetProperty(ref this.grossDie, value);
            }
        }

        private string inspectionID = "";
        public string InspectionID
        {
            get => this.inspectionID;
            set
            {
                SetProperty(ref this.inspectionID, value);
            }
        }

        private bool isCheckedFront = false;
        public bool IsCheckedFront
        {
            get => this.isCheckedFront;
            set
            {
                SetProperty(ref this.isCheckedFront, value);
            }
        }

        private bool isCheckedBack = false;
        public bool IsCheckedBack
        {
            get => this.isCheckedBack;
            set
            {
                SetProperty(ref this.isCheckedBack, value);

                RedrawDefectListBack();
            }
        }
        private bool isCheckedEdge = false;
        public bool IsCheckedEdge
        {
            get => this.isCheckedEdge;
            set
            {
                SetProperty(ref this.isCheckedEdge, value);
            }
        }
        private bool isCheckedEBR = false;
        public bool IsCheckedEBR
        {
            get => this.isCheckedEBR;
            set
            {
                SetProperty(ref this.isCheckedEBR, value);
            }
        }

        private ObservableCollection<UIElement> baseElements;
        public ObservableCollection<UIElement> BaseElements
        {
            get => this.baseElements;
            set
            {
                SetProperty<ObservableCollection<UIElement>>(ref this.baseElements, value);
            }
        }

        private ObservableCollection<Rectangle> baseDieList;
        public ObservableCollection<Rectangle> BaseDieList
        {
            get => this.baseDieList;
            set
            {
                SetProperty<ObservableCollection<Rectangle>>(ref this.baseDieList, value);
            }
        }

        private ObservableCollection<Rectangle> trendMapDieList;
        public ObservableCollection<Rectangle> TrendMapDieList
        {
            get => this.trendMapDieList;
            set
            {
                SetProperty<ObservableCollection<Rectangle>>(ref this.trendMapDieList, value);
            }
        }


        private ObservableCollection<Ellipse> frontItems;
        public ObservableCollection<Ellipse> FrontItems
        {
            get => this.frontItems;
            set
            {
                SetProperty<ObservableCollection<Ellipse>>(ref this.frontItems, value);
            }
        }

        private ObservableCollection<Ellipse> backItems;
        public ObservableCollection<Ellipse> BackItems
        {
            get => this.backItems;
            set
            {
                SetProperty<ObservableCollection<Ellipse>>(ref this.backItems, value);
            }
        }

        private ObservableCollection<Ellipse> edgeItems;
        public ObservableCollection<Ellipse> EdgeItems
        {
            get => this.edgeItems;
            set
            {
                SetProperty<ObservableCollection<Ellipse>>(ref this.edgeItems, value);
            }
        }

        private ObservableCollection<Ellipse> ebrItems;
        public ObservableCollection<Ellipse> EBRItems
        {
            get => this.ebrItems;
            set
            {
                SetProperty<ObservableCollection<Ellipse>>(ref this.ebrItems, value);
            }
        }
        // Dependency Properties

        public double CanvasWidth { get; set; }

        public double CanvasHeight { get; set; }
        #endregion

        #region [Command]
        public ICommand LoadedCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    RedrawBaseWafer();
                    RedrawMap();
                });
            }
        }

        public ICommand SizeChangedCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    RedrawBaseWafer();
                    RedrawMap();
                    RedrawTrendMap();
                    RedrawDefectListFront();
                    RedrawDefectListBack();
                });
            }
        }
        #endregion



        public void SetData(string recipeID, string waferID, List<Defect> defectList, string inspectionID)
        {
            SettingItem_Setup setupSetting = GlobalObjects.Instance.Get<Settings>().GetItem<SettingItem_Setup>();

            string sRecipePath = setupSetting.RecipeRootPath;

            string recipeFrontPath = System.IO.Path.Combine(sRecipePath, setupSetting.FrontRecipeFolderName);
            string recipeBackPath = System.IO.Path.Combine(sRecipePath, setupSetting.BackRecipeFolderName);
            string recipeEdgePath = System.IO.Path.Combine(sRecipePath, setupSetting.EdgeRecipeFolderName);
            string recipeEBRPath = System.IO.Path.Combine(sRecipePath, setupSetting.EBRRecipeFolderName);

            recipeFrontPath = System.IO.Path.Combine(recipeFrontPath, recipeID + "\\" + recipeID + ".rcp");
            recipeBackPath = System.IO.Path.Combine(recipeBackPath, recipeID + "\\" + recipeID + ".rcp");
            recipeEdgePath = System.IO.Path.Combine(recipeEdgePath, recipeID + "\\" + recipeID + ".rcp");
            recipeEBRPath = System.IO.Path.Combine(recipeEBRPath, recipeID + "\\" + recipeID + ".rcp");

            if (File.Exists(recipeFrontPath))
            {
                this.IsCheckedFront = true;
                this.recipeFront.Read(recipeFrontPath);

                this.MapSizeX = this.recipeFront.WaferMap.MapSizeX.ToString();
                this.MapSizeY = this.recipeFront.WaferMap.MapSizeY.ToString();
                this.GrossDie = this.recipeFront.WaferMap.GrossDie.ToString();

                

                CreateMap(this.recipeFront.WaferMap);
            }
            else
            {
                this.IsCheckedFront = false;
            }


            if (File.Exists(recipeBackPath))
            {
                this.IsCheckedBack = true;
                this.recipeBack.Read(recipeBackPath);

                this.MapSizeX = this.recipeBack.WaferMap.MapSizeX.ToString();
                this.MapSizeY = this.recipeBack.WaferMap.MapSizeY.ToString();
                this.GrossDie = this.recipeBack.WaferMap.GrossDie.ToString();

                CreateMap(this.recipeBack.WaferMap);
            }
            else
            {
                this.IsCheckedBack = false;
            }


            if (File.Exists(recipeEdgePath))
            {
                this.IsCheckedEdge = true;
                this.recipeEdge.Read(recipeEdgePath);
            }
            else
            {
                this.IsCheckedEdge = false;
            }


            if (File.Exists(recipeEBRPath))
            {
                this.IsCheckedEBR = true;
                this.recipeEBR.Read(recipeEBRPath);
            }
            else
            {
                this.IsCheckedEBR = false;
            }


            this.defectList = defectList;

            this.InspectionID = inspectionID;

            this.RecipeID = recipeID;
            this.WaferID = waferID;

            ClassifyDefectList();
        }

        private void ClassifyDefectList()
        {
            ClassifyDefectListFront();
            ClassifyDefectListBack();
            ClassifyDefectListEdge();


            CreateDefectCountMap();
        }

        private void ClassifyDefectListFront()
        {
            // Front
            if (this.recipeFront == null) return;

            OriginRecipe originRecipe = this.recipeFront.GetItem<OriginRecipe>();
            RecipeType_WaferMap waferMap = this.recipeFront.WaferMap;

            if (waferMap.MapSizeX == 0 || waferMap.MapSizeY == 0) return;

            int sizeX = waferMap.MapSizeX;
            int sizeY = waferMap.MapSizeY;

            int offsetX = waferOffsetX + dieOffsetX;
            int offsetY = waferOffsetY + dieOffsetY;

            double canvasChipWidth = (double)(CanvasWidth - offsetX * 2) / (double)sizeX;
            double canvasChipHeight = (double)(CanvasHeight - offsetY * 2) / (double)sizeY;

            this.FrontItems.Clear();
            this.frontDefectList.Clear();

            ObservableCollection<Ellipse> defectList = new ObservableCollection<Ellipse>();

            int count = 0;

            foreach (Defect defect in this.defectList)
            {
                if (CheckDefectCode(defect.m_nDefectCode) != MODULE_TYPE.Front)
                    continue;

                count++;

                Ellipse defectUI = new Ellipse();
                defectUI.Fill = Brushes.Red;
                defectUI.Width = 4;
                defectUI.Height = 4;

                int mapX = defect.m_nChipIndexX;
                int mapY = defect.m_nChipIndexY;

                int canvasChipPosX = (int)(offsetX + mapX * canvasChipWidth);
                int canvasChipPosY = (int)(offsetY + (mapY + 1) * canvasChipHeight); // 좌하단 기준

                int canvasDefectPosX = canvasChipPosX + (int)(defect.m_fRelX / originRecipe.OriginWidth * canvasChipWidth);
                int canvasDefectPosY = canvasChipPosY - (int)(defect.m_fRelY / originRecipe.OriginHeight * canvasChipHeight);

                Canvas.SetLeft(defectUI, canvasDefectPosX);
                Canvas.SetTop(defectUI, canvasDefectPosY);

                defectList.Add(defectUI);

                this.frontDefectList.Add(new DefectData(defect, defectUI));
                //this.FrontDefectList.Add(defectUI);
            }

            this.FrontItems = defectList;
            this.FrontDefectCount = count;
        }

        private void ClassifyDefectListBack()
        {
            // Front
            if (this.recipeBack == null) return;

            OriginRecipe originRecipe = this.recipeBack.GetItem<OriginRecipe>();
            BacksideRecipe backRecipe = this.recipeBack.GetItem<BacksideRecipe>();
            RecipeType_WaferMap waferMap = this.recipeBack.WaferMap;

            if (waferMap.MapSizeX == 0 || waferMap.MapSizeY == 0) return;


            //int centerX = backRecipe.CenterX;
            //int centerY = backRecipe.CenterY;
            int radius = backRecipe.Radius;

            int sizeX = waferMap.MapSizeX;
            int sizeY = waferMap.MapSizeY;

            int offsetX = waferOffsetX + dieOffsetX;
            int offsetY = waferOffsetY + dieOffsetY;


            double canvasRadius = (CanvasWidth - (waferOffsetX * 2)) / 2;

            double ratio = canvasRadius / radius;
            int canvasCenterX = (int)CanvasWidth / 2;
            int canvasCenterY = (int)CanvasHeight / 2;

            //double canvasChipWidth = (double)(CanvasWidth - offsetX * 2) / (double)sizeX;
            //double canvasChipHeight = (double)(CanvasHeight - offsetY * 2) / (double)sizeY;

            this.BackItems.Clear();
            this.backDefectList.Clear();

            ObservableCollection<Ellipse> defectList = new ObservableCollection<Ellipse>();

            int count = 0;
            foreach (Defect defect in this.defectList)
            {
                if (CheckDefectCode(defect.m_nDefectCode) != MODULE_TYPE.Back)
                    continue;

                count++;

                Ellipse defectUI = new Ellipse();
                defectUI.Fill = Brushes.Blue;
                defectUI.Width = 4;
                defectUI.Height = 4;


                int canvasDefectPosX = (int)(canvasCenterX + defect.m_fRelX * ratio);
                int canvasDefectPosY = (int)(canvasCenterY - defect.m_fRelY * ratio);

                //int mapX = defect.m_nChipIndexX;
                //int mapY = defect.m_nChipIndexY;

                //int canvasChipPosX = (int)(offsetX + mapX * canvasChipWidth);
                //int canvasChipPosY = (int)(offsetY + (mapY + 1) * canvasChipHeight); // 좌하단 기준

                //int canvasDefectPosX = canvasChipPosX + (int)(defect.m_fRelX / originRecipe.OriginWidth * canvasChipWidth);
                //int canvasDefectPosY = canvasChipPosY - (int)(defect.m_fRelY / originRecipe.OriginHeight * canvasChipHeight);

                Canvas.SetLeft(defectUI, canvasDefectPosX);
                Canvas.SetTop(defectUI, canvasDefectPosY);

                defectList.Add(defectUI);
                this.backDefectList.Add(new DefectData(defect, defectUI));
                //this.BackDefectList.Add(defectUI);
            }

            this.BackItems = defectList;
            this.BackDefectCount = count;

            RedrawDefectListBack();
        }

        private void ClassifyDefectListEdge()
        {
            return;
            // Front
            if (this.recipeFront == null) return;

            OriginRecipe originRecipe = this.recipeFront.GetItem<OriginRecipe>();
            RecipeType_WaferMap waferMap = this.recipeFront.WaferMap;

            if (waferMap.MapSizeX == 0 || waferMap.MapSizeY == 0) return;

            int sizeX = waferMap.MapSizeX;
            int sizeY = waferMap.MapSizeY;

            int offsetX = waferOffsetX + dieOffsetX;
            int offsetY = waferOffsetY + dieOffsetY;

            double canvasChipWidth = (double)(CanvasWidth - offsetX * 2) / (double)sizeX;
            double canvasChipHeight = (double)(CanvasHeight - offsetY * 2) / (double)sizeY;

            this.FrontItems.Clear();

            this.FrontDefectCount = this.defectList.Count;

            foreach (Defect defect in this.defectList)
            {
                if (CheckDefectCode(defect.m_nDefectCode) != MODULE_TYPE.Front)
                    continue;

                Ellipse defectUI = new Ellipse();
                defectUI.Fill = Brushes.Red;
                defectUI.Width = 4;
                defectUI.Height = 4;

                int mapX = defect.m_nChipIndexX;
                int mapY = defect.m_nChipIndexY;

                int canvasChipPosX = (int)(offsetX + mapX * canvasChipWidth);
                int canvasChipPosY = (int)(offsetY + (mapY + 1) * canvasChipHeight); // 좌하단 기준

                int canvasDefectPosX = canvasChipPosX + (int)(defect.m_fRelX / originRecipe.OriginWidth * canvasChipWidth);
                int canvasDefectPosY = canvasChipPosY - (int)(defect.m_fRelY / originRecipe.OriginHeight * canvasChipHeight);

                Canvas.SetLeft(defectUI, canvasDefectPosX);
                Canvas.SetTop(defectUI, canvasDefectPosY);

                this.FrontItems.Add(defectUI);
            }
        }

        private MODULE_TYPE CheckDefectCode(int defectcode)
        {
            MODULE_TYPE type = (MODULE_TYPE)(Math.Floor((double)defectcode / 10000));
            // Defect Code 통일 및 DB화 필요

            return type;
        }


        public void RedrawDefectListFront()
        {
            if (this.isCheckedFront == false)
            {
                foreach (Ellipse rect in this.FrontItems)
                    rect.Visibility = Visibility.Hidden;

                return;
            }
            else
            {
                foreach (Ellipse rect in this.FrontItems)
                    rect.Visibility = Visibility.Visible;
            }
            if (this.recipeFront == null) return;


            OriginRecipe originRecipe = this.recipeFront.GetItem<OriginRecipe>();
            RecipeType_WaferMap waferMap = this.recipeFront.WaferMap;

            if (waferMap.MapSizeX == 0 || waferMap.MapSizeY == 0) return;

            int sizeX = waferMap.MapSizeX;
            int sizeY = waferMap.MapSizeY;

            int offsetX = waferOffsetX + dieOffsetX;
            int offsetY = waferOffsetY + dieOffsetY;

            double canvasChipWidth = (double)(CanvasWidth - offsetX * 2) / (double)sizeX;
            double canvasChipHeight = (double)(CanvasHeight - offsetY * 2) / (double)sizeY;

            foreach(DefectData data in this.frontDefectList)
            {
                Ellipse defectUI = data.UIElement;
                Defect defect = data.Defect;

                int mapX = defect.m_nChipIndexX;
                int mapY = defect.m_nChipIndexY;

                int canvasChipPosX = (int)(offsetX + mapX * canvasChipWidth);
                int canvasChipPosY = (int)(offsetY + (mapY + 1) * canvasChipHeight); //좌하단 기준

                int canvasDefectPosX = canvasChipPosX + (int)(defect.m_fRelX / originRecipe.OriginWidth * canvasChipWidth);
                int canvasDefectPosY = canvasChipPosY - (int)(defect.m_fRelY / originRecipe.OriginHeight * canvasChipHeight);

                //if(data.IsSelected == true)
                //{
                //    defectUI.Fill = Brushes.Yellow;
                //    defectUI.Width = 6;
                //    defectUI.Height = 6;
                //}
                //else
                //{
                //    defectUI.Fill = Brushes.Red;
                //    defectUI.Width = 4;
                //    defectUI.Height = 4;
                //}

                Canvas.SetLeft(defectUI, canvasDefectPosX);
                Canvas.SetTop(defectUI, canvasDefectPosY);
            }
        }

        public void RedrawDefectListBack()
        {
            if (this.isCheckedBack == false)
            {
                foreach (Ellipse rect in this.BackItems)
                    rect.Visibility = Visibility.Hidden;

                return;
            }
            else
            {
                foreach (Ellipse rect in this.BackItems)
                    rect.Visibility = Visibility.Visible;
            }
            
            if (this.recipeBack == null) return;


            OriginRecipe originRecipe = this.recipeBack.GetItem<OriginRecipe>();
            BacksideRecipe backRecipe = this.recipeBack.GetItem<BacksideRecipe>();
            RecipeType_WaferMap waferMap = this.recipeBack.WaferMap;

            if (waferMap.MapSizeX == 0 || waferMap.MapSizeY == 0) return;

            int radius = backRecipe.Radius;

            int sizeX = waferMap.MapSizeX;
            int sizeY = waferMap.MapSizeY;

            int offsetX = waferOffsetX + dieOffsetX;
            int offsetY = waferOffsetY + dieOffsetY;


            double canvasRadius = (CanvasWidth - (waferOffsetX * 2)) / 2;

            double ratio = canvasRadius / radius;
            int canvasCenterX = (int)CanvasWidth / 2;
            int canvasCenterY = (int)CanvasHeight / 2;

            foreach (DefectData data in this.backDefectList)
            {
                Ellipse defectUI = data.UIElement;
                Defect defect = data.Defect;

                int canvasDefectPosX = (int)(canvasCenterX + defect.m_fRelX * ratio);
                int canvasDefectPosY = (int)(canvasCenterY - defect.m_fRelY * ratio);

                Canvas.SetLeft(defectUI, canvasDefectPosX);
                Canvas.SetTop(defectUI, canvasDefectPosY);
            }
        }



        #region [Drawing Objects]
        private Ellipse baseWafer;

        private int waferOffsetX = 0;
        private int waferOffsetY = 0;

        private int dieOffsetX = 0;
        private int dieOffsetY = 0;
        #endregion

        #region [Drawing Method]
        private void InitBaseElements()
        {
            this.baseElements = new ObservableCollection<UIElement>();

            // Wafer
            this.baseWafer = new Ellipse();
            this.baseWafer.Fill = new SolidColorBrush(Color.FromArgb(0x55, 0xFF, 0xFF, 0xFF));
            this.baseWafer.Stroke = Brushes.Black;
            this.baseWafer.StrokeThickness = 0.5;
            this.baseWafer.Width = CanvasWidth;
            this.baseWafer.Height = CanvasHeight;

            Canvas.SetZIndex(this.baseWafer, 1);
            //Canvas.SetLeft(this.baseWafer, baseOffset);
            //Canvas.SetTop(this.baseWafer, baseOffset);

            this.baseDieList = new ObservableCollection<Rectangle>();
            this.trendMapDieList = new ObservableCollection<Rectangle>();

            this.BaseElements.Add(baseWafer);
        }


        #region [Wafer Map]
        private void RedrawBaseWafer()
        {
        
            int diameter = (int)(CanvasWidth > CanvasHeight ? CanvasHeight : CanvasWidth);

            int margin = (int)diameter / 100 * 8;

            diameter -= margin * 2;

            this.baseWafer.Width = diameter;
            this.baseWafer.Height = diameter;

            //this.dieOffsetX = (int)(diameter / 2 * 0.414);
            //this.dieOffsetY = (int)(diameter / 2 * 0.414);

            this.dieOffsetX = (int)(diameter / 2 * 0.1);
            this.dieOffsetY = (int)(diameter / 2 * 0.1);

            if (CanvasWidth > CanvasHeight)
            {
                this.waferOffsetX = (int)(CanvasWidth - diameter) / 2;
                this.waferOffsetY = margin;

                //this.dieOffsetX = this.waferOffsetX + 
            }
            else
            {
                this.waferOffsetX = margin;
                this.waferOffsetY = (int)(CanvasHeight - diameter) / 2;
            }

            if (!this.BaseElements.Contains(baseWafer)) return;


            Canvas.SetLeft(this.baseWafer, this.waferOffsetX);
            Canvas.SetTop(this.baseWafer, this.waferOffsetY);
        }
        #endregion

        #region [DieList]

        RecipeType_WaferMap waferMap = null;

        #endregion
        private void CreateMap(RecipeType_WaferMap waferMap)
        {
            if (waferMap.MapSizeX == 0 || waferMap.MapSizeY == 0) return;

            baseDieList.Clear();
            TrendMapDieList.Clear();

            int sizeX = waferMap.MapSizeX;
            int sizeY = waferMap.MapSizeY;

            int offsetX = waferOffsetX + dieOffsetX;
            int offsetY = waferOffsetY + dieOffsetY;

            double chipWidth = (double)(CanvasWidth - offsetX * 2) / (double)sizeX;
            double chipHeight = (double)(CanvasHeight - offsetY * 2) / (double)sizeY;

            var mapData = waferMap.Data;
            if (mapData == null) return;

            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    if (mapData[y * sizeX + x] == (int)CHIP_TYPE.NORMAL)
                    {
                        Rectangle rect = new Rectangle();
                        rect.Width = chipWidth;
                        rect.Height = chipHeight;
                        Canvas.SetLeft(rect, (chipWidth * x) + offsetY);
                        Canvas.SetTop(rect, (chipHeight * y) + offsetY);

                        rect.Tag = new CPoint(x, y);
                        rect.ToolTip = string.Format("({0}, {1})", x, y); // chip index
                        rect.Stroke = Brushes.Transparent;
                        rect.Opacity = 0.7;
                        rect.StrokeThickness = 2;
                        rect.Fill = Brushes.LightGray;

                        baseDieList.Add(rect);

                        Rectangle trendRect = new Rectangle();
                        trendRect.Width = chipWidth;
                        trendRect.Height = chipHeight;
                        Canvas.SetLeft(trendRect, (chipWidth * x) + offsetY);
                        Canvas.SetTop(trendRect, (chipHeight * y) + offsetY);

                        trendRect.Tag = new CPoint(x, y);
                        trendRect.ToolTip = string.Format("({0}, {1})", x, y); // chip index
                        trendRect.Stroke = Brushes.Transparent;
                        trendRect.Opacity = 1.0;
                        trendRect.StrokeThickness = 2;
                        
                        trendRect.Fill = Brushes.LightGray;

                        Canvas.SetZIndex(trendRect, 20);
                        TrendMapDieList.Add(trendRect);
                    }
                }
            }

            this.waferMap = waferMap;

           


            RedrawMap();
            RedrawTrendMap();
        }

        public void CreateDefectCountMap()
        {
            RecipeType_WaferMap waferMap = this.recipeFront.WaferMap;
            int sizeX = waferMap.MapSizeX;
            int sizeY = waferMap.MapSizeY;

            this.defectCountMap = new int[sizeX * sizeY];
            int[] mapdata = waferMap.Data;

            for(int i =0; i < sizeY; i++)
            {
                for(int j = 0; j < sizeX; j++)
                {
                    if(mapdata[i * sizeX + j] == (int)CHIP_TYPE.NO_CHIP)
                    {
                        this.defectCountMap[i * sizeX + j] = -1;
                    }
                    else
                    {
                        this.defectCountMap[i * sizeX + j] = GetDefectCountChip(frontDefectList, j, i);
                    }
                }
            }
        }

        private void RedrawMap()
        {
            if (this.IsCheckedDisplayChip == false)
            {
                foreach(Rectangle rect in this.baseDieList)
                {
                    rect.Visibility = Visibility.Hidden;
                }
                return;
            }
            else
            {
                foreach (Rectangle rect in this.baseDieList)
                {
                    if(rect.Visibility != Visibility.Visible)
                        rect.Visibility = Visibility.Visible;
                }
            }

            RecipeType_WaferMap waferMap = this.waferMap;

            if (waferMap.MapSizeX == 0 || waferMap.MapSizeY == 0) return;

            int sizeX = waferMap.MapSizeX;
            int sizeY = waferMap.MapSizeY;

            int offsetX = waferOffsetX + dieOffsetX;
            int offsetY = waferOffsetY + dieOffsetY;

            double chipWidth = (double)(CanvasWidth - offsetX * 2)/ (double)sizeX ;
            double chipHeight = (double)(CanvasHeight - offsetY * 2) / (double)sizeY;

            var mapData = waferMap.Data;
            if (mapData == null) return;

            int index = 0;
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    if (mapData[y * sizeX + x] == (int)CHIP_TYPE.NORMAL)
                    {
                        Rectangle rect = baseDieList[index];

                        rect.Width = chipWidth;
                        rect.Height = chipHeight;
                        Canvas.SetLeft(rect, (chipWidth * x) + offsetX);
                        Canvas.SetTop(rect, (chipHeight * y) + offsetY);
                        index++;
                    }
                }
            }
        }

        private void RedrawTrendMap()
        {
            // 일단 Front 전용
            if (frontDefectList == null) return;

            if (this.IsCheckedDisplayTrendMap == false)
            {
                foreach (Rectangle rect in this.trendMapDieList)
                {
                    rect.Visibility = Visibility.Hidden;
                }
                return;
            }
            else
            {
                foreach (Rectangle rect in this.trendMapDieList)
                {
                    if (rect.Visibility != Visibility.Visible)
                        rect.Visibility = Visibility.Visible;
                }
            }

            RecipeType_WaferMap waferMap = this.waferMap;

            if (waferMap.MapSizeX == 0 || waferMap.MapSizeY == 0) return;

            int sizeX = waferMap.MapSizeX;
            int sizeY = waferMap.MapSizeY;

            int offsetX = waferOffsetX + dieOffsetX;
            int offsetY = waferOffsetY + dieOffsetY;

            double chipWidth = (double)(CanvasWidth - offsetX * 2) / (double)sizeX;
            double chipHeight = (double)(CanvasHeight - offsetY * 2) / (double)sizeY;

            var mapData = waferMap.Data;
            if (mapData == null) return;

            int index = 0;
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    if (mapData[y * sizeX + x] == (int)CHIP_TYPE.NORMAL)
                    {
                        int count = GetDefectCountChip(frontDefectList, x, y);

                        Rectangle rect = trendMapDieList[index];

                        if(count == 0)
                            rect.Fill = new SolidColorBrush(Color.FromArgb(255, 150, 180, 150));
                        else if (count <= 5)
                            rect.Fill = new SolidColorBrush(Color.FromArgb(255, 190, 170, 170));
                        else if (count <= 20)
                            rect.Fill = new SolidColorBrush(Color.FromArgb(255, 200, 140, 140));
                        else if (count <= 50)
                            rect.Fill = new SolidColorBrush(Color.FromArgb(255, 220, 120, 120));
                        else if (count <= 300)
                            rect.Fill = new SolidColorBrush(Color.FromArgb(255, 240, 100, 100));
                        else /*if(count > 1000)*/
                            rect.Fill = new SolidColorBrush(Color.FromArgb(255, (byte)255, 80, 80));

                        rect.Width = chipWidth;
                        rect.Height = chipHeight;
                        Canvas.SetLeft(rect, (chipWidth * x) + offsetX);
                        Canvas.SetTop(rect, (chipHeight * y) + offsetY);
                        index++;
                    }
                }
            }
        }

        public int GetDefectCountChip(List<DefectData> defectList ,int x, int y)
        {
            int count = 0;
            foreach(DefectData defect in defectList)
            {
                if(defect.Defect.m_nChipIndexX == x &&
                    defect.Defect.m_nChipIndexY == y)
                {
                    count++;
                }
            }
            return count;
        }

        public void SetSelectedDefect(Defect selected)
        {
            foreach(DefectData defect in this.frontDefectList)
            {
                if(defect.IsSelected == true)
                {
                    defect.IsSelected = false;
                    defect.UIElement.Height = 4;
                    defect.UIElement.Width = 4;
                    defect.UIElement.Fill = Brushes.Red;
                }

                if (defect.Defect.m_strInspectionID == selected.m_strInspectionID &&
                    defect.Defect.m_nDefectIndex == selected.m_nDefectIndex &&
                    defect.Defect.m_nDefectCode == selected.m_nDefectCode)
                {
                    defect.IsSelected = true;
                    defect.UIElement.Height = 6;
                    defect.UIElement.Width = 6;
                    defect.UIElement.Fill = Brushes.Yellow;

                    if(this.FrontItems.Contains(defect.UIElement))
                    {
                        this.FrontItems.Remove(defect.UIElement);

                        Canvas.SetZIndex(defect.UIElement, 10);
                        this.FrontItems.Add(defect.UIElement);
                    }
                }                   
            }

            foreach (DefectData defect in this.backDefectList)
            {
                if (defect.IsSelected == true)
                {
                    defect.IsSelected = false;
                    defect.UIElement.Height = 4;
                    defect.UIElement.Width = 4;
                    defect.UIElement.Fill = Brushes.Blue;
                }

                if (defect.Defect.m_strInspectionID == selected.m_strInspectionID &&
                    defect.Defect.m_nDefectIndex == selected.m_nDefectIndex &&
                    defect.Defect.m_nDefectCode == selected.m_nDefectCode)
                {
                    defect.IsSelected = true;
                    defect.UIElement.Height = 6;
                    defect.UIElement.Width = 6;
                    defect.UIElement.Fill = Brushes.Yellow;
                    if (this.BackItems.Contains(defect.UIElement))
                    {
                        this.BackItems.Remove(defect.UIElement);

                        Canvas.SetZIndex(defect.UIElement, 10);
                        this.BackItems.Add(defect.UIElement);
                    }
                }
                else
                    defect.IsSelected = false;
            }

            foreach (DefectData defect in this.edgeDefectList)
            {
                if (defect.IsSelected == true)
                {
                    defect.IsSelected = false;
                    defect.UIElement.Height = 4;
                    defect.UIElement.Width = 4;
                    defect.UIElement.Fill = Brushes.YellowGreen;
                }

                if (defect.Defect.m_strInspectionID == selected.m_strInspectionID &&
                    defect.Defect.m_nDefectIndex == selected.m_nDefectIndex &&
                    defect.Defect.m_nDefectCode == selected.m_nDefectCode)
                {
                    defect.IsSelected = true;
                    defect.UIElement.Height = 6;
                    defect.UIElement.Width = 6;
                    defect.UIElement.Fill = Brushes.Yellow;
                }
                else
                    defect.IsSelected = false;
            }

            foreach (DefectData defect in this.ebrDefectList)
            {
                if (defect.IsSelected == true)
                {
                    defect.IsSelected = false;
                    defect.UIElement.Height = 4;
                    defect.UIElement.Width = 4;
                    defect.UIElement.Fill = Brushes.Orange;
                }

                if (defect.Defect.m_strInspectionID == selected.m_strInspectionID &&
                    defect.Defect.m_nDefectIndex == selected.m_nDefectIndex &&
                    defect.Defect.m_nDefectCode == selected.m_nDefectCode)
                {
                    defect.IsSelected = true;
                    defect.UIElement.Height = 6;
                    defect.UIElement.Width = 6;
                    defect.UIElement.Fill = Brushes.Yellow;
                }
                else
                    defect.IsSelected = false;
            }


            //RedrawDefectListFront();
            //RedrawDefectListBack();
        }
        #endregion
    }
}
