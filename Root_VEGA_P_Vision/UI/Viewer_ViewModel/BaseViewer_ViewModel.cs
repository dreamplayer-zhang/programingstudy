using Root_VEGA_P_Vision.Engineer;
using RootTools;
using RootTools.Memory;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Root_VEGA_P_Vision
{
    public class BaseViewer_ViewModel : RootViewer_ViewModel
    {
        int selectedIdx;
        string tabName;
        int ROImaskIdx;
        public int ROIMaskIdx
        {
            get => ROImaskIdx;
            set => SetProperty(ref ROImaskIdx, value);
        }
        public int SelectedIdx
        {
            get => selectedIdx;
            set 
            {
                SetProperty(ref selectedIdx, value);
                SetImageSource();
            } 
        }
        public string TabName
        {
            get => tabName;
            set => SetProperty(ref tabName, value);
        }
        public BaseViewer_ViewModel(string imagedata)
        {
            rectList = new List<TRect>();
            p_ImageData = GlobalObjects.Instance.GetNamed<ImageData>(imagedata);
            p_ROILayer = GlobalObjects.Instance.GetNamed<ImageData>(App.mMaskLayer);
            selectedIdx = 0;
            init(p_ImageData);
            TabName = imagedata + ".Inspection";
        }

        #region Override
        public override int p_MouseX
        {
            get
            {
                return m_MouseX;
            }
            set
            {
                if (p_ImgSource != null)
                {
                    if (p_CanvasWidth != 0 && p_CanvasHeight != 0)
                    {
                        if (p_MouseX < p_ImgSource.Width && p_MouseY < p_ImgSource.Height)
                        {
                            p_MouseMemY = p_View_Rect.Y + p_MouseY * p_View_Rect.Height / p_CanvasHeight;
                            p_MouseMemX = p_View_Rect.X + p_MouseX * p_View_Rect.Width / p_CanvasWidth;

                            byte[] pixel = new byte[1];
                            p_ImgSource.CopyPixels(new Int32Rect(p_MouseX, p_MouseY, 1, 1), pixel, 1, 0);
                            p_PixelData = "GV " + pixel[0];
                        }
                    }
                }

                SetProperty(ref m_MouseX, value);
            }
        }
        public void _openImage(int ch)
        {
            if (p_ImageData == null)
            {
                System.Windows.Forms.MessageBox.Show("Image를 열어주세요");
                return;
            }
            _CancelCopy();

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files(*.bmp;*.jpg)|*.bmp;*.jpg";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (m_DialogService != null)
                {
                    var viewModel = new Dialog_ImageOpenViewModel(this as RootViewer_ViewModel);
                    Nullable<bool> result = m_DialogService.ShowDialog(viewModel);
                    if (!result.HasValue || !result.Value)
                    {
                        return;
                    }
                }

                p_ImageData.OpenFile(ofd.FileName, p_CopyOffset, ch);
            }
        }

        public override unsafe void SetImageSource()
        {
            try
            {
                if (p_ImageData != null)
                {
                    System.Drawing.Imaging.PixelFormat format = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
                    format = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;

                    if(p_CanvasWidth>0)
                        m_CanvasWidth = m_CanvasWidth - m_CanvasWidth % 4;

                    int stride = (int)Math.Ceiling((double)p_CanvasWidth / 4) * 4;
                    System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(p_CanvasWidth, p_CanvasHeight, format);
                    ColorPalette palette = bmp.Palette;

                    for (int i = 0; i < 256; i++)
                        palette.Entries[i] = System.Drawing.Color.FromArgb(i, i, i);

                    bmp.Palette = palette;

                    BitmapData bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, p_CanvasWidth, p_CanvasHeight), ImageLockMode.WriteOnly, format);

                    IntPtr pointer = bmpData.Scan0;
                    IntPtr ptrMem;

                    ptrMem = p_ImageData.GetPtr(SelectedIdx);

                    if (ptrMem == IntPtr.Zero)
                        return;

                    int rectX, rectY, rectWidth, rectHeight, sizeX;

                    rectX = p_View_Rect.X;
                    rectY = p_View_Rect.Y;
                    rectWidth = p_View_Rect.Width;
                    rectHeight = p_View_Rect.Height;

                    sizeX = p_ImageData.p_Size.X;

                    byte* arrByte = (byte*)ptrMem.ToPointer();
                    byte* pointerbyte = (byte*)pointer.ToPointer();

                    Parallel.For(0, p_CanvasHeight, (yy) =>
                    {
                        long pix_y = rectY + yy * rectHeight / p_CanvasHeight;
                        for (int xx = 0; xx < p_CanvasWidth; xx++)
                        {
                            long pix_x = rectX + xx * rectWidth / p_CanvasWidth;
                            long idx = pix_x + pix_y * sizeX;
                            byte pixel = arrByte[idx];
                            pointerbyte[yy * p_CanvasWidth + xx] = ApplyContrastAndBrightness(pixel);
                        }
                    });
                    bmp.UnlockBits(bmpData);

                    p_ImgSource = Tools.ConvertBitmapToSource(bmp);
                }
                if (p_ROILayer != null)
                    SetMaskLayerSource();
            }
            catch (Exception ee)
            {
                RootTools_Vision.TempLogger.Write("RootViewer", ee);
                //System.Windows.MessageBox.Show(ee.ToString());
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
                color = System.Windows.Media.Brushes.Yellow;
            }

            CPoint canvasLeftTop = GetCanvasPoint(leftTop);
            CPoint canvasRightBottom = GetCanvasPoint(new CPoint(rightBottom));

            System.Windows.Shapes.Rectangle rt = new System.Windows.Shapes.Rectangle();
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
                color = System.Windows.Media.Brushes.Yellow;
            }

            CPoint canvasLeftTop = GetCanvasPoint(new CPoint(rect.Left, rect.Top));
            CPoint canvasRightBottom = GetCanvasPoint(new CPoint(rect.Right, rect.Bottom));

            System.Windows.Shapes.Rectangle rt = new System.Windows.Shapes.Rectangle();
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
