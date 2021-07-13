using RootTools;
using RootTools.Memory;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Root_WIND2
{
    public struct InfoTextBolck
    {
        public InfoTextBolck(Grid grid, TRect pos)
        {
            this.grid = grid;
            this.pos = pos;
        }
        public Grid grid;
        public TRect pos;
    }

    public delegate void EventDrawDone(CPoint leftTop, CPoint rightBottom); //임시

    public class FrontsideInspection_ImageViewer_ViewModel : RootViewer_ViewModel
    {
        public event EventDrawDone DrawDone;

        public FrontsideInspection_ImageViewer_ViewModel()
        {
            if (GlobalObjects.Instance.GetNamed<ImageData>("FrontImage").GetPtr() == IntPtr.Zero && GlobalObjects.Instance.GetNamed<ImageData>("FrontImage").m_eMode != ImageData.eMode.OtherPCMem)
                return;

            base.init(GlobalObjects.Instance.GetNamed<ImageData>("FrontImage"), GlobalObjects.Instance.Get<DialogService>());
            p_VisibleMenu = Visibility.Visible;
            //Shapes.CollectionChanged += Shapes_CollectionChanged;
            //InfoTextBolcks.CollectionChanged += Texts_CollectionChanged;
        }

        private void Shapes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var shapes = sender as ObservableCollection<TShape>;
            foreach (TShape shape in shapes)
            {
                if (!p_DrawElement.Contains(shape.UIElement))
                    p_DrawElement.Add(shape.UIElement);
            }   
        }
        private void Texts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var infoTexts = sender as ObservableCollection<InfoTextBolck>;
            foreach (InfoTextBolck text in infoTexts)
            {
                if (!p_DrawElement.Contains(text.grid))
                    p_DrawElement.Add(text.grid);
            }
        }
        
        public ObservableCollection<TShape> Shapes = new ObservableCollection<TShape>();
        public ObservableCollection<InfoTextBolck> InfoTextBolcks = new ObservableCollection<InfoTextBolck>();
        TShape rectInfo;

        public enum ColorType
        {
            MasterFeature,
            ShotFeature,
            ChipFeature,
            FeatureMatching,
            FeatureMatchingFail,
            Defect,

        }


        #region Command
        public void DrawRect(CPoint LT, CPoint RB, ColorType color, String text = null, int FontSz = 15)
        {
            SetShapeColor(color);

            TRect rect = rectInfo as TRect;
            rect.MemPointBuffer = LT;
            rect.MemoryRect.Left = LT.X;
            rect.MemoryRect.Top = LT.Y;
            rectInfo = Drawing(rectInfo, RB);
            Shapes.Add(rectInfo);
            p_DrawElement.Add(rectInfo.UIElement);
                
            if (text != null)
            {
                Grid textGrid = WriteInfoText(text, rect, color, FontSz);
                InfoTextBolcks.Add(new InfoTextBolck(textGrid, rect));
            }            
        }
        public void DrawRect(List<CRect> RectList, ColorType color, List<String> textList = null, int FontSz = 15)
        {
            int i = 0;
            foreach (CRect rectPoint in RectList)
            {
                SetShapeColor(color);
                TRect rect = rectInfo as TRect;

                rect.MemPointBuffer = new CPoint(rectPoint.Left, rectPoint.Top);
                rect.MemoryRect.Left = rectPoint.Left;
                rect.MemoryRect.Top = rectPoint.Top;
                rectInfo = Drawing(rectInfo, new CPoint(rectPoint.Right, rectPoint.Bottom));

                Shapes.Add(rectInfo);
                p_DrawElement.Add(rectInfo.UIElement);
                 
                if (textList[i] != null)
                {
                    Grid textGrid = WriteInfoText(textList[i++], rect, color, FontSz);
                    InfoTextBolcks.Add(new InfoTextBolck(textGrid, rect));
                }
            }
        }
        public override void SetRoiRect()
        {
            Shapes.CollectionChanged += Shapes_CollectionChanged;
            InfoTextBolcks.CollectionChanged += Texts_CollectionChanged;

            base.SetRoiRect();
            RedrawShapes();
            ReWriteText();

            Shapes.CollectionChanged -= Shapes_CollectionChanged;
            InfoTextBolcks.CollectionChanged -= Texts_CollectionChanged;
        }
        public override void CanvasMovePoint_Ref(CPoint point, int nX, int nY)
        {
            Shapes.CollectionChanged += Shapes_CollectionChanged;
            InfoTextBolcks.CollectionChanged += Texts_CollectionChanged;

            base.CanvasMovePoint_Ref(point, nX, nY);
            RedrawShapes();
            ReWriteText();

            Shapes.CollectionChanged -= Shapes_CollectionChanged;
            InfoTextBolcks.CollectionChanged -= Texts_CollectionChanged;
        }
        #endregion

        private void SetShapeColor(ColorType color)
        {
            switch(color)
            {
                case ColorType.MasterFeature :
                    rectInfo = new TRect(Brushes.DarkMagenta, 4, 1);
                    break;
                case ColorType.ShotFeature:
                    rectInfo = new TRect(Brushes.Navy, 4, 1); 
                    break;
                case ColorType.ChipFeature:
                    rectInfo = new TRect(Brushes.DarkBlue, 4, 1);
                    break;
                case ColorType.FeatureMatching:
                    rectInfo = new TRect(Brushes.Gold, 4, 1);
                    break;
                case ColorType.FeatureMatchingFail:
                    rectInfo = new TRect(Brushes.Red, 4, 1);
                    break;
                case ColorType.Defect:
                    rectInfo = new TRect(Brushes.Red, 4, 1);
                    break;
                default:
                    rectInfo = new TRect(Brushes.Black, 4, 1);
                    break;
            }
            
        }
        private System.Windows.Media.SolidColorBrush GetColorBrushType(ColorType color)
        {
            switch (color)
            {
                case ColorType.MasterFeature:
                    return Brushes.DarkMagenta;
                case ColorType.ShotFeature:
                    return Brushes.Navy;
                case ColorType.ChipFeature:
                    return Brushes.DarkBlue;
                case ColorType.FeatureMatching:
                    return Brushes.Gold;
                case ColorType.Defect:
                    return Brushes.Red;
                default:
                    return Brushes.Black;
            }
        }
        private TShape Drawing(TShape shape, CPoint memPt)
        {
            TRect rect = shape as TRect;
            // memright가 0인상태로 canvas rect width가 정해져서 버그...
            // 0이면 min정해줘야되나
            if (rect.MemPointBuffer.X > memPt.X)
            {
                rect.MemoryRect.Left = memPt.X;
                rect.MemoryRect.Right = rect.MemPointBuffer.X;
            }
            else
            {
                rect.MemoryRect.Left = rect.MemPointBuffer.X;
                rect.MemoryRect.Right = memPt.X;
            }
            if (rect.MemPointBuffer.Y > memPt.Y)
            {
                rect.MemoryRect.Top = memPt.Y;
                rect.MemoryRect.Bottom = rect.MemPointBuffer.Y;
            }
            else
            {
                rect.MemoryRect.Top = rect.MemPointBuffer.Y;
                rect.MemoryRect.Bottom = memPt.Y;
            }

            CPoint LT = new CPoint(rect.MemoryRect.Left, rect.MemoryRect.Top);
            CPoint RB = new CPoint(rect.MemoryRect.Right, rect.MemoryRect.Bottom);
            CPoint canvasLT = new CPoint(GetCanvasPoint(LT));
            CPoint canvasRB = new CPoint(GetCanvasPoint(RB));

            int width = Math.Abs(canvasRB.X - canvasLT.X);
            int height = Math.Abs(canvasRB.Y - canvasLT.Y);
            Canvas.SetLeft(rect.CanvasRect, canvasLT.X);
            Canvas.SetTop(rect.CanvasRect, canvasLT.Y);
            Canvas.SetRight(rect.CanvasRect, canvasRB.X);
            Canvas.SetBottom(rect.CanvasRect, canvasRB.Y);
            rect.CanvasRect.Width = width;
            rect.CanvasRect.Height = height;

            return shape;
        }
        private Grid WriteInfoText(string text, TRect rect, ColorType color, int Fontsz)
        {
            Grid grid = new Grid();
            TextBlock tb = new TextBlock();

            CPoint LT = new CPoint(rect.MemoryRect.Left, rect.MemoryRect.Top);
            CPoint RB = new CPoint(rect.MemoryRect.Right, rect.MemoryRect.Bottom);

            CPoint canvasLT = new CPoint(GetCanvasPoint(LT));
            CPoint canvasRB = new CPoint(GetCanvasPoint(RB));
            int width = Math.Abs(canvasRB.X - canvasLT.X);
            int height = Math.Abs(canvasRB.Y - canvasLT.Y);
            rect.CanvasRect.Width = width;
            rect.CanvasRect.Height = height;
            Canvas.SetLeft(grid, canvasLT.X);
            Canvas.SetTop(grid, canvasRB.Y);

            tb.Foreground = GetColorBrushType(color);
            tb.FontSize = Fontsz;
            if(color == ColorType.Defect)
                tb.FontWeight = System.Windows.FontWeights.Bold;


            tb.Text = text;
            grid.Children.Add(tb);
            p_DrawElement.Add(grid);
            return grid;
        }

        //static long time = 0;
        static StopWatch watch = new StopWatch();
        private void RedrawShapes()
        {
            // 개선중..
            //watch.p_secTimeout = 1;
            //if (watch.IsRunning == true)
            //{
            //    time += watch.ElapsedMilliseconds;
            //    watch.Restart();
            //}
            //else
            //{
            //    watch.Start();
            //}

            //if (time < 1000 / 20)
            //{
            //    return;
            //}
            //else
            //{
            //    time = 0;
            //}

            foreach (TShape shape in Shapes)
            {
                TRect rect = shape as TRect;
                if (!this.p_View_Rect.Contains(rect.MemoryRect.Left, rect.MemoryRect.Top) || !this.p_View_Rect.Contains(rect.MemoryRect.Right, rect.MemoryRect.Bottom))
                {
                    rect.CanvasRect.Visibility = Visibility.Hidden;
                    continue;
                }
                else
                {
                    rect.CanvasRect.Visibility = Visibility.Visible;
                }

                CPoint LT = new CPoint(rect.MemoryRect.Left, rect.MemoryRect.Top);
                CPoint RB = new CPoint(rect.MemoryRect.Right, rect.MemoryRect.Bottom);

                CPoint canvasLT = new CPoint(GetCanvasPoint(LT));
                CPoint canvasRB = new CPoint(GetCanvasPoint(RB));

                    

                int width = Math.Abs(canvasRB.X - canvasLT.X);
                int height = Math.Abs(canvasRB.Y - canvasLT.Y);
                rect.CanvasRect.Width = width;
                rect.CanvasRect.Height = height;
                Canvas.SetLeft(rect.CanvasRect, canvasLT.X);
                Canvas.SetTop(rect.CanvasRect, canvasLT.Y);
                Canvas.SetRight(rect.CanvasRect, canvasRB.X);
                Canvas.SetBottom(rect.CanvasRect, canvasRB.Y);
            }
        }

        static long time1 = 0;
        static StopWatch watch1 = new StopWatch();
        private void ReWriteText()
        {

            watch1.p_secTimeout = 1;
            if (watch1.IsRunning == true)
            {


                time1 += watch1.ElapsedMilliseconds;
                watch1.Restart();
            }
            else
            {
                watch1.Start();
            }
            if (time1 < 1000 / 20) 
            {
                return;
            }
            else
            {
                time1 = 0;
            }


            foreach (InfoTextBolck info in InfoTextBolcks)
            {
                TRect rect = info.pos;
                if (!this.p_View_Rect.Contains(rect.MemoryRect.Left, rect.MemoryRect.Top) || !this.p_View_Rect.Contains(rect.MemoryRect.Right, rect.MemoryRect.Bottom))
                {

                    info.grid.Visibility = Visibility.Hidden;
                    continue;
                }
                else
                {
                    info.grid.Visibility = Visibility.Visible;
                }
                CPoint LT = new CPoint(rect.MemoryRect.Left, rect.MemoryRect.Top);
                CPoint RB = new CPoint(rect.MemoryRect.Right, rect.MemoryRect.Bottom);

                CPoint canvasLT = new CPoint(GetCanvasPoint(LT));
                CPoint canvasRB = new CPoint(GetCanvasPoint(RB));
                int width = Math.Abs(canvasRB.X - canvasLT.X);
                int height = Math.Abs(canvasRB.Y - canvasLT.Y);
                rect.CanvasRect.Width = width;
                rect.CanvasRect.Height = height;
                Canvas.SetLeft(info.grid, canvasLT.X);
                Canvas.SetTop(info.grid, canvasRB.Y);                
            }
        }
        
        public void Clear()
        {
            Shapes.Clear();
            InfoTextBolcks.Clear();
            p_DrawElement.Clear();
        }

        public void DefectClear()
        {
            //foreach (TShape shape in Shapes)
            //{
                

            //}
            Shapes.Clear();
            InfoTextBolcks.Clear();
            p_DrawElement.Clear();
        }


        public CPoint leftClickPoint = new CPoint();
        public CPoint rightClickPoint = new CPoint();
        //임시
        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            base.PreviewMouseDown(sender, e);
            if (m_KeyEvent != null)
                if (m_KeyEvent.Key == Key.LeftShift && m_KeyEvent.IsDown)
                    return;
            CPoint CanvasPt = new CPoint(p_MouseX, p_MouseY);
            CPoint MemPt = GetMemPoint(CanvasPt);

            if (e.RightButton == MouseButtonState.Pressed)
            {
                rightClickPoint = MemPt;
            }
            else if(e.LeftButton == MouseButtonState.Pressed)
            {
                leftClickPoint = MemPt;
            }

            if (this.DrawDone != null)
                DrawDone(leftClickPoint, rightClickPoint);
            
        }
    }
}
