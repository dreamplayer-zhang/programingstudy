using ATI;
using RootTools;
using RootTools.Inspects;
using RootTools.Memory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
namespace Root_Vega
{
    class _2_9_BevelViewModel : ObservableObject
    {
        /// <summary>
		/// 외부 Thread에서 UI를 Update하기 위한 Dispatcher
		/// </summary>
		protected Dispatcher _dispatcher;
        Vega_Engineer m_Engineer;
        MemoryTool m_MemoryModule;
        List<ImageData> m_Image = new List<ImageData>();
        DrawData m_DD;
        VegaRecipeData m_Recipe;

        SqliteDataDB VSDBManager;
        int currentDefectIdx;
        System.Data.DataTable VSDataInfoDT;
        System.Data.DataTable VSDataDT;

        private string inspDefaultDir;
        private string inspFileName;

        public VegaRecipeData p_Recipe
        {
            get
            {
                return m_Recipe;
            }
            set
            {
                SetProperty(ref m_Recipe, value);
            }
        }

        //int tempImageWidth = 640;
        //int tempImageHeight = 480;

        public _2_9_BevelViewModel(Vega_Engineer engineer, IDialogService dialogService)
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            m_Engineer = engineer;
            Init(engineer, dialogService);


        }
        /// <summary>
        /// UI에 추가된 Defect을 빨간색 상자로 표시할 수 있도록 추가하는 메소드
        /// </summary>
        /// <param name="source">UI에 추가할 Defect List</param>
        /// <param name="args">arguments. 사용이 필요한 경우 수정해서 사용</param>
        private void M_InspManager_AddDefect(DefectData[] source, EventArgs args)
        {
            //string tempInspDir = @"C:\vsdb\TEMP_IMAGE";

            foreach (var item in source)
            {
                CPoint ptStart = new CPoint(Convert.ToInt32(item.fPosX - item.nWidth / 2.0), Convert.ToInt32(item.fPosY - item.nHeight / 2.0));
                CPoint ptEnd = new CPoint(Convert.ToInt32(item.fPosX + item.nWidth / 2.0), Convert.ToInt32(item.fPosY + item.nHeight / 2.0));

                CRect resultBlock = new CRect(ptStart.X, ptStart.Y, ptEnd.X, ptEnd.Y);

                //CRect ImageSizeBlock = new CRect(
                //	(int)item.fPosX - tempImageWidth / 2,
                //	(int)item.fPosY - tempImageHeight / 2,
                //	(int)item.fPosX + tempImageWidth / 2,
                //	(int)item.fPosY + tempImageHeight / 2);

                //string filename = currentDefectIdx.ToString("D8") + ".bmp";
                //m_ImageViewer.p_ImageData.SaveRectImage(ImageSizeBlock, System.IO.Path.Combine(tempInspDir, filename));

                m_DD.AddRectData(resultBlock, System.Drawing.Color.Red);

                //여기서 DB에 Defect을 추가하는 부분도 구현한다
                System.Data.DataRow dataRow = VSDataDT.NewRow();

                //Data,@No(INTEGER),DCode(INTEGER),Size(INTEGER),Length(INTEGER),Width(INTEGER),Height(INTEGER),InspMode(INTEGER),FOV(INTEGER),PosX(INTEGER),PosY(INTEGER)

                dataRow["No"] = currentDefectIdx;
                currentDefectIdx++;
                dataRow["DCode"] = item.nClassifyCode;
                dataRow["AreaSize"] = item.fAreaSize;
                dataRow["Length"] = item.nLength;
                dataRow["Width"] = item.nWidth;
                dataRow["Height"] = item.nHeight;
                //dataRow["FOV"] = item.FOV;
                dataRow["PosX"] = item.fPosX;
                dataRow["PosY"] = item.fPosY;

                VSDataDT.Rows.Add(dataRow);
            }
            _dispatcher.Invoke(new Action(delegate ()
            {
                RedrawUIElement();
            }));
        }

        void Init(Vega_Engineer engineer, IDialogService dialogService)
        {
            m_DD = new DrawData();
            p_Recipe = engineer.m_recipe.VegaRecipeData;

            m_MemoryModule = engineer.ClassMemoryTool();
            //m_MemoryModule.CreatePool(sPool, 8);
            //m_MemoryModule.GetPool(sPool).CreateGroup(sGroup);

            //m_MemoryModule.GetPool(sPool).p_gbPool = 2;


            for (int i = 0; i < 4; i++)
            {
                m_Image.Add(new ImageData(m_MemoryModule.GetMemory(App.sSidePool, App.sSideGroup, App.m_bevelMem[i])));
                p_ImageViewer_List.Add(new ImageViewer_ViewModel(m_Image[i], dialogService)); //!! m_Image 는 추후 각 part에 맞는 이미지가 들어가게 수정.
            }

            for (int _Index = 0; _Index < 2; _Index++)
            {
                m_DrawHistoryWorker_List.Add(new DrawHistoryWorker());
                p_SimpleShapeDrawer_List.Add(new List<SimpleShapeDrawerVM>());
                for (int i = 0; i < 4; i++)
                {
                    p_SimpleShapeDrawer_List[_Index].Add(new SimpleShapeDrawerVM(p_ImageViewer_List[i]));
                    p_SimpleShapeDrawer_List[_Index][i].RectangleKeyValue = Key.D1;
                }
            }
            for (int i = 0; i < 4; i++)
            {
                p_ImageViewer_List[i].SetDrawer((DrawToolVM)p_SimpleShapeDrawer_List[0][i]);
                p_ImageViewer_List[i].m_HistoryWorker = m_DrawHistoryWorker_List[0];
            }

            p_ImageViewer_Top = p_ImageViewer_List[0];
            p_ImageViewer_Left = p_ImageViewer_List[1];
            p_ImageViewer_Right = p_ImageViewer_List[2];
            p_ImageViewer_Bottom = p_ImageViewer_List[3];


            //p_ListRoi = m_Recipe.m_RD.p_Roi;

            //m_Recipe.m_RD.p_Roi = new List<Roi>(); //Mask#1, Mask#2... New List Mask


            //Roi Mask, Mask2;
            //Mask = new Roi("MASK1", Roi.Item.Test);  // Mask Number.. New Mask
            //Mask.m_Side.p_Parameter = new ObservableCollection<SurFace_ParamData>();
            //Mask.m_Side.m_NonPattern = new List<NonPattern>(); // List Rect in Mask
            //NonPattern rect = new NonPattern(); // New Rect
            //rect.m_rt = new CRect(); // Rect Info
            //SurFace_ParamData param = new SurFace_ParamData();
            //Mask.m_Surface.p_Parameter.Add(param);
            //Mask.m_Surface.m_NonPattern.Add(rect); // Add Rect to Rect List
            //                                       //m_Recipe.m_RD.p_Roi.Add(Mask);
            //                                       //p_ListRoi.Add(m_Mask);

            //Mask2 = new Roi("MASK2", Roi.Item.Test);  // Mask Number.. New Mask
            //Mask2.m_Surface.p_Parameter = new ObservableCollection<SurFace_ParamData>();
            //Mask2.m_Surface.m_NonPattern = new List<NonPattern>(); // List Rect in Mask
            //NonPattern rect2 = new NonPattern(); // New Rect
            //rect2.m_rt = new CRect(); // Rect Info
            //SurFace_ParamData param2 = new SurFace_ParamData();
            //Mask2.m_Surface.p_Parameter.Add(param2);
            //Mask2.m_Surface.m_NonPattern.Add(rect2); // Add Rect to Rect List

            //p_Recipe.p_RecipeData.p_Roi.Add(Mask);
            //p_Recipe.p_RecipeData.p_Roi.Add(Mask2);
        }

        #region Property
        string _test;
        public string Test
        {
            get
            {

                return _test;
            }
            set
            {
                SetProperty(ref _test, value);
            }

        }
        string _test2;
        public string Test2
        {
            get
            {
                return _test2;
            }
            set
            {
                SetProperty(ref _test2, value);
            }
        }

        //private ObservableCollection<Roi> _ListRoi = new ObservableCollection<Roi>();
        //public ObservableCollection<Roi> p_ListRoi
        //{
        //    get
        //    {
        //        return _ListRoi;
        //    }
        //    set
        //    {
        //        SetProperty(ref _ListRoi, value);
        //        //m_Recipe.m_RD.p_Roi = p_ListRoi.ToList<Roi>();
        //    }
        //}

        //public SurfaceParamData p_SurFace_ParamData
        //{
        //    get
        //    {
        //        if (m_Recipe.RoiList[p_IndexMask].Surface.ParameterList.Count != 0)
        //            return m_Recipe.RoiList[p_IndexMask].Surface.ParameterList[0];
        //        else
        //            return new SurfaceParamData();
        //    }
        //    set
        //    {
        //        if (m_Recipe.RoiList[p_IndexMask].Surface.ParameterList.Count != 0)
        //            m_Recipe.RoiList[p_IndexMask].Surface.ParameterList[0] = value;
        //        RaisePropertyChanged();
        //    }
        //}

        private List<List<SimpleShapeDrawerVM>> m_SimpleShapeDrawer_List = new List<List<SimpleShapeDrawerVM>>();
        public List<List<SimpleShapeDrawerVM>> p_SimpleShapeDrawer_List
        {
            get
            {
                return m_SimpleShapeDrawer_List;
            }
            set
            {
                SetProperty(ref m_SimpleShapeDrawer_List, value);
            }
        }

        public List<DrawHistoryWorker> m_DrawHistoryWorker_List = new List<DrawHistoryWorker>();

        //private int _IndexMask = 0;
        //public int p_IndexMask
        //{
        //    get
        //    {
        //        return _IndexMask;
        //    }
        //    set
        //    {
        //        SetProperty(ref _IndexMask, value);

        //        p_SurFace_ParamData = p_Recipe.RoiList[_IndexMask].Surface.ParameterList[0];
        //        for (int i = 0; i < 4; i++)
        //        {
        //            p_ImageViewer_List[i].SetDrawer((DrawToolVM)p_SimpleShapeDrawer_List[_IndexMask][i]);
        //            p_ImageViewer_List[i].m_HistoryWorker = m_DrawHistoryWorker_List[_IndexMask];
        //            p_ImageViewer_List[i].SetImageSource();
        //            p_SurFace_ParamData = p_Recipe.RoiList[_IndexMask].Surface.ParameterList[0];
        //        }

        //    }
        //}

        private List<ImageViewer_ViewModel> m_ImageViewer_List = new List<ImageViewer_ViewModel>();
        public List<ImageViewer_ViewModel> p_ImageViewer_List
        {
            get
            {
                return m_ImageViewer_List;
            }
            set
            {
                SetProperty(ref m_ImageViewer_List, value);
            }
        }


        private ImageViewer_ViewModel m_ImageViewer_Top;
        public ImageViewer_ViewModel p_ImageViewer_Top
        {
            get
            {
                return m_ImageViewer_Top;
            }
            set
            {
                SetProperty(ref m_ImageViewer_Top, value);
            }
        }
        private ImageViewer_ViewModel m_ImageViewer_Left;
        public ImageViewer_ViewModel p_ImageViewer_Left
        {
            get
            {
                return m_ImageViewer_Left;
            }
            set
            {
                SetProperty(ref m_ImageViewer_Left, value);
            }
        }
        private ImageViewer_ViewModel m_ImageViewer_Right;
        public ImageViewer_ViewModel p_ImageViewer_Right
        {
            get
            {
                return m_ImageViewer_Right;
            }
            set
            {
                SetProperty(ref m_ImageViewer_Right, value);
            }
        }
        private ImageViewer_ViewModel m_ImageViewer_Bottom;
        public ImageViewer_ViewModel p_ImageViewer_Bottom
        {
            get
            {
                return m_ImageViewer_Bottom;
            }
            set
            {
                SetProperty(ref m_ImageViewer_Bottom, value);
            }
        }

        private ObservableCollection<UIElement> _UIelement = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> p_UIElement
        {
            get
            {
                return _UIelement;
            }
            set
            {
                SetProperty(ref _UIelement, value);
            }
        }

        private System.Windows.Input.Cursor _recipeCursor;
        public System.Windows.Input.Cursor RecipeCursor
        {
            get
            {
                return _recipeCursor;
            }
            set
            {
                SetProperty(ref _recipeCursor, value);
            }
        }

        private System.Windows.Input.MouseEventArgs _mouseEvent;
        public System.Windows.Input.MouseEventArgs MouseEvent
        {
            get
            {
                return _mouseEvent;
            }
            set
            {
                SetProperty(ref _mouseEvent, value);
            }
        }

        private bool _draw_IsChecked = false;
        public bool Draw_IsChecked
        {
            get
            {
                return _draw_IsChecked;
            }
            set
            {
                SetProperty(ref _draw_IsChecked, value);
                //_btnDraw();
            }
        }
        #endregion

        public void SaveCurrentMask()
        {
            //@용도에 따라 사용 혹은 삭제
            //CRect rect = p_ImageViewer_List.GetCurrentRect_MemPos();
            //p_Recipe.p_RecipeData.p_Roi[p_IndexMask].m_Surface.m_NonPattern[0].m_rt = rect;

        }
        #region Command


        public ICommand btnClear
        {
            get
            {
                return new RelayCommand(_btnClear);
            }
        }
        public ICommand btnInspTest
        {
            get
            {
                return new RelayCommand(_btnInspTest);
            }
        }
        public RelayCommand CommandSaveMask
        {
            get
            {
                return new RelayCommand(SaveCurrentMask);
            }
        }

        private void ClearUI()
        {
            if (p_UIElement != null)
                p_UIElement.Clear();
        }
        private void _btnClear()
        {
            //p_Recipe.RoiList[p_IndexMask].Surface.NonPatternList[0].Area = new CRect();

            //foreach (var viewer in p_ImageViewer_List)
            //{
            //    viewer.ClearShape();
            //    viewer.SetImageSource();
            //}
            //p_IndexMask = _IndexMask;
        }


        //insp 결과 display를 위해 임시 redrawUI 구현
        private void RedrawUIElement()
        {
            //	RedrawRect();
            //	RedrawStr();
        }

        List<CRect> DrawRectList;
        private void _btnInspTest()
        {
            ClearUI();//재검사 전 UI 정리

            if (m_DD != null)
                m_DD.Clear();//Draw Data정리

            if (DrawRectList != null)
                DrawRectList.Clear();//검사영역 draw용 Rect List 정리

            currentDefectIdx = 0;

            CRect Mask_Rect = p_Recipe.RoiList[0].Surface.NonPatternList[0].Area;
            int nblocksize = 500;


            //DrawRectList = m_Engineer.m_InspManager.CreateInspArea(Mask_Rect, nblocksize,
            //	p_Recipe.p_RecipeData.p_Roi[0].m_Surface.p_Parameter[0],
            //	p_Recipe.p_RecipeData.p_bDefectMerge, p_Recipe.p_RecipeData.p_nMergeDistance);

            for (int i = 0; i < DrawRectList.Count; i++)
            {
                CRect inspblock = DrawRectList[i];
                m_DD.AddRectData(inspblock, System.Drawing.Color.Orange);

            }
            System.Diagnostics.Debug.WriteLine("Start Insp");

            inspDefaultDir = @"C:\vsdb";
            if (!System.IO.Directory.Exists(inspDefaultDir))
            {
                System.IO.Directory.CreateDirectory(inspDefaultDir);
            }
            inspFileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_inspResult.vega_result";
            var targetVsPath = System.IO.Path.Combine(inspDefaultDir, inspFileName);
            string VSDB_configpath = @"C:/vsdb/init/vsdb.txt";

            if (VSDBManager == null)
            {
                VSDBManager = new SqliteDataDB(targetVsPath, VSDB_configpath);
            }
            else if (VSDBManager.IsConnected)
            {
                VSDBManager.Disconnect();
                VSDBManager = new SqliteDataDB(targetVsPath, VSDB_configpath);
            }
            if (VSDBManager.Connect())
            {
                VSDBManager.CreateTable("Datainfo");
                VSDBManager.CreateTable("Data");

                VSDataInfoDT = VSDBManager.GetDataTable("Datainfo");
                VSDataDT = VSDBManager.GetDataTable("Data");
            }

            //m_Engineer.m_InspManager.StartInspection(InspectionType.Surface, m_Image.p_Size.X, m_Image.p_Size.Y);//사용 예시

            return;
        }
        #endregion
    }
}
