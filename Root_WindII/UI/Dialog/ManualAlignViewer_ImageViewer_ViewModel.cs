using RootTools;
using RootTools.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Root_WindII
{
    public class ManualAlignViewer_ImageViewer_ViewModel : RootViewer_ViewModel
    {
        public class DefineColors
        {
            public static SolidColorBrush LineColor = Brushes.Magenta;
        }

        public ManualAlignViewer_ImageViewer_ViewModel(ImageData imageData, Axis axisRotate)
        {
            this.p_VisibleMenu = System.Windows.Visibility.Collapsed;
            this.init(imageData);

            this.axisRotate = axisRotate;

            InitializeUIElement();
        }

        private Axis axisRotate;

        private double measuredAngle = 0.0;
        public double MeasuredAngle
        {
            get => this.measuredAngle;
            set
            {
                SetProperty(ref this.measuredAngle, value);
            }
        }


        #region [UI Objects]

        private Grid StartPointUI;
        private Grid EndPointUI;
        private Line lineUI;

        private CPoint startMemPoint;
        private CPoint endMemPoint;


        public void InitializeUIElement()
        {
            this.startMemPoint = new CPoint();
            this.endMemPoint = new CPoint();

            this.StartPointUI = new Grid();

            Line line1 = new Line();
            line1.X1 = 0;
            line1.Y1 = -20;
            line1.X2 = 0;
            line1.Y2 = 20;
            line1.Stroke = DefineColors.LineColor;
            line1.StrokeThickness = 3;
            line1.Opacity = 1;

            Line line2 = new Line();
            line2.X1 = -20;
            line2.Y1 = 0;
            line2.X2 = 20;
            line2.Y2 = 0;
            line2.Stroke = DefineColors.LineColor;
            line2.StrokeThickness = 3;
            line2.Opacity = 1;

            StartPointUI.Children.Add(line1);
            StartPointUI.Children.Add(line2);

            this.EndPointUI = new Grid();

            Line line3 = new Line();
            line3.X1 = 0;
            line3.Y1 = -20;
            line3.X2 = 0;
            line3.Y2 = 20;
            line3.Stroke = DefineColors.LineColor;
            line3.StrokeThickness = 3;
            line3.Opacity = 1;

            Line line4 = new Line();
            line4.X1 = -20;
            line4.Y1 = 0;
            line4.X2 = 20;
            line4.Y2 = 0;
            line4.Stroke = DefineColors.LineColor;
            line4.StrokeThickness = 3;
            line4.Opacity = 1;

            EndPointUI.Children.Add(line3);
            EndPointUI.Children.Add(line4);


            this.lineUI = new Line();

            this.lineUI.Stroke = DefineColors.LineColor;
            this.lineUI.StrokeThickness = 3;
            this.lineUI.Opacity = 1;
        }
        #endregion

        #region [Drawing]
        private enum DRAWING_STATE
        {
            NONE = 0,
            FIRST,
            SECOND,
        }

        private DRAWING_STATE drawingState = DRAWING_STATE.NONE;



        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            base.PreviewMouseDown(sender, e);

            if (m_KeyEvent != null)
                if (m_KeyEvent.Key == Key.LeftShift && m_KeyEvent.IsDown)
                    return;

            switch (this.drawingState)
            {
                case DRAWING_STATE.NONE:
                    this.startMemPoint.X = this.p_MouseMemX;
                    this.startMemPoint.Y = this.p_MouseMemY;

                    this.drawingState = DRAWING_STATE.FIRST;
                    break;
                case DRAWING_STATE.FIRST:
                    this.endMemPoint.X = this.p_MouseMemX;
                    this.endMemPoint.Y = this.p_MouseMemY;

                    double dx = this.endMemPoint.Y - this.startMemPoint.Y;
                    double dy = this.endMemPoint.X - this.startMemPoint.X;

                    this.MeasuredAngle = Math.Atan2(dy, dx) *  180.0 / Math.PI;

                    this.drawingState = DRAWING_STATE.SECOND;
                    break;
                case DRAWING_STATE.SECOND:

                    this.startMemPoint.X = 0;
                    this.startMemPoint.Y = 0;

                    this.endMemPoint.X = 0;
                    this.endMemPoint.Y = 0;

                    this.drawingState = DRAWING_STATE.NONE;
                    break;
            }
            RedrawShapes();
        }




        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);
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


        private bool isDrawing = false;
        private void RedrawShapes()
        {
            if (this.isDrawing == true) return;

            this.isDrawing = true;
            //ClearObjects();

            switch (this.drawingState)
            {
                case DRAWING_STATE.NONE:
                    ClearObjects();

                    break;
                case DRAWING_STATE.FIRST:
                    DrawStartPosition();
                    break;
                case DRAWING_STATE.SECOND:
                    DrawStartPosition();
                    DrawEndPosition();
                    DrawLine();
                    break;
            }

            this.isDrawing = false;
        }

        private void DrawStartPosition()
        {
            if (!this.p_ViewElement.Contains(this.StartPointUI))
                this.p_ViewElement.Add(this.StartPointUI);

            CPoint canvasPt = GetCanvasPoint(this.startMemPoint);

            Canvas.SetLeft(this.StartPointUI, canvasPt.X);
            Canvas.SetTop(this.StartPointUI, canvasPt.Y);
        }

        private void DrawEndPosition()
        {
            if (!this.p_ViewElement.Contains(this.EndPointUI))
                this.p_ViewElement.Add(this.EndPointUI);

            CPoint canvasPt = GetCanvasPoint(this.endMemPoint);

            Canvas.SetLeft(this.EndPointUI, canvasPt.X);
            Canvas.SetTop(this.EndPointUI, canvasPt.Y);
        }

        private void DrawLine()
        {
            if (!this.p_ViewElement.Contains(this.lineUI))
                this.p_ViewElement.Add(this.lineUI);

            CPoint canvasStartPt = GetCanvasPoint(this.startMemPoint);
            CPoint canvasEndPt = GetCanvasPoint(this.endMemPoint);

            this.lineUI.X1 = canvasStartPt.X;
            this.lineUI.Y1 = canvasStartPt.Y;

            this.lineUI.X2 = canvasEndPt.X;
            this.lineUI.Y2 = canvasEndPt.Y;
        }

        public void ClearObjects()
        {
            this.p_ViewElement.Clear();
        }
        #endregion

        #region [Command]
        public ICommand btnAlignCommand
        {
            get => new RelayCommand(() =>
            {
                axisRotate.StartShift(axisRotate.p_dRelPos + this.MeasuredAngle * 1000);
                //axisRotate.StartMove(axisRotate.p_dRelPos + this.MeasuredAngle * 1000);
                axisRotate.WaitReady();
            });
        }

        public ICommand btnClearCommand
        {
            get => new RelayCommand(() =>
            {
                ClearObjects();
            });
        }
        #endregion
    }
}
