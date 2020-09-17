using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RootTools
{
    public class OriginTool_ViewModel :RootViewer_ViewModel
    {

        public event addOrigin AddOrigin;
        public delegate void addOrigin(object e);
        public event addPitch AddPitch;
        public delegate void addPitch(object e);
        public CPoint BoxOffset = new CPoint();
        CPoint viewMemPoint = new CPoint();
        CPoint PitchPoint = new CPoint();

        Grid Origin_UI;
        Grid Pitch_UI;
        OriginProcess eOriginProcess;

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
                AddOrigin(m_OriginPoint);
                AddOriginPoint(m_OriginPoint, Brushes.Red);
                return m_OriginPoint;
            }
            set
            {
                SetProperty(ref m_OriginPoint, value);
            }
        }
        private CPoint m_PitchSize = new CPoint();
        public CPoint p_PitchSize
        {
            get
            {
                AddPitch(PitchPoint);
                AddPitchPoint(PitchPoint, Brushes.Green);
                return m_PitchSize;
            }
            set
            {
                SetProperty(ref m_PitchSize, value);
            }
        }
        #endregion
        

        public OriginTool_ViewModel(ImageData image = null, IDialogService dialogService = null)
        {
            base.init(image, dialogService);
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

                    if (PitchPoint.X != 0 && PitchPoint.Y != 0)
                    {
                        newX = Math.Abs(PitchPoint.X - p_OriginPoint.X);
                        newY = Math.Abs(PitchPoint.Y - p_OriginPoint.Y);
                        p_PitchSize = new CPoint(newX, newY);
                    }
                    break;
                case OriginProcess.Pitch:
                    AddPitchPoint(OriginMemPt, Brushes.Green);
                    PitchPoint = OriginMemPt;
                    newX = Math.Abs(PitchPoint.X - m_OriginPoint.X);
                    newY = Math.Abs(PitchPoint.Y - m_OriginPoint.Y);
                    p_PitchSize = new CPoint(newX, newY);

                    p_UsePitch = false;
                    p_Cursor = Cursors.Arrow;
                    eOriginProcess = OriginProcess.None;
                    AddPitch(PitchPoint);
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


        private void AddOriginPoint(CPoint originMemPt, Brush color)
        {
            if (p_ViewElement.Contains(Origin_UI))
                p_ViewElement.Remove(Origin_UI);

            if(originMemPt.X == 0 && originMemPt.Y == 0)
                return;

            if (originMemPt.X < BoxOffset.X || originMemPt.Y < BoxOffset.Y)
                return;

            CPoint viewPt = originMemPt - BoxOffset;
            CPoint canvasPt = GetCanvasPoint(viewPt);

            Origin_UI = new Grid();
            Origin_UI.Tag = originMemPt;
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
        }
        private void AddPitchPoint(CPoint originMemPt, Brush color)
        {
            if (p_ViewElement.Contains(Pitch_UI))
                p_ViewElement.Remove(Pitch_UI);

            if (originMemPt.X < BoxOffset.X || originMemPt.Y < BoxOffset.Y)
                return;

            CPoint viewPt = originMemPt - BoxOffset;
            CPoint canvasPt = GetCanvasPoint(viewPt);
            Pitch_UI = new Grid();
            Pitch_UI.Tag = originMemPt;
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
        }
        private void RedrawShape()
        {
            if (p_ViewElement.Contains(Origin_UI))
            {
                CPoint memPtPitchBOX = Origin_UI.Tag as CPoint;
                AddOriginPoint(m_OriginPoint, Brushes.Red);
            }
            if (p_ViewElement.Contains(Pitch_UI))
            {
                CPoint memPtPitchBOX = Pitch_UI.Tag as CPoint;
                AddPitchPoint(memPtPitchBOX, Brushes.Green);
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
            PitchPoint = new CPoint();
            if (p_ViewElement.Contains(Origin_UI))
                p_ViewElement.Remove(Origin_UI);
            if (p_ViewElement.Contains(Pitch_UI))
                p_ViewElement.Remove(Pitch_UI);
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
