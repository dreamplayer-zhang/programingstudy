using RootTools;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using RootTools_Vision;
using System.Diagnostics;

namespace Root_WIND2
{
    public class FrontsideOriginTool_ViewModel : RootViewer_ViewModel
    {
        /// <summary>
        /// BoxTool에서 Box의 Memory Left, Top 좌표 Offset
        /// </summary>
        public CPoint Offset = new CPoint();

        OriginProcess eOriginProcess;

        Grid Origin_UI;
        Grid Pitch_UI;
        TRect InspArea;

        CPoint OriginPoint = new CPoint();
        CPoint PitchPoint = new CPoint();

        Recipe m_Recipe;
        RecipeData_Origin m_RecipeData_Origin;

        public FrontsideOriginTool_ViewModel(Recipe _recipe)
        {
            base.init();
            m_Recipe = _recipe;
            m_RecipeData_Origin = _recipe.GetRecipeData(typeof(RecipeData_Origin)) as RecipeData_Origin;
        }


        #region Delegate
        public event addOrigin AddOrigin;
        public delegate void addOrigin(object e);
        public event addPitch AddPitch;
        public delegate void addPitch(object e);
        public event addArea DelegateInspArea;
        public delegate void addArea(object e);
        #endregion

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
        private CPoint m_OriginPoint = new CPoint();
        public CPoint p_OriginPoint
        {
            get
            {
                DrawOriginPoint(m_OriginPoint);

                if (m_PitchSize.X != 0 && m_PitchSize.Y != 0)
                {
                    DrawOriginArea(m_Padding);
                }

                m_RecipeData_Origin.OriginX = m_OriginPoint.X;
                m_RecipeData_Origin.OriginY = m_OriginPoint.Y;
                return m_OriginPoint;
            }
            set
            {
                m_RecipeData_Origin.OriginX = value.X;
                m_RecipeData_Origin.OriginY = value.Y;

                SetProperty(ref m_OriginPoint, value);
            }
        }
        private CPoint m_PitchSize = new CPoint();
        public CPoint p_PitchSize
        {
            get
            {                
                //if (m_PitchSize.X != 0 || m_PitchSize.Y != 0)
                //{
                //    int x = OriginPoint.X + m_PitchSize.X;
                //    int y = OriginPoint.Y - m_PitchSize.Y;
                //    PitchPoint = new CPoint(x, y);
                //    DrawPitchPoint(PitchPoint);
                //    DrawOriginArea(m_Padding);
                //}
                m_RecipeData_Origin.DiePitchX = m_PitchSize.X;
                m_RecipeData_Origin.DiePitchY = m_PitchSize.Y;

                return m_PitchSize;
            }
            set
            {
                m_RecipeData_Origin.DiePitchX = value.X;
                m_RecipeData_Origin.DiePitchY = value.Y;

                SetProperty(ref m_PitchSize, value);
            }
        }
        private CPoint m_Padding = new CPoint();
        public CPoint p_Padding
        {
            get
            {
                DrawOriginArea(m_Padding);
                m_RecipeData_Origin.InspectionBufferOffsetX = m_Padding.X;
                m_RecipeData_Origin.InspectionBufferOffsetY = m_Padding.Y;

                return m_Padding;
            }
            set
            {
                DrawOriginArea(value);
                m_RecipeData_Origin.InspectionBufferOffsetX = value.X;
                m_RecipeData_Origin.InspectionBufferOffsetY = value.Y;

                SetProperty(ref m_Padding, value);
            }
        }
        #endregion

        public void LoadOriginData(CPoint ptOrigin, CPoint ptDiepitch, CPoint ptPadding)
        {
            p_OriginPoint = ptOrigin;
            p_PitchSize = ptDiepitch;
            p_Padding = ptPadding;
        }

        private void DrawOriginPoint(CPoint memPt)
        {
            if (memPt.X == 0 || memPt.Y == 0)
                return;

            CPoint viewPt = memPt - Offset;
            CPoint canvasPt = GetCanvasPoint(viewPt);

            if (Origin_UI == null)
            {
                Origin_UI = new Grid();
                Origin_UI.Width = 20;
                Origin_UI.Height = 20;
                #region Line
                Line line1 = new Line();
                line1.X1 = 0;
                line1.Y1 = 0;
                line1.X2 = 1;
                line1.Y2 = 1;
                line1.Stroke = Brushes.Red;
                line1.StrokeThickness = 2;
                line1.Stretch = Stretch.Fill;

                Line line2 = new Line();
                line2.X1 = 0;
                line2.Y1 = 1;
                line2.X2 = 1;
                line2.Y2 = 0;
                line2.Stroke = Brushes.Red;
                line2.StrokeThickness = 2;
                line2.Stretch = Stretch.Fill;

                Origin_UI.Children.Add(line1);
                Origin_UI.Children.Add(line2);
                #endregion
            }

            Canvas.SetLeft(Origin_UI, canvasPt.X - 10);
            Canvas.SetTop(Origin_UI, canvasPt.Y - 10);

            var aa = Origin_UI.Children;

            if (p_ViewElement.Contains(Origin_UI))
                p_ViewElement.Remove(Origin_UI);

            
            p_ViewElement.Add(Origin_UI); // UI

            // Recipe
            m_RecipeData_Origin.OriginX = memPt.X; 
            m_RecipeData_Origin.OriginY = memPt.Y;
        }
        private void DrawPitchPoint(CPoint memPt)
        {
            if (memPt.X == 0 || memPt.Y == 0)
                return;

            CPoint viewPt = memPt - Offset;
            CPoint canvasPt = GetCanvasPoint(viewPt);

            if (Pitch_UI == null)
            {
                Pitch_UI = new Grid();
                Pitch_UI.Width = 20;
                Pitch_UI.Height = 20;
                #region Line
                Line line1 = new Line();
                line1.X1 = 0;
                line1.Y1 = 0;
                line1.X2 = 1;
                line1.Y2 = 1;
                line1.Stroke = Brushes.Green;
                line1.StrokeThickness = 2;
                line1.Stretch = Stretch.Fill;
                Line line2 = new Line();
                line2.X1 = 0;
                line2.Y1 = 1;
                line2.X2 = 1;
                line2.Y2 = 0;
                line2.Stroke = Brushes.Green;
                line2.StrokeThickness = 2;
                line2.Stretch = Stretch.Fill;
                Pitch_UI.Children.Add(line1);
                Pitch_UI.Children.Add(line2);
                #endregion
            }
            Canvas.SetLeft(Pitch_UI, canvasPt.X - 10);
            Canvas.SetTop(Pitch_UI, canvasPt.Y - 10);

            if (p_ViewElement.Contains(Pitch_UI))
                p_ViewElement.Remove(Pitch_UI);


            p_ViewElement.Add(Pitch_UI); // UI
        }
        private void DrawOriginArea(CPoint padding)
        {
            if (InspArea == null)
            {
                InspArea = new TRect(Brushes.Yellow, 2, 0.5);
            }
            int left = OriginPoint.X - padding.X;
            int bottom = OriginPoint.Y + padding.Y;
            int right = PitchPoint.X + padding.X;
            int top = PitchPoint.Y - padding.Y;

            //InspArea의 크기가 변할때만 Delegate Call

            if (InspArea.MemoryRect.Left != left || InspArea.MemoryRect.Right != right ||
                InspArea.MemoryRect.Top != top || InspArea.MemoryRect.Bottom != bottom)
            {
                InspArea.MemoryRect.Left = left;
                InspArea.MemoryRect.Bottom = bottom;
                InspArea.MemoryRect.Right = right;
                InspArea.MemoryRect.Top = top;
                DelegateInspArea(InspArea);
            }

            InspArea.MemoryRect.Left = left;
            InspArea.MemoryRect.Bottom = bottom;
            InspArea.MemoryRect.Right = right;
            InspArea.MemoryRect.Top = top;


            CPoint viewOriginPt = OriginPoint - Offset;
            CPoint viewPitchPt = PitchPoint - Offset;

            CPoint LT = new CPoint(viewOriginPt.X - padding.X, viewPitchPt.Y - padding.Y);
            CPoint RB = new CPoint(viewPitchPt.X + padding.X, viewOriginPt.Y + padding.Y);
            CPoint canvasLT = new CPoint(GetCanvasPoint(LT));
            CPoint canvasRB = new CPoint(GetCanvasPoint(RB));

            int width = Math.Abs(canvasRB.X - canvasLT.X);
            int height = Math.Abs(canvasRB.Y - canvasLT.Y);

            Canvas.SetLeft(InspArea.CanvasRect, canvasLT.X);
            Canvas.SetTop(InspArea.CanvasRect, canvasLT.Y);

            InspArea.CanvasRect.Width = width;
            InspArea.CanvasRect.Height = height;

            if (p_ViewElement.Contains(InspArea.CanvasRect))
            {
                p_ViewElement.Remove(InspArea.CanvasRect);
            }
            p_ViewElement.Add(InspArea.CanvasRect);

        }

        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            base.PreviewMouseDown(sender, e);
            if (m_KeyEvent != null)
                if (m_KeyEvent.Key == Key.LeftShift && m_KeyEvent.IsDown)
                    return;

            CPoint CanvasPt = new CPoint(p_MouseX, p_MouseY);
            CPoint MemoryPt = new CPoint(p_MouseMemX, p_MouseMemY);
            int width, height;
            switch (eOriginProcess)
            {
                case OriginProcess.None:
                    break;
                case OriginProcess.Origin:
                    OriginPoint = MemoryPt;
                    p_UseOrigin = false;
                    p_Cursor = Cursors.Arrow;
                    eOriginProcess = OriginProcess.None;

                    p_OriginPoint = MemoryPt; // UI Text
                    DrawOriginPoint(OriginPoint);

                    if (PitchPoint.X != 0 && PitchPoint.Y != 0)
                    {
                        width = Math.Abs(PitchPoint.X - OriginPoint.X);
                        height = Math.Abs(PitchPoint.Y - OriginPoint.Y);
                        p_PitchSize = new CPoint(width, height);
                        DrawOriginArea(m_Padding);
                    }
                    break;
                case OriginProcess.Pitch:
                    if (OriginPoint.X == 0 && OriginPoint.Y == 0)
                    {
                        System.Windows.MessageBox.Show("Set Origin Point First");
                        p_UsePitch = false;
                        p_Cursor = Cursors.Arrow;
                        eOriginProcess = OriginProcess.None;
                        break;
                    }

                    PitchPoint = MemoryPt;
                    p_UsePitch = false;
                    p_Cursor = Cursors.Arrow;
                    eOriginProcess = OriginProcess.None;

                    width = Math.Abs(PitchPoint.X - OriginPoint.X);
                    height = Math.Abs(PitchPoint.Y - OriginPoint.Y);
                    p_PitchSize = new CPoint(width, height); // UI TEXT

                    DrawPitchPoint(PitchPoint);
                    DrawOriginArea(m_Padding);
                    break;
            }
        }
        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);

            p_MouseMemX += Offset.X;
            p_MouseMemY += Offset.Y;
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

        private void RedrawShape()
        {
            if (Origin_UI != null)
            {
                DrawOriginPoint(OriginPoint);
            }
            if (Pitch_UI != null)
            {
                DrawPitchPoint(PitchPoint);
            }
            if (InspArea != null &&
                InspArea.MemoryRect.Left != 0 && InspArea.MemoryRect.Right != 0 &&
                InspArea.MemoryRect.Top != 0 && InspArea.MemoryRect.Bottom != 0)
            {
                DrawOriginArea(m_Padding);
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
            OriginPoint = new CPoint();
            PitchPoint = new CPoint();
            DrawOriginArea(m_Padding);
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

        private enum OriginProcess
        {
            None,
            Origin,
            Pitch,
        }
    }
}
