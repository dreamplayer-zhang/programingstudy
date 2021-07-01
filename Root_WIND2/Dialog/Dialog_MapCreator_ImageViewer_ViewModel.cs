using RootTools;
using RootTools_Vision;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Root_WIND2
{
    public enum DIALOG_MAP_CREATOR_VIEWER_STATE
    {
        Normal,
        SelectChip,
        SelectRoi,
        DrawMap,
        EraseMap,
    }

    public delegate void EventViewerStateChagned();
    public delegate void EventSelectChipBoxDone();
    public delegate void EventSelectChipPointDone();
    public delegate void EventSelectChipBoxReset();
    public delegate void EventSelectRoiBoxDone();
    public delegate void EventSelectRoiPointDone();
    public delegate void EventSelectRoiBoxReset();

    class Dialog_MapCreator_ImageViewer_ViewModel : RootViewer_ViewModel
    {
        #region [Color]
        public class DefineColors
        {
            public static SolidColorBrush SelectChipColor = Brushes.Blue;
            public static SolidColorBrush SelectChipBoxColor = Brushes.Blue;

            public static SolidColorBrush SelectRoiColor = Brushes.Yellow;
            public static SolidColorBrush SelectRoiBoxColor = Brushes.Yellow;
        }
        #endregion

        #region [Event]
        public event EventViewerStateChagned ViewerStateChanged;
        public event EventSelectChipPointDone SelectChipPointDone;
        public event EventSelectChipBoxDone SelectChipBoxDone;
        public event EventSelectChipBoxReset SelectChipBoxReset;
        public event EventSelectRoiPointDone SelectRoiPointDone;
        public event EventSelectRoiBoxDone SelectRoiBoxDone;
        public event EventSelectRoiBoxReset SelectRoiBoxReset;
        #endregion

        #region [ViewerState]
        public Dialog_MapCreator_ImageViewer_ViewModel()
        {
            p_VisibleMenu = System.Windows.Visibility.Collapsed;

            this.ViewerState = DIALOG_MAP_CREATOR_VIEWER_STATE.Normal;
            this.ViewerStateChanged += ViewerStateChanged_Callback;

            InitializeUIElement();
        }

        private DIALOG_MAP_CREATOR_VIEWER_STATE viewerState = DIALOG_MAP_CREATOR_VIEWER_STATE.Normal;
        public DIALOG_MAP_CREATOR_VIEWER_STATE ViewerState
        {
            get => this.viewerState;
            set
            {
                switch (value)
                {
                    case DIALOG_MAP_CREATOR_VIEWER_STATE.Normal:
                        break;
                    case DIALOG_MAP_CREATOR_VIEWER_STATE.SelectChip:
                        selectChipState = PROCESS_SELECT_CHIP_STATE.SelectChipLeftTop;
                        break;
                    case DIALOG_MAP_CREATOR_VIEWER_STATE.SelectRoi:
                        selectRoiState = PROCESS_SELECT_ROI_STATE.SelectRoiLeftTop;
                        break;
                    case DIALOG_MAP_CREATOR_VIEWER_STATE.DrawMap:
                        drawMapState = PROCESS_DRAW_MAP_STATE.Draw;
                        break;
                    case DIALOG_MAP_CREATOR_VIEWER_STATE.EraseMap:
                        eraseMapState = PROCESS_ERASE_MAP_STATE.Erase;
                        break;
                }

                SetProperty<DIALOG_MAP_CREATOR_VIEWER_STATE>(ref this.viewerState, value);

                if (ViewerStateChanged != null)
                    this.ViewerStateChanged();
            }
        }

        public void ViewerStateChanged_Callback()
        {
            this.DisplayViewerState = this.ViewerState.ToString();
            if (this.ViewerState == DIALOG_MAP_CREATOR_VIEWER_STATE.Normal)
            {
                if (this.IsSelectChipChecked == true)
                {
                    this.IsSelectChipChecked = false;
                }
                if (this.IsSelectRoiChecked == true)
                {
                    this.IsSelectRoiChecked = false;
                }
                if (this.IsDrawChecked == true)
                {
                    this.IsDrawChecked = false;
                }
                if (this.IsEraseChecked == true)
                {
                    this.IsEraseChecked = false;
                }
            }
        }
        #endregion

        #region [Properties]
        private bool isSelectChipChecked = false;
        public bool IsSelectChipChecked
        {
            get => this.isSelectChipChecked;
            set
            {
                if (value == true)
                {
                    this.IsSelectRoiChecked = false;
                }
                else
                {
                    if (!p_UIElement.Contains(SelectChipBox_UI))
                    {
                        ClearObjects();
                    }
                    else
                    {
                        this.selectChipLeftTop.X = this.selectChipBox.Left;
                        this.selectChipLeftTop.Y = this.selectChipBox.Top;
                        RedrawShapes();
                    }
                }

                SetProperty<bool>(ref this.isSelectChipChecked, value);
            }
        }

        private bool isSelectRoiChecked = false;
        public bool IsSelectRoiChecked
        {
            get => this.isSelectRoiChecked;
            set
            {
                if (value == true)
                {
                    this.IsSelectChipChecked = false;
                }
                else
                {
                    if (!p_UIElement.Contains(SelectRoiBox_UI))
                    {
                        ClearObjects();
                    }
                    else
                    {
                        this.selectRoiLeftTop.X = this.selectRoiBox.Left;
                        this.selectRoiLeftTop.Y = this.selectRoiBox.Top;
                        RedrawShapes();
                    }
                }

                SetProperty<bool>(ref this.isSelectRoiChecked, value);
            }
        }

        private bool isDrawChecked = false;
        public bool IsDrawChecked
        {
            get => this.isDrawChecked;
            set
            {
                SetProperty<bool>(ref this.isDrawChecked, value);
            }
        }

        private bool isEraseChecked = false;
        public bool IsEraseChecked
        {
            get => this.isEraseChecked;
            set
            {
                SetProperty<bool>(ref this.isEraseChecked, value);
            }
        }

        private string displayViewerState = DIALOG_MAP_CREATOR_VIEWER_STATE.Normal.ToString();
        public string DisplayViewerState
        {
            get => this.displayViewerState;
            set
            {
                SetProperty<string>(ref this.displayViewerState, value);
            }
        }

        private bool isFindDone = false;
        public bool IsFindDone
        {
            get => this.isFindDone;
            set
            {
                SetProperty<bool>(ref this.isFindDone, value);
            }
        }

        private int mapWidth = 0;
        public int MapWidth
        {
            get => this.mapWidth;
            set
            {
                SetProperty<int>(ref this.mapWidth, value);
            }
        }

        private int mapHeight = 0;
        public int MapHeight
        {
            get => this.mapHeight;
            set
            {
                SetProperty<int>(ref this.mapHeight, value);
            }
        }

        private int[] searchedWaferMap = null;
        public int[] SearchedWaferMap
        {
            get => this.searchedWaferMap;
            set
            {
                SetProperty<int[]>(ref this.searchedWaferMap, value);
            }
        }
        #endregion

        #region [Command]
        public ICommand btnModeSelectChipCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (this.IsSelectChipChecked == true)
                    {
                        this.IsSelectRoiChecked = false;
                        this.ViewerState = DIALOG_MAP_CREATOR_VIEWER_STATE.SelectChip;
                        this.DisplayViewerState = this.ViewerState.ToString();

                        if (this.p_UIElement.Contains(SelectChipLeftTop_UI))
                        {
                            this.p_UIElement.Remove(SelectChipLeftTop_UI); 
                        }
                        if (this.p_UIElement.Contains(SelectChipRightBottom_UI))
                        {
                            this.p_UIElement.Remove(SelectChipRightBottom_UI); 
                        }
                        if (this.p_UIElement.Contains(SelectChipBox_UI))
                        {
                            this.p_UIElement.Remove(SelectChipBox_UI);
                        }
                    }
                    else
                    {
                        this.IsSelectRoiChecked = true;
                        this.ViewerState = DIALOG_MAP_CREATOR_VIEWER_STATE.Normal;
                        this.DisplayViewerState = this.ViewerState.ToString();
                    }
                });
            }
        }

        public ICommand btnModeSelectRoiCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (this.IsSelectRoiChecked == true)
                    {
                        this.IsSelectChipChecked = false;
                        this.ViewerState = DIALOG_MAP_CREATOR_VIEWER_STATE.SelectRoi;
                        this.DisplayViewerState = this.ViewerState.ToString();
                        
                        if (this.p_UIElement.Contains(SelectRoiLeftTop_UI))
                        {
                            this.p_UIElement.Remove(SelectRoiLeftTop_UI);
                        }
                        if (this.p_UIElement.Contains(SelectRoiRightBottom_UI))
                        {
                            this.p_UIElement.Remove(SelectRoiRightBottom_UI);
                        }
                        if (this.p_UIElement.Contains(SelectRoiBox_UI))
                        {
                            this.p_UIElement.Remove(SelectRoiBox_UI);
                        }
                    }
                    else
                    {
                        this.IsSelectChipChecked = true;
                        this.ViewerState = DIALOG_MAP_CREATOR_VIEWER_STATE.Normal;
                        this.DisplayViewerState = this.ViewerState.ToString();

                    }
                });
            }
        }

        public ICommand btnModeDrawCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (this.IsFindDone == true)
                    {
                        if (this.IsDrawChecked == true)
                        {
                            this.IsEraseChecked = false;
                            this.ViewerState = DIALOG_MAP_CREATOR_VIEWER_STATE.DrawMap;
                            this.DisplayViewerState = this.ViewerState.ToString();
                        }
                        else
                        {
                            this.IsEraseChecked = true;
                            this.ViewerState = DIALOG_MAP_CREATOR_VIEWER_STATE.EraseMap;
                            this.DisplayViewerState = this.ViewerState.ToString();
                        }
                    }
                });
            }
        }

        public ICommand btnModeEraseCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (this.IsFindDone == true)
                    {
                        if (this.IsEraseChecked == true)
                        {
                            this.IsDrawChecked = false;
                            this.ViewerState = DIALOG_MAP_CREATOR_VIEWER_STATE.EraseMap;
                            this.DisplayViewerState = this.ViewerState.ToString();
                        }
                        else
                        {
                            this.IsDrawChecked = true;
                            this.ViewerState = DIALOG_MAP_CREATOR_VIEWER_STATE.DrawMap;
                            this.DisplayViewerState = this.ViewerState.ToString();
                        }
                    }
                });
            }
        }

        public RelayCommand btnOpenCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this._openImage();
                });
            }
        }

        public RelayCommand btnSaveCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this._saveImage();
                });
            }
        }

        public RelayCommand btnClearCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this._clearImage();
                });
            }
        }

        public RelayCommand btnDrawClearCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.IsFindDone = false;
                    this.MapWidth = 0;
                    this.MapHeight = 0;
                    this.SearchedWaferMap = null;
                    rectList.Clear();
                    gridList.Clear();
                    searchedChipPoint.Clear();
                    ClearObjects();
                });
            }
        }
        #endregion

        #region [Draw 관련 멤버]

        Grid SelectChipLeftTop_UI = null;
        Grid SelectChipRightBottom_UI = null;
        Grid SelectChipBox_UI = null;

        Grid SelectRoiLeftTop_UI = null;
        Grid SelectRoiRightBottom_UI = null;
        Grid SelectRoiBox_UI = null;

        public CPoint selectChipLeftTop = new CPoint();
        public CPoint selectChipRightBottom = new CPoint();
        public CRect selectChipBox = new CRect();

        public CPoint selectRoiLeftTop = new CPoint();
        public CPoint selectRoiRightBottom = new CPoint();
        public CRect selectRoiBox = new CRect();

        public List<C3Point> searchedChipPoint = new List<C3Point>();
        TShape CurrentShape;
        List<TRect> rectList = new List<TRect>();
        List<Grid> gridList = new List<Grid>();

        public void InitializeUIElement()
        {
            SelectChipLeftTop_UI = new Grid();
            SelectChipLeftTop_UI.Children.Add(new Line());
            SelectChipLeftTop_UI.Children.Add(new Line());

            SelectChipRightBottom_UI = new Grid();
            SelectChipRightBottom_UI.Children.Add(new Line());
            SelectChipRightBottom_UI.Children.Add(new Line());

            SelectRoiLeftTop_UI = new Grid();

            Line line1 = new Line();
            line1.X1 = 0;
            line1.Y1 = -10;
            line1.X2 = 0;
            line1.Y2 = 40;
            line1.Stroke = DefineColors.SelectRoiColor;
            line1.StrokeThickness = 3;
            line1.Opacity = 1;

            Line line2 = new Line();
            line2.X1 = -40;
            line2.Y1 = 0;
            line2.X2 = 10;
            line2.Y2 = 0;
            line2.Stroke = DefineColors.SelectRoiColor;
            line2.StrokeThickness = 3;
            line2.Opacity = 1;

            SelectRoiLeftTop_UI.Children.Add(line1);
            SelectRoiLeftTop_UI.Children.Add(line2);

            SelectRoiRightBottom_UI = new Grid();

            Line line3 = new Line();
            line3.X1 = 0;
            line3.Y1 = -40;
            line3.X2 = 0;
            line3.Y2 = 10;
            line3.Stroke = DefineColors.SelectRoiColor;
            line3.StrokeThickness = 3;
            line3.Opacity = 1;

            Line line4 = new Line();
            line4.X1 = 10;
            line4.Y1 = 0;
            line4.X2 = -40;
            line4.Y2 = 0;
            line4.Stroke = DefineColors.SelectRoiColor;
            line4.StrokeThickness = 3;
            line4.Opacity = 1;

            SelectRoiRightBottom_UI.Children.Add(line3);
            SelectRoiRightBottom_UI.Children.Add(line4);

            SelectChipBox_UI = new Grid();
            SelectChipBox_UI.Children.Add(new Line()); // Left
            SelectChipBox_UI.Children.Add(new Line()); // Top
            SelectChipBox_UI.Children.Add(new Line()); // Right
            SelectChipBox_UI.Children.Add(new Line()); // Bottom

            SelectRoiBox_UI = new Grid();
            SelectRoiBox_UI.Children.Add(new Line()); // Left
            SelectRoiBox_UI.Children.Add(new Line()); // Top
            SelectRoiBox_UI.Children.Add(new Line()); // Right
            SelectRoiBox_UI.Children.Add(new Line()); // Bottom
        }
        #endregion

        #region [Mouse Event Overrides]
        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            base.PreviewMouseDown(sender, e);

            if (m_KeyEvent != null)
                if (m_KeyEvent.Key == Key.LeftShift && m_KeyEvent.IsDown)
                    return;

            switch (this.ViewerState)
            {
                case DIALOG_MAP_CREATOR_VIEWER_STATE.Normal:
                    ProcessNormal(e);
                    break;
                case DIALOG_MAP_CREATOR_VIEWER_STATE.SelectChip:
                    ProcessSelectChip(e);
                    break;
                case DIALOG_MAP_CREATOR_VIEWER_STATE.SelectRoi:
                    ProcessSelectRoi(e);
                    break;
                case DIALOG_MAP_CREATOR_VIEWER_STATE.DrawMap:
                    ProcessDrawMap(e);
                    break;
                case DIALOG_MAP_CREATOR_VIEWER_STATE.EraseMap:
                    ProcessEraseMap(e);
                    break;
            }
        }
        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);

            switch (this.ViewerState)
            {
                case DIALOG_MAP_CREATOR_VIEWER_STATE.Normal:
                    break;
                case DIALOG_MAP_CREATOR_VIEWER_STATE.SelectChip:
                    if (this.selectChipState == PROCESS_SELECT_CHIP_STATE.SelectChipRightBottom)
                    {
                        selectChipRightBottom.X = p_MouseMemX;
                        selectChipRightBottom.Y = p_MouseMemY;
                        DrawSelectChipRightBottomPoint(selectChipRightBottom);
                    }
                    else if (this.selectChipState == PROCESS_SELECT_CHIP_STATE.SelectChipLeftTop)
                    {
                        selectChipLeftTop.X = p_MouseMemX;
                        selectChipLeftTop.Y = p_MouseMemY;
                        DrawSelectChipLeftTopPoint(selectChipLeftTop);
                    }
                    break;
                case DIALOG_MAP_CREATOR_VIEWER_STATE.SelectRoi:
                    if (this.selectRoiState == PROCESS_SELECT_ROI_STATE.SelectRoiRightBottom)
                    {
                        selectRoiRightBottom.X = p_MouseMemX;
                        selectRoiRightBottom.Y = p_MouseMemY;
                        DrawSelectRoiRightBottomPoint(selectRoiRightBottom);
                    }
                    else if (this.selectRoiState == PROCESS_SELECT_ROI_STATE.SelectRoiLeftTop)
                    {
                        selectRoiLeftTop.X = p_MouseMemX;
                        selectRoiLeftTop.Y = p_MouseMemY;
                        DrawSelectRoiLeftTopPoint(selectRoiLeftTop);
                    }
                    break;
            }
        }
        public override void PreviewMouseUp(object sender, MouseEventArgs e)
        {

        }
        public override void SetRoiRect()
        {
            base.SetRoiRect();
            RedrawShapes();
        }
        public override void CanvasMovePoint_Ref(CPoint point, int nX, int nY)
        {
            base.CanvasMovePoint_Ref(point, nX, nY);
            RedrawShapes();
        }

        public override void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            base.MouseWheel(sender, e);

            RedrawShapes();
        }

        #endregion

        #region [Process Normal]
        public void ProcessNormal(MouseEventArgs e)
        {

        }
        #endregion

        #region [Process SelectChip]
        private enum PROCESS_SELECT_CHIP_STATE
        {
            None,
            SelectChipLeftTop,
            SelectChipRightBottom,
        }

        PROCESS_SELECT_CHIP_STATE selectChipState = PROCESS_SELECT_CHIP_STATE.None;

        public void ProcessSelectChip(MouseEventArgs e)
        {
            CPoint canvasPt = new CPoint(p_MouseX, p_MouseY);
            CPoint memPt = GetMemPoint(canvasPt);

            switch (selectChipState)
            {
                case PROCESS_SELECT_CHIP_STATE.None:
                    break;
                case PROCESS_SELECT_CHIP_STATE.SelectChipLeftTop:

                    //ClearObjects();
                    this.p_UIElement.Remove(SelectChipBox_UI);

                    if (this.SelectChipPointDone != null)
                        this.SelectChipPointDone();

                    p_Cursor = Cursors.Arrow;
                    selectChipLeftTop = memPt;
                    DrawSelectChipLeftTopPoint(selectChipLeftTop);
                    SetSelectChipPoint();

                    selectChipState = PROCESS_SELECT_CHIP_STATE.SelectChipRightBottom;
                    break;
                case PROCESS_SELECT_CHIP_STATE.SelectChipRightBottom:

                    p_Cursor = Cursors.Arrow;

                    if( (memPt.X - selectChipLeftTop.X) > 30000 || (selectChipLeftTop.Y - memPt.Y) > 30000)
                    {
                        MessageBox.Show("검사 영역의 크기는 높이 30000 또는 너비 30000을 넘을 수 없습니다.");
                        return;
                    }
                    
                    selectChipRightBottom.X = memPt.X;
                    selectChipRightBottom.Y = memPt.Y;

                    selectChipBox.Left = selectChipLeftTop.X;
                    selectChipBox.Right = selectChipRightBottom.X;
                    selectChipBox.Top = selectChipLeftTop.Y;
                    selectChipBox.Bottom = selectChipRightBottom.Y;

                    DrawSelectChipRightBottomPoint(selectChipRightBottom);
                    DrawSelectChipBox();
                    SetSelectChip();

                    selectChipState = PROCESS_SELECT_CHIP_STATE.None;
                    ViewerState = DIALOG_MAP_CREATOR_VIEWER_STATE.Normal;
                    break;
            }
        }
        #endregion

        #region [Process SelectRoi]
        private enum PROCESS_SELECT_ROI_STATE
        {
            None,
            SelectRoiLeftTop,
            SelectRoiRightBottom,
        }

        PROCESS_SELECT_ROI_STATE selectRoiState = PROCESS_SELECT_ROI_STATE.None;

        public void ProcessSelectRoi(MouseEventArgs e)
        {
            CPoint canvasPt = new CPoint(p_MouseX, p_MouseY);
            CPoint memPt = GetMemPoint(canvasPt);

            switch (selectRoiState)
            {
                case PROCESS_SELECT_ROI_STATE.None:
                    break;
                case PROCESS_SELECT_ROI_STATE.SelectRoiLeftTop:

                    //ClearObjects();
                    this.p_UIElement.Remove(SelectRoiBox_UI);

                    if (this.SelectRoiPointDone != null)
                        this.SelectRoiPointDone();

                    p_Cursor = Cursors.Arrow;
                    selectRoiLeftTop = memPt;
                    DrawSelectRoiLeftTopPoint(selectRoiLeftTop);
                    SetSelectRoiPoint();

                    selectRoiState = PROCESS_SELECT_ROI_STATE.SelectRoiRightBottom;
                    break;
                case PROCESS_SELECT_ROI_STATE.SelectRoiRightBottom:

                    p_Cursor = Cursors.Arrow;

                    if ((memPt.X - selectRoiLeftTop.X) > 30000 || (selectRoiLeftTop.Y - memPt.Y) > 30000)
                    {
                        MessageBox.Show("검사 영역의 크기는 높이 30000 또는 너비 30000을 넘을 수 없습니다.");
                        return;
                    }

                    selectRoiRightBottom.X = memPt.X;
                    selectRoiRightBottom.Y = memPt.Y;

                    selectRoiBox.Left = selectRoiLeftTop.X;
                    selectRoiBox.Right = selectRoiRightBottom.X;
                    selectRoiBox.Top = selectRoiLeftTop.Y;
                    selectRoiBox.Bottom = selectRoiRightBottom.Y;

                    DrawSelectRoiLeftTopPoint(selectRoiLeftTop);
                    DrawSelectRoiRightBottomPoint(selectRoiRightBottom);
                    DrawSelectRoiBox();
                    SetSelectRoi();

                    selectRoiState = PROCESS_SELECT_ROI_STATE.None;
                    ViewerState = DIALOG_MAP_CREATOR_VIEWER_STATE.Normal;
                    break;
            }
        }
        #endregion

        #region [Process DrawMap]
        private enum PROCESS_DRAW_MAP_STATE
        {
            None,
            Draw,
        }

        PROCESS_DRAW_MAP_STATE drawMapState = PROCESS_DRAW_MAP_STATE.None;

        public void ProcessDrawMap(MouseEventArgs e)
        {
            CPoint canvasPt = new CPoint(p_MouseX, p_MouseY);
            CPoint memPt = GetMemPoint(canvasPt);

            switch (drawMapState)
            {
                case PROCESS_DRAW_MAP_STATE.None:
                    break;
                case PROCESS_DRAW_MAP_STATE.Draw:
                    foreach (var item in rectList)
                    {
                        if (memPt.X >= item.MemoryRect.Left && memPt.X <= item.MemoryRect.Right && memPt.Y >= item.MemoryRect.Top && memPt.Y <= item.MemoryRect.Bottom)
                        {
                            int idx = rectList.IndexOf(item);
                            if (this.SearchedWaferMap[idx] == 0 && searchedChipPoint[idx].Z > 0)
                            {
                                this.SearchedWaferMap[idx] = 1;
                                searchedChipPoint[idx].Z = searchedChipPoint[idx].Z - 100;
                                DrawSearchedChipBox(searchedChipPoint[idx]);

                                RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeFront>().WaferMap;
                                Array.Copy(this.SearchedWaferMap, this.SearchedWaferMap, this.MapWidth * this.MapHeight);
                                waferMap.CreateWaferMap(this.MapWidth, this.MapHeight, this.SearchedWaferMap);
                            }
                            break;
                        }
                    }
                    break;
            }
        }
        #endregion

        #region [Process EraseMap]
        private enum PROCESS_ERASE_MAP_STATE
        {
            None,
            Erase,
        }

        PROCESS_ERASE_MAP_STATE eraseMapState = PROCESS_ERASE_MAP_STATE.None;

        public void ProcessEraseMap(MouseEventArgs e)
        {
            CPoint canvasPt = new CPoint(p_MouseX, p_MouseY);
            CPoint memPt = GetMemPoint(canvasPt);

            switch (eraseMapState)
            {
                case PROCESS_ERASE_MAP_STATE.None:
                    break;
                case PROCESS_ERASE_MAP_STATE.Erase:
                    foreach (var item in rectList)
                    {
                        if (memPt.X >= item.MemoryRect.Left && memPt.X <= item.MemoryRect.Right && memPt.Y >= item.MemoryRect.Top && memPt.Y <= item.MemoryRect.Bottom)
                        {
                            int idx = rectList.IndexOf(item);
                            int sumRow = 0;
                            int sumCol = 0;

                            if (this.SearchedWaferMap[idx] == 1)
                            {
                                this.SearchedWaferMap[idx] = 0;
                                searchedChipPoint[idx].Z = searchedChipPoint[idx].Z + 100;
                                DrawSearchedChipBox(searchedChipPoint[idx]);

                                // Check empty column
                                for (int j = 0; j < this.MapWidth; j++)
                                {
                                    for (int i = 0; i < this.MapHeight; i++)
                                    {
                                        sumCol = sumCol + this.SearchedWaferMap[this.MapWidth * i + j];
                                    }
                                    if (sumCol == 0) // Clear column
                                    {
                                        for (int i = 0; i < this.MapHeight; i++)
                                        {
                                            int removeIdx = this.MapWidth * i + j - i;

                                            this.SearchedWaferMap = this.SearchedWaferMap.Where((source, index) => index != removeIdx).ToArray();
                                            this.searchedChipPoint.RemoveAt(removeIdx);
                                            p_UIElement.Remove(rectList[removeIdx].CanvasRect);
                                            p_UIElement.Remove(gridList[removeIdx]);
                                            this.rectList.RemoveAt(removeIdx);
                                            this.gridList.RemoveAt(removeIdx);
                                        }
                                        this.MapWidth = this.MapWidth - 1;
                                    }
                                    else
                                    {
                                        sumCol = 0;
                                    }
                                }

                                // Check empty row
                                for (int j = 0; j < this.MapHeight; j++)
                                {
                                    for (int i = 0; i < this.MapWidth; i++)
                                    {
                                        sumRow = sumRow + this.SearchedWaferMap[this.MapWidth * j + i];
                                    }
                                    if (sumRow == 0) // Clear row
                                    {
                                        for (int i = 0; i < this.MapWidth; i++)
                                        {
                                            int removeIdx = this.MapWidth * j;

                                            this.SearchedWaferMap = this.SearchedWaferMap.Where((source, index) => index != removeIdx).ToArray();
                                            this.searchedChipPoint.RemoveAt(removeIdx);
                                            p_UIElement.Remove(rectList[removeIdx].CanvasRect);
                                            p_UIElement.Remove(gridList[removeIdx]);
                                            this.rectList.RemoveAt(removeIdx);
                                            this.gridList.RemoveAt(removeIdx);
                                        }
                                        this.MapHeight = this.MapHeight - 1;
                                    }
                                    else
                                    {
                                        sumRow = 0;
                                    }
                                }

                                RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeFront>().WaferMap;
                                Array.Copy(this.SearchedWaferMap, this.SearchedWaferMap, this.MapWidth * this.MapHeight);
                                waferMap.CreateWaferMap(this.MapWidth, this.MapHeight, this.SearchedWaferMap);
                            }
                            break;
                        }
                    }
                    break;
            }
        }
        #endregion

        #region [Draw Method]
        private void DrawSelectChipLeftTopPoint(CPoint memPt, bool bRecipeLoaded = false)
        {
            if (memPt.X == 0 || memPt.Y == 0)
                return;

            CPoint viewPt = memPt;
            CPoint canvasPt = GetCanvasPoint(viewPt);

            SelectChipLeftTop_UI.Width = 40;
            SelectChipLeftTop_UI.Height = 40;

            Line line1 = SelectChipLeftTop_UI.Children[0] as Line;
            line1.X1 = 0;
            line1.Y1 = 40;
            line1.X2 = 0;
            line1.Y2 = -10;
            line1.Stroke = DefineColors.SelectChipColor;
            line1.StrokeThickness = 3;
            line1.Opacity = 1;

            Line line2 = SelectChipLeftTop_UI.Children[1] as Line;
            line2.X1 = -10;
            line2.Y1 = 0;
            line2.X2 = 40;
            line2.Y2 = 0;
            line2.Stroke = DefineColors.SelectChipColor;
            line2.StrokeThickness = 3;
            line2.Opacity = 1;

            Canvas.SetLeft(SelectChipLeftTop_UI, canvasPt.X);
            Canvas.SetTop(SelectChipLeftTop_UI, canvasPt.Y);

            if (!p_UIElement.Contains(SelectChipLeftTop_UI))
            {
                p_UIElement.Add(SelectChipLeftTop_UI);
            }
        }

        private void DrawSelectChipRightBottomPoint(CPoint memPt, bool bRecipeLoaded = false)
        {
            if (memPt.X == 0 || memPt.Y == 0)
                return;

            CPoint viewPt = memPt;
            CPoint canvasPt = GetCanvasPoint(viewPt);

            SelectChipRightBottom_UI.Width = 40;
            SelectChipRightBottom_UI.Height = 40;

            Line line1 = SelectChipRightBottom_UI.Children[0] as Line;
            line1.X1 = 0;
            line1.Y1 = 10;
            line1.X2 = 0;
            line1.Y2 = -40;
            line1.Stroke = DefineColors.SelectChipColor;
            line1.StrokeThickness = 3;
            line1.Opacity = 1;

            Line line2 = SelectChipRightBottom_UI.Children[1] as Line;
            line2.X1 = 10;
            line2.Y1 = 0;
            line2.X2 = -40;
            line2.Y2 = 0;
            line2.Stroke = DefineColors.SelectChipColor;
            line2.StrokeThickness = 3;
            line2.Opacity = 1;

            Canvas.SetLeft(SelectChipRightBottom_UI, canvasPt.X);
            Canvas.SetTop(SelectChipRightBottom_UI, canvasPt.Y);

            if (!p_UIElement.Contains(SelectChipRightBottom_UI))
            {
                p_UIElement.Add(SelectChipRightBottom_UI);
            }
        }

        private void DrawSelectRoiLeftTopPoint(CPoint memPt, bool bRecipeLoaded = false)
        {
            if (memPt.X == 0 || memPt.Y == 0)
                return;

            CPoint viewPt = memPt;
            CPoint canvasPt = GetCanvasPoint(viewPt);

            SelectRoiLeftTop_UI.Width = 40;
            SelectRoiLeftTop_UI.Height = 40;

            Line line1 = SelectRoiLeftTop_UI.Children[0] as Line;
            line1.X1 = 0;
            line1.Y1 = 40;
            line1.X2 = 0;
            line1.Y2 = -10;
            line1.Stroke = DefineColors.SelectRoiColor;
            line1.StrokeThickness = 3;
            line1.Opacity = 1;

            Line line2 = SelectRoiLeftTop_UI.Children[1] as Line;
            line2.X1 = -10;
            line2.Y1 = 0;
            line2.X2 = 40;
            line2.Y2 = 0;
            line2.Stroke = DefineColors.SelectRoiColor;
            line2.StrokeThickness = 3;
            line2.Opacity = 1;

            Canvas.SetLeft(SelectRoiLeftTop_UI, canvasPt.X);
            Canvas.SetTop(SelectRoiLeftTop_UI, canvasPt.Y);

            if (!p_UIElement.Contains(SelectRoiLeftTop_UI))
            {
                p_UIElement.Add(SelectRoiLeftTop_UI);
            }
        }

        private void DrawSelectRoiRightBottomPoint(CPoint memPt, bool bRecipeLoaded = false)
        {
            if (memPt.X == 0 || memPt.Y == 0)
                return;

            CPoint viewPt = memPt;
            CPoint canvasPt = GetCanvasPoint(viewPt);

            SelectRoiRightBottom_UI.Width = 40;
            SelectRoiRightBottom_UI.Height = 40;

            Line line1 = SelectRoiRightBottom_UI.Children[0] as Line;
            line1.X1 = 0;
            line1.Y1 = 10;
            line1.X2 = 0;
            line1.Y2 = -40;
            line1.Stroke = DefineColors.SelectRoiColor;
            line1.StrokeThickness = 3;
            line1.Opacity = 1;

            Line line2 = SelectRoiRightBottom_UI.Children[1] as Line;
            line2.X1 = 10;
            line2.Y1 = 0;
            line2.X2 = -40;
            line2.Y2 = 0;
            line2.Stroke = DefineColors.SelectRoiColor;
            line2.StrokeThickness = 3;
            line2.Opacity = 1;

            Canvas.SetLeft(SelectRoiRightBottom_UI, canvasPt.X);
            Canvas.SetTop(SelectRoiRightBottom_UI, canvasPt.Y);

            if (!p_UIElement.Contains(SelectRoiRightBottom_UI))
            {
                p_UIElement.Add(SelectRoiRightBottom_UI);
            }
        }

        private void DrawSelectChipBox()
        {
            int left = this.selectChipBox.Left;
            int top = this.selectChipBox.Top;

            int right = this.selectChipBox.Right;
            int bottom = this.selectChipBox.Bottom;
            
            CPoint canvasLeftTop = GetCanvasPoint(new CPoint(left, top));
            CPoint canvasLeftBottom = GetCanvasPoint(new CPoint(left, bottom));
            CPoint canvasRightTop = GetCanvasPoint(new CPoint(right, top));
            CPoint canvasRightBottom = GetCanvasPoint(new CPoint(right, bottom));

            int offset = 40; // OriginPoint Line 길이

            SelectChipBox_UI.Width = Math.Abs(canvasRightTop.X - canvasLeftTop.X);
            SelectChipBox_UI.Height = Math.Abs(canvasLeftBottom.Y - canvasLeftTop.Y);

            // Left
            Line leftLine = SelectChipBox_UI.Children[0] as Line;
            leftLine.X1 = 0;
            leftLine.Y1 = offset;
            leftLine.X2 = 0;
            leftLine.Y2 = SelectChipBox_UI.Height;
            leftLine.Stroke = DefineColors.SelectChipBoxColor;
            leftLine.StrokeThickness = 1;
            leftLine.Opacity = 0.75;
            leftLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

            // Top
            Line topLine = SelectChipBox_UI.Children[1] as Line;
            topLine.X1 = offset;
            topLine.Y1 = 0;
            topLine.X2 = SelectChipBox_UI.Width;
            topLine.Y2 = 0;
            topLine.Stroke = DefineColors.SelectChipBoxColor;
            topLine.StrokeThickness = 1;
            topLine.Opacity = 0.75;
            topLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

            // Right
            Line rightLine = SelectChipBox_UI.Children[2] as Line;
            rightLine.X1 = SelectChipBox_UI.Width;
            rightLine.Y1 = 0;
            rightLine.X2 = SelectChipBox_UI.Width;
            rightLine.Y2 = SelectChipBox_UI.Height - offset;
            rightLine.Stroke = DefineColors.SelectChipBoxColor;
            rightLine.StrokeThickness = 1;
            rightLine.Opacity = 0.75;
            rightLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

            // bottom
            Line bottomLine = SelectChipBox_UI.Children[3] as Line;
            bottomLine.X1 = 0;
            bottomLine.Y1 = SelectChipBox_UI.Height;
            bottomLine.X2 = SelectChipBox_UI.Width - offset;
            bottomLine.Y2 = SelectChipBox_UI.Height;
            bottomLine.Stroke = DefineColors.SelectChipBoxColor;
            bottomLine.StrokeThickness = 1;
            bottomLine.Opacity = 0.75;
            bottomLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

            Canvas.SetLeft(SelectChipBox_UI, canvasLeftTop.X);
            Canvas.SetTop(SelectChipBox_UI, canvasLeftTop.Y);
            Canvas.SetRight(SelectChipBox_UI, canvasRightBottom.X);
            Canvas.SetBottom(SelectChipBox_UI, canvasRightBottom.Y);

            if (!p_UIElement.Contains(SelectChipBox_UI))
            {
                p_UIElement.Add(SelectChipBox_UI);
            }
        }

        private void DrawSelectRoiBox()
        {
            int left = this.selectRoiBox.Left;
            int top = this.selectRoiBox.Top;

            int right = this.selectRoiBox.Right;
            int bottom = this.selectRoiBox.Bottom;

            CPoint canvasLeftTop = GetCanvasPoint(new CPoint(left, top));
            CPoint canvasLeftBottom = GetCanvasPoint(new CPoint(left, bottom));
            CPoint canvasRightTop = GetCanvasPoint(new CPoint(right, top));
            CPoint canvasRightBottom = GetCanvasPoint(new CPoint(right, bottom));

            int offset = 40; // OriginPoint Line 길이

            SelectRoiBox_UI.Width = Math.Abs(canvasRightTop.X - canvasLeftTop.X);
            SelectRoiBox_UI.Height = Math.Abs(canvasLeftBottom.Y - canvasLeftTop.Y);

            // Left
            Line leftLine = SelectRoiBox_UI.Children[0] as Line;
            leftLine.X1 = 0;
            leftLine.Y1 = offset;
            leftLine.X2 = 0;
            leftLine.Y2 = SelectRoiBox_UI.Height;
            leftLine.Stroke = DefineColors.SelectRoiBoxColor;
            leftLine.StrokeThickness = 1;
            leftLine.Opacity = 0.75;
            leftLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

            // Top
            Line topLine = SelectRoiBox_UI.Children[1] as Line;
            topLine.X1 = offset;
            topLine.Y1 = 0;
            topLine.X2 = SelectRoiBox_UI.Width;
            topLine.Y2 = 0;
            topLine.Stroke = DefineColors.SelectRoiBoxColor;
            topLine.StrokeThickness = 1;
            topLine.Opacity = 0.75;
            topLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

            // Right
            Line rightLine = SelectRoiBox_UI.Children[2] as Line;
            rightLine.X1 = SelectRoiBox_UI.Width;
            rightLine.Y1 = 0;
            rightLine.X2 = SelectRoiBox_UI.Width;
            rightLine.Y2 = SelectRoiBox_UI.Height - offset;
            rightLine.Stroke = DefineColors.SelectRoiBoxColor;
            rightLine.StrokeThickness = 1;
            rightLine.Opacity = 0.75;
            rightLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

            // bottom
            Line bottomLine = SelectRoiBox_UI.Children[3] as Line;
            bottomLine.X1 = 0;
            bottomLine.Y1 = SelectRoiBox_UI.Height;
            bottomLine.X2 = SelectRoiBox_UI.Width - offset;
            bottomLine.Y2 = SelectRoiBox_UI.Height;
            bottomLine.Stroke = DefineColors.SelectRoiBoxColor;
            bottomLine.StrokeThickness = 1;
            bottomLine.Opacity = 0.75;
            bottomLine.StrokeDashArray = new DoubleCollection() { 6, 2 };

            Canvas.SetLeft(SelectRoiBox_UI, canvasLeftTop.X);
            Canvas.SetTop(SelectRoiBox_UI, canvasLeftTop.Y);
            Canvas.SetRight(SelectRoiBox_UI, canvasRightBottom.X);
            Canvas.SetBottom(SelectRoiBox_UI, canvasRightBottom.Y);

            if (!p_UIElement.Contains(SelectRoiBox_UI))
            {
                p_UIElement.Add(SelectRoiBox_UI);
            }
        }

        public void DrawSearchedChipBox(C3Point chip)
        {
            if (chip.Z == 0 || chip.Z > 100) // Under similarity score or Erased
            {
                CurrentShape = new TRect(Brushes.Blue, 2, 0.5);
            }
            else
            {
                CurrentShape = new TRect(Brushes.Red, 2, 0.5);
            }
            TRect rect = CurrentShape as TRect;

            rect.MemPointBuffer = new CPoint(chip.X, chip.Y);
            rect.MemoryRect.Left = chip.X;
            rect.MemoryRect.Top = chip.Y;
            rect.MemoryRect.Right = rect.MemoryRect.Left + this.selectChipBox.Width;
            rect.MemoryRect.Bottom = rect.MemoryRect.Top + this.selectChipBox.Height;
            rect.MemoryRect.Width = this.selectChipBox.Width;
            rect.MemoryRect.Height = this.selectChipBox.Height;

            Grid grid = new Grid();
            TextBlock tb = new TextBlock();
            tb.FontWeight = FontWeights.UltraBold;
            tb.TextAlignment = TextAlignment.Center;

            if (chip.Z == 0) // Under similarity score
            {
                tb.Foreground = Brushes.Blue;
                tb.Text = "";
            }
            else if (chip.Z > 100) // Erased
            {
                tb.Foreground = Brushes.Blue;
                tb.Text = string.Format("{0}", chip.Z - 100);
            }
            else
            {
                tb.Foreground = Brushes.Red;
                tb.Text = string.Format("{0}", chip.Z);
            }

            double pixSizeX = (double)p_CanvasWidth / (double)p_View_Rect.Width;
            double pixSizeY = (double)p_CanvasHeight / (double)p_View_Rect.Height;

            CPoint LT = new CPoint(rect.MemoryRect.Left, rect.MemoryRect.Top);
            CPoint RB = new CPoint(rect.MemoryRect.Right, rect.MemoryRect.Bottom);

            CPoint canvasLT = new CPoint(GetCanvasPoint(LT));
            CPoint canvasRB = new CPoint(GetCanvasPoint(RB));
            int width = Math.Abs(canvasRB.X - canvasLT.X);
            int height = Math.Abs(canvasRB.Y - canvasLT.Y);
            rect.CanvasRect.Width = width + pixSizeX;
            rect.CanvasRect.Height = height + pixSizeY;

            tb.Width = width + pixSizeX;
            tb.Height = height + pixSizeY;
            tb.FontSize = (int)(0.5 * Math.Min(tb.Width, tb.Height));
            tb.Padding = new Thickness(0, (int)((tb.Height - tb.FontSize) / 2), 0, 0);

            Canvas.SetLeft(rect.CanvasRect, canvasLT.X - pixSizeX / 2);
            Canvas.SetTop(rect.CanvasRect, canvasLT.Y - pixSizeY / 2);
            Canvas.SetRight(rect.CanvasRect, canvasRB.X);
            Canvas.SetBottom(rect.CanvasRect, canvasRB.Y);

            Canvas.SetLeft(grid, canvasLT.X - pixSizeX / 2);
            Canvas.SetTop(grid, canvasLT.Y - pixSizeY / 2);

            if (!p_UIElement.Contains(rect.CanvasRect))
            {
                p_UIElement.Add(rect.CanvasRect);
            }

            grid.Children.Add(tb);
            if (!p_UIElement.Contains(grid))
            {
                p_UIElement.Add(grid);
            }

            bool isNew = true;
            foreach (var item in rectList)
            {
                int idx = rectList.IndexOf(item);

                if (item.MemPointBuffer == rect.MemPointBuffer)
                {
                    p_UIElement.Remove(rectList[idx].CanvasRect);
                    p_UIElement.Remove(gridList[idx]);
                    rectList[idx] = rect;
                    gridList[idx] = grid;
                    isNew = false;
                    break;
                }
            }
            if (isNew == true)
            {
                rectList.Add(rect);
                gridList.Add(grid);
            }
        }

        public void RedrawShapes()
        {
            if (p_UIElement.Contains(SelectChipLeftTop_UI))
            {
                DrawSelectChipLeftTopPoint(selectChipLeftTop);
            }
            if (p_UIElement.Contains(SelectChipRightBottom_UI))
            {
                DrawSelectChipRightBottomPoint(selectChipRightBottom);
            }
            if (p_UIElement.Contains(SelectRoiLeftTop_UI))
            {
                DrawSelectRoiLeftTopPoint(selectRoiLeftTop);
            }
            if (p_UIElement.Contains(SelectRoiRightBottom_UI))
            {
                DrawSelectRoiRightBottomPoint(selectRoiRightBottom);
            }

            if (p_UIElement.Contains(SelectChipBox_UI))
            {
                DrawSelectChipBox();
            }
            if (p_UIElement.Contains(SelectRoiBox_UI))
            {
                DrawSelectRoiBox();
            }

            foreach (var item in rectList)
            {
                if (p_UIElement.Contains(item.CanvasRect))
                {
                    p_UIElement.Remove(item.CanvasRect);
                }
            }
            rectList.Clear();

            foreach (var item in gridList)
            {
                if (p_UIElement.Contains(item))
                {
                    p_UIElement.Remove(item);
                }
            }
            gridList.Clear();

            foreach (var item in searchedChipPoint)
            {
                DrawSearchedChipBox(item);
            }
        }
        #endregion

        #region [Viewer Method]
        public void DisplayFull()
        {
            this.p_Zoom = 1;
        }

        public void DisplayBox()
        {
            if(p_UIElement.Contains(SelectChipBox_UI))
            {
                int left = this.selectChipLeftTop.X;
                int top = this.selectChipLeftTop.Y;

                int right = this.selectChipRightBottom.X;
                int bottom = this.selectChipRightBottom.Y;

                int width = right - left;
                int height = bottom - top;

                double full_ratio = 1;
                double ratio = 1;

                if (this.p_CanvasHeight > this.p_CanvasWidth)
                {
                    full_ratio = full_ratio = (double)this.p_ImageData.p_Size.Y / (double)this.p_CanvasHeight;
                }
                else
                {
                    full_ratio = full_ratio = (double)this.p_ImageData.p_Size.X / (double)this.p_CanvasWidth;
                }

                double canvas_w_h_ratio = (double)(this.p_CanvasHeight) / (double)(p_CanvasWidth); // 가로가 더 길 경우 1 이하
                double box_w_h_ratio = (double)height / (double)width;

                if (box_w_h_ratio > canvas_w_h_ratio) // Canvas보다 가로 비율이 더 높을 경우,  box의 세로에 맞춰야함.
                {
                    ratio = (double)height / (double)this.p_CanvasHeight;
                }
                else
                {
                    ratio = (double)width / (double)this.p_CanvasWidth;
                }

                this.p_Zoom = ratio / full_ratio;

                this.p_View_Rect = new System.Drawing.Rectangle(new System.Drawing.Point(left, top), new System.Drawing.Size(width, height));

                this.SetRoiRect();

            }
            else
            {
                MessageBox.Show("Chip 영역이 설정되지 않았습니다");
            }
        }

        /// <summary>
        /// 양방향에서 그려진 영역을 Clear할 수 있으므로
        /// Parent에서 Clear를 요청한 경우 BoxReset을 발생 시키지 않는다.
        /// </summary>
        /// <param name="isFromParent"></param>
        public void ClearObjects(bool isFromParent = false)
        {
            this.p_UIElement.Clear();

            this.selectChipState = PROCESS_SELECT_CHIP_STATE.None;
            this.selectRoiState = PROCESS_SELECT_ROI_STATE.None;
            this.drawMapState = PROCESS_DRAW_MAP_STATE.None;
            this.eraseMapState = PROCESS_ERASE_MAP_STATE.None;

            if (this.SelectChipBoxReset != null && isFromParent == false)
                this.SelectChipBoxReset();
            if (this.SelectRoiBoxReset != null && isFromParent == false)
                this.SelectRoiBoxReset();
        }

        public void SetSelectChipPoint()
        {
            if (this.SelectChipPointDone != null)
                this.SelectChipPointDone();
        }

        public void SetSelectChip()
        {
            if (this.SelectChipBoxDone != null)
                this.SelectChipBoxDone();
        }

        public void SetSelectRoiPoint()
        {
            if (this.SelectRoiPointDone != null)
                this.SelectRoiPointDone();
        }

        public void SetSelectRoi()
        {
            if (this.SelectRoiBoxDone != null)
                this.SelectRoiBoxDone();
        }
        #endregion

    }
}
