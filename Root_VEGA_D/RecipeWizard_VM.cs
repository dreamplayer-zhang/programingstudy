using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Root_VEGA_D.Engineer;
using Root_VEGA_D.Module;
using RootTools;
using RootTools.Control;
using RootTools.Memory;

namespace Root_VEGA_D
{
    public class RecipeWizard_VM : RootViewer_ViewModel
    {
        #region Property
        public enum Coordinate
        {
            FIRSTDIELEFT,
            FIRSTDIERIGHT,
            SECONDDIELEFT,
            LASTDIERIGHT,
            FIRSTDIEBOTTOM,
            FIRSTDIEUP,
            SECONDDIEBOTTOM,
            LASTDIEUP
        }

        public enum AlignPosition
        {
            First,
            Second,
            Line
        }

        AlignPosition p_AlignPos { get; set; } = AlignPosition.First;

        Coordinate m_coordinateEnum;
        public Coordinate p_coordinateEnum
        {
            get
            {
                return m_coordinateEnum;
            }
            set
            {
                SetProperty(ref m_coordinateEnum, value);
            }
        }

        Coordinate m_modifyCoordinateEnum = Coordinate.FIRSTDIELEFT;

        bool m_isAlign = true;
        public bool p_isAlign
        {
            get
            {
                return m_isAlign;
            }
            set
            {
                if (m_isAlign == value)
                    return;
                SetProperty(ref m_isAlign, value);
                if (value)
                {
                    p_DrawElement.Clear();
                    for (int i = 0; i < p_lineShape.Count; i++)
                    {
                        p_DrawElement.Add(p_lineShape[i].UIElement);
                    }
                }
            }
        }

        bool m_isManual = false;
        public bool p_isManual
        {
            get
            {
                return m_isManual;
            }
            set
            {
                if (m_isManual == value)
                    return;
                SetProperty(ref m_isManual, value);
                if (value)
                {
                    p_DrawElement.Clear();
                    for (int i = 0; i < p_pointShape.Count; i++)
                    {
                        p_DrawElement.Add(p_pointShape[i].UIElement);
                    }
                }
            }
        }

        CPoint m_firstPoint = new CPoint();
        public CPoint p_firstPoint
        {
            get
            {
                return m_firstPoint;
            }
            set
            {
                SetProperty(ref m_firstPoint, value);
            }
        }

        CPoint m_secondPoint = new CPoint();
        public CPoint p_secondPoint
        {
            get
            {
                return m_secondPoint;
            }
            set
            {
                SetProperty(ref m_secondPoint, value);
            }
        }

        ObservableCollection<TLine> m_lineShape = new ObservableCollection<TLine>();
        public ObservableCollection<TLine> p_lineShape
        {
            get
            {
                return m_lineShape;
            }
            set
            {
                SetProperty(ref m_lineShape, value);
            }
        }

        ObservableCollection<TLine> m_pointShape = new ObservableCollection<TLine>();
        public ObservableCollection<TLine> p_pointShape
        {
            get
            {
                return m_pointShape;
            }
            set
            {
                SetProperty(ref m_pointShape, value);
            }
        }

        double m_degree = 0.0f;
        public double p_degree
        {
            get
            {
                return m_degree;
            }
            set
            {
                SetProperty(ref m_degree, value);
            }
        }

        CPoint m_firstDieLeftPoint;
        int m_firstDieLeft = 0;
        public int p_firstDieLeft
        {
            get
            {
                return m_firstDieLeft;
            }
            set
            {
                SetProperty(ref m_firstDieLeft, value);
                SetManualPoint(value, ref m_firstDieLeftPoint, Coordinate.FIRSTDIELEFT);
            }
        }

        bool m_isManualChange = false;

        CPoint m_firstDieRightPoint;
        int m_firstDieRight = 0;
        public int p_firstDieRight
        {
            get
            {
                return m_firstDieRight;
            }
            set
            {
                SetProperty(ref m_firstDieRight, value);
                SetManualPoint(value, ref m_firstDieRightPoint, Coordinate.FIRSTDIERIGHT);
            }
        }

        CPoint m_secondDieLeftPoint;
        int m_secondDieLeft = 0;
        public int p_secondDieLeft
        {
            get
            {
                return m_secondDieLeft;
            }
            set
            {
                SetProperty(ref m_secondDieLeft, value);
                SetManualPoint(value, ref m_secondDieLeftPoint, Coordinate.SECONDDIELEFT);
            }
        }

        CPoint m_lastDieRightPoint;
        int m_lastDieRight = 0;
        public int p_lastDieRight
        {
            get
            {
                return m_lastDieRight;
            }
            set
            {
                SetProperty(ref m_lastDieRight, value);
                SetManualPoint(value, ref m_lastDieRightPoint, Coordinate.LASTDIERIGHT);
            }
        }

        CPoint m_firstDieBottomPoint;
        int m_firstDieBottom = 0;
        public int p_firstDieBottom
        {
            get
            {
                return m_firstDieBottom;
            }
            set
            {
                SetProperty(ref m_firstDieBottom, value);
                SetManualPoint(value, ref m_firstDieBottomPoint, Coordinate.FIRSTDIEBOTTOM);
            }
        }

        CPoint m_firstDieUpPoint;
        int m_firstDieUp = 0;
        public int p_firstDieUp
        {
            get
            {
                return m_firstDieUp;
            }
            set
            {
                SetProperty(ref m_firstDieUp, value);
                SetManualPoint(value, ref m_firstDieUpPoint, Coordinate.FIRSTDIEUP);
            }
        }

        CPoint m_secondDieBottomPoint;
        int m_secondDieBottom = 0;
        public int p_secondDieBottom
        {
            get
            {
                return m_secondDieBottom;
            }
            set
            {
                SetProperty(ref m_secondDieBottom, value);
                SetManualPoint(value, ref m_secondDieBottomPoint, Coordinate.SECONDDIEBOTTOM);
            }
        }

        CPoint m_lastDieUpPoint;
        int m_lastDieUp = 0;
        public int p_lastDieUp
        {
            get
            {
                return m_lastDieUp;
            }
            set
            {
                SetProperty(ref m_lastDieUp, value);
                SetManualPoint(value, ref m_lastDieUpPoint, Coordinate.LASTDIEUP);

            }
        }

        void CalcSize()
        {
            p_dieSize = new CPoint(p_firstDieRight - p_firstDieLeft, p_firstDieBottom - p_firstDieUp);
            p_strDieSize = p_dieSize.X + ", " + p_dieSize.Y;

            p_scribeLaneSize = new CPoint(p_secondDieLeft - p_firstDieRight, p_firstDieUp - p_lastDieUp);
            p_strScribeLaneSize = p_scribeLaneSize.X + ", " + p_scribeLaneSize.Y;

            p_shotSize = new CPoint(p_secondDieLeft - p_firstDieLeft, p_firstDieBottom - p_secondDieBottom);
            p_strShotSize = p_shotSize.X + ", " + p_shotSize.Y;

            p_shot = new CPoint(p_lastDieRight - p_firstDieLeft, p_lastDieRight - p_lastDieUp);
            p_strShot = p_shot.X + ", " + p_shot.Y;
        }

        CPoint m_coordinatePoint = new CPoint();
        public CPoint p_coordinatePoint
        {
            get
            {
                return m_coordinatePoint;
            }
            set
            {
                SetProperty(ref m_coordinatePoint, value);
            }
        }

        bool m_isManualInput = false;
        public bool p_isManualInput
        {
            get
            {
                return m_isManualInput;
            }
            set
            {
                SetProperty(ref m_isManualInput, value);
            }
        }

        bool m_isAutoNext = false;
        public bool p_isAutoNext
        {
            get
            {
                return m_isAutoNext;
            }
            set
            {
                SetProperty(ref m_isAutoNext, value);
            }
        }

        public CPoint p_dieSize { get; set; } = new CPoint();
        string m_strDieSize = "0, 0";
        public string p_strDieSize
        {
            get
            {
                return m_strDieSize;
            }
            set
            {
                SetProperty(ref m_strDieSize, value);
            }
        }
        public CPoint p_scribeLaneSize { get; set; } = new CPoint();
        string m_strScribeLaneSize = "0, 0";
        public string p_strScribeLaneSize
        {
            get
            {
                return m_strScribeLaneSize;
            }
            set
            {
                SetProperty(ref m_strScribeLaneSize, value);
            }
        }
        public CPoint p_shotSize { get; set; } = new CPoint();
        string m_strShotSize = "0, 0";
        public string p_strShotSize
        {
            get
            {
                return m_strShotSize;
            }
            set
            {
                SetProperty(ref m_strShotSize, value);
            }
        }
        public CPoint p_shot { get; set; } = new CPoint();
        string m_strShot = "0, 0";
        public string p_strShot
        {
            get
            {
                return m_strShot;
            }
            set
            {
                SetProperty(ref m_strShot, value);
            }
        }

        ImageData m_imgOtherPC;
        public ImageData p_imgOtherPC
        {
            get { return m_imgOtherPC; }
            set { SetProperty(ref m_imgOtherPC, value); }
        }

        Vision m_vision;
        #endregion
        public RecipeWizard_VM()
        {
            m_vision = ((VEGA_D_Handler)App.m_engineer.ClassHandler()).m_vision;
            Init();
        }

        void Init()
        {
            //p_rootViewer = new RootViewer_ViewModel();
            //this.init(new ImageData(App.m_engineer.ClassMemoryTool().GetMemory("Vision.Memory", "Vision", "Main")));
            this.p_VisibleMenu = System.Windows.Visibility.Collapsed;

            Vision vision = App.m_engineer.m_handler.m_vision;
            if (vision != null)
            {
                MemoryTool memoryTool = App.m_engineer.ClassMemoryTool();
                p_imgOtherPC = new ImageData(vision.MemoryPool.p_id, vision.MemoryGroup.p_id, vision.MemoryOtherPC.p_id, memoryTool, vision.MemoryOtherPC.p_nCount, vision.MemoryOtherPC.p_nByte);

                init(p_imgOtherPC);
            }
        }

        #region Function
        void CalcAngle()
        {
            int calcX1, calcX2, calcY1, calcY2;
            calcX1 = p_firstPoint.X;
            calcX2 = p_secondPoint.X;
            calcY1 = p_firstPoint.Y;
            calcY2 = p_secondPoint.Y;

            if (calcY1 > calcY2)
            {
                int temp = calcY1;
                calcY1 = calcY2;
                calcY2 = temp;

                temp = calcX1;
                calcX1 = calcX2;
                calcX2 = temp;
            }

            
            double degree = Math.Atan2(calcX1 - calcX2, calcY1 - calcY2) * 180 / Math.PI;

            if (degree > 0)
            {
                p_degree = degree - 180;
            }
            else
            {
                p_degree = 180 + degree;
            }
        }

        void CheckCoordinate()
        {
            if(p_isAlign)
                p_isAlign = false;
            if(!p_isManual)
                p_isManual = true;
            switch (p_coordinateEnum)
            {
                case Coordinate.FIRSTDIELEFT:
                    m_firstDieLeftPoint = p_coordinatePoint;
                    p_firstDieLeft = p_coordinatePoint.X;
                    break;
                case Coordinate.FIRSTDIERIGHT:
                    m_firstDieRightPoint = p_coordinatePoint;
                    p_firstDieRight = p_coordinatePoint.X;
                    break;
                case Coordinate.SECONDDIELEFT:
                    m_secondDieLeftPoint = p_coordinatePoint;
                    p_secondDieLeft = p_coordinatePoint.X;
                    break;
                case Coordinate.LASTDIERIGHT:
                    m_lastDieRightPoint = p_coordinatePoint;
                    p_lastDieRight = p_coordinatePoint.X;
                    break;
                case Coordinate.FIRSTDIEBOTTOM:
                    m_firstDieBottomPoint = p_coordinatePoint;
                    p_firstDieBottom = p_coordinatePoint.Y;
                    break;
                case Coordinate.FIRSTDIEUP:
                    m_firstDieUpPoint = p_coordinatePoint;
                    p_firstDieUp = p_coordinatePoint.Y;
                    break;
                case Coordinate.SECONDDIEBOTTOM:
                    m_secondDieBottomPoint = p_coordinatePoint;
                    p_secondDieBottom = p_coordinatePoint.Y;
                    break;
                case Coordinate.LASTDIEUP:
                    m_lastDieUpPoint = p_coordinatePoint;
                    p_lastDieUp = p_coordinatePoint.Y;
                    break;
            }
        }
        #endregion

        #region Command

        public ICommand CmdCalcAlign
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Axis rotate = m_vision.AxisRotate;
                    double moveVal = p_degree * 1000;
                    Thread thread = new Thread(() =>
                    {
                        rotate.StartMove(rotate.p_posActual - moveVal);
                        rotate.WaitReady();
                    });
                    thread.Start();
                });
            }
        }

        public ICommand CmdSnap
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Thread thread = new Thread(() =>
                    {
                        Run_GrabLineScan grab = m_vision.CloneModuleRun("GrabLineScan") as Run_GrabLineScan;
                        grab.m_grabMode.m_ScanStartLine = 0;
                        grab.m_grabMode.m_ScanLineNum = 2;
                        m_vision.StartRun(grab);
                    });
                    thread.Start();
                });
            }
        }

        public ICommand CmdCheckCoordinate
        {
            get
            {
                return new RelayCommand(() =>
                {
                    CheckCoordinate();
                });
            }
        }

        #endregion


        bool m_isFirstCheck { get; set; }
        bool m_isSecondCheck { get; set; }
        #region Mouse Event
        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            if (m_KeyEvent == null)
                return;
            base.PreviewMouseDown(sender, e);

            if (m_KeyEvent != null && !(m_KeyEvent.Key == Key.LeftShift && m_KeyEvent.IsDown))
            {
                if (p_isAlign)
                {

                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        //ClearDrawElement();
                        //p_lineShape.Clear();
                       // if (!m_isFirstCheck || m_isSecondCheck)
                        {
                            p_firstPoint = new CPoint(p_MouseMemX, p_MouseMemY);
                            DrawAlignCrossLine();
                            m_isFirstCheck = true;
                        }
                        //DrawCrossLine(p_firstPoint);

                    }
                    else
                    {
                        if (m_isFirstCheck)
                        {
                            p_secondPoint = new CPoint(p_MouseMemX, p_MouseMemY);
                            DrawAlignCrossLine(false);
                            DrawLines();
                            m_isSecondCheck = true;
                        }
                    }

                    CalcAngle();
                }
                else
                {
                    p_coordinatePoint = new CPoint(p_MouseMemX, p_MouseMemY);
                    CheckCoordinate();
                    DrawCoordinateCrossLine();
                    RedrawPoint();

                    if (p_isAutoNext)
                    {
                        if ((int)p_coordinateEnum == 7)
                        {
                            return;
                        }
                        p_coordinateEnum += 1;

                    }
                    CalcSize();
                }

            }
            else
            {
                RedrawShape();
            }
            
        }

        bool CheckDraw()
        {
            foreach (TShape shape in p_pointShape)
            {
                TLine line = shape as TLine;
                if ((Coordinate)line.Tag == p_coordinateEnum)
                    return true;
            }

            return false;
        }

        bool CheckAlignDraw(bool isLine = false)
        {
            foreach (TShape shape in p_lineShape)
            {
                TLine line = shape as TLine;
                if (isLine)
                {
                    if ((AlignPosition)line.Tag == AlignPosition.Line)
                        return true;
                }
                else
                {
                    if ((AlignPosition)line.Tag == p_AlignPos)
                        return true;
                }

            }

            return false;
        }

        Brush GetBrush()
        {
            switch (p_coordinateEnum)
            {
                case Coordinate.FIRSTDIELEFT:
                    return Brushes.Red;
                case Coordinate.FIRSTDIERIGHT:
                    return Brushes.Orange;
                case Coordinate.SECONDDIELEFT:
                    return Brushes.Green;
                case Coordinate.LASTDIERIGHT:
                    return Brushes.Blue;
                case Coordinate.FIRSTDIEBOTTOM:
                    return Brushes.Yellow;
                case Coordinate.FIRSTDIEUP:
                    return Brushes.Aqua;
                case Coordinate.SECONDDIEBOTTOM:
                    return Brushes.Brown;
                case Coordinate.LASTDIEUP:
                    return Brushes.Indigo;
            }
            return Brushes.Black;
        }

        void DrawCoordinateCrossLine()
        {
            double LineThick = 3;
            TLine line1;
            TLine line2;
            int size = 200;
            CPoint pt;

            pt = p_coordinatePoint;
            if (CheckDraw())
            {
                int idx = 0;
                foreach (TShape shape in p_pointShape)
                {
                    TLine line = shape as TLine;

                    Coordinate cd = m_isManualChange ? m_modifyCoordinateEnum : p_coordinateEnum;
                    if ((Coordinate)line.Tag == cd)
                    {

                        if(idx == 0)
                        {
                            line.MemoryStartPoint = new CPoint(pt.X - size, pt.Y - size);
                            line.MemoryEndPoint = new CPoint(pt.X + size, pt.Y + size);
                        }
                        else
                        {
                            line.MemoryStartPoint = new CPoint(pt.X - size, pt.Y + size);
                            line.MemoryEndPoint = new CPoint(pt.X + size, pt.Y - size);
                        }

                        idx++;

                        if (idx == 2)
                            idx = 0;
                    }
                }
            }
            else
            {
                line1 = new TLine(GetBrush(), LineThick, 1.0);
                line2 = new TLine(GetBrush(), LineThick, 1.0);

                line1.Tag = p_coordinateEnum;
                line2.Tag = p_coordinateEnum;

                line1.MemoryStartPoint = new CPoint(pt.X - size, pt.Y - size);
                line1.MemoryEndPoint = new CPoint(pt.X + size, pt.Y + size);

                m_pointShape.Add(line1);
                p_DrawElement.Add(line1.UIElement);

                line2.MemoryStartPoint = new CPoint(pt.X - size, pt.Y + size);
                line2.MemoryEndPoint = new CPoint(pt.X + size, pt.Y - size);
                p_pointShape.Add(line2);
                p_DrawElement.Add(line2.UIElement);
            }
            RedrawPoint(); 
        }

        void DrawAlignCrossLine(bool isFirst = true)
        {
            double LineThick = 3;
            TLine line1;
            TLine line2;
            int size = 200;
            CPoint pt;
            if (isFirst)
            {
                line1 = new TLine(Brushes.Red, LineThick, 1.0);
                line2 = new TLine(Brushes.Red, LineThick, 1.0);
                pt = p_firstPoint;
                line1.Tag = AlignPosition.First;
                line2.Tag = AlignPosition.First;
                p_AlignPos = AlignPosition.First;
            }
            else
            {
                line1 = new TLine(Brushes.Yellow, LineThick, 1.0);
                line2 = new TLine(Brushes.Yellow, LineThick, 1.0);
                pt = p_secondPoint;
                line1.Tag = AlignPosition.Second;
                line2.Tag = AlignPosition.Second;
                p_AlignPos = AlignPosition.Second;
            }

            if (CheckAlignDraw())
            {

                int idx = 0;
                foreach (TShape shape in p_lineShape)
                {
                    TLine line = shape as TLine;
                    if((AlignPosition)line.Tag == p_AlignPos)
                    {
                        if (idx == 0)
                        {
                            line.MemoryStartPoint = new CPoint(pt.X - size, pt.Y - size);
                            line.MemoryEndPoint = new CPoint(pt.X + size, pt.Y + size);
                        }
                        else
                        {
                            line.MemoryStartPoint = new CPoint(pt.X - size, pt.Y + size);
                            line.MemoryEndPoint = new CPoint(pt.X + size, pt.Y - size);
                        }
                        idx++;

                        if (idx == 2)
                            idx = 0;
                    }
                    //if (line.Tag != null && line.Tag.ToString() == tag + (idx + 1))
                    //{
                    //    line.MemoryStartPoint = new CPoint(pt.X - size, pt.Y - size);
                    //    line.MemoryEndPoint = new CPoint(pt.X + size, pt.Y + size);
                    //}
                    //else if (line.Tag != null && line.Tag.ToString() == tag + (idx + 2))
                    //{
                    //    line.MemoryStartPoint = new CPoint(pt.X - size, pt.Y + size);
                    //    line.MemoryEndPoint = new CPoint(pt.X + size, pt.Y - size);
                    //}
                    
                }
                if (m_isFirstCheck && m_isSecondCheck)
                    DrawLines();
            }
            else
            {

                line1.MemoryStartPoint = new CPoint(pt.X - size, pt.Y - size);
                line1.MemoryEndPoint = new CPoint(pt.X + size, pt.Y + size);

                p_lineShape.Add(line1);
                p_DrawElement.Add(line1.UIElement);

                line2.MemoryStartPoint = new CPoint(pt.X - size, pt.Y + size);
                line2.MemoryEndPoint = new CPoint(pt.X + size, pt.Y - size);
                p_lineShape.Add(line2);
                p_DrawElement.Add(line2.UIElement);
            }


            RedrawLine();
        }
        void DrawLines()
        {
            if(CheckAlignDraw(true))
            {
                foreach(TShape shape in p_lineShape)
                {
                    TLine line = shape as TLine;

                    if(line.Tag != null && (AlignPosition)line.Tag == AlignPosition.Line)
                    {
                        line.MemoryStartPoint = p_firstPoint;
                        line.MemoryEndPoint = p_secondPoint;
                    }
                }
            }
            else
            {
                Brush LineBrush = Brushes.Red;
                double LineThick = 3;
                TLine line = new TLine(LineBrush, LineThick, 1.0);
                line.MemoryStartPoint = p_firstPoint;
                line.MemoryEndPoint = p_secondPoint;
                line.Tag = AlignPosition.Line;
                p_lineShape.Add(line);
                p_DrawElement.Add(line.UIElement);
              
            }
            RedrawLine();
            //Canvas.SetLeft(this.p_CanvasWidth);

        }

        void RedrawShape()
        {
            RedrawLine();
            RedrawPoint();
        }
        void RedrawPoint()
        {
            foreach (TShape shape in p_pointShape)
            {
                TLine line = shape as TLine;
                //if((Coordinate)line.Tag == p_coordinateEnum)
                {
                    CPoint top = new CPoint(line.MemoryStartPoint.X, line.MemoryStartPoint.Y);
                    CPoint btm = new CPoint(line.MemoryEndPoint.X, line.MemoryEndPoint.Y);

                    CPoint canvasTop = new CPoint(GetCanvasPoint(top));
                    CPoint canvasBtm = new CPoint(GetCanvasPoint(btm));

                    //int width = Math.Abs(canvasBtm.X - canvasTop.X);
                    //int height = Math.Abs(canvasBtm.Y - canvasTop.Y);
                    //line.CanvasLine.Width = width;
                    //line.CanvasLine.Height = height;

                    line.CanvasLine.X1 = canvasTop.X;
                    line.CanvasLine.Y1 = canvasTop.Y;
                    line.CanvasLine.X2 = canvasBtm.X;
                    line.CanvasLine.Y2 = canvasBtm.Y;
                }

               
            }
        }
        void RedrawLine()
        {
            foreach(TShape shape in p_lineShape)
            {
                TLine line = shape as TLine;

                CPoint top = new CPoint(line.MemoryStartPoint.X, line.MemoryStartPoint.Y);
                CPoint btm = new CPoint(line.MemoryEndPoint.X, line.MemoryEndPoint.Y);

                CPoint canvasTop = new CPoint(GetCanvasPoint(top));
                CPoint canvasBtm = new CPoint(GetCanvasPoint(btm));

                //int width = Math.Abs(canvasBtm.X - canvasTop.X);
                //int height = Math.Abs(canvasBtm.Y - canvasTop.Y);
                //line.CanvasLine.Width = width;
                //line.CanvasLine.Height = height;

                line.CanvasLine.X1 = canvasTop.X;
                line.CanvasLine.Y1 = canvasTop.Y;
                line.CanvasLine.X2 = canvasBtm.X;
                line.CanvasLine.Y2 = canvasBtm.Y;

                //Canvas.SetTop(line.CanvasLine, canvasTop.Y);
                //Canvas.SetBottom(line.CanvasLine, canvasBtm.Y);
            }
        }

        void SetManualPoint(int val, ref CPoint pt, Coordinate coordinate)
        {
            if (p_isManual && p_isManualInput)
            {
                if (pt == null)
                    pt = new CPoint();
                if ((int)coordinate < 4)
                    pt.X = val;
                else
                    pt.Y = val;
                p_coordinatePoint = pt;
                //p_coordinateEnum = coordinate;
                m_modifyCoordinateEnum = coordinate;
                m_isManualChange = true;
                DrawCoordinateCrossLine();
                m_isManualChange = false;

                CalcSize();
            }
        }


        public override void SetRoiRect()
        {
            base.SetRoiRect();
            RedrawLine();
        }
        
        public override void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            base.MouseWheel(sender, e);
            RedrawShape();
        }
        public override void CanvasMovePoint_Ref(CPoint point, int nX, int nY)
        {
            base.CanvasMovePoint_Ref(point, nX, nY);
            //RedrawLine();
            RedrawShape();
        }
        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender,e);

            this.p_Cursor = Cursors.Pen;
            CPoint CanvasPt = new CPoint(p_MouseX, p_MouseY);
            CPoint MemPt = GetMemPoint(CanvasPt);
            p_MouseMemX = MemPt.X;
            p_MouseMemY = MemPt.Y;
        }
        #endregion
    }
}
