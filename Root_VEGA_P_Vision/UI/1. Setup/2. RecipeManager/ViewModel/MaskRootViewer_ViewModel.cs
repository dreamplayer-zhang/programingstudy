﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Root_VEGA_P_Vision.Engineer;
using RootTools;
using RootTools.Memory;
using RootTools_Vision;

namespace Root_VEGA_P_Vision
{
    public class MaskRootViewer_ViewModel : RootViewer_ViewModel
    {
        ToolProcess m_eToolProcess;
        public ToolType m_eToolType;
        ThresholdMode m_eThresholdMode;
        Color m_Color = Colors.Red;

        Stack<Work> m_History;
        Work m_CurrentWork;
        TShape m_CurrentShape;
        RecipeSetting_ViewModel recipeSetting;

        public MaskRootViewer_ViewModel(string imagedata,RecipeSetting_ViewModel recipeSetting)
        {
            MemoryTool Memory = GlobalObjects.Instance.Get<VEGA_P_Vision_Engineer>().ClassMemoryTool();
            p_ImageData = new ImageData(Memory.GetMemory(App.mPool, App.mGroup, imagedata));
            p_ROILayer = new ImageData(Memory.GetMemory(App.mPool, App.mGroup, App.mMaskLayer));
            this.recipeSetting = recipeSetting;
            base.init(p_ImageData);

            m_History = new Stack<Work>();

            rectList = new List<TRect>();
            boxList = new List<TRect>();

            Init_PenCursor();
        }

        #region Override
        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            base.PreviewMouseDown(sender, e);
            if (m_KeyEvent != null)
                if (m_KeyEvent.Key == Key.LeftShift && m_KeyEvent.IsDown)
                    return;

            CPoint memPt = new CPoint(p_MouseMemX, p_MouseMemY);


            if (m_eToolType != ToolType.None)
            {
                m_CurrentWork = new Work(m_eToolType);
            }

            switch (m_eToolProcess)
            {

                case ToolProcess.None:

                    switch (m_eToolType)
                    {
                        case ToolType.None:
                            break;
                        case ToolType.Pen:
                            m_CurrentWork.Points.Add(memPt);
                            //if (m_timer.IsBusy == false) 
                            //    m_timer.RunWorkerAsync();
                            Pen(memPt, _nThickness);
                            m_eToolProcess = ToolProcess.Drawing;
                            break;
                        case ToolType.Eraser:
                            m_CurrentWork.Points.Add(memPt);
                            Eraser(memPt, _nThickness);
                            m_eToolProcess = ToolProcess.Drawing;
                            break;
                        case ToolType.Rect:
                            Start_Rect(memPt);
                            m_eToolProcess = ToolProcess.Drawing;
                            break;
                        case ToolType.Circle:
                            break;
                        case ToolType.Crop:
                            break;
                        case ToolType.Threshold:
                            Start_Rect(memPt);
                            m_eToolProcess = ToolProcess.Drawing;
                            break;
                    }
                    break;
                case ToolProcess.Drawing:
                    switch (m_eToolType)
                    {
                        case ToolType.None:
                            break;
                        case ToolType.Pen:
                            m_CurrentWork.Points.Add(memPt);
                            Pen(memPt, _nThickness);
                            break;
                        case ToolType.Eraser:
                            m_CurrentWork.Points.Add(memPt);
                            Eraser(memPt, _nThickness);
                            break;
                        case ToolType.Rect:
                            break;
                        case ToolType.Circle:
                            break;
                        case ToolType.Crop:
                            break;
                        case ToolType.Threshold:
                            break;
                    }
                    break;
                case ToolProcess.Modifying:
                    break;
                default:
                    break;
            }
        }
        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);
            if (m_KeyEvent != null)
                if (m_KeyEvent.Key == Key.LeftShift && m_KeyEvent.IsDown)
                {
                    PenCursor.Visibility = Visibility.Collapsed;
                    return;
                }

            CPoint memPt = new CPoint(p_MouseMemX, p_MouseMemY);

            m_eToolType = recipeSetting.m_eToolType;
            p_nThickness = recipeSetting.p_nThickness;
            PenCursor.Visibility = Visibility.Collapsed;

            switch (m_eToolProcess)
            {
                case ToolProcess.None:
                    if (m_eToolType == ToolType.Pen || m_eToolType == ToolType.Eraser)
                    {
                        PenCursor.Visibility = Visibility.Visible;
                        Draw_PenCursor();
                    }
                    break;
                case ToolProcess.Drawing:
                    switch (m_eToolType)
                    {
                        case ToolType.None:
                            break;
                        case ToolType.Pen:
                            PenCursor.Visibility = Visibility.Visible;
                            Draw_PenCursor();
                            if (e.LeftButton == MouseButtonState.Pressed)
                            {
                                m_CurrentWork.Points.Add(memPt);
                                Pen(memPt, _nThickness);
                            }
                            break;
                        case ToolType.Eraser:
                            PenCursor.Visibility = Visibility.Visible;

                            Draw_PenCursor();
                            if (e.LeftButton == MouseButtonState.Pressed)
                            {
                                m_CurrentWork.Points.Add(memPt);
                                Eraser(memPt, _nThickness);
                            }
                            break;
                        case ToolType.Rect:
                            if (e.LeftButton == MouseButtonState.Pressed)
                            {
                                Drawing_Rect(memPt);
                            }
                            break;
                        case ToolType.Circle:
                            break;
                        case ToolType.Crop:
                            break;
                        case ToolType.Threshold:
                            if (e.LeftButton == MouseButtonState.Pressed)
                            {
                                Drawing_Rect(memPt);
                            }
                            break;
                    }
                    break;
                case ToolProcess.Modifying:
                    break;
                default:
                    break;
            }

        }
        public override void PreviewMouseUp(object sender, MouseEventArgs e)
        {
            switch (m_eToolProcess)

            {
                case ToolProcess.None:
                    break;
                case ToolProcess.Drawing:
                    switch (m_eToolType)
                    {
                        case ToolType.None:
                            break;
                        case ToolType.Pen:
                            m_eToolProcess = ToolProcess.None;
                            break;
                        case ToolType.Eraser:
                            m_eToolProcess = ToolProcess.None;
                            break;
                        case ToolType.Rect:
                            Done_Rect();
                            m_eToolProcess = ToolProcess.None;
                            break;
                        case ToolType.Circle:
                            break;
                        case ToolType.Crop:
                            break;
                        case ToolType.Threshold:
                            m_eThresholdMode = (ThresholdMode)recipeSetting.p_nThresholdMode;
                            p_nThreshold = recipeSetting.p_nThreshold;
                            Done_Threshold();
                            m_eToolProcess = ToolProcess.None;
                            break;
                        default:
                            break;
                    }
                    m_History.Push(m_CurrentWork);
                    break;
                case ToolProcess.Modifying:
                    break;
                default:
                    break;
            }
        }
        public override void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            base.MouseWheel(sender, e);
            if (m_eToolType == ToolType.Pen || m_eToolType == ToolType.Eraser)
                Draw_PenCursor();
        }
        #endregion

        Queue<CPoint> pointsq = new Queue<CPoint>();

        #region Tool
        private unsafe void Pen(CPoint cPt, int size)
        {
            IntPtr ptrMem = p_ROILayer.GetPtr();
            if (ptrMem == IntPtr.Zero)
                return;
            byte* imagePtr = (byte*)ptrMem.ToPointer();
            uint* fPtr = (uint*)imagePtr;

            //pointsq.Enqueue(cPt);

            Parallel.For(cPt.X, cPt.X + size, i =>
            {
                for (int j = cPt.Y; j < cPt.Y + size; j++)
                {
                    CPoint pixelPt = new CPoint(i, j);

                    int pos = (j * p_ROILayer.p_Size.X + i) /** 4*/;
                    uint clr = m_Color.A;
                    clr = clr << 8;
                    clr += m_Color.R;
                    clr = clr << 8;
                    clr += m_Color.G;
                    clr = clr << 8;
                    clr += m_Color.B;
                    fPtr[pos] += clr;
                }
            });
            SetLayerSource();
            //SetMaskLayerSource();
        }
        private void Eraser(CPoint cPt, int size)
        {
            Parallel.For(cPt.X, cPt.X + size, i =>
            {
                for (int j = cPt.Y; j < cPt.Y + size; j++)
                {
                    CPoint pixelPt = new CPoint(i, j);
                    DrawPixelBitmap(pixelPt, 0, 0, 0, 0);
                }
            });
            SetLayerSource();
        }
        private void Start_Rect(CPoint cPt)
        {
            Brush brush = new SolidColorBrush(m_Color);
            m_CurrentShape = new TRect(brush, 2, 0.5);
            TRect rect = m_CurrentShape as TRect;

            rect.MemPointBuffer = cPt;
            rect.MemoryRect.Left = cPt.X;
            rect.MemoryRect.Top = cPt.Y;

            p_UIElement.Add(rect.CanvasRect);
        }
        private void Drawing_Rect(CPoint cPt)
        {
            TRect rect = m_CurrentShape as TRect;

            if (rect.MemPointBuffer.X > cPt.X)
            {
                rect.MemoryRect.Left = cPt.X;
                rect.MemoryRect.Right = rect.MemPointBuffer.X;
            }
            else
            {
                rect.MemoryRect.Left = rect.MemPointBuffer.X;
                rect.MemoryRect.Right = cPt.X;
            }
            if (rect.MemPointBuffer.Y > cPt.Y)
            {
                rect.MemoryRect.Top = cPt.Y;
                rect.MemoryRect.Bottom = rect.MemPointBuffer.Y;
            }
            else
            {
                rect.MemoryRect.Top = rect.MemPointBuffer.Y;
                rect.MemoryRect.Bottom = cPt.Y;
            }

            double pixSizeX = (double)p_CanvasWidth / (double)p_View_Rect.Width;
            double pixSizeY = (double)p_CanvasHeight / (double)p_View_Rect.Height;

            CPoint LT = new CPoint(rect.MemoryRect.Left, rect.MemoryRect.Top);
            CPoint RB = new CPoint(rect.MemoryRect.Right, rect.MemoryRect.Bottom);

            Point canvasLT = new Point(GetCanvasDoublePoint(LT).X, GetCanvasDoublePoint(LT).Y);
            Point canvasRB = new Point(GetCanvasDoublePoint(RB).X, GetCanvasDoublePoint(RB).Y);

            Canvas.SetLeft(rect.CanvasRect, canvasLT.X - pixSizeX / 2);
            Canvas.SetTop(rect.CanvasRect, canvasLT.Y - pixSizeY / 2);

            rect.CanvasRect.Width = Math.Abs(canvasRB.X - canvasLT.X + pixSizeX);
            rect.CanvasRect.Height = Math.Abs(canvasRB.Y - canvasLT.Y + pixSizeY);
        }
        private void Done_Rect()
        {
            TRect rect = m_CurrentShape as TRect;
            rect.MemoryRect.Left = rect.MemoryRect.Left;
            rect.MemoryRect.Right = rect.MemoryRect.Right;
            rect.MemoryRect.Top = rect.MemoryRect.Top;
            rect.MemoryRect.Bottom = rect.MemoryRect.Bottom;

            Parallel.For(rect.MemoryRect.Top, rect.MemoryRect.Bottom + 1, y =>
            {
                for (int x = rect.MemoryRect.Left; x <= rect.MemoryRect.Right; x++)
                {
                    CPoint pixelPt = new CPoint(x, y);
                    m_CurrentWork.Points.Add(pixelPt);
                    DrawPixelBitmap(pixelPt, m_Color.R, m_Color.G, m_Color.B, m_Color.A);
                }
            });
            SetLayerSource();
            m_History.Push(m_CurrentWork);
            p_UIElement.Remove(rect.CanvasRect);
        }
        private unsafe void Done_Threshold()
        {
            TRect rect = m_CurrentShape as TRect;

            CRect ThresholdRect = new CRect(rect.MemoryRect.Left, rect.MemoryRect.Top, rect.MemoryRect.Right + 1, rect.MemoryRect.Bottom + 1);
            IntPtr ptr = p_ImageData.GetPtr();
            for (int i = ThresholdRect.Height - 1; i >= 0; i--)
            {
                byte* gv = (byte*)((IntPtr)((long)ptr + ThresholdRect.Left * p_ImageData.p_nByte + ((long)i + ThresholdRect.Top) * p_ImageData.p_Stride));
                for (int j = 0; j < ThresholdRect.Width; j++)
                {

                    if (m_eThresholdMode == ThresholdMode.Down)
                    {
                        if (gv[j] < _nThreshold)
                        {
                            CPoint pixelPt = new CPoint(ThresholdRect.Left + j, ThresholdRect.Top + i);
                            m_CurrentWork.Points.Add(pixelPt);
                            DrawPixelBitmap(pixelPt, m_Color.R, m_Color.G, m_Color.B, m_Color.A);
                        }
                    }
                    else if (m_eThresholdMode == ThresholdMode.Up)
                    {
                        if (gv[j] > _nThreshold)
                        {
                            CPoint pixelPt = new CPoint(ThresholdRect.Left + j, ThresholdRect.Top + i);
                            m_CurrentWork.Points.Add(pixelPt);
                            DrawPixelBitmap(pixelPt, m_Color.R, m_Color.G, m_Color.B, m_Color.A);
                        }
                    }
                }
            }
            SetLayerSource();



            p_UIElement.Remove(rect.CanvasRect);
        }

        #endregion

        #region Property

        public int p_nThickness
        {
            get
            {
                return _nThickness;
            }
            set
            {
                SetProperty(ref _nThickness, value);
            }
        }
        private int _nThickness = 5;

        public int p_nThreshold
        {
            get
            {
                return _nThreshold;
            }
            set
            {
                SetProperty(ref _nThreshold, value);
            }
        }
        private int _nThreshold = 50;

        #endregion

        #region Pen/Eraser Cursor
        Rectangle PenCursor;
        void Init_PenCursor()
        {
            if (p_ViewElement.Contains(PenCursor))
                p_ViewElement.Remove(PenCursor);

            PenCursor = new Rectangle();
            PenCursor.Opacity = 0.5;
            PenCursor.Fill = Brushes.LightCoral;
            PenCursor.Stroke = Brushes.LightCoral;
            PenCursor.StrokeThickness = 1;
            PenCursor.StrokeDashArray = new DoubleCollection { 3, 4 };
            p_ViewElement.Add(PenCursor);

        }
        void Draw_PenCursor()
        {
            if (PenCursor == null)
                return;

            double pixSizeX = p_CanvasWidth / (double)p_View_Rect.Width;
            double pixSizeY = p_CanvasHeight / (double)p_View_Rect.Height;
            PenCursor.Width = pixSizeX * _nThickness;
            PenCursor.Height = pixSizeY * _nThickness;
            Point newPt = GetCanvasDoublePoint(new CPoint(p_MouseMemX, p_MouseMemY));
            Canvas.SetLeft(PenCursor, newPt.X - pixSizeX / 2);
            Canvas.SetTop(PenCursor, newPt.Y - pixSizeY / 2);

        }
        #endregion

        #region ICommand
        public ICommand cmdClear
        {
            get
            {
                return new RelayCommand(() =>
                {
                    IntPtr ptrMem = p_ROILayer.GetPtr();
                    if (ptrMem == IntPtr.Zero)
                        return;

                    byte[] buf = new byte[p_ROILayer.p_Size.X * p_ROILayer.GetBytePerPixel()];
                    for (int i = 0; i < p_ROILayer.p_Size.Y; i++)
                    {
                        Marshal.Copy(buf, 0, (IntPtr)((long)ptrMem + (long)p_ROILayer.p_Size.X * p_ROILayer.GetBytePerPixel() * i), buf.Length);
                    }
                    SetLayerSource();
                });
            }
        }
        #endregion

        #region Draw Method
        List<TRect> rectList;
        List<TRect> boxList;

        public void RemoveObjectsByTag(string tag)
        {
            p_DrawElement.Clear();

            List<TRect> newRectList = new List<TRect>();

            foreach (TRect rt in rectList)
            {
                if ((string)rt.UIElement.Tag != tag)
                {
                    newRectList.Add(rt);
                    p_DrawElement.Add(rt.UIElement);
                }
            }

            rectList = newRectList;
        }
        public void ClearObjects()
        {
            rectList.Clear();
            p_DrawElement.Clear();
        }
        public class DrawDefines
        {
            public static int RectTickness = 4;
            public static int BoxTickness = 1;
        }
        public void AddDrawRect(CPoint leftTop, CPoint rightBottom, SolidColorBrush color = null, string tag = "")
        {
            if (color == null)
            {
                color = Brushes.Yellow;
            }

            CPoint canvasLeftTop = GetCanvasPoint(leftTop);
            CPoint canvasRightBottom = GetCanvasPoint(new CPoint(rightBottom));

            Rectangle rt = new Rectangle();
            rt.Width = canvasRightBottom.X - canvasLeftTop.X;
            rt.Height = canvasRightBottom.Y - canvasLeftTop.Y;

            rt.Stroke = color;
            rt.StrokeThickness = DrawDefines.RectTickness;
            rt.Opacity = 1;
            rt.Tag = tag.Clone();

            Canvas.SetLeft(rt, canvasLeftTop.X);
            Canvas.SetTop(rt, canvasLeftTop.Y);

            TRect tRect = new TRect();
            tRect.UIElement = rt;
            tRect.MemoryRect.Left = leftTop.X;
            tRect.MemoryRect.Top = leftTop.Y;
            tRect.MemoryRect.Right = rightBottom.X;
            tRect.MemoryRect.Bottom = rightBottom.Y;

            rectList.Add(tRect);

            p_DrawElement.Add(rt);
        }
        public void AddDrawRectList(List<CRect> rectList, SolidColorBrush color = null, string tag = "")
        {
            foreach (CRect rect in rectList)
            {
                AddDrawRect(rect, color, tag);
            }
        }
        public void AddDrawRect(CRect rect, SolidColorBrush color = null, string tag = "")
        {
            if (color == null)
            {
                color = Brushes.Yellow;
            }

            CPoint canvasLeftTop = GetCanvasPoint(new CPoint(rect.Left, rect.Top));
            CPoint canvasRightBottom = GetCanvasPoint(new CPoint(rect.Right, rect.Bottom));

            Rectangle rt = new Rectangle();
            rt.Width = canvasRightBottom.X - canvasLeftTop.X;
            rt.Height = canvasRightBottom.Y - canvasLeftTop.Y;

            rt.Stroke = color;
            rt.StrokeThickness = DrawDefines.RectTickness;
            rt.Opacity = 1;
            rt.Tag = tag.Clone();

            Canvas.SetLeft(rt, canvasLeftTop.X);
            Canvas.SetTop(rt, canvasLeftTop.Y);

            TRect tRect = new TRect();
            tRect.UIElement = rt;
            tRect.MemoryRect.Left = rect.Left;
            tRect.MemoryRect.Top = rect.Top;
            tRect.MemoryRect.Right = rect.Right;
            tRect.MemoryRect.Bottom = rect.Bottom;

            rectList.Add(tRect);
            p_DrawElement.Add(rt);
        }

        #endregion
    }
}
