using RootTools;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using RootTools_Vision;

namespace Root_WIND2
{
    public class OriginTool_ViewModel :RootViewer_ViewModel
    {
        OriginProcess eOriginProcess;
        Grid Origin_UI;
        Grid Pitch_UI;
        TRect InspArea;

        public event addOrigin AddOrigin;
        public delegate void addOrigin(object e);
        public event addPitch AddPitch;
        public delegate void addPitch(object e);
        public event addArea AddArea;
        public delegate void addArea(object e);
        public CPoint BoxOffset = new CPoint();
        CPoint viewMemPoint = new CPoint();
        CPoint OriginPitchPoint = new CPoint();
        

        #region Property
        private bool m_UseOrigin = false;
        public bool p_UseOrigin
        {
            get
            {
                return m_UseOrigin;
            }
            set
            {
                if (value)
                {
                    p_UsePitch = false;
                    eOriginProcess = OriginProcess.Origin;
                    p_Cursor = Cursors.Cross;
                }
                else
                {
                    eOriginProcess = OriginProcess.None;
                    p_Cursor = Cursors.Arrow;
                }
                SetProperty(ref m_UseOrigin, value);
            }
        }
        private bool m_UsePitch = false;
        public bool p_UsePitch
        {
            get
            {
                return m_UsePitch;
            }
            set
            {
                if (value)
                {
                    p_UseOrigin = false;
                    eOriginProcess = OriginProcess.Pitch;
                    p_Cursor = Cursors.Cross;
                }
                else
                {
                    eOriginProcess = OriginProcess.None;
                    p_Cursor = Cursors.Arrow;
                }
                SetProperty(ref m_UsePitch, value);
            }
        }
        private CPoint m_OriginPoint = new CPoint(); // Memory 좌표
        public CPoint p_OriginPoint
        {
            get
            {
                AddOrigin(m_OriginPoint);
                AddOriginPoint(m_OriginPoint, Brushes.Red);
                AddInspArea(m_Offset);
                return m_OriginPoint;
            }
            set
            {
                SetProperty(ref m_OriginPoint, value);
            }
        } // Origin 포인트
        private CPoint m_PitchSize = new CPoint();  // Memory Size
        public CPoint p_PitchSize
        {
            get
            {
                int newX = m_OriginPoint.X + m_PitchSize.X;
                int newY = m_OriginPoint.Y - m_PitchSize.Y;
                OriginPitchPoint = new CPoint(newX, newY);
                AddPitch(OriginPitchPoint);
                AddPitchPoint(OriginPitchPoint, Brushes.Green);
                AddInspArea(m_Offset);
                return m_PitchSize;
            }
            set
            {
                SetProperty(ref m_PitchSize, value);
            }
        } // Origin Pitch Size

        private CPoint m_Offset = new CPoint();
        public CPoint p_Offset
        {
            get
            {
                AddInspArea(m_Offset);
                return m_Offset;
            }
            set
            {
                SetProperty(ref m_Offset, value);
            }
        }
        #endregion

        Recipe m_Recipe;
        RecipeData_Origin m_RecipeData_Origin;
        
        public OriginTool_ViewModel(Recipe _recipe)
        {
            base.init();
            m_Recipe = _recipe;
            m_RecipeData_Origin = _recipe.GetRecipeData(typeof(RecipeData_Origin)) as RecipeData_Origin;
        }

        public OriginTool_ViewModel(Recipe _Recipe, ImageData image = null, IDialogService dialogService = null)
        {
            base.init(image, dialogService);

            m_Recipe = _Recipe;
            //m_RecipeData_Origin = _Recipe.GetRecipeData().m_ReicpeData_Origin;
            m_RecipeData_Origin = _Recipe.GetRecipeData(typeof(RecipeData_Origin)) as RecipeData_Origin;
        }

        
        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            base.PreviewMouseDown(sender, e);
            if (m_KeyEvent != null)
                if (m_KeyEvent.Key == Key.LeftCtrl && m_KeyEvent.IsDown)
                    return;

            CPoint CanvasPt = new CPoint(p_MouseX, p_MouseY);
            CPoint OriginMemPt = new CPoint(p_MouseMemX, p_MouseMemY);
            int newX, newY;
            switch (eOriginProcess)
            {
                case OriginProcess.None:
                    break;
                case OriginProcess.Origin:
                    p_OriginPoint = OriginMemPt;
                    p_UseOrigin = false;
                    p_Cursor = Cursors.Arrow;
                    eOriginProcess = OriginProcess.None;

                    if (OriginPitchPoint.X != 0 && OriginPitchPoint.Y != 0)
                    {
                        newX = Math.Abs(OriginPitchPoint.X - m_OriginPoint.X);
                        newY = Math.Abs(OriginPitchPoint.Y - m_OriginPoint.Y);
                        p_PitchSize = new CPoint(newX, newY);
                    }
                    break;
                case OriginProcess.Pitch:
                    if (m_OriginPoint.X == 0 && m_OriginPoint.Y == 0)
                    {
                        System.Windows.MessageBox.Show("Set Origin Point First");
                        p_UsePitch = false;
                        p_Cursor = Cursors.Arrow;
                        eOriginProcess = OriginProcess.None;
                        break;
                    }

                    OriginPitchPoint = OriginMemPt;
                    newX = Math.Abs(OriginPitchPoint.X - m_OriginPoint.X);
                    newY = Math.Abs(OriginPitchPoint.Y - m_OriginPoint.Y);
                    p_PitchSize = new CPoint(newX, newY);

                    p_UsePitch = false;
                    p_Cursor = Cursors.Arrow;
                    eOriginProcess = OriginProcess.None;

                    //AddInspArea(p_Offset);
                    break;
            }
        }
        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);
            viewMemPoint.X = p_MouseMemX;
            viewMemPoint.Y = p_MouseMemY;

            p_MouseMemX += BoxOffset.X;
            p_MouseMemY += BoxOffset.Y;
        }
        public override void SetRoiRect()
        {
            base.SetRoiRect();
            RedrawShape();
        }
        public override void CanvasMovePoint_Ref(CPoint point, int nX, int nY)
        {
            base.CanvasMovePoint_Ref(point, nX, nY);
            RedrawShape();
        }
        private void AddOriginPoint(CPoint memPt, Brush color)
        {
            if (p_ViewElement.Contains(Origin_UI))
                p_ViewElement.Remove(Origin_UI);

            if(memPt.X == 0 && memPt.Y == 0)
                return;

            if (memPt.X < BoxOffset.X || memPt.Y < BoxOffset.Y)
                return;

            CPoint viewPt = memPt - BoxOffset;
            CPoint canvasPt = GetCanvasPoint(viewPt);

            Origin_UI = new Grid();
            Origin_UI.Tag = memPt;
            Origin_UI.Width = 20;
            Origin_UI.Height = 20;
            Canvas.SetLeft(Origin_UI, canvasPt.X-10);
            Canvas.SetTop(Origin_UI, canvasPt.Y-10);
            Line line1 = new Line();
            line1.X1 = 0;
            line1.Y1 = 0;
            line1.X2 = 1;
            line1.Y2 = 1;
            line1.Stroke = color;
            line1.StrokeThickness = 3;
            line1.Stretch = Stretch.Fill;
            Line line2 = new Line();
            line2.X1 = 0;
            line2.Y1 = 1;
            line2.X2 = 1;
            line2.Y2 = 0;
            line2.Stroke = color;
            line2.StrokeThickness = 3;
            line2.Stretch = Stretch.Fill;

            Origin_UI.Children.Add(line1);
            Origin_UI.Children.Add(line2);
            p_ViewElement.Add(Origin_UI);

            m_RecipeData_Origin.OriginX = memPt.X;
            m_RecipeData_Origin.OriginY = memPt.Y;
        }
        private void AddPitchPoint(CPoint memPt, Brush color)
        {
            if (p_ViewElement.Contains(Pitch_UI))
                p_ViewElement.Remove(Pitch_UI);

            if (memPt.X == 0 && memPt.Y == 0)
                return;

            if (memPt.X < BoxOffset.X || memPt.Y < BoxOffset.Y)
                return;

            CPoint viewPt = memPt - BoxOffset;
            CPoint canvasPt = GetCanvasPoint(viewPt);
            Pitch_UI = new Grid();
            Pitch_UI.Tag = memPt;
            Pitch_UI.Width = 20;
            Pitch_UI.Height = 20;
            Canvas.SetLeft(Pitch_UI, canvasPt.X - 10);
            Canvas.SetTop(Pitch_UI, canvasPt.Y - 10);
            Line line1 = new Line();
            line1.X1 = 0;
            line1.Y1 = 0;
            line1.X2 = 1;
            line1.Y2 = 1;
            line1.Stroke = color;
            line1.StrokeThickness = 3;
            line1.Stretch = Stretch.Fill;
            Line line2 = new Line();
            line2.X1 = 0;
            line2.Y1 = 1;
            line2.X2 = 1;
            line2.Y2 = 0;
            line2.Stroke = color;
            line2.StrokeThickness = 3;
            line2.Stretch = Stretch.Fill;

            Pitch_UI.Children.Add(line1);
            Pitch_UI.Children.Add(line2);
            p_ViewElement.Add(Pitch_UI);


            m_RecipeData_Origin.DiePitchX = viewPt.X;
            m_RecipeData_Origin.DiePitchY = viewPt.Y;
        }
        private void AddInspArea(CPoint offset)
        {
            if(InspArea != null)
                if (p_ViewElement.Contains(InspArea.CanvasRect))
                    p_ViewElement.Remove(InspArea.CanvasRect);
            if (OriginPitchPoint.X == 0 || OriginPitchPoint.Y == 0)
            {
                InspArea = new TRect(Brushes.Yellow, 2, 0.5);
                AddArea(InspArea);
                return;
            }
            if (m_OriginPoint.X == 0 || m_OriginPoint.Y == 0)
            {
                InspArea = new TRect(Brushes.Yellow, 2, 0.5);
                AddArea(InspArea);
                return;
            }

            InspArea = new TRect(Brushes.Yellow, 2, 0.5);
            InspArea.MemoryRect.Left = m_OriginPoint.X - offset.X;
            InspArea.MemoryRect.Top = OriginPitchPoint.Y - offset.Y;
            InspArea.MemoryRect.Right = OriginPitchPoint.X + offset.X;
            InspArea.MemoryRect.Bottom = m_OriginPoint.Y + offset.Y;

            
            CPoint viewOriginPt = m_OriginPoint - BoxOffset;
            CPoint viewPitchPt = OriginPitchPoint - BoxOffset;


            CPoint LT = new CPoint(viewOriginPt.X - offset.X, viewPitchPt.Y - offset.Y);
            CPoint RB = new CPoint(viewPitchPt.X + offset.X, viewOriginPt.Y + offset.Y);
            CPoint canvasLT = new CPoint(GetCanvasPoint(LT));
            CPoint canvasRB = new CPoint(GetCanvasPoint(RB));

            int width = Math.Abs(canvasRB.X - canvasLT.X);
            int height = Math.Abs(canvasRB.Y - canvasLT.Y);

            Canvas.SetLeft(InspArea.CanvasRect, canvasLT.X);
            Canvas.SetTop(InspArea.CanvasRect, canvasLT.Y);
            Canvas.SetRight(InspArea.CanvasRect, canvasRB.X);
            Canvas.SetBottom(InspArea.CanvasRect, canvasRB.Y);

            InspArea.CanvasRect.Width = width;
            InspArea.CanvasRect.Height = height;

            p_ViewElement.Add(InspArea.CanvasRect);
            AddArea(InspArea);

            
            //m_RecipeData_Origin.m_ptABSOrigin = m_OriginPoint;
            //m_RecipeData_Origin.SetOrigin(InspArea.MemoryRect);
            m_RecipeData_Origin.InspectionBufferOffsetX = offset.X;
            m_RecipeData_Origin.InspectionBufferOffsetY = offset.Y;
            
            //m_RecipeData_Origin.SetOrigin(InspArea.MemoryRect);
        }
        private void RedrawShape()
        {
            if (Origin_UI != null)
            {
                CPoint OriginPt = Origin_UI.Tag as CPoint;
                if (OriginPt != null && OriginPt.X != 0 && OriginPt.Y != 0)
                    AddOriginPoint(m_OriginPoint, Brushes.Red);
            }
            if (Pitch_UI != null)
            {
                CPoint PitchPt = OriginPitchPoint;
                if (PitchPt != null && PitchPt.X != 0 && PitchPt.Y != 0)
                    AddPitchPoint(PitchPt, Brushes.Green);
            }
            if (InspArea != null)
            {
                AddInspArea(p_Offset);
            }
        }
        public void _setOriginPoint()
        {
            eOriginProcess = OriginProcess.Origin;
        }
        public void _setPitchPoint()
        {
            eOriginProcess = OriginProcess.Pitch;
        }
        public void _clearOrigin()
        {
            p_OriginPoint = new CPoint();
            p_PitchSize = new CPoint();
            OriginPitchPoint = new CPoint();
            if (p_ViewElement.Contains(Origin_UI))
                p_ViewElement.Remove(Origin_UI);
            if (p_ViewElement.Contains(Pitch_UI))
                p_ViewElement.Remove(Pitch_UI);
            if (p_ViewElement.Contains(InspArea.CanvasRect))
                p_ViewElement.Remove(InspArea.CanvasRect);
        }
        public ICommand SetOriginPoint
        {
            get
            {
                return new RelayCommand(_setOriginPoint);
            }
        }
        public ICommand SetPitchPoint
        {
            get
            {
                return new RelayCommand(_setPitchPoint);
            }
        }
        public ICommand ClearOrigin
        {
            get
            {
                return new RelayCommand(_clearOrigin);
            }
        }
    }
    enum OriginProcess
    {
        None,
        Origin,
        Pitch,
    }
}
