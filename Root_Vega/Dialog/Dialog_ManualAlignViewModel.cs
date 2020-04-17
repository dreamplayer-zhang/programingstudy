using RootTools;
using RootTools.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace Root_Vega
{
    class Dialog_ManualAlignViewModel : ObservableObject, IDialogRequestClose
    {
        #region Property
        private bool AlignStartPoint = false;
        private bool AlignEndPoint = false;
        private Point MemStart;
        private Point MemEnd;

        private string _AlignText;
        public string AlignText
        {
            get { return _AlignText; }
            set
            {

                SetProperty(ref _AlignText, value);
            }
        }

        private int _vm_X1;
        public int vm_X1
        {
            get { return _vm_X1; }
            set
            {

                SetProperty(ref _vm_X1, value);
            }
        }

        private int _vm_Y1;
        public int vm_Y1
        {
            get { return _vm_Y1; }
            set
            {

                SetProperty(ref _vm_Y1, value);
            }
        }
        private int _vm_X2;
        public int vm_X2
        {
            get { return _vm_X2; }
            set
            {
                SetProperty(ref _vm_X2, value);

            }
        }
        private int _vm_Y2;
        public int vm_Y2
        {
            get { return _vm_Y2; }
            set
            {

                SetProperty(ref _vm_Y2, value);
            }
        }
        private System.Windows.Visibility _vm_vi;
        public System.Windows.Visibility vm_vi
        {
            get { return _vm_vi; }
            set
            {

                SetProperty(ref _vm_vi, value);
            }
        }
        private System.Windows.Visibility _vm_v1;
        public System.Windows.Visibility vm_v1
        {
            get { return _vm_v1; }
            set
            {

                SetProperty(ref _vm_v1, value);
            }
        }
        private System.Windows.Visibility _vm_v2;
        public System.Windows.Visibility vm_v2
        {
            get { return _vm_v2; }
            set
            {

                SetProperty(ref _vm_v2, value);
            }
        }

        private System.Windows.Thickness _vm_P1;
        public System.Windows.Thickness vm_P1
        {
            get
            {
                return _vm_P1;
            }
            set
            {
                SetProperty(ref _vm_P1, value);
            }
        }
        private System.Windows.Thickness _vm_P2;
        public System.Windows.Thickness vm_P2
        {
            get
            {
                return _vm_P2;
            }
            set
            {
                SetProperty(ref _vm_P2, value);
            }
        }


        private int _e_X1;
        public int e_X1
        {
            get { return _e_X1; }
            set
            {

                SetProperty(ref _e_X1, value);
            }
        }

        private int _e_Y1;
        public int e_Y1
        {
            get { return _e_Y1; }
            set
            {

                SetProperty(ref _e_Y1, value);
            }
        }
        private int _e_X2;
        public int e_X2
        {
            get { return _e_X2; }
            set
            {
                SetProperty(ref _e_X2, value);
            }
        }
        private int _e_Y2;
        public int e_Y2
        {
            get { return _e_Y2; }
            set
            {

                SetProperty(ref _e_Y2, value);
            }
        }
        private System.Windows.Input.MouseEventArgs _mouseEvent;
        public System.Windows.Input.MouseEventArgs MouseEvent
        {
            get
            {
                return _mouseEvent;
            }
            set
            {
                SetProperty(ref _mouseEvent, value);

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
        #endregion
        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;
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

        ImageData m_imagedata;

        public Dialog_ManualAlignViewModel(MemoryData data)
        {
            if (data == null)
            {
                CloseRequested(this, new DialogCloseRequestedEventArgs(false));
                return;
            }
            m_imagedata = new ImageData(data);
            p_ImageViewer = new ImageViewer_ViewModel(m_imagedata);
            p_ImageViewer.SetImageData(m_imagedata);
            vm_vi = Visibility.Hidden;
            vm_v1 = Visibility.Hidden;
            vm_v2 = Visibility.Hidden;
        }

        private void OnOkButton()
        {
            CloseRequested(this, new DialogCloseRequestedEventArgs(true));
        }

        private void OnCancelButton()
        {
            CloseRequested(this, new DialogCloseRequestedEventArgs(false));
        }

        void MouseLeftButtonDownCommand_function()
        {
            if (KeyEvent != null && KeyEvent.Key == Key.LeftCtrl && KeyEvent.IsDown)
            {
                p_ImageViewer.p_Mode = ImageViewer_ViewModel.DrawingMode.Tool; //이부분 영향이 없는것같음.
                MemStart = new Point(p_ImageViewer.p_View_Rect.X + p_ImageViewer.p_MouseX * p_ImageViewer.p_View_Rect.Width / p_ImageViewer.p_CanvasWidth,
                                    p_ImageViewer.p_View_Rect.Y + p_ImageViewer.p_MouseY * p_ImageViewer.p_View_Rect.Height / p_ImageViewer.p_CanvasHeight);

                if (AlignEndPoint == false)
                {
                    AlignText = string.Format("");

                    AlignDrawing(MemStart, MemStart);

                }
                else if (AlignEndPoint == true)
                {
                    if (vm_vi != System.Windows.Visibility.Visible)
                        vm_vi = System.Windows.Visibility.Visible;

                    AlignDrawing(MemStart, MemEnd);
                    AlignCalculator();
                }
                AlignStartPoint = true;
                p_ImageViewer.p_Mode = ImageViewer_ViewModel.DrawingMode.None;
            }
        }

        void MouseRightButtonDownCommand_function()
        {
            if (KeyEvent != null && KeyEvent.Key == Key.LeftCtrl && KeyEvent.IsDown)
            {
                p_ImageViewer.p_Mode = ImageViewer_ViewModel.DrawingMode.Tool; //이부분 영향이 없는것같음.
                MemEnd = new Point(p_ImageViewer.p_View_Rect.X + p_ImageViewer.p_MouseX * p_ImageViewer.p_View_Rect.Width / p_ImageViewer.p_CanvasWidth,
                                   p_ImageViewer.p_View_Rect.Y + p_ImageViewer.p_MouseY * p_ImageViewer.p_View_Rect.Height / p_ImageViewer.p_CanvasHeight);
                if (AlignStartPoint == false)
                {
                    AlignText = string.Format("");

                    AlignDrawing(MemEnd, MemEnd);
                }
                else if (AlignStartPoint == true)
                {
                    if (vm_vi != System.Windows.Visibility.Visible)
                        vm_vi = System.Windows.Visibility.Visible;

                    AlignDrawing(MemStart, MemEnd);
                    AlignCalculator();
                }
                AlignEndPoint = true;
                p_ImageViewer.p_Mode = ImageViewer_ViewModel.DrawingMode.None;
            }
        }

        #region AlignFunction
        private void AlignRedrawingWhenDrag()
        {
            if (MouseEvent != null && MouseEvent.LeftButton == MouseButtonState.Pressed)
            {
                AlignRedrawing();
            }
        }
        private void AlignRedrawing()
        {
            if (p_ImageViewer.p_Mode == ImageViewer_ViewModel.DrawingMode.None)
                if (AlignStartPoint & AlignEndPoint == true)
                    AlignDrawing(MemStart, MemEnd);
                else if (AlignStartPoint == true)
                    AlignDrawing(MemStart, MemStart);
                else if (AlignEndPoint == true)
                    AlignDrawing(MemEnd, MemEnd);
        }

        private void AlignDrawing(Point Start, Point End)
        {
            double relativeStart_X = (Start.X - p_ImageViewer.p_View_Rect.X) * p_ImageViewer.p_CanvasWidth / p_ImageViewer.p_View_Rect.Width;
            double relativeStart_Y = (Start.Y - p_ImageViewer.p_View_Rect.Y) * p_ImageViewer.p_CanvasHeight / p_ImageViewer.p_View_Rect.Height;

            vm_X1 = (int)relativeStart_X;
            vm_Y1 = (int)relativeStart_Y;
            e_X1 = (int)relativeStart_X - 5;
            e_Y1 = (int)relativeStart_Y - 5;

            if (relativeStart_X > p_ImageViewer.p_CanvasWidth || relativeStart_Y > p_ImageViewer.p_CanvasHeight)
                vm_v1 = Visibility.Hidden;
            else
                vm_v1 = Visibility.Visible;


            double relativeEnd_X = (End.X - p_ImageViewer.p_View_Rect.X) * p_ImageViewer.p_CanvasWidth / p_ImageViewer.p_View_Rect.Width;
            double relativeEnd_Y = (End.Y - p_ImageViewer.p_View_Rect.Y) * p_ImageViewer.p_CanvasHeight / p_ImageViewer.p_View_Rect.Height;

            vm_X2 = (int)relativeEnd_X;
            vm_Y2 = (int)relativeEnd_Y;
            e_X2 = (int)relativeEnd_X - 5;
            e_Y2 = (int)relativeEnd_Y - 5;

            if (relativeEnd_X > p_ImageViewer.p_CanvasWidth || relativeEnd_Y > p_ImageViewer.p_CanvasHeight)
                vm_v2 = Visibility.Hidden;
            else
                vm_v2 = Visibility.Visible;


        }

        private void AlignCalculator()
        {
            double dx, dy;

            dx = MemEnd.X - MemStart.X;

            dy = MemEnd.Y - MemStart.Y;
            if (dy < 0)
                dy = -dy;
            else
                dx = -dx;

            AlignText = string.Format(Math.Round((180 * Math.Atan(dx / dy) / Math.PI), 3).ToString() + "°");

        }
        #endregion

        #region RelayCommand
        public RelayCommand OkCommand
        {
            get
            {
                return new RelayCommand(OnOkButton);
            }
        }
        public RelayCommand CancelCommand
        {
            get
            {
                return new RelayCommand(OnCancelButton);
            }
        }
        #endregion

        #region ICommand
        public ICommand MouseLeftButtonDownCommand
        {
            get
            {
                return new RelayCommand(MouseLeftButtonDownCommand_function);
            }
        }

        public ICommand MouseRightButtonDownCommand
        {
            get
            {
                return new RelayCommand(MouseRightButtonDownCommand_function);
            }
        }

        public ICommand MouseMoveCommand
        {
            get
            {

                return new RelayCommand(AlignRedrawingWhenDrag);
            }
        }

        public ICommand MouseWheelCommand
        {
            get
            {
                return new RelayCommand(AlignRedrawing);
            }
        }
        #endregion





    }
}
