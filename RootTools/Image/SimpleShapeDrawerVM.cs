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

namespace RootTools
{

    public enum process
    {
        None,
        Start,
        Drawing,
        End
    }

    public enum Work
    {
        Create,
        Delete,
        Modify,

    }

    public class DrawHistoryWorker
    {
        int HistoryIndex = 0;
        Stack<DrawHistory> m_History = new Stack<DrawHistory>();
        Stack<DrawHistory> m_future = new Stack<DrawHistory>();
        DrawToolVM m_DrawerToolVM; //가장 최근 DrawHistoryWorker에 사용된 DrawerToolVM
        DrawHistory tempHistory;
        DrawHistory TargetHistory;
        //public void AddHistory(DrawHistory _History)

        #region MakeHistory
        public void SetTool(DrawToolVM _DrawerToolVM)
        {
            m_DrawerToolVM = _DrawerToolVM;
        }
        public void AddHistory(DrawToolVM _DrawerToolVM, Work _Work, Shape _StartShape, Rect _StartRect, Shape _EndShape = null, Rect _EndRect = new Rect(), bool _Continue = false)
        {
            m_DrawerToolVM = _DrawerToolVM;
            AddHistory(_Work, _StartShape, _StartRect, _EndShape, _EndRect, _Continue);
        }

        public void AddHistory(Work _Work, Shape _StartShape, Rect _StartRect, Shape _EndShape = null, Rect _EndRect = new Rect(), bool _Continue = false)
        {
            m_History.Push(new DrawHistory(m_DrawerToolVM, _Work, HistoryIndex, _StartShape, _StartRect, _EndShape, _EndRect));
            if (m_future.Any())
            {
                m_future.Clear();
            }

            if (!_Continue)
                HistoryIndex++;
        }
        #endregion

        public void Do_CtrlZ(ModifyManager _ModifyManager = null)
        {
            if (m_History.Any())
                HistoryIndex--;
            else
                return;

            while (m_History.Any())
            {
                tempHistory = m_History.Pop();

                if (tempHistory.p_Index != HistoryIndex)
                {
                    m_History.Push(tempHistory);
                    break;
                }
                if (_ModifyManager != null && tempHistory.p_EndShape != _ModifyManager.p_ModifyTarget)
                {
                    m_History.Push(tempHistory);
                    break;
                }

                switch (tempHistory.p_Work)
                {
                    case Work.Create:
                        WorkDelete(tempHistory);
                        break;
                    case Work.Delete:
                        WorkCreate(tempHistory);
                        break;
                    case Work.Modify:
                        //tempHistory.
                        if (_ModifyManager != null)
                        {
                            WorkingModify(tempHistory, Key.Z);
                            break;
                        }
                        else
                        {
                            WorkModify(tempHistory, Key.Z);
                            break;
                        }
                }
                m_future.Push(tempHistory);
                TargetHistory = tempHistory;


            }
            Refresh(TargetHistory);

        }

        public void Do_CtrlY(ModifyManager _ModifyManager = null)
        {
            bool Doing = false;

            while (m_future.Any())
            {
                Doing = true;

                tempHistory = m_future.Pop();

                if (tempHistory.p_Index != HistoryIndex)
                {
                    m_future.Push(tempHistory);
                    break;
                }
                if (_ModifyManager != null && tempHistory.p_StartShape != _ModifyManager.p_ModifyTarget)
                {
                    m_future.Push(tempHistory);
                    break;
                }

                switch (tempHistory.p_Work)
                {
                    case Work.Create:
                        WorkCreate(tempHistory);
                        break;
                    case Work.Delete:
                        WorkDelete(tempHistory);
                        break;
                    case Work.Modify:

                        if (_ModifyManager != null)
                        {
                            WorkingModify(tempHistory, Key.Y);
                            break;
                        }
                        else
                        {
                            WorkModify(tempHistory, Key.Y);
                            break;
                        }

                }
                m_History.Push(tempHistory);
                TargetHistory = tempHistory;

            }
            if (Doing == true)
                HistoryIndex++;
            else
                return;

            Refresh(TargetHistory);

        }

        private void Refresh(DrawHistory _tempHistory)
        {
            _tempHistory.p_DrawerToolVM.m_Element.Clear();
            foreach (UIElement myObj in _tempHistory.p_DrawerToolVM.m_ListShape)
            {
                _tempHistory.p_DrawerToolVM.m_Element.Add(myObj);
            }

            _tempHistory.p_DrawerToolVM.p_ImageViewer.p_Element.Clear();
            foreach (UIElement myObj in _tempHistory.p_DrawerToolVM.m_Element)
            {
                _tempHistory.p_DrawerToolVM.p_ImageViewer.p_Element.Add(myObj);
            }
            tempHistory.p_DrawerToolVM.Redrawing();

        }

        public void WorkDelete(DrawHistory _tempHistory)
        {
            int _index;
            _index = _tempHistory.p_DrawerToolVM.m_ListShape.IndexOf(_tempHistory.p_StartShape);

            _tempHistory.p_DrawerToolVM.m_ListShape.RemoveAt(_index);
            _tempHistory.p_DrawerToolVM.m_ListRect.RemoveAt(_index);
        }
        public void WorkCreate(DrawHistory _tempHistory)
        {
            _tempHistory.p_DrawerToolVM.m_ListShape.Add(_tempHistory.p_StartShape);
            _tempHistory.p_DrawerToolVM.m_ListRect.Add(_tempHistory.p_StartRect);
        }
        public void WorkModify(DrawHistory _tempHistory, Key _Key)
        {
            int _index;
            if (_Key == Key.Z)
            {
                _index = _tempHistory.p_DrawerToolVM.m_ListShape.IndexOf(_tempHistory.p_EndShape);

                _tempHistory.p_DrawerToolVM.m_ListShape[_index] = _tempHistory.p_StartShape;
                _tempHistory.p_DrawerToolVM.m_ListRect[_index] = _tempHistory.p_StartRect;
            }
            else if (_Key == Key.Y)
            {
                _index = _tempHistory.p_DrawerToolVM.m_ListShape.IndexOf(_tempHistory.p_StartShape);

                _tempHistory.p_DrawerToolVM.m_ListShape[_index] = _tempHistory.p_EndShape;
                _tempHistory.p_DrawerToolVM.m_ListRect[_index] = _tempHistory.p_EndRect;
            }
        }

        public void WorkingModify(DrawHistory _tempHistory, Key _Key)
        {
            if (_Key == Key.Z)
            {
                //tempHistory.t = tempHistory.p_StartShape;
                //tempHistory.p_EndRect = tempHistory.p_StartRect;
            }
            else if (_Key == Key.Y)
            {
                tempHistory.p_StartShape = tempHistory.p_EndShape;
                tempHistory.p_StartRect = tempHistory.p_EndRect;
            }
        }

        public void Clear()
        {
            m_History.Clear();
            m_future.Clear();
        }
    }

    public class DrawHistory
    {

        public DrawToolVM p_DrawerToolVM;
        public Work p_Work;
        public int p_Index;
        public Shape p_StartShape;
        public Shape p_EndShape;
        public Rect p_StartRect;
        public Rect p_EndRect;

        //List<Shape> _List;

        public DrawHistory(DrawToolVM _DrawerToolVM, Work _Work, int _Index, Shape _StartShape, Rect _StartRect, Shape _EndShape = null, Rect _EndRect = new Rect())
        {
            p_DrawerToolVM = _DrawerToolVM;
            p_Work = _Work;
            p_Index = _Index;  //n번째 작업
            p_StartShape = _StartShape;
            p_StartRect = _StartRect;
            if (_Work == Work.Modify)
            {
                p_EndShape = _EndShape;
                p_EndRect = _EndRect;
            }
        }

    }

    public abstract class DrawToolVM : ObservableObject
    {
        public List<Shape> m_ListShape = new List<Shape>();
        public List<Rect> m_ListRect = new List<Rect>();

        //m_ListRect와 동일해보이나, Drawing중인 객체는 m_ListRect에는 없고, p_Element에만 있다.
        public ObservableCollection<UIElement> m_Element = new ObservableCollection<UIElement>();
        public Shape m_ShapeIcon;

        public System.Windows.Media.Brush m_Stroke = System.Windows.Media.Brushes.Red;
        public int m_StrokeThickness = 2;
        public DoubleCollection m_StrokeDashArray = new DoubleCollection { 3, 2 };

        public abstract ImageViewer_ViewModel p_ImageViewer
        { get; set; }
        public abstract KeyEventArgs p_KeyEvent
        { get; }
        public abstract bool p_State
        { get; set; }
        //System.Drawing.Rectangle p_View_Rect
        //{ get; set; }

        public abstract void SetShape(int p_KeyShape);
        public Key[] p_KeyList
        { get; set; }

        public abstract void Redrawing();
        public abstract void DrawStart();
        public abstract void Drawing();
        public abstract void DrawEnd();
        public abstract void Clear();

    }

    public class SimpleShapeDrawerVM : DrawToolVM
    {
        //  public Shape ModifyTarget=null;

        public enum SimpleShape
        {
            Ellipse,
            Rectangle,
            Line
        }

        public override KeyEventArgs p_KeyEvent
        {
            get
            {
                return p_ImageViewer.KeyEvent;
            }
        }

        //true -> drawing
        //false -> none
        // if drawing is start, then p_State setting true
        protected bool m_State = false;
        public override bool p_State
        {
            get { return m_State; }
            set
            {
                if (!value)
                    m_Process = process.None;
                m_State = value;
            }

        }

        protected double m_mouseX;
        public double p_mouseX
        {
            get { return m_mouseX; }
            set
            {
                m_mouseX = p_ImageViewer.p_MouseX;
            }
        }

        protected double m_mouseY;
        public double p_mouseY
        {
            get { return m_mouseY; }
            set
            {
                m_mouseY = p_ImageViewer.p_MouseY;
            }
        }

        private ImageViewer_ViewModel m_ImageViewer;
        public override ImageViewer_ViewModel p_ImageViewer
        {
            get { return m_ImageViewer; }
            set { m_ImageViewer = value; }
        }

        public Key CircleKeyValue
        {
            get { return p_KeyList[0]; }
            set { p_KeyList[0] = value; }
        }
        public Key RectangleKeyValue
        {
            get { return p_KeyList[1]; }
            set { p_KeyList[1] = value; }
        }
        public Key LineKeyValue
        {
            get { return p_KeyList[2]; }
            set { p_KeyList[2] = value; }
        }

        public SimpleShape m_Shape;
        protected process m_Process;

        protected Shape m_SelectedShape;
        protected Rect m_TempRect;

        // List<Ellipse> ListEllipse = new List<Ellipse>();

        protected Point m_ClickPos;
        protected Point m_MousePos;
        protected Point m_StartPos;
        protected Point m_EndPos;

        public override void SetShape(int _ShapeNum)
        {
            if (_ShapeNum != -1)
            {
                m_Shape = (SimpleShape)_ShapeNum;
                SetIcon();
            }
        }

        public virtual void SetIcon()
        {

            Point tempPoint = new Point(p_ImageViewer.p_View_Rect.X, p_ImageViewer.p_View_Rect.Y);

            int _start = 5;
            int _end = 20;
            switch (m_Shape)
            {
                case SimpleShape.Rectangle:
                    m_ShapeIcon = new Rectangle();
                    break;

                case SimpleShape.Ellipse:
                    m_ShapeIcon = new Ellipse();
                    break;
                case SimpleShape.Line:
                    m_ShapeIcon = new Line();
                    break;
            }

            SetTopLeft(m_ShapeIcon, new Point(_start, _start));
            SetBottomRight(m_ShapeIcon, new Point(_end, _end));

            m_ShapeIcon.Stroke = m_Stroke;
            m_ShapeIcon.StrokeThickness = m_StrokeThickness;
            m_ShapeIcon.StrokeDashArray = new DoubleCollection(1);

        }

        public SimpleShapeDrawerVM(ImageViewer_ViewModel _ImageViewer)
        {
            p_State = false;
            m_Shape = SimpleShape.Rectangle;
            p_ImageViewer = _ImageViewer;
            p_KeyList = new Key[3];
            p_KeyList[0] = Key.None;
            p_KeyList[1] = Key.None;
            p_KeyList[2] = Key.None;
        }

        #region ProcessStart
        public override void DrawStart()
        {

            if (p_State && m_Process == process.None)
            {
                m_Process = process.Start;
                ProcessStart();
                m_Process = process.Drawing;
            }
        }

        public void ProcessStart()
        {
            Shape tempShape;
            switch (m_Shape)
            {
                default:
                    tempShape = new Rectangle();
                    break;
                case SimpleShape.Ellipse:
                    tempShape = new Ellipse();
                    break;
                case SimpleShape.Rectangle:
                    tempShape = new Rectangle();
                    break;
                case SimpleShape.Line:
                    tempShape = new Line();
                    break;
            }
            MakeStartpointOfShape(tempShape);
        }

        public void MakeStartpointOfShape<T>(T element) where T : Shape
        {

            element.Stroke = m_Stroke;
            element.StrokeThickness = m_StrokeThickness;
            element.StrokeDashArray = m_StrokeDashArray;

            //ClickPos = GetCanvasPos();

            m_ClickPos = new Point(p_ImageViewer.p_View_Rect.X + p_ImageViewer.p_MouseX * p_ImageViewer.p_View_Rect.Width / p_ImageViewer.p_CanvasWidth,
                                   p_ImageViewer.p_View_Rect.Y + p_ImageViewer.p_MouseY * p_ImageViewer.p_View_Rect.Height / p_ImageViewer.p_CanvasHeight);

            m_StartPos = GetCanvasPoint(m_ClickPos);
            m_EndPos = m_StartPos;


            SetTopLeft(element, m_StartPos);


            m_Element.Clear();
            foreach (UIElement myObj in m_ListShape)
            {
                m_Element.Add(myObj);
            }

            m_Element.Add(element);
            m_SelectedShape = (Shape)element;
        }
        #endregion

        #region ProcessDoing
        public override void Drawing()
        {
            if (p_State && m_Process == process.Drawing)
                ProcessDoing();
        }

        public void ProcessDoing()
        {
            ProcessDoingShape(m_SelectedShape);
        }

        public void ProcessDoingShape<T>(T element) where T : Shape
        {

            m_MousePos = new Point(p_ImageViewer.p_View_Rect.X + p_ImageViewer.p_MouseX * p_ImageViewer.p_View_Rect.Width / p_ImageViewer.p_CanvasWidth,
                                   p_ImageViewer.p_View_Rect.Y + p_ImageViewer.p_MouseY * p_ImageViewer.p_View_Rect.Height / p_ImageViewer.p_CanvasHeight);


            m_TempRect = new Rect(m_ClickPos, (Size)m_MousePos);
            MakeRect(element, ref m_TempRect);

            m_StartPos = GetCanvasPoint(m_ClickPos);
            m_EndPos = GetCanvasPoint(m_MousePos);

            SetTopLeft(element, m_StartPos);
            SetBottomRight(element, m_EndPos);

        }
        #endregion

        private void MakeRect(Shape _Shape, ref Rect _Rect)
        {
            if (_Shape.GetType() != typeof(Line))
            {
                double temp;
                if (_Rect.X > _Rect.Width)
                {
                    temp = _Rect.X;
                    _Rect.X = _Rect.Width;
                    _Rect.Width = temp;
                }
                if (_Rect.Y > _Rect.Height)
                {
                    temp = _Rect.Y;
                    _Rect.Y = _Rect.Height;
                    _Rect.Height = temp;
                }
            }
        }

        #region DrawEnd()

        public override void DrawEnd()
        {
            if (p_State && m_Process == process.Drawing)
            {
                m_Process = process.End;
                ProcessEnd(m_SelectedShape);
                m_Process = process.None;
            }
        }

        public void ProcessEnd<T>(T element) where T : Shape
        {
            element.StrokeDashArray = new DoubleCollection(1);

            element.MouseDown += new MouseButtonEventHandler(MyElementMouseDownEvent);
            m_ListShape.Add(element);
            m_ListRect.Add(m_TempRect);
            p_ImageViewer.m_HistoryWorker.AddHistory(this, Work.Create, element, m_TempRect);

            // m_TempCRect
        }
        #endregion

        public override void Redrawing()
        {
            Shape tempShape;
            Point TopLeft = new Point();
            Point BottomRight = new Point();
            for (int i = 0; i < m_ListShape.Count; i++)
            {
                tempShape = m_ListShape[i];

                TopLeft = GetCanvasPoint(m_ListRect[i].Location);
                SetTopLeft(tempShape, TopLeft);
                BottomRight = GetCanvasPoint((Point)(m_ListRect[i].Size));
                SetBottomRight(tempShape, BottomRight);

            }
        }

        void SetTopLeft(Shape _shape, Point _point)
        {
            if (_shape.GetType() == typeof(Line))
            {
                ((Line)_shape).X1 = _point.X;
                ((Line)_shape).Y1 = _point.Y;
            }
            else
            {
                Canvas.SetLeft(_shape, _point.X);
                Canvas.SetTop(_shape, _point.Y);
            }
        }
        void SetBottomRight(Shape _shape, Point _point)
        {
            if (_shape.GetType() == typeof(Line))
            {
                ((Line)_shape).X2 = _point.X;
                ((Line)_shape).Y2 = _point.Y;
            }
            else
            {
                Point StartPoint = new Point(Canvas.GetLeft(_shape), Canvas.GetTop(_shape));
                if (_point.X > StartPoint.X)
                {
                    _shape.Width = _point.X - StartPoint.X;
                }
                else
                {
                    Canvas.SetLeft(_shape, _point.X);
                    _shape.Width = StartPoint.X - _point.X;
                }

                if (_point.Y > StartPoint.Y)
                {
                    _shape.Height = _point.Y - StartPoint.Y;
                }
                else
                {
                    Canvas.SetTop(_shape, _point.Y);
                    _shape.Height = StartPoint.Y - _point.Y;
                }
            }
        }

        protected Point GetMemPoint(Point canvas)
        {
            double nX = p_ImageViewer.p_View_Rect.X + canvas.X * p_ImageViewer.p_View_Rect.Width / p_ImageViewer.p_CanvasWidth;
            double nY = p_ImageViewer.p_View_Rect.Y + canvas.Y * p_ImageViewer.p_View_Rect.Height / p_ImageViewer.p_CanvasHeight;
            return new Point(nX, nY);
        }
        protected Point GetCanvasPoint(Point mem)
        {
            if (p_ImageViewer.p_View_Rect.Width > 0 && p_ImageViewer.p_View_Rect.Height > 0)
            {
                double nX = (int)Math.Round((double)(mem.X - p_ImageViewer.p_View_Rect.X) * p_ImageViewer.p_CanvasWidth / p_ImageViewer.p_View_Rect.Width, MidpointRounding.ToEven);
                double nY = (int)Math.Round((double)(mem.Y - p_ImageViewer.p_View_Rect.Y) * p_ImageViewer.p_CanvasHeight / p_ImageViewer.p_View_Rect.Height, MidpointRounding.AwayFromZero);
                return new Point(nX, nY);
            }
            return new Point(0, 0);
        }

        protected void MyElementMouseDownEvent(object sender, MouseButtonEventArgs e)
        {
            if (m_Process == process.None && p_ImageViewer.p_Mode == ImageViewer_ViewModel.DrawingMode.None)
            {
                //foreach (UIElement p_myObj in p_Element)
                //{
                //    if(p_myObj is Shape)
                //        ((Shape)p_myObj).StrokeDashArray = new DoubleCollection(1);
                //}
                ((Shape)sender).StrokeDashArray = m_StrokeDashArray;


                //ModifyTarget = 
                p_ImageViewer.m_ModifyManager.SetModifyData(this, (Shape)sender);

                p_ImageViewer.p_Mode = ImageViewer_ViewModel.DrawingMode.Modify;

            }
        }

        public override void Clear()
        {
            p_State = false;
            m_Process = process.None;
            m_ListRect.Clear();
            m_ListShape.Clear();
            m_Element.Clear();
        }
    }

    public class UniquenessDrawerVM : SimpleShapeDrawerVM
    {

        public TextBlock DrawnTb = new TextBlock();
        public new Rect m_TempRect;

        public UniquenessDrawerVM(ImageViewer_ViewModel _ImageViewer)
            : base(_ImageViewer)
        {

        }

        public override void SetShape(int _ShapeNum)
        {
            if (_ShapeNum != -1)
            {
                m_Shape = (SimpleShape)_ShapeNum;
            }

        }

        public override void DrawStart()
        {
            if (p_State && m_Process == process.None)
            {
                m_Process = process.Start;
                ProcessStart();
                m_Process = process.Drawing;
            }
        }

        public new void ProcessStart()
        {
            switch (m_Shape)
            {
                case SimpleShape.Rectangle:
                    for (int i = 0; i < m_ListShape.Count; i++)
                    {
                        if (m_ListShape[i].GetType() == typeof(Rectangle))
                        {
                            p_ImageViewer.m_HistoryWorker.AddHistory(this, Work.Delete, m_ListShape[i], m_ListRect[i], _Continue: true);
                            m_ListShape.RemoveAt(i);
                            m_ListRect.RemoveAt(i);

                            break;
                        }
                    }
                    break;
                case SimpleShape.Ellipse:
                    for (int i = 0; i < m_ListShape.Count; i++)
                    {
                        if (m_ListShape[i].GetType() == typeof(Ellipse))
                        {
                            p_ImageViewer.m_HistoryWorker.AddHistory(this, Work.Delete, m_ListShape[i], m_ListRect[i], _Continue: true);
                            m_ListShape.RemoveAt(i);
                            m_ListRect.RemoveAt(i);
                            break;
                        }
                    }
                    break;
                case SimpleShape.Line:
                    for (int i = 0; i < m_ListShape.Count; i++)
                    {
                        if (m_ListShape[i].GetType() == typeof(Line))
                        {
                            p_ImageViewer.m_HistoryWorker.AddHistory(this, Work.Delete, m_ListShape[i], m_ListRect[i], _Continue: true);
                            m_ListShape.RemoveAt(i);
                            m_ListRect.RemoveAt(i);
                            break;
                        }
                    }
                    break;
            }
            base.ProcessStart();
        }

        public override void Drawing()
        {
            if (p_State && m_Process == process.Drawing)
                ProcessDoing();
        }

        public new void ProcessDoing()
        {
            ProcessDoingShape(m_SelectedShape);

        }

        public override void DrawEnd()
        {
            if (p_State && m_Process == process.Drawing)
            {
                m_Process = process.End;
                ProcessEnd(m_SelectedShape);
                m_Process = process.None;
            }
        }

        public new void ProcessEnd<T>(T element) where T : Shape
        {
            element.StrokeDashArray = new DoubleCollection(1);
            if (element.GetType() == typeof(Line))
                element.StrokeDashArray = new DoubleCollection { 0.5, 1 };
            //element.StrokeDashArray = new DoubleCollection { 0.5, 1, 0.5, 1, 0.5, 1, 0.5, 1, 0.5, 1, 0.5, 1, 0.5, 1, 0.5, 1, 0.5, 1, 0.5, 1, 1.5, 1 };

            m_ListShape.Add(element);
            m_ListRect.Add(m_TempRect);
            p_ImageViewer.m_HistoryWorker.AddHistory(this, Work.Create, element, m_TempRect);
        }

        public new void ProcessDoingShape<T>(T element) where T : Shape
        {
            base.ProcessDoingShape(element);
            m_TempRect = base.m_TempRect;
            string msg;
            if (element.GetType() == typeof(Line))
                msg = "Length : " + Math.Round(Math.Sqrt(Math.Pow((m_TempRect.Location.X - m_TempRect.Size.Width), 2) + Math.Pow((m_TempRect.Location.Y - m_TempRect.Size.Height), 2)));
            else
                msg = "Width :" + (m_TempRect.BottomRight.X - m_TempRect.TopLeft.X).ToString() + " Height :" + (m_TempRect.BottomRight.Y - m_TempRect.TopLeft.Y).ToString();

            DrawnTb.Text = msg;
        }

    }

    public sealed class OriginDrawerVM : UniquenessDrawerVM
    {
        public CursorDelegate SetStateDelegate;

        public new Rect m_TempRect;
        public bool p_ButtonCheckState = false;

        public OriginDrawerVM(ImageViewer_ViewModel _ImageViewer)
            : base(_ImageViewer)
        {

        }

        public override void SetShape(int _ShapeNum)
        {
            if (_ShapeNum != -1 && p_ButtonCheckState)
            {
                m_Shape = (SimpleShape)_ShapeNum;
                SetIcon();
                SetStateDelegate(true, 0);
            }
            else
            {
                m_ShapeIcon = null;
                SetStateDelegate(false, 0);
            }
        }

        public override void DrawEnd()
        {
            if (p_State && m_Process == process.Drawing)
            {
                m_Process = process.End;
                ProcessEnd(m_SelectedShape);
                m_Process = process.None;

            }
            SetStateDelegate(false, 0);

        }

        public override void DrawStart()
        {

            if (p_ButtonCheckState)
            {
                base.DrawStart();
            }
        }

        public new void ProcessEnd<T>(T element) where T : Shape
        {
            element.MouseDown += new MouseButtonEventHandler(MyElementMouseDownEvent);
            base.ProcessEnd(element);
        }

    }

    public delegate void CursorDelegate(bool _state, int _Count);
    public sealed class PositionDrawerVM : SimpleShapeDrawerVM
    {
        public CursorDelegate SetStateDelegate;
        public bool p_Feature_isChecked;
        //public List<Shape> p_DoneShape = new List<Shape>();

        public int Counter
        {
            get { return m_Element.Count; }
        }

        public PositionDrawerVM(ImageViewer_ViewModel _ImageViewer)
            : base(_ImageViewer)
        {

        }

        public override void SetShape(int _ShapeNum)
        {
            if (_ShapeNum != -1 && Counter < 4 && p_Feature_isChecked)
            {
                base.SetShape(_ShapeNum);
                SetStateDelegate(true, Counter);
            }
            else
            {
                m_ShapeIcon = null;
                SetStateDelegate(false, Counter);
            }
        }


        public override void DrawStart()
        {

            if (p_Feature_isChecked && p_State && m_Process == process.None && Counter < 4)
            {
                m_Process = process.Start;
                ProcessStart();
                m_Process = process.Drawing;
                //SetStateDelegate(true, Counter);
            }
            else
                p_State = false;
        }

        public override void DrawEnd()
        {
            base.DrawEnd();
            //if (p_Feature_isChecked && p_State && m_Process == process.None && Counter < 4)
            //{
            //    SetStateDelegate(true, Counter);
            //}
            //else
            SetStateDelegate(false, Counter);
        }


        public override void Clear()
        {
            m_ListShape.Clear();
            m_ListRect.Clear();
            m_Element.Clear();
            //p_DoneShape.Clear();
            Redrawing();


        }


    }

    //public class WorkData
    //{
    //    public Rect p_RectData = new Rect();
    //    public List<Point> p_PointData = new List<Point>();


    //    public void MakeRect(Point _input1, Point _input2)
    //    {
    //        p_RectData = new Rect();

    //        if (_input1.X > _input2.X)
    //        {
    //            p_RectData.X = _input2.X;
    //            p_RectData.Width = _input1.X - _input2.X;
    //        }
    //        else
    //        {
    //            p_RectData.X = _input1.X;
    //            p_RectData.Width = _input2.X - _input1.X;
    //        }

    //        if (_input1.Y > _input2.Y)
    //        {
    //            p_RectData.Y = _input2.Y;
    //            p_RectData.Height = _input1.Y - _input2.Y;
    //        }
    //        else
    //        {
    //            p_RectData.Y = _input1.Y;
    //            p_RectData.Height = _input2.Y - _input1.Y;
    //        }


    //    }
    //}


}
