using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using RootTools;
using RootTools.Memory;

namespace Root_VEGA_D
{
    public class RecipeWizard_VM : RootViewer_ViewModel, IDialogRequestClose
    {
        #region Property
        //RootViewer_ViewModel m_rootViewer;
        //public RootViewer_ViewModel p_rootViewer
        //{
        //    get
        //    {
        //        return m_rootViewer;
        //    }
        //    set
        //    {
        //        SetProperty(ref m_rootViewer, value);
        //    }
        //}

        bool m_isAlign = true;
        public bool p_isAlign
        {
            get
            {
                return m_isAlign;
            }
            set
            {
                SetProperty(ref m_isAlign, value);
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
                SetProperty(ref m_isManual, value);
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

        bool p_isFirst { get; set; } = false;
        #endregion

        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;
        public RecipeWizard_VM()
        {
            Init();
        }

        void Init()
        {
            //p_rootViewer = new RootViewer_ViewModel();
            this.init(new ImageData(App.m_engineer.ClassMemoryTool().GetMemory("Vision.Memory", "Vision", "Main")));
        }


        #region Mouse Event

        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            base.PreviewMouseDown(sender, e);

            if (m_KeyEvent != null && !(m_KeyEvent.Key == Key.LeftShift && m_KeyEvent.IsDown))
            {
                if (m_isAlign)
                {

                    if (p_isFirst)
                    {
                        ClearDrawElement();
                        p_firstPoint = new CPoint(p_MouseX, p_MouseY);

                        //DrawCrossLine(p_firstPoint);
                    }
                    else
                    {
                        p_secondPoint = new CPoint(p_MouseX, p_MouseY);
                        DrawLines();
                    }
                    p_isFirst = !p_isFirst;
                }
            }
            
        }

        void DrawLines()
        {
            Line line = new Line();
            Brush LineBrush = Brushes.Red;
            double LineThick = 3;
            //DoubleCollection LineDash = new DoubleCollection { 3, 4 };

            line.Stroke = LineBrush;
            line.StrokeThickness = LineThick;

            line.X1 = p_firstPoint.X;
            line.Y1 = p_firstPoint.Y;

            line.X2 = p_secondPoint.X;
            line.Y2 = p_secondPoint.Y;
            p_DrawElement.Add(line);

        }

        public override void CanvasMovePoint_Ref(CPoint point, int nX, int nY)
        {
            base.CanvasMovePoint_Ref(point, nX, nY);
        }
        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender,e);
            CPoint CanvasPt = new CPoint(p_MouseX, p_MouseY);
            CPoint MemPt = GetMemPoint(CanvasPt);
            p_MouseMemX = MemPt.X;
            p_MouseMemY = MemPt.Y;
        }
        #endregion
    }
}
