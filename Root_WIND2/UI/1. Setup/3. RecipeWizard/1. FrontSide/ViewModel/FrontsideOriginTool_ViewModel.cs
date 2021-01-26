using RootTools;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using RootTools_Vision;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

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


        public FrontsideOriginTool_ViewModel()
        {
            base.init();
            p_VisibleMenu = System.Windows.Visibility.Collapsed;
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

                OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();
                originRecipe.OriginX = m_OriginPoint.X;
                originRecipe.OriginY = m_OriginPoint.Y;
                return m_OriginPoint;
            }
            set
            {
                OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();
                originRecipe.OriginX = value.X;
                originRecipe.OriginY = value.Y;

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

                // Recipe
                OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();

                originRecipe.DiePitchX = m_PitchSize.X;
                originRecipe.DiePitchY = m_PitchSize.Y;

                return m_PitchSize;
            }
            set
            {
                OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();
                originRecipe.DiePitchX = value.X;
                originRecipe.DiePitchY = value.Y;

                SetProperty(ref m_PitchSize, value);
            }
        }
        private CPoint m_Padding = new CPoint();
        public CPoint p_Padding
        {
            get
            {
                DrawOriginArea(m_Padding);

                OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();
                originRecipe.InspectionBufferOffsetX = m_Padding.X;
                originRecipe.InspectionBufferOffsetY = m_Padding.Y;

                return m_Padding;
            }
            set
            {
                DrawOriginArea(value);

                OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();
                originRecipe.InspectionBufferOffsetX = value.X;
                originRecipe.InspectionBufferOffsetY = value.Y;

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

            double pixSizeX = p_CanvasWidth / p_View_Rect.Width;
            double pixSizeY = p_CanvasHeight / p_View_Rect.Height;
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
                line1.StrokeThickness = 3;
                line1.Stretch = Stretch.Fill;

                Line line2 = new Line();
                line2.X1 = 0;
                line2.Y1 = 1;
                line2.X2 = 1;
                line2.Y2 = 0;
                line2.Stroke = Brushes.Red;
                line2.StrokeThickness = 3;
                line2.Stretch = Stretch.Fill;

                Origin_UI.Children.Add(line1);
                Origin_UI.Children.Add(line2);
                #endregion
            }

            Canvas.SetLeft(Origin_UI, canvasPt.X - pixSizeX/2 - 10);
            Canvas.SetTop(Origin_UI, canvasPt.Y + pixSizeY/2 - 10);


            if (p_ViewElement.Contains(Origin_UI))
                p_ViewElement.Remove(Origin_UI);

            
            p_ViewElement.Add(Origin_UI); // UI

            // Recipe
            RecipeFront recipe = GlobalObjects.Instance.Get<RecipeFront>();
            OriginRecipe originRecipe = recipe.GetItem<OriginRecipe>();

            //recipe.LoadMasterImage();

            originRecipe.OriginX = memPt.X;
            originRecipe.OriginY = memPt.Y;
        }
        private void DrawPitchPoint(CPoint memPt)
        {
            if (memPt.X == 0 || memPt.Y == 0)
                return;

            double pixSizeX = p_CanvasWidth / p_View_Rect.Width;
            double pixSizeY = p_CanvasHeight / p_View_Rect.Height;
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
                line1.StrokeThickness = 3;
                line1.Stretch = Stretch.Fill;
                Line line2 = new Line();
                line2.X1 = 0;
                line2.Y1 = 1;
                line2.X2 = 1;
                line2.Y2 = 0;
                line2.Stroke = Brushes.Green;
                line2.StrokeThickness = 3;
                line2.Stretch = Stretch.Fill;
                Pitch_UI.Children.Add(line1);
                Pitch_UI.Children.Add(line2);
                #endregion
            }
            Canvas.SetLeft(Pitch_UI, canvasPt.X + pixSizeX/2 - 10);
            Canvas.SetTop(Pitch_UI, canvasPt.Y - pixSizeY/2 - 10);

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

            if (p_View_Rect.Width == 0) return;
            if (p_View_Rect.Height == 0) return;

            double pixSizeX = p_CanvasWidth / p_View_Rect.Width;
            double pixSizeY = p_CanvasHeight / p_View_Rect.Height;

            CPoint LT = new CPoint(viewOriginPt.X - padding.X, viewPitchPt.Y - padding.Y);
            CPoint RB = new CPoint(viewPitchPt.X + padding.X, viewOriginPt.Y + padding.Y);

            CPoint canvasLT = new CPoint(GetCanvasPoint(LT));
            CPoint canvasRB = new CPoint(GetCanvasPoint(RB));



            Canvas.SetLeft(InspArea.CanvasRect, canvasLT.X - pixSizeX/2);
            Canvas.SetTop(InspArea.CanvasRect, canvasLT.Y - pixSizeY/2);

            InspArea.CanvasRect.Width = Math.Abs(canvasRB.X - canvasLT.X + pixSizeX);
            InspArea.CanvasRect.Height = Math.Abs(canvasRB.Y - canvasLT.Y + pixSizeY);

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
        public void _saveMasterImage()
        {
            int posX = this.Offset.X;
            int posY = this.Offset.Y;
            int width = this.p_ImageData.p_Size.X;
            int height = this.p_ImageData.p_Size.Y;
            int byteCount = this.p_ImageData.p_nByte;
            byte[] rawdata = this.p_ImageData.GetByteArray();

            RecipeFront recipe = GlobalObjects.Instance.Get<RecipeFront>();

            recipe.SaveMasterImage(posX, posY, width, height, byteCount, rawdata);
        }
        public void _loadMasterImage()
        {
            RecipeFront recipe = GlobalObjects.Instance.Get<RecipeFront>();

            recipe.LoadMasterImage();

            OriginRecipe originRecipe = recipe.GetItem<OriginRecipe>();

            ImageData BoxImageData = new ImageData(originRecipe.MasterImage.Width, originRecipe.MasterImage.Height, originRecipe.MasterImage.ByteCnt);


            BoxImageData.m_eMode = ImageData.eMode.ImageBuffer;
            BoxImageData.SetData(Marshal.UnsafeAddrOfPinnedArrayElement(originRecipe.MasterImage.RawData, 0)
                , new CRect(0, 0, originRecipe.MasterImage.Width, originRecipe.MasterImage.Height)
                , originRecipe.MasterImage.Width, originRecipe.MasterImage.ByteCnt);


            this.Offset = new CPoint(originRecipe.MasterImage.PositionX, originRecipe.MasterImage.PositionY);
            this.p_ImageData = BoxImageData;

            this.SetRoiRect();
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
        public ICommand SaveMasterImage
        {
            get
            {
                return new RelayCommand(_saveMasterImage);
            }
        }
        public ICommand LoadMasterImage
        {
            get
            {
                return new RelayCommand(_loadMasterImage);
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
