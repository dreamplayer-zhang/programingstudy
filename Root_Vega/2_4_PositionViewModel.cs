using RootTools;
using RootTools.Inspects;
using RootTools.Memory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Root_Vega
{
    class _2_4_PositionViewModel : ObservableObject
    {
        Vega_Engineer m_Engineer;
        MemoryTool m_MemoryModule;
        ImageData m_Image;
      
        Recipe m_Recipe;
        Roi m_Roi;
        Stack<Feature> m_PasteFeature = new Stack<Feature>();

        string sPool = "pool";
        string sGroup = "group";
        string sMem = "mem";

        public _2_4_PositionViewModel(Vega_Engineer engineer, IDialogService dialogService)
        {
            m_Engineer = engineer;
            Init(engineer, dialogService);
            //p_ImageViewer.m_AfterLoaded += Redraw;
        }
        void Init(Vega_Engineer engineer, IDialogService dialogService)
        {
            m_Recipe = engineer.m_recipe;
            m_Roi = new Roi("Position", Roi.Item.Position);

            m_MemoryModule = engineer.ClassMemoryTool();
            //m_MemoryModule.GetPool(sPool).p_gbPool = 3;
            //m_MemoryModule.GetPool(sPool).GetGroup(sGroup).CreateMemory(sMem, 1, 1, new CPoint(MemWidth, MemHeight));
            //m_MemoryModule.GetPool(sPool).GetGroup(sGroup).GetMemory(sMem);

            m_Image = new ImageData(m_MemoryModule.GetMemory(sPool, sGroup, sMem));
            p_ImageViewer = new ImageViewer_ViewModel(m_Image, dialogService);
            p_SimpleShapeDrawer = new PositionDrawerVM(p_ImageViewer);
            p_SimpleShapeDrawer.RectangleKeyValue = Key.D1;

            p_ImageViewer.SetDrawer((DrawToolVM)p_SimpleShapeDrawer);

            p_SimpleShapeDrawer.SetStateDelegate += SetState;
        }
        //int aa = 0;
        #region Property
        
        private ImageViewer_ViewModel m_ImageViewer;
        public ImageViewer_ViewModel p_ImageViewer
        {
            get
            {
                return m_ImageViewer;
            }
            set
            {
                SetProperty(ref m_ImageViewer, value);
            }
        }

        private System.Windows.Input.Cursor _Cursor;
        public System.Windows.Input.Cursor p_Cursor
        {
            get
            {
                return _Cursor;
            }
            set
            {
                SetProperty(ref _Cursor, value);
            }
        }

        private bool _feature_isChecked = false;
        public bool p_Feature_isChecked
        {
            get
            {
                return _feature_isChecked;
            }
            set
            {
                SetProperty(ref _feature_isChecked, value);
                p_SimpleShapeDrawer.p_Feature_isChecked = value;
                CheckFeatureState();

            }
        }

        private int m_FeatureCnt;
        public int p_FeatureCnt
        {
            get
            {
                return m_FeatureCnt;
            }
            set
            {
                SetProperty(ref m_FeatureCnt, value);
            }
        }

        private KeyEventArgs _keyEvent;
        public KeyEventArgs KeyEvent
        {
            get
            {
                return _keyEvent;
            }
            set
            {
                SetProperty(ref _keyEvent, value);
            }
        }

        private PositionDrawerVM m_SimpleShapeDrawer;
        public PositionDrawerVM p_SimpleShapeDrawer
        {
            get
            {
                return m_SimpleShapeDrawer;
            }
            set
            {
                SetProperty(ref m_SimpleShapeDrawer, value);
            }
        }
        #endregion

        #region Command

        public ICommand btnDone
        {
            get
            {
                return new RelayCommand(_btnDone);
            }
        }
        void _btnDone()
        {
            foreach (UIElement element in p_SimpleShapeDrawer.m_Element)
            {
                ((Shape)element).Stroke = System.Windows.Media.Brushes.Yellow;
                //p_SimpleShapeDrawer_1.p_DoneShape.Add((Shape)element); 고민1
                //고민1
                //색깔로만 변경 되게 해야하나? 
                //아니면 p_DoneShape에 넣어서 List로써 관리 해야하나?
                //색깔이 고정된다면 애초에 p_Element에서 해당 색으로 List를 추릴수 있는데,
                //추후 이 Rect 4개가 어느 정도 수준까지 고려되야하는지에 따라서 고민.
                //객체로 관리 -> 추후 관리 편함
                //색깔로만 관리 -> Ctrl Z, Y에 적용 가능.

            }
            p_ImageViewer.SetImageSource();
        }

        public ICommand btnClear
        {
            get
            {
                return new RelayCommand(_btnClear);
            }
        }
        void _btnClear()
        {

            p_SimpleShapeDrawer.Clear();
            p_ImageViewer.m_HistoryWorker.Clear();
            p_ImageViewer.m_BasicTool.Clear();
            p_ImageViewer.SetImageSource();
            p_FeatureCnt = 0;
        }
        #endregion

        private bool CheckFeatureState()
        {
            //if (p_SimpleShapeDrawer_1.Counter < 4 && p_Feature_isChecked)
            //{
            //    p_ImageViewer_1.MouseCursor = Cursors.Pen;
            //    return true;
            //}
            //else
            //    p_ImageViewer_1.MouseCursor = Cursors.Arrow;
            return false;
        }

        private void SetState(bool _State, int _Count)
        {
            p_FeatureCnt = _Count;
            if (_State && p_Feature_isChecked)
                p_ImageViewer.p_MouseCursor = Cursors.Pen;
            else
                p_ImageViewer.p_MouseCursor = Cursors.Arrow;
        }


        //PositionMode m_PositionMode = PositionMode.None;
        PositionProgress m_PositionProgress = PositionProgress.None;
        enum PositionMode
        {
            None,
            Feature,
            EdgeBox,
        }
        enum PositionProgress
        {
            None,
            Start,
            Drawing,
            Done,
        }

    }
}
