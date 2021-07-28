using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RootTools.Memory;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Emgu.CV.Dnn;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Collections;
using RootTools_CLR;
using System.Security.Cryptography.X509Certificates;

namespace RootTools
{
    public class ImageData : ObservableObject
    {
        public static string MainFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        private static System.Drawing.Imaging.ColorPalette mono;
        public enum eMode
        {
            MemoryRead,
            ImageBuffer,
            OtherPCMem,
        }
        public eMode m_eMode = eMode.MemoryRead;

        public MemoryTool m_ToolMemory;

        string m_id = "";
        public string p_id
        {
            get
            {
                return m_id;
            }
            set
            {
                SetProperty(ref m_id, value);
            }
        }

        CPoint m_Size = new CPoint();
        public CPoint p_Size
        {
            get
            {
                return m_Size;
            }
            set
            {
                if ((m_Size.X != value.X) && (m_Size.Y != value.Y) && m_eMode == eMode.ImageBuffer)
                {
                    ReAllocate(value, p_nByte * p_nPlane);
                }
                SetProperty(ref m_Size, value);
            }
        }

        int _nByte = 1;
        public int p_nByte
        {
            get
            {
                return _nByte;
            }
            set
            {
                SetProperty(ref _nByte, value);

            }
        }

        int m_nPlane = 1;
        public int p_nPlane
        {
            get
            {
                return m_nPlane;
            }
            set
            {
                SetProperty(ref m_nPlane, value);
            }
        }

        public int GetBytePerPixel()
        {
            return p_nByte * p_nPlane;
        }

        public long p_Stride
        {
            get
            {
                return (long)p_nByte * p_Size.X;
            }
        }

        public IntPtr m_ptrImg;
        public MemoryData m_MemData;
        public byte[] m_aBuf;
        byte[] m_aBufFileOpen;

        ObservableCollection<object> m_element = new ObservableCollection<object>();
        public ObservableCollection<object> p_Element
        {
            get
            {
                return m_element;
            }
            set
            {
                SetProperty(ref m_element, value);
            }
        }

        public delegate void DelegateNoParameter();
        public event DelegateNoParameter OnUpdateImage;
        public event DelegateNoParameter OnCreateNewImage;
        public delegate void DelegateOneInt(int nInt);
        public event DelegateOneInt UpdateOpenProgress;

        public BackgroundWorker Worker_MemoryCopy = new BackgroundWorker();
        public BackgroundWorker Worker_MemoryClear = new BackgroundWorker();


        int m_nProgress = 0;
        public int p_nProgress
        {
            get
            {
                return m_nProgress;
            }
            set
            {
                m_nProgress = value;

                if (UpdateOpenProgress != null)
                    UpdateOpenProgress(m_nProgress);
            }
        }

        public ImageData(int Width, int Height, int nByte = 1)
        {
            m_eMode = eMode.ImageBuffer;
            p_nByte = nByte;
            p_nPlane = 1;
            p_Size = new CPoint(Width, Height);
            ReAllocate(p_Size, nByte);

            var bmp = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

            //Grayscale 이미지를 위한 Pallete 설정
            mono = bmp.Palette;
            System.Drawing.Color[] ent = mono.Entries;

            Parallel.For(0, 256, (j) =>
            {
                System.Drawing.Color b = new System.Drawing.Color();
                b = System.Drawing.Color.FromArgb((byte)j, (byte)j, (byte)j);
                ent[j] = b;
            });
        }

        public ImageData(string sIP, string sPool, string sGroup, string sMem, MemoryTool tool)
        {
            m_eMode = eMode.OtherPCMem;
            m_ToolMemory = tool;
        }
        string m_sPool;
        string m_sGroup;
        string m_sMem;
        public ImageData(string sPool, string sGroup, string sMem, MemoryTool tool, int nPlane, int nByte)
        {
            m_sPool = sPool;
            m_sGroup = sGroup;
            m_sMem = sMem;
            m_eMode = eMode.OtherPCMem;
            p_nPlane = nPlane;
            p_nByte = nByte;
            p_Size = new CPoint(40000, 40000);
            m_ToolMemory = tool;
        }
        //public ImageData(ImageData copyData)
        //{
        //    //this.m_aBuf = copyData.m_aBuf;
        //    m_aBuf = new byte[copyData.m_aBuf.Length];
        //    Buffer.BlockCopy(copyData.m_aBuf, 0, m_aBuf, 0, copyData.m_aBuf.Length);
        //    if (copyData.m_aBufFileOpen != null)
        //    {
        //        m_aBufFileOpen = new byte[copyData.m_aBufFileOpen.Length];
        //        Buffer.BlockCopy(copyData.m_aBufFileOpen, 0, m_aBufFileOpen, 0, copyData.m_aBufFileOpen.Length);
        //    }
        //    this.m_element = copyData.m_element;
        //    this.m_eMode = copyData.m_eMode;
        //    this.p_id = copyData.p_id;
        //    if (copyData.m_MemData != null)
        //        this.m_MemData = copyData.m_MemData;
        //    this.m_nPlane = copyData.m_nPlane;
        //    //this.m_ptrImg =
        //    //this.m_Size = copyData.m_Size;
        //    this.m_sPool = copyData.m_sPool;
        //    this.m_ToolMemory = copyData.m_ToolMemory;
        //    //this.p_Stride = copyData.p_Stride;
        //    this.p_nPlane = copyData.p_nPlane;
        //    this.p_Size = copyData.p_Size;
        //    this.p_nByte = copyData.p_nByte;

        //    //this.m_aBufFileOpen
        //}

        public byte[] GetData(System.Drawing.Rectangle View_Rect, int CanvasWidth, int CanvasHeight)
        {
            return m_ToolMemory.GetOtherMemory(View_Rect, CanvasWidth, CanvasHeight, m_sPool, m_sGroup, m_sMem, p_nByte, p_nPlane);
            //return new byte[5];  // 이게 머여??
        }

        public async Task<byte[]> GetDataAsync(System.Drawing.Rectangle View_Rect, int CanvasWidth, int CanvasHeight)
        {
            Task<byte[]> getMemory = m_ToolMemory.GetOtherMemoryAsync(View_Rect, CanvasWidth, CanvasHeight, m_sPool, m_sGroup, m_sMem, p_nByte, p_nPlane);

            return await getMemory;
        }

        public unsafe void SetData(IntPtr ptr, CRect rect, int stride, int nByte = 1)
        {
            for (int i = 0; i < rect.Height; i++)
            {
                Marshal.Copy((IntPtr)((long)ptr + rect.Left * nByte + ((long)i + (long)rect.Top) * stride), m_aBuf, i * rect.Width * nByte, rect.Width * nByte);
            }
            var asdf = m_aBuf;
            // 병렬처리
            //Parallel.For(rect.Height-1, 0, (i) =>
            //     {
            //         Marshal.Copy((In tPtr)((long)ptr + rect.Left * nByte + ((long)i + (long)rect.Top) * stride * nByte), m_aBuf, i * rect.Width * nByte, rect.Width * nByte);
            //     }
            //);
        }

        public unsafe void SetData_12bit(IntPtr ptr, CRect rect, int stride, int nByte = 2)
        {
            // variable
            byte* memPtr = (byte*)ptr.ToPointer();

            //implement
            for (int y = 0; y < rect.Height; y++)
            {
                byte[] arrByte = new byte[rect.Width];
                for (int x = 0; x < rect.Width; x++)
                {
                    byte b1 = memPtr[(((long)rect.Top + y) * stride + (long)rect.Left + x) * 2 + 0];
                    byte b2 = memPtr[(((long)rect.Top + y) * stride + (long)rect.Left + x) * 2 + 1];
                    ushort us = BitConverter.ToUInt16(new byte[] { b1, b2 }, 0);
                    byte b = (byte)(((double)us / (Math.Pow(2, 16) - 1)) * (Math.Pow(2, 8) - 1));
                    arrByte[x] = b;
                }
                arrByte.CopyTo(m_aBuf, y * rect.Width);
            }

            return;
        }

        public void GetRectData(TRect rect, ImageData boxImage)
        {
            byte[] viewptr = boxImage.m_aBuf;
            CRect rt = rect.MemoryRect;
            rt.X = rect.MemoryRect.Left;
            rt.Y = rect.MemoryRect.Top;
            int Width = rt.Width, Height = rt.Height;
            byte[] ptr = GetData(new System.Drawing.Rectangle(rt.X, rt.Y, rt.Width, rt.Height), rt.Width, rt.Height);

            if(boxImage.GetBytePerPixel() == 1 || boxImage.GetBytePerPixel() == 2 )
            {
                int position = 0;

                for (int i = 0; i < p_Size.Y; i++)
                {
                    Marshal.Copy((IntPtr)((long)GetPtr() + (((long)i) * p_Size.X * p_nByte)), viewptr, position, p_Size.X * p_nByte);
                    position += (p_Size.X * p_nByte);
                }
            }
            else if(boxImage.GetBytePerPixel() == 3)
            {
                int nTerm = Width * Height;

                Parallel.For(0, Height, new ParallelOptions { MaxDegreeOfParallelism = 12 }, (yy) =>
                {
                    {
                    for (int xx = 0; xx < Width; xx++)
                        {
                            viewptr[(yy * Width + xx)* boxImage.GetBytePerPixel()] = ptr[yy * Width + xx];
                            viewptr[(yy * Width + xx) * boxImage.GetBytePerPixel() + 1] = ptr[yy * Width + xx + nTerm];
                            viewptr[(yy * Width + xx) * boxImage.GetBytePerPixel() + 2] = ptr[yy * Width + xx+ nTerm * 2];
                        }
                    }
                });
            }
        }

        public unsafe void SetData(ImageData imgData, CRect rect, int stride, int nByte = 1)
        {
            if (nByte == 1 || nByte == 2)
            {
                IntPtr ptr = imgData.GetPtr();
                for (int i = rect.Height - 1; i >= 0; i--)
                {

                    Marshal.Copy((IntPtr)((long)ptr + rect.Left * nByte + ((long)i + rect.Top) * stride), m_aBuf, i * rect.Width * nByte, rect.Width * nByte);
                }
            }

            else if (nByte == 3)
            {
                if (imgData.m_eMode == eMode.ImageBuffer)
                {
                    if (imgData.m_aBuf == null)
                    {
                        System.Windows.MessageBox.Show("ImageData SetData() 실패\nm_aBuf == null");
                        return;
                    }

                    int idx = 0;
                    long xOffset;
                    for (int r = rect.Top; r < rect.Top + rect.Height; r++)
                    {
                        for (int c = rect.Left; c < rect.Left + rect.Width; c++, idx++)
                        {
                            xOffset = c + (long)r * stride;
                            m_aBuf[nByte * idx + 0] = imgData.m_aBuf[3 * (r * imgData.m_Size.X + c) + 2];
                            m_aBuf[nByte * idx + 1] = imgData.m_aBuf[3 * (r * imgData.m_Size.X + c) + 1];
                            m_aBuf[nByte * idx + 2] = imgData.m_aBuf[3 * (r * imgData.m_Size.X + c) + 0];
                        }
                    }
                }

                else if (imgData.m_eMode == eMode.MemoryRead || imgData.m_eMode == eMode.OtherPCMem)
                {
                    if (imgData.m_MemData == null)
                    {
                        System.Windows.MessageBox.Show("ImageData SetData() 실패\nMemoryData == null");
                        return;
                    }

                    byte* Rptr = (byte*)imgData.m_MemData.GetPtr(0).ToPointer();
                    byte* Gptr = (byte*)imgData.m_MemData.GetPtr(1).ToPointer();
                    byte* Bptr = (byte*)imgData.m_MemData.GetPtr(2).ToPointer();

                    int idx = 0;
                    long xOffset;
                    for (int r = rect.Top; r < rect.Top + rect.Height; r++)
                    {
                        for (int c = rect.Left; c < rect.Left + rect.Width; c++, idx++)
                        {
                            xOffset = c + (long)r * stride;
                            m_aBuf[nByte * idx + 0] = *(Rptr + xOffset);
                            m_aBuf[nByte * idx + 1] = *(Gptr + xOffset);
                            m_aBuf[nByte * idx + 2] = *(Bptr + xOffset);
                        }
                    }
                }
            }
        }
        public unsafe void SetData()
        {

        }

        public byte[] GetByteArray()
        {
            byte[] aBuf = new byte[(long)p_Size.X * p_nByte * p_Size.Y];
            int position = 0;

            for (int i = 0; i < p_Size.Y; i++)
            {
                Marshal.Copy((IntPtr)((long)GetPtr() + (((long)i) * p_Size.X * p_nByte)), aBuf, position, p_Size.X * p_nByte);
                position += (p_Size.X * p_nByte);
            }
            return aBuf;
        }
        public ImageData(MemoryData data)
        {
            if (data == null) return;
            m_eMode = eMode.MemoryRead;
            m_ptrImg = data.GetPtr();
            m_MemData = data;
            p_nPlane = data.p_nCount;
            p_nByte = data.p_nByte;
            p_Size = data.p_sz;

            SetBackGroundWorker();
        }

        public void SetBackGroundWorker()
        {
            Worker_MemoryCopy.DoWork += Worker_MemoryCopy_DoWork;
            Worker_MemoryCopy.RunWorkerCompleted += Worker_MemoryCopy_RunWorkerCompleted;
            Worker_MemoryCopy.WorkerSupportsCancellation = true;
            Worker_MemoryClear.DoWork += Worker_MemoryClear_DoWork;
            Worker_MemoryClear.RunWorkerCompleted += Worker_MemoryClear_RunWorkerCompleted;
            Worker_MemoryClear.WorkerSupportsCancellation = true;
        }

        public void UpdateImage()
        {
            OnUpdateImage();
        }

        public Bitmap GetBitmapToArray(int width, int height, byte[] imageData)
        {
            // need to edit (same with GetByteToBitmap)
            var bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            bmp.Palette = mono;
            using (var stream = new MemoryStream(imageData))
            {
                System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0,
                                                                bmp.Width,
                                                                bmp.Height),
                                                  System.Drawing.Imaging.ImageLockMode.WriteOnly,
                                                  bmp.PixelFormat);
                IntPtr pNative = bmpData.Scan0;
                Marshal.Copy(imageData, 0, pNative, imageData.Length);
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        public Bitmap GetByteToBitmap(int width, int height, byte[] imageData)
        {
            var bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            bmp.Palette = mono;
            using (var stream = new MemoryStream(imageData))
            {
                System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0,
                                                                bmp.Width,
                                                                bmp.Height),
                                                  System.Drawing.Imaging.ImageLockMode.WriteOnly,
                                                  bmp.PixelFormat);
                IntPtr pNative = bmpData.Scan0;
                Marshal.Copy(imageData, 0, pNative, imageData.Length);
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }
        public byte[] GetRectByteArray(CRect rect)
        {
            int position = 0;

            if (rect.Width % 4 != 0)
            {
                rect.Right += 4 - rect.Width % 4;
            }

            byte[] aBuf = new byte[(long)rect.Width * p_nByte * rect.Height];
            //나중에 거꾸로 나왔던것 확인해야 함. 일단 지금은 정순으로 바꿔둠
            for (int i = 0; i < rect.Height; i++)
            {
                Marshal.Copy((IntPtr)((long)GetPtr() + rect.Left + ((long)i + (long)rect.Top) * p_Size.X), aBuf, position, rect.Width * p_nByte);
                position += (rect.Width * p_nByte);
            }
            return aBuf;
        }
        public Bitmap GetRectImage(CRect rect)
        {
            #region TEMP
            //byte[] aBuf = new byte[54 + (1024 * 2) + rect.Width * rect.Height];
            //byte[] temp;
            //int position = 0;
            //temp = BitConverter.GetBytes(Convert.ToUInt16(0x4d42));
            //temp.CopyTo(aBuf, position);
            //position += temp.Length;

            //temp = BitConverter.GetBytes(Convert.ToUInt32(54 + 1024 + rect.Width * rect.Height));
            //temp.CopyTo(aBuf, position);
            //position += temp.Length;

            //temp = BitConverter.GetBytes(Convert.ToUInt16(0));
            //temp.CopyTo(aBuf, position);
            //position += temp.Length;
            //temp = BitConverter.GetBytes(Convert.ToUInt16(0));
            //temp.CopyTo(aBuf, position);
            //position += temp.Length;
            //temp = BitConverter.GetBytes(Convert.ToUInt32(1078));
            //temp.CopyTo(aBuf, position);
            //position += temp.Length;

            //temp = BitConverter.GetBytes(Convert.ToUInt32(40));
            //temp.CopyTo(aBuf, position);
            //position += temp.Length;
            //temp = BitConverter.GetBytes(Convert.ToUInt32(rect.Width));
            //temp.CopyTo(aBuf, position);
            //position += temp.Length;
            //temp = BitConverter.GetBytes(Convert.ToUInt32(rect.Height));
            //temp.CopyTo(aBuf, position);
            //position += temp.Length;

            //temp = BitConverter.GetBytes(Convert.ToUInt16(1));
            //temp.CopyTo(aBuf, position);
            //position += temp.Length;
            //temp = BitConverter.GetBytes(Convert.ToUInt16(8));
            //temp.CopyTo(aBuf, position);
            //position += temp.Length;
            //temp = BitConverter.GetBytes(Convert.ToUInt32(0));
            //temp.CopyTo(aBuf, position);
            //position += temp.Length;

            //temp = BitConverter.GetBytes(Convert.ToUInt32(rect.Width * rect.Height));
            //temp.CopyTo(aBuf, position);
            //position += temp.Length;
            //temp = BitConverter.GetBytes(Convert.ToUInt32(0));
            //temp.CopyTo(aBuf, position);
            //position += temp.Length;
            //temp = BitConverter.GetBytes(Convert.ToUInt32(0));
            //temp.CopyTo(aBuf, position);
            //position += temp.Length;
            //temp = BitConverter.GetBytes(Convert.ToUInt32(256));
            //temp.CopyTo(aBuf, position);
            //position += temp.Length;
            //temp = BitConverter.GetBytes(Convert.ToUInt32(256));
            //temp.CopyTo(aBuf, position);
            //position += temp.Length;

            //for (int i = 0; i < 256; i++)
            //{

            //    temp = BitConverter.GetBytes(Convert.ToByte(i));
            //    temp.CopyTo(aBuf, position);
            //    position += temp.Length;
            //    temp = BitConverter.GetBytes(Convert.ToByte(i));
            //    temp.CopyTo(aBuf, position);
            //    position += temp.Length;
            //    temp = BitConverter.GetBytes(Convert.ToByte(i));
            //    temp.CopyTo(aBuf, position);
            //    position += temp.Length;
            //    temp = BitConverter.GetBytes(Convert.ToByte(255));
            //    temp.CopyTo(aBuf, position);
            //    position += temp.Length;
            //}
            #endregion

            int position = 0;
            if (rect.Width % 4 != 0)
            {
                rect.Right += 4 - rect.Width % 4;
            }
            byte[] aBuf = new byte[(long)rect.Width * rect.Height];
            //나중에 거꾸로 나왔던것 확인해야 함. 일단 지금은 정순으로 바꿔둠
            for (int i = 0; i < rect.Height; i++)
            {
                Marshal.Copy((IntPtr)((long)GetPtr() + rect.Left + ((long)i + (long)rect.Top) * p_Size.X), aBuf, position, rect.Width);
                position += rect.Width;
            }
            return GetByteToBitmap(rect.Width, rect.Height, aBuf);
        }

        public unsafe BitmapSource GetBitMapSource(int nByteCnt = 1)
        {
            if (nByteCnt == 1)
            {
                Image<Gray, byte> image = new Image<Gray, byte>(p_Size.X, p_Size.Y);
                IntPtr ptrMem = GetPtr();

                for (int y = 0; y < p_Size.Y; y++)
                    for (int x = 0; x < p_Size.X; x++)
                    {
                        image.Data[y, x, 0] = ((byte*)ptrMem)[(long)x + (long)y * p_Size.X];
                    }

                return ImageHelper.ToBitmapSource(image);
            }
            else if (nByteCnt == 3)
            {
                Image<Rgb, byte> image = new Image<Rgb, byte>(p_Size.X, p_Size.Y);
                IntPtr ptrMem = GetPtr();

                for (int y = 0; y < p_Size.Y; y++)
                    for (int x = 0; x < p_Size.X; x++)
                    {
                        image.Data[y, x, 0] = ((byte*)ptrMem)[0 + p_nByte * (x + (long)y * p_Size.X)];
                        image.Data[y, x, 1] = ((byte*)ptrMem)[1 + p_nByte * (x + (long)y * p_Size.X)];
                        image.Data[y, x, 2] = ((byte*)ptrMem)[2 + p_nByte * (x + (long)y * p_Size.X)];
                    }
                return ImageHelper.ToBitmapSource(image);
            }
            else if (nByteCnt == 4)
            {
                Image<Bgra, byte> image = new Image<Bgra, byte>(p_Size.X, p_Size.Y);
                IntPtr ptrMem = GetPtr();

                Parallel.For(0, p_Size.Y, y =>
                {
                    Parallel.For(0, p_Size.X, x =>
                    {
                        image.Data[y, x, 0] = ((byte*)ptrMem)[0 + p_nByte * (x + (long)y * p_Size.X)];
                        image.Data[y, x, 1] = ((byte*)ptrMem)[1 + p_nByte * (x + (long)y * p_Size.X)];
                        image.Data[y, x, 2] = ((byte*)ptrMem)[2 + p_nByte * (x + (long)y * p_Size.X)];
                        image.Data[y, x, 3] = ((byte*)ptrMem)[3 + p_nByte * (x + (long)y * p_Size.X)];
                    });
                });
                //for (int y = 0; y < p_Size.Y; y++)
                //	for (int x = 0; x < p_Size.X; x++)
                //	{
                //		image.Data[y, x, 0] = ((byte*)ptrMem)[0 + p_nByte * (x + (long)y * p_Size.X)];
                //		image.Data[y, x, 1] = ((byte*)ptrMem)[1 + p_nByte * (x + (long)y * p_Size.X)];
                //		image.Data[y, x, 2] = ((byte*)ptrMem)[2 + p_nByte * (x + (long)y * p_Size.X)];
                //		image.Data[y, x, 3] = ((byte*)ptrMem)[3 + p_nByte * (x + (long)y * p_Size.X)];
                //	}
                return ImageHelper.ToBitmapSource(image);
            }
            return null;
        }
        public Bitmap GetRectImagePattern(CRect rect)
        {
            if (rect.Width % 4 != 0)
            {
                rect.Right += 4 - rect.Width % 4;
            }
            return GetByteToBitmap(rect.Width, rect.Height, GetRectByteArray(rect));
        }
        public void SaveRectImage(CRect memRect, int nByte = 1, bool red = true, bool green = true, bool blue = true)
        {
            bool isGrayScale = (p_nByte * p_nPlane != 3) ? true : false;

            SaveRectImage(memRect, isGrayScale, nByte, red, green, blue);
        }
        public void SaveRectImage(CRect memRect, bool isGrayScale, int nByte = 1, bool red = true, bool green = true, bool blue = true)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "BMP파일|*.bmp";

            if (sfd.ShowDialog() == DialogResult.OK)
                SaveRectImage(memRect, sfd.FileName, isGrayScale, nByte, red, green, blue);
        }
        public void SaveRectImage(CRect memRect, string saveTargetPath, int nByte = 1, bool red = true, bool green = true, bool blue = true)
        {
            bool isGrayScale = (p_nByte * p_nPlane != 3) ? true : false;

            SaveRectImage(memRect, saveTargetPath, isGrayScale, nByte, red, green, blue);
        }
        public void SaveRectImage(CRect memRect, string saveTargetPath, bool isGrayScale, int nByte = 1, bool red = true, bool green = true, bool blue = true)
        {
            List<object> arguments = new List<object>();
            arguments.Add(saveTargetPath);
            arguments.Add(memRect);
            arguments.Add(red);
            arguments.Add(green);
            arguments.Add(blue);
            arguments.Add(isGrayScale);
            arguments.Add(nByte);

            BackgroundWorker Worker_MemorySave = new BackgroundWorker();
            Worker_MemorySave.DoWork += new DoWorkEventHandler(Worker_MemorySave_DoWork);
            Worker_MemorySave.RunWorkerAsync(arguments);
        }
        public void SaveWholeImage()
        {
            bool isGrayScale = (p_nByte * p_nPlane != 3) ? true : false;
            int nByte = (p_nByte * p_nPlane != 3) ? p_nByte : p_nByte * p_nPlane;

            SaveWholeImage(isGrayScale, nByte, true, true, true);
        }
        public void SaveWholeImage(bool isGrayScale, int nByte = 1, bool red = true, bool green = true, bool blue = true)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "BMP파일|*.bmp";

            if (sfd.ShowDialog() == DialogResult.OK)
                SaveWholeImage(sfd.FileName, isGrayScale, nByte, red, green, blue);
        }
        public void SaveWholeImage(string targetPath, bool isGrayScale, int nByte = 1, bool red = true, bool green = true, bool blue = true)
        {
            List<object> arguments = new List<object>();
            arguments.Add(targetPath);
            arguments.Add(new CRect(0, 0, p_Size.X, p_Size.Y));
            arguments.Add(red);
            arguments.Add(green);
            arguments.Add(blue);
            arguments.Add(isGrayScale);
            arguments.Add(nByte);

            BackgroundWorker Worker_MemorySave = new BackgroundWorker();
            Worker_MemorySave.DoWork += new DoWorkEventHandler(Worker_MemorySave_DoWork);
            Worker_MemorySave.RunWorkerAsync(arguments);
        }
        /// <summary>
        /// 비동기 이미지 Load
        /// </summary>
        public void LoadImageSync(string filePath, CPoint offset)
        {
            OpenBMPFile(filePath, null, offset);
        }
        void Worker_MemorySave_DoWork(object sender, DoWorkEventArgs e)
        {
            List<object> arguments = (List<object>)(e.Argument);

            string sPath = arguments[0].ToString();
            CRect MemRect = (CRect)arguments[1];
            bool red = (bool)arguments[2];
            bool green = (bool)arguments[3];
            bool blue = (bool)arguments[4];
            bool isGrayScale = (bool)arguments[5];
            int nByte = (int)arguments[6];

            // 저장하려는 이미지가 그레이/컬러인지에 따라 분기
            if (isGrayScale)
            {
                eRgbChannel channel = eRgbChannel.None;
                if (p_nByte * p_nPlane == 3)
                {
                    if (red == true) channel = eRgbChannel.R;
                    else if (green == true) channel = eRgbChannel.G;
                    else if (blue == true) channel = eRgbChannel.B;
                }
                FileSaveGrayBMP(sPath, MemRect, nByte, channel);
            }
            else
            {
                FileSaveRgbBMP(sPath, MemRect, red, green, blue);
            }

        }
        unsafe void FileSaveBMP(string sFile, IntPtr ptr, CRect rect, bool red = true, bool green = true, bool blue = true)
        {
            //int width = (int)(rect.Right * 0.25);
            //if (width * 4 != rect.Right) rect.Right = (width + 1) * 4;


            FileStream fs = new FileStream(sFile, FileMode.Create, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fs);

            bw.Write(Convert.ToUInt16(0x4d42));//ushort bfType = br.ReadUInt16();
            if (p_nByte == 1)
            {
                if ((Int64)rect.Width * (Int64)rect.Height > Int32.MaxValue) bw.Write(Convert.ToUInt32(54 + 1024 + p_nByte * 1000 * 1000));
                else bw.Write(Convert.ToUInt32(54 + 1024 + p_nByte * (Int64)rect.Width * (Int64)rect.Height));
            }
            else if (p_nByte == 3)
            {
                if ((Int64)rect.Width * (Int64)rect.Height > Int32.MaxValue) bw.Write(Convert.ToUInt32(54 + p_nByte * 1000 * 1000));//uint bfSize = br.ReadUInt32();
                else bw.Write(Convert.ToUInt32(54 + p_nByte * (Int64)rect.Width * (Int64)rect.Height));//uint bfSize = br.ReadUInt32();
            }

            //image 크기 bw.Write();   bmfh.bfSize = sizeof(14byte) + nSizeHdr + rect.right * rect.bottom;
            bw.Write(Convert.ToUInt16(0));   //reserved // br.ReadUInt16();
            bw.Write(Convert.ToUInt16(0));   //reserved //br.ReadUInt16();
            if (p_nByte == 1)
                bw.Write(Convert.ToUInt32(1078));
            else if (p_nByte == 3)
                bw.Write(Convert.ToUInt32(54));//uint bfOffBits = br.ReadUInt32();

            bw.Write(Convert.ToUInt32(40));// uint biSize = br.ReadUInt32();
            bw.Write(Convert.ToInt32(rect.Width));// nWidth = br.ReadInt32();
            bw.Write(Convert.ToInt32(rect.Height));// nHeight = br.ReadInt32();
            bw.Write(Convert.ToUInt16(1));// a = br.ReadUInt16();
            bw.Write(Convert.ToUInt16(8 * p_nByte));     //byte       // nByte = br.ReadUInt16() / 8;                
            bw.Write(Convert.ToUInt32(0));      //compress //b = br.ReadUInt32();
            if ((Int64)rect.Width * (Int64)rect.Height > Int32.MaxValue) bw.Write(Convert.ToUInt32(1000 * 1000));// b = br.ReadUInt32();
            else bw.Write(Convert.ToUInt32((Int64)rect.Width * (Int64)rect.Height));// b = br.ReadUInt32();
            bw.Write(Convert.ToInt32(0));//a = br.ReadInt32();
            bw.Write(Convert.ToInt32(0));// a = br.ReadInt32();
            bw.Write(Convert.ToUInt32(256));      //color //b = br.ReadUInt32();
            bw.Write(Convert.ToUInt32(256));      //import // b = br.ReadUInt32();
            if (p_nByte == 1)
            {
                for (int i = 0; i < 256; i++)
                {
                    bw.Write(Convert.ToByte(i));
                    bw.Write(Convert.ToByte(i));
                    bw.Write(Convert.ToByte(i));
                    bw.Write(Convert.ToByte(255));
                }
            }
            if (rect.Width % 4 != 0)
            {
                rect.Right += 4 - rect.Width % 4;
            }
            byte[] aBuf = new byte[(long)p_nByte * rect.Width];
            for (int i = rect.Height - 1; i >= 0; i--)
            {
                Marshal.Copy((IntPtr)((long)ptr + rect.Left + ((long)i + (long)rect.Top) * p_Size.X * p_nByte), aBuf, 0, rect.Width * p_nByte);
                bw.Write(aBuf);
                p_nProgress = Convert.ToInt32(((double)(rect.Height - i) / rect.Height) * 100);
            }

            //byte[] pBuf = br.ReadBytes(nWidth);
            //Marshal.Copy(pBuf, 0, (IntPtr)((long)m_ptrImg + offset.X + (long)p_Size.X * ((long)offset.Y + y)), lowwidth);
            //p_nProgress = Convert.ToInt32(((double)y / lowheight) * 100);
            //for (int i = rect.Height - 1; i >= 0; i--)
            //{  
            //    Marshal.Copy(ptr + i * rect.Width, aBuf, 0, rect.Width);
            //    p_nProgress = Convert.ToInt32(((double)(rect.Height - i) / rect.Height) * 100);
            //    bw.Write(aBuf);
            //}
            bw.Close();
            fs.Close();
        }
        bool WriteBitmapFileHeader(BinaryWriter bw, int nByte, int width, int height)
        {
            if (bw == null)
                return false;

            int rowSize = (width * nByte + 3) & ~3;
            int paletteSize = (nByte == 1 ? (256 * 4) : 0);

            int size = 14 + 40 + paletteSize + rowSize * height;
            int offbit = 14 + 40 + (nByte == 1 ? (256 * 4) : 0);

            bw.Write(Convert.ToUInt16(0x4d42));                     // bfType;
            bw.Write(Convert.ToUInt32((uint)size));                // bfSize
            bw.Write(Convert.ToUInt16(0));                          // bfReserved1
            bw.Write(Convert.ToUInt16(0));                          // bfReserved2
            bw.Write(Convert.ToUInt32(offbit));                     // bfOffbits

            return true;
        }
        bool WriteBitmapInfoHeader(BinaryWriter bw, int width, int height, bool isGrayScale, int nByte)
        {
            if (bw == null)
                return false;

            int biBitCount = (isGrayScale ? 1 : p_nPlane) * nByte * 8;

            bw.Write(Convert.ToUInt32(40));                         // biSize
            bw.Write(Convert.ToInt32(width));                       // biWidth
            bw.Write(Convert.ToInt32(height));                      // biHeight
            bw.Write(Convert.ToUInt16(1));                          // biPlanes
            bw.Write(Convert.ToUInt16(biBitCount));                 // biBitCount
            bw.Write(Convert.ToUInt32(0));                          // biCompression
            bw.Write(Convert.ToUInt32(0));                          // biSizeImage
            bw.Write(Convert.ToInt32(0));                           // biXPelsPerMeter
            bw.Write(Convert.ToInt32(0));                           // biYPelsPerMeter
            bw.Write(Convert.ToUInt32((isGrayScale == true) ? 256 : 0));   // biClrUsed
            bw.Write(Convert.ToUInt32((isGrayScale == true) ? 256 : 0));   // biClrImportant

            return true;
        }
        bool WritePalette(BinaryWriter bw)
        {
            if (bw == null)
                return false;

            for (int i = 0; i < 256; i++)
            {
                bw.Write(Convert.ToByte(i));
                bw.Write(Convert.ToByte(i));
                bw.Write(Convert.ToByte(i));
                bw.Write(Convert.ToByte(255));
            }

            return true;
        }
        public enum eRgbChannel
        {
            None, R, G, B
        }
        public unsafe void FileSaveGrayBMP(string sFile, CRect rect, int nByte, eRgbChannel channel = eRgbChannel.None, int nBitShiftOffset = 0)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(sFile, FileMode.Create, FileAccess.Write);
            }
            catch (Exception)
            {
                return;
            }

            BinaryWriter bw = null;
            try
            {
                bw = new BinaryWriter(fs);
            }
            catch (Exception)
            {
                fs.Close();
            }

            try
            {
                // Bitmap File Header
                if (WriteBitmapFileHeader(bw, nByte, rect.Width, rect.Height) == false)
                    return;

                // Bitmap Info Header
                if (WriteBitmapInfoHeader(bw, rect.Width, rect.Height, true, nByte) == false)
                    return;

                // Palette (if it's 1byte gray image)
                if (nByte == 1)
                    WritePalette(bw);

                // Pixel data
                int rowSize = (rect.Width * nByte + 3) & ~3;
                byte[] aBuf = new byte[(long)rowSize];
                IntPtr ptr = IntPtr.Zero;

                if (p_nPlane == 3)
                {
                    ptr = IntPtr.Zero;
                    switch (channel)
                    {
                        case eRgbChannel.R: ptr = GetPtr(0); break;
                        case eRgbChannel.G: ptr = GetPtr(1); break;
                        case eRgbChannel.B: ptr = GetPtr(2); break;
                        default:
                            break;
                    }
                }
                else
                    ptr = GetPtr();

                if (ptr != IntPtr.Zero)
                {
                    if(p_nByte == nByte)
                    {
                        for (int j = rect.Top + rect.Height - 1; j >= rect.Top; j--)
                        {
                            Array.Clear(aBuf, 0, rowSize);

                            long idx = ((long)j * p_Size.X + rect.Left) * p_nByte;
                            IntPtr srcPtr = new IntPtr(ptr.ToInt64() + idx);
                            Marshal.Copy(srcPtr, aBuf, 0, rowSize);

                            bw.Write(aBuf);
                            p_nProgress = Convert.ToInt32(((double)(rect.Height - (j - rect.Top)) / rect.Height) * 100);
                        }
                    }
                    else
                    {
                        for (int j = rect.Top + rect.Height - 1; j >= rect.Top; j--)
                        {
                            Array.Clear(aBuf, 0, rowSize);

                            long idx = ((long)j * p_Size.X + rect.Left) * p_nByte;

                            Parallel.For(0, rect.Width, new ParallelOptions { MaxDegreeOfParallelism = 12 }, (i) =>
                            {
                                byte* arrByte = (byte*)ptr.ToPointer();

                                if (nByte == 1) // 2byte -> 1byte
                                {
                                    //byte val1 = arrByte[idx + i * p_nByte + 0];
                                    //byte val2 = arrByte[idx + i * p_nByte + 1];
                                    ////byte[] arrb1b2 = new byte[2] { val1, val2 };

                                    ////aBuf[i] = (byte)(BitConverter.ToUInt16(arrb1b2, 0) / (Math.Pow(2, 8 * p_nByte) - 1));
                                    //aBuf[i] = val2;


                                    byte[] arrVal = new byte[sizeof(long)];
                                    for (int tempIdx = 0; tempIdx < p_nByte; tempIdx++)
                                    {
                                        arrVal[tempIdx] = arrByte[idx + i * p_nByte + tempIdx];
                                    }

                                    ulong nVal = BitConverter.ToUInt64(arrVal, 0);
                                    nVal = nVal << nBitShiftOffset;

                                    aBuf[i] = (byte)((nVal >> (8 * (p_nByte - nByte))) & 0xFF);

                                }
                                else /*if(nByte == 2)*/ // 1byte -> 2byte
                                {
                                    int val = arrByte[idx + i * p_nByte];
                                    val = (int)((val / 255.0) * (Math.Pow(2, 8 * nByte) - 1));
                                    aBuf[i * nByte + 0] = (byte)((val & 0xFF00) >> 8);
                                    aBuf[i * nByte + 1] = (byte)((val & 0x00FF));
                                }
                            });

                            bw.Write(aBuf);
                            p_nProgress = Convert.ToInt32(((double)(rect.Height - (j - rect.Top)) / rect.Height) * 100);
                        }
                    }
                }
            }
            catch (Exception) { }
            finally
            {
                bw.Close();
                fs.Close();
            }
        }
        unsafe void FileSaveRgbBMP(string sFile, CRect rect, bool red = true, bool green = true, bool blue = true)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(sFile, FileMode.Create, FileAccess.Write);
            }
            catch (Exception)
            {
                return;
            }

            BinaryWriter bw = null;
            try
            {
                bw = new BinaryWriter(fs);
            }
            catch (Exception)
            {
                fs.Close();
                return;
            }

            try
            {
                /// Bitmap File Header
                if (WriteBitmapFileHeader(bw, 3, rect.Width, rect.Height) == false)
                    return;

                /// Bitmap Info Header
                if (WriteBitmapInfoHeader(bw, rect.Width, rect.Height, false, 1) == false)
                    return;

                /// Pixel data
                int rowSize = (rect.Width * 3 + 3) & ~3;
                byte[] aBuf = new byte[(long)rowSize];
                IntPtr ptrR = IntPtr.Zero;
                IntPtr ptrG = IntPtr.Zero;
                IntPtr ptrB = IntPtr.Zero;
                if (p_nPlane == 1)
                {
                    ptrR = ptrG = ptrB = GetPtr(0);
                }
                else if (p_nPlane == 3)
                {
                    // RGB 채널별 데이터 얻어오기
                    ptrR = GetPtr(0);
                    ptrG = GetPtr(1);
                    ptrB = GetPtr(2);
                }

                if (ptrR != IntPtr.Zero && ptrG != IntPtr.Zero && ptrB != IntPtr.Zero)
                {
                    for (long j = rect.Top + rect.Height - 1; j >= rect.Top; j--)
                    {
                        Array.Clear(aBuf, 0, rowSize);

                        Parallel.For(0, rect.Width, new ParallelOptions { MaxDegreeOfParallelism = 12 }, (i) =>
                        {

                            if (Worker_MemoryCopy.CancellationPending)
                                return;

                            long idx = (long) (j * p_Size.X + i + rect.Left) * p_nByte;
                            if (p_nByte == 1)
                            {
                                aBuf[(long)i * 3 + 0] = (byte)(blue ? ((byte*)ptrB.ToPointer())[idx] : 0);
                                aBuf[(long)i * 3 + 1] = (byte)(green ? ((byte*)ptrG.ToPointer())[idx] : 0);
                                aBuf[(long)i * 3 + 2] = (byte)(red ? ((byte*)ptrR.ToPointer())[idx] : 0);
                            }
                            else if (p_nByte == 2)
                            {
                                int b = (int)((int*)ptrB.ToPointer())[idx];
                                int g = (int)((int*)ptrG.ToPointer())[idx];
                                int r = (int)((int*)ptrR.ToPointer())[idx];

                                aBuf[(long)i * 3 + 0] = (byte)(b / 0xffff);
                                aBuf[(long)i * 3 + 1] = (byte)(g / 0xffff);
                                aBuf[(long)i * 3 + 2] = (byte)(r / 0xffff);
                            };
                        });

                        bw.Write(aBuf);
                        p_nProgress = Convert.ToInt32(((double)(rect.Height - (j - rect.Top)) / rect.Height) * 100);
                    }
                }
            }
            catch (Exception) { }
            finally
            {
                bw.Close();
                fs.Close();
            }
        }
        public void OpenFile(string sFileName, CPoint offset, int channel = 0)
        {
            FileInfo fileInfo = new FileInfo(sFileName);
            if (fileInfo.Exists)
            {
                List<object> arguments = new List<object>();
                arguments.Add(sFileName);
                arguments.Add(offset);
                arguments.Add(channel);
                Worker_MemoryCopy.RunWorkerAsync(arguments);
            }
            else
            {
                ImageData data = new ImageData(123, 123);
                System.Windows.MessageBox.Show("OpenFile() - 파일이 존재 하지 않거나 열기에 실패하였습니다. - " + sFileName);
            }
        }

        void Worker_MemoryCopy_DoWork(object sender, DoWorkEventArgs e)
        {
            List<object> arguments = (List<object>)(e.Argument);

            string sPath = arguments[0].ToString();
            CPoint offset = (CPoint)(arguments[1]);
            int channel = (int)(arguments[2]);
            if (sPath.ToLower().IndexOf(".bmp") >= 0)
            {
                OpenBMPFile(sPath, e, offset, channel);
            }
            else if (sPath.ToLower().IndexOf(".jpg") >= 0)
            {

            }
            if (m_eMode == eMode.MemoryRead)
            {

            }
            m_aBufFileOpen = new byte[1];
        }

        void Worker_MemoryCopy_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            p_nProgress = 100;
            OnCreateNewImage();
        }


        unsafe void Worker_MemoryClear_DoWork(object sender, DoWorkEventArgs e)
        {
            CPoint sz = p_Size;
            int np = sz.Y / 100;
            byte[] pBuf = new byte[(long)sz.X * p_nByte];
            int nProgress = 0;

            if (m_ptrImg == IntPtr.Zero) return;

            Parallel.For(0, sz.Y, new ParallelOptions { MaxDegreeOfParallelism = 20 }, (y) =>
            {
                if (Worker_MemoryClear.CancellationPending)
                    return;

                Marshal.Copy(pBuf, 0, (IntPtr)((long)m_ptrImg + (long)sz.X * p_nByte * y), sz.X * p_nByte);
                if (GetBytePerPixel() == 3)
                {
                    Marshal.Copy(pBuf, 0, (IntPtr)((long)m_MemData.GetPtr(1) + (long)sz.X * p_nByte * y), sz.X * p_nByte);
                    Marshal.Copy(pBuf, 0, (IntPtr)((long)m_MemData.GetPtr(2) + (long)sz.X * p_nByte * y), sz.X * p_nByte);
                }
                nProgress++;
                if (nProgress % np == 0)
                    p_nProgress = Convert.ToInt32(((double)nProgress / sz.Y) * 100); ;
            });
        }

        public unsafe void ClearImage_TEST()
        {
            CPoint sz = p_Size;
            int np = sz.Y / 100;
            byte[] pBuf = new byte[(long)sz.X * p_nByte];
            int nProgress = 0;

            if (m_ptrImg == IntPtr.Zero) return;

            Parallel.For(0, sz.Y, new ParallelOptions { MaxDegreeOfParallelism = 20 }, (y) =>
            {
                if (Worker_MemoryClear.CancellationPending)
                    return;

                Marshal.Copy(pBuf, 0, (IntPtr)((long)m_ptrImg + (long)sz.X * p_nByte * y), sz.X * p_nByte);
                if (GetBytePerPixel() == 3)
                {
                    Marshal.Copy(pBuf, 0, (IntPtr)((long)m_MemData.GetPtr(1) + (long)sz.X * p_nByte * y), sz.X * p_nByte);
                    Marshal.Copy(pBuf, 0, (IntPtr)((long)m_MemData.GetPtr(2) + (long)sz.X * p_nByte * y), sz.X * p_nByte);
                }
                nProgress++;
                if (nProgress % np == 0)
                    p_nProgress = Convert.ToInt32(((double)nProgress / sz.Y) * 100); ;
            });
        }

        public void SaveImageSync(string targetPath)
        {
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(m_aBuf.Length);
            Marshal.Copy(m_aBuf, 0, unmanagedPointer, m_aBuf.Length);
            FileSaveBMP(targetPath, unmanagedPointer, new CRect(0, 0, m_Size.X, m_Size.Y));
            Marshal.FreeHGlobal(unmanagedPointer);
        }

        void Worker_MemoryClear_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            p_nProgress = 100;
            OnCreateNewImage();
        }

        int nLine = 0;

        //         unsafe void OpenBMPFile(string sFile, DoWorkEventArgs e, CPoint offset)
        //         {
        //             int nByte;
        //             int nWidth = 0, nHeight = 0;
        //             FileStream fs = null;
        //             try
        //             {
        //                 fs = new FileStream(sFile, FileMode.Open, FileAccess.Read, FileShare.Read, 32768, true);
        //             }
        //             catch (Exception)
        //             {
        //                 return;
        //             }
        // 
        //             int a = 0;
        //             UInt32 b = 0;
        //             BinaryReader br = new BinaryReader(fs);
        //             ushort bfType = br.ReadUInt16();  //2 4 2 2 4 4 4 4 2  2 4 4 4 4 4 4 256*4 1024  54 
        //             uint bfSize = br.ReadUInt32();
        //             br.ReadUInt16();
        //             br.ReadUInt16();
        //             uint bfOffBits = br.ReadUInt32();
        //             if (bfType != 0x4D42)
        //                 return;
        //             uint biSize = br.ReadUInt32();
        //             nWidth = br.ReadInt32();
        //             nHeight = br.ReadInt32();
        //             a = br.ReadUInt16();
        //             nByte = br.ReadUInt16() / 8;
        //             b = br.ReadUInt32();
        //             b = br.ReadUInt32();
        //             a = br.ReadInt32();
        //             a = br.ReadInt32();
        //             b = br.ReadUInt32();
        //             b = br.ReadUInt32();
        // 
        //             if (bfOffBits != 54)
        //                 br.ReadBytes((int)bfOffBits - 54);
        // 
        //             int lowwidth = 0, lowheight = 0;
        //             //lowwidth = nWidth < p_Size.X / p_nByte - offset.X ? nWidth : p_Size.X / p_nByte - offset.X;
        //             lowwidth = nWidth < p_Size.X - offset.X ? nWidth : p_Size.X - offset.X;
        //             lowheight = nHeight < p_Size.Y - offset.Y ? nHeight : p_Size.Y - offset.Y;
        // 
        // 
        //             if (m_eMode == eMode.MemoryRead)
        //             {
        //                 p_nByte = nByte;
        //                 //byte[] hRGB;
        //                 //            if (p_nByte != 3)
        //                 //                hRGB = br.ReadBytes(256 * 4);
        //                 if (p_nByte == 1)
        //                 {
        //                     nLine = 0;
        //                     int nNum = 4;
        //                     Thread[] multiThread = new Thread[nNum];
        // 
        //                     for (int i = 0; i < nNum; i++)
        //                     {
        //                         int nStartHeight = lowheight * (nNum - i) / nNum;
        //                         int nEndHeight = lowheight * (nNum - i - 1) / nNum;
        //                         multiThread[i] = new Thread(() => RunCopyThread(sFile, nWidth, nHeight, lowwidth, lowheight, nStartHeight, nEndHeight, offset));
        //                         multiThread[i].Start();
        //                     }
        //                     while (true)
        //                     {
        //                         bool bEnd = true;
        //                         for (int i = 0; i < nNum; i++)
        //                         {
        //                             if (multiThread[i].IsAlive)
        //                                 bEnd = false;
        //                         }
        //                         Thread.Sleep(10);
        //                         p_nProgress = Convert.ToInt32(((double)nLine / lowheight) * 100);
        //                         if (bEnd)
        //                             break;
        //                     }
        //                     //for (int y = lowheight - 1; y >= 0; y--)
        //                     //{
        //                     //    if (Worker_MemoryCopy.CancellationPending)
        //                     //        return;
        // 
        //                     //    byte[] pBuf = br.ReadBytes(p_nByte * nWidth);
        //                     //    Marshal.Copy(pBuf, 0, (IntPtr)((long)m_ptrImg + p_nByte * (offset.X + p_Size.X / p_nByte * ((long)offset.Y + y))), p_nByte * lowwidth);
        //                     //    p_nProgress = Convert.ToInt32(((double)(lowheight - y) / lowheight) * 100);
        //                     //}
        //                 }
        //                 else if (p_nByte == 3)
        //                 {
        // 
        //                     nLine = 0;
        //                     int nNum = 2;
        //                     Thread[] multiThread = new Thread[nNum];
        // 
        //                     for (int i = 0; i < nNum; i++)
        //                     {
        //                         int nStartHeight = lowheight * (nNum - i) / nNum;
        //                         int nEndHeight = lowheight * (nNum - i - 1) / nNum;
        //                         multiThread[i] = new Thread(() => RunCopyThreadColor(sFile, nWidth, nHeight, lowwidth, lowheight, nStartHeight, nEndHeight, offset));
        //                         multiThread[i].Start();
        //                     }
        //                     while (true)
        //                     {
        //                         bool bEnd = true;
        //                         for (int i = 0; i < nNum; i++)
        //                         {
        //                             if (multiThread[i].IsAlive)
        //                                 bEnd = false;
        //                         }
        //                         Thread.Sleep(10);
        //                         p_nProgress = Convert.ToInt32(((double)nLine / lowheight) * 100);
        //                         if (bEnd)
        //                             break;
        //                     }
        // 
        // 
        //                     //for (int y = lowheight - 1; y >= 0; y--)
        //                     //{
        //                     //	if (Worker_MemoryCopy.CancellationPending)
        //                     //		return;
        // 
        //                     //	byte[] pBuf = br.ReadBytes(p_nByte * nWidth);
        //                     //	IntPtr ptrR =  m_MemData.GetPtr(0);					
        //                     //	IntPtr ptrG = m_MemData.GetPtr(1);
        //                     //	IntPtr ptrB = m_MemData.GetPtr(2);
        //                     //	if (ptrR == IntPtr.Zero || ptrB == IntPtr.Zero || ptrG == IntPtr.Zero)
        //                     //	{
        //                     //                       System.Windows.MessageBox.Show("Memory Count Error");
        //                     //		return;
        //                     //	}
        //                     //	for (int i = 0; i < lowwidth*3; i = i + 3)
        //                     //	{
        //                     //		((byte*)(ptrB))[i / 3 + (long)y * p_Size.X] = pBuf[i];
        //                     //                       ((byte*)(ptrG))[i / 3 + (long)y * p_Size.X] = pBuf[i+1];
        //                     //		((byte*)(ptrR))[i / 3 + (long)y * p_Size.X] = pBuf[i+2];
        //                     //	}
        //                     //	//Marshal.Copy(pBuf, 0, (IntPtr)((long)m_ptrImg + p_nByte * (offset.X + p_Size.X / p_nByte * ((long)offset.Y + y))), p_nByte * lowwidth);
        //                     //	p_nProgress = Convert.ToInt32(((double)(lowheight - y) / lowheight) * 100);
        //                     //}
        //                 }
        //             }
        //             else
        //             {
        //                 p_nByte = nByte;
        // 
        //                 p_Size = new CPoint(nWidth + offset.X, nHeight + offset.Y);
        //                 ReAllocate(p_Size, _nByte);
        // 
        //                 byte[] pBuf = new byte[(int)nWidth * nByte];
        // 
        //                 for (int y = p_Size.Y - 1; y >= 0; y--)
        //                 {
        //                     pBuf = br.ReadBytes((int)nWidth * nByte);
        //                     Buffer.BlockCopy(pBuf, 0, m_aBuf, (int)(offset.X + (offset.Y + y) * p_Stride), (int)nWidth * nByte);
        //                     p_nProgress = Convert.ToInt32(((double)(p_Size.Y - y) / p_Size.Y) * 100);
        // 
        //                 }
        //             }
        //             br.Close();
        //         }

        public void RunCopyThread(string sFile, int nWidth, int nHeight, int nLowWidth, int nLowHeight, int nStartHeight, int nEndHeight, CPoint offset)
        {
            FileStream fss = new FileStream(sFile, FileMode.Open, FileAccess.Read, FileShare.Read, 32768, true);
            byte[] buf = new byte[nWidth];

            BinaryReader br = new BinaryReader(fss);
            ushort bfType = br.ReadUInt16();  //2 4 2 2 4 4 4 4 2  2 4 4 4 4 4 4 256*4 1024  54 
            uint bfSize = br.ReadUInt32();
            br.ReadUInt16();
            br.ReadUInt16();
            uint bfOffBits = br.ReadUInt32();

            fss.Seek(bfOffBits + (nLowHeight - nStartHeight) * (long)nWidth, SeekOrigin.Begin);
            for (int i = nStartHeight - 1; i >= nEndHeight; i--)
            {
                if (Worker_MemoryCopy.CancellationPending)
                    return;
                fss.Read(buf, 0, nWidth);
                Marshal.Copy(buf, 0, (IntPtr)((long)m_ptrImg + (offset.X + p_Size.X * ((long)offset.Y + i))), nLowWidth);
                nLine++;
            }
            fss.Close();
        }

        public unsafe void RunCopyThreadColor(string sFile, int nWidth, int nHeight, int nLowWidth, int nLowHeight, int nStartHeight, int nEndHeight, CPoint offset)
        {
            int nByte = 3;
            FileStream fss = new FileStream(sFile, FileMode.Open, FileAccess.Read, FileShare.Read, 32768, true);
            byte[] buf = new byte[nWidth * nByte];
            IntPtr ptrR = m_MemData.GetPtr(0);
            IntPtr ptrG = m_MemData.GetPtr(1);
            IntPtr ptrB = m_MemData.GetPtr(2);
            if (ptrR == IntPtr.Zero || ptrB == IntPtr.Zero || ptrG == IntPtr.Zero)
            {
                System.Windows.MessageBox.Show("Memory Count Error");
                return;
            }

            //
            //int a = 0;
            //UInt32 b = 0;
            BinaryReader br = new BinaryReader(fss);
            ushort bfType = br.ReadUInt16();  //2 4 2 2 4 4 4 4 2  2 4 4 4 4 4 4 256*4 1024  54 
            uint bfSize = br.ReadUInt32();
            br.ReadUInt16();
            br.ReadUInt16();
            uint bfOffBits = br.ReadUInt32();

            fss.Seek(bfOffBits + (nLowHeight - nStartHeight) * (long)nWidth * nByte, SeekOrigin.Begin);
            for (int y = nStartHeight - 1; y >= nEndHeight; y--)
            {
                if (Worker_MemoryCopy.CancellationPending)
                    return;
                fss.Read(buf, 0, nWidth * nByte);
                for (int i = 0; i < nLowWidth * 3; i = i + 3)
                {
                    ((byte*)(ptrB))[i / 3 + (long)y * p_Size.X] = buf[i];
                    ((byte*)(ptrG))[i / 3 + (long)y * p_Size.X] = buf[i + 1];
                    ((byte*)(ptrR))[i / 3 + (long)y * p_Size.X] = buf[i + 2];
                }
                nLine++;
            }
            fss.Close();

            //for (int y = lowheight - 1; y >= 0; y--)
            //{
            //	if (Worker_MemoryCopy.CancellationPending)
            //		return;

            //	byte[] pBuf = br.ReadBytes(p_nByte * nWidth);
            //	IntPtr ptrR =  m_MemData.GetPtr(0);					
            //	IntPtr ptrG = m_MemData.GetPtr(1);
            //	IntPtr ptrB = m_MemData.GetPtr(2);
            //	if (ptrR == IntPtr.Zero || ptrB == IntPtr.Zero || ptrG == IntPtr.Zero)
            //	{
            //                       System.Windows.MessageBox.Show("Memory Count Error");
            //		return;
            //	}
            //	for (int i = 0; i < lowwidth*3; i = i + 3)
            //	{
            //		((byte*)(ptrB))[i / 3 + (long)y * p_Size.X] = pBuf[i];
            //                       ((byte*)(ptrG))[i / 3 + (long)y * p_Size.X] = pBuf[i+1];
            //		((byte*)(ptrR))[i / 3 + (long)y * p_Size.X] = pBuf[i+2];
            //	}
            //	//Marshal.Copy(pBuf, 0, (IntPtr)((long)m_ptrImg + p_nByte * (offset.X + p_Size.X / p_nByte * ((long)offset.Y + y))), p_nByte * lowwidth);
            //	p_nProgress = Convert.ToInt32(((double)(lowheight - y) / lowheight) * 100);
            //}

        }


        public unsafe bool ReAllocate(CPoint sz, int nByte)
        {
            if (nByte <= 0)
                return false;
            if ((sz.X < 1) || (sz.Y < 1))
                return false;
            if (m_eMode == eMode.ImageBuffer)
            {
                Array.Resize(ref m_aBuf, sz.X * nByte * sz.Y);
            }

            return true;
        }
        bool ReadBitmapFileHeader(BinaryReader br, ref uint bfOffbits)
        {
            if (br == null)
                return false;

            ushort bfType;
            uint bfSize;

            bfType = br.ReadUInt16();       // bfType;
            bfSize = br.ReadUInt32();       // bfSize
            br.ReadUInt16();                // bfReserved1
            br.ReadUInt16();                // bfReserved2
            bfOffbits = br.ReadUInt32();    // bfOffbits

            if (bfType != 0x4d42)
                return false;

            return true;
        }
        bool ReadBitmapInfoHeader(BinaryReader br, ref int width, ref int height, ref int nByte)
        {
            if (br == null)
                return false;

            uint biSize;

            biSize = br.ReadUInt32();       // biSize
            width = br.ReadInt32();         // biWidth
            height = br.ReadInt32();        // biHeight
            br.ReadUInt16();                // biPlanes
            nByte = br.ReadUInt16() / 8;    // biBitCount
            br.ReadUInt32();                // biCompression
            br.ReadUInt32();                // biSizeImage
            br.ReadInt32();                 // biXPelsPerMeter
            br.ReadInt32();                 // biYPelsPerMeter
            br.ReadUInt32();                // biClrUsed
            br.ReadUInt32();                // biClrImportant

            return true;
        }
        unsafe void OpenBMPFile(string sFile, DoWorkEventArgs e, CPoint offset, int channel = 0)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(sFile, FileMode.Open, FileAccess.Read, FileShare.Read, 32768, true);
            }
            catch (Exception)
            {
                return;
            }

            BinaryReader br = null;
            try
            {
                br = new BinaryReader(fs);
            }
            catch (Exception)
            {
                fs.Close();
                return;
            }

            try
            {
                // Bitmap File Header
                uint bfOffbits = 0;
                if (ReadBitmapFileHeader(br, ref bfOffbits) == false)
                    return;

                // Bitmap Info Header
                int width = 0;
                int height = 0;
                int nByte = 0;
                if (ReadBitmapInfoHeader(br, ref width, ref height, ref nByte) == false)
                    return;

                // 이미지 파일의 채널 개수가 이미 생성된 메모리의 채널 개수보다 많을 경우 Open 과정 중단
                if (nByte > GetBytePerPixel())
                {
                    //System.Windows.MessageBox.Show("Not enough memory count to load image file");
                    return;
                }

                // Offset과 이미지 사이즈에 따른 체크
                if (offset.X > p_Size.X || offset.X < -width ||
                    offset.Y > p_Size.Y || offset.Y < -height)
                    return;

                // 읽어올 이미지 내 영역
                CRect rect = new CRect(0, 0, width, height);
                if (offset.X + width > p_Size.X) rect.Right -= offset.X + width - p_Size.X;
                if (offset.Y + height > p_Size.Y) rect.Bottom -= offset.Y + height - p_Size.Y;
                if (offset.X < 0) rect.Left = -offset.X;
                if (offset.Y < 0) rect.Top = -offset.Y;

                // 픽셀 데이터 존재하는 부분으로 Seek
                fs.Seek(bfOffbits, SeekOrigin.Begin);

                // 파일로부터 픽셀 데이터 읽어오기
                if (m_eMode == eMode.MemoryRead)
                {
                    byte[] aBuf = new byte[(long)rect.Width * nByte];
                    int fileRowSize = (width * nByte + 3) & ~3; // 파일 내 하나의 열당 너비 사이즈 (4의 배수)
                    if (nByte == 1 || nByte == 2)
                    {
                        IntPtr ptr = m_MemData.GetPtr(channel);
                        if (ptr == IntPtr.Zero)
                            return;

                        // 읽지 않아도 되는 하단부분은 읽지 않고 스킵
                        fs.Seek((height - rect.Bottom) * fileRowSize, SeekOrigin.Current);

                        for (int j = rect.Bottom - 1; j >= rect.Top; j--)
                        {
                            Array.Clear(aBuf, 0, rect.Width * nByte);

                            if (Worker_MemoryCopy.CancellationPending)
                                return;

                            // 이미지 좌측 영역 스킵
                            fs.Seek(rect.Left * nByte, SeekOrigin.Current);

                            // 파일에서 필요한 만큼 데이터를 읽어 메모리에 복사
                            fs.Read(aBuf, 0, rect.Width * nByte);
                            //IntPtr destPtr = ptr + ((j + offset.Y) * p_Size.X) * p_nByte;
                            IntPtr destPtr = new IntPtr(ptr.ToInt64() + (((long)j + offset.Y) * p_Size.X) * p_nByte);
                            if (offset.X >= 0)
                                destPtr += offset.X * p_nByte;

                            Marshal.Copy(aBuf, 0, destPtr, rect.Width * nByte);

                            // 이미지 우측 영역 스킵
                            fs.Seek(fileRowSize - rect.Right * nByte, SeekOrigin.Current);

                            // 진행상황 표시
                            p_nProgress = Convert.ToInt32(((double)rect.Height - (j - rect.Top)) / rect.Height * 100);
                        }
                    }
                    else/* if (nByte == 3)*/
                    {
                        IntPtr ptrR = m_MemData.GetPtr(0);
                        IntPtr ptrG = m_MemData.GetPtr(1);
                        IntPtr ptrB = m_MemData.GetPtr(2);
                        if (ptrR == IntPtr.Zero || ptrB == IntPtr.Zero || ptrG == IntPtr.Zero)
                            return;

                        // 읽지 않아도 되는 하단부분은 읽지 않고 스킵
                        fs.Seek((height - rect.Bottom) * fileRowSize, SeekOrigin.Current);

                        for (int j = rect.Bottom - 1; j >= rect.Top; j--)
                        {
                            if (Worker_MemoryCopy.CancellationPending)
                                return;

                            Array.Clear(aBuf, 0, rect.Width * nByte);

                            // 이미지 좌측 영역 스킵
                            fs.Seek(rect.Left * nByte, SeekOrigin.Current);

                            // 파일에서 필요한 만큼 데이터를 읽어 메모리에 복사
                            fs.Read(aBuf, 0, rect.Width * nByte);

                            Parallel.For(0, rect.Width, new ParallelOptions { MaxDegreeOfParallelism = 12 }, (i) =>
                            {
                                Int64 idx = ((Int64)j + offset.Y) * p_Size.X + i;
                                if (offset.X >= 0)
                                    idx += offset.X;

                                ((byte*)(ptrB))[idx] = aBuf[i * 3];
                                ((byte*)(ptrG))[idx] = aBuf[i * 3 + 1];
                                ((byte*)(ptrR))[idx] = aBuf[i * 3 + 2];
                            });

                            // 이미지 우측 영역 스킵
                            fs.Seek(fileRowSize - rect.Right * nByte, SeekOrigin.Current);

                            // 진행상황 표시
                            p_nProgress = Convert.ToInt32(((double)rect.Height - (j - rect.Top)) / rect.Height * 100);
                        }
                    }
                }
                else
                {
                    p_Size = new CPoint(rect.Width + offset.X, rect.Height + offset.Y);
                    ReAllocate(p_Size, GetBytePerPixel());

                    byte[] pBuf = new byte[(long)rect.Width * nByte];

                    for (int y = p_Size.Y - 1; y >= 0; y--)
                    {
                        pBuf = br.ReadBytes((int)rect.Width * nByte);
                        Buffer.BlockCopy(pBuf, 0, m_aBuf, (int)(offset.X + (offset.Y + y) * p_Stride), (int)rect.Width * nByte);
                        p_nProgress = Convert.ToInt32(((double)(p_Size.Y - y) / p_Size.Y) * 100);
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                br.Close();
                fs.Close();
            }
        }

        unsafe void OpenBMPFile2(string sFile, DoWorkEventArgs e, CPoint offset, int channel = 0)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(sFile, FileMode.Open, FileAccess.Read, FileShare.Read, 32768, true);
            }
            catch (Exception)
            {
                return;
            }

            BinaryReader br = null;
            try
            {
                br = new BinaryReader(fs);
            }
            catch (Exception)
            {
                fs.Close();
                return;
            }

            try
            {
                // Bitmap File Header
                uint bfOffbits = 0;
                if (ReadBitmapFileHeader(br, ref bfOffbits) == false)
                    return;

                // Bitmap Info Header
                int width = 0;
                int height = 0;
                int nByte = 0;
                if (ReadBitmapInfoHeader(br, ref width, ref height, ref nByte) == false)
                    return;

                // 이미지 파일의 채널 개수가 이미 생성된 메모리의 채널 개수보다 많을 경우 Open 과정 중단
                if (nByte > GetBytePerPixel())
                {
                    //System.Windows.MessageBox.Show("Not enough memory count to load image file");
                    return;
                }

                // Offset과 이미지 사이즈에 따른 체크
                if (offset.X > p_Size.X || offset.X < -width ||
                    offset.Y > p_Size.Y || offset.Y < -height)
                    return;

                // 읽어올 이미지 내 영역
                CRect rect = new CRect(0, 0, width, height);
                if (offset.X + width > p_Size.X) rect.Right -= offset.X + width - p_Size.X;
                if (offset.Y + height > p_Size.Y) rect.Bottom -= offset.Y + height - p_Size.Y;
                if (offset.X < 0) rect.Left = -offset.X;
                if (offset.Y < 0) rect.Top = -offset.Y;

                int fileRowSize = (width * nByte + 3) & ~3; // 파일 내 하나의 열당 너비 사이즈 (4의 배수)
                m_nThreadReadCount = (Int64)bfOffbits + (height - rect.Bottom) * fileRowSize;

                List<Thread> threads = new List<Thread>();
                for (int i = 0; i < 12; i++)
                {
                    Thread thread = new Thread(() => OpenBMPFileThread(sFile, rect, offset, channel));
                    threads.Add(thread);
                    thread.Start();
                }

                foreach (Thread thread in threads)
                {
                    thread.Join();
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                br.Close();
                fs.Close();
            }
        }

        Int64 m_nThreadReadCount = 0;
        object m_openThreadLock = new object();

        public unsafe void OpenBMPFileThread(string sFile, CRect rect, CPoint offset, int channel = 0)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(sFile, FileMode.Open, FileAccess.Read, FileShare.Read, 32768, true);
            }
            catch (Exception)
            {
                return;
            }

            BinaryReader br = null;
            try
            {
                br = new BinaryReader(fs);
            }
            catch (Exception)
            {
                fs.Close();
                return;
            }

            try
            {
                // Bitmap File Header
                uint bfOffbits = 0;
                if (ReadBitmapFileHeader(br, ref bfOffbits) == false)
                    return;

                // Bitmap Info Header
                int width = 0;
                int height = 0;
                int nByte = 0;
                if (ReadBitmapInfoHeader(br, ref width, ref height, ref nByte) == false)
                    return;

                // 픽셀 데이터 존재하는 부분으로 Seek
                fs.Seek(bfOffbits, SeekOrigin.Begin);

                // 파일로부터 픽셀 데이터 읽어오기
                if (m_eMode == eMode.MemoryRead)
                {
                    int j = 0;
                    bool bFinish = false;
                    byte[] aBuf = new byte[(long)rect.Width * nByte];
                    int fileRowSize = (width * nByte + 3) & ~3; // 파일 내 하나의 열당 너비 사이즈 (4의 배수)
                    Int64 fileSize = (new System.IO.FileInfo(sFile)).Length;
                    if (nByte == 1 || nByte == 2)
                    {
                        IntPtr ptr = m_MemData.GetPtr(channel);
                        if (ptr == IntPtr.Zero)
                            return;

                        while (bFinish == false)
                        {
                            if (Worker_MemoryCopy.CancellationPending)
                                return;

                            Array.Clear(aBuf, 0, rect.Width * nByte);

                            lock (m_openThreadLock)
                            {
                                j = (int)(m_nThreadReadCount - ((Int64)bfOffbits + (height - rect.Bottom) * fileRowSize));
                                j = (rect.Bottom - 1) - rect.Top - (j / fileRowSize);

                                fs.Seek(m_nThreadReadCount, SeekOrigin.Begin);

                                // 이미지 좌측 영역 스킵
                                fs.Seek(rect.Left * nByte, SeekOrigin.Current);

                                // 파일에서 필요한 만큼 데이터를 읽기
                                fs.Read(aBuf, 0, rect.Width * nByte);
                                m_nThreadReadCount += rect.Width * nByte;

                                // 이미지 우측 영역 스킵
                                fs.Seek(fileRowSize - rect.Right * nByte, SeekOrigin.Current);

                                if (m_nThreadReadCount >= fileSize)
                                    bFinish = true;
                            }

                            // 진행상황 표시
                            p_nProgress = Convert.ToInt32(((double)rect.Height - (j - rect.Top)) / rect.Height * 100);

                            // 읽어온 데이터 메모리에 복사
                            IntPtr destPtr = ptr + ((j + offset.Y) * p_Size.X) * p_nByte;
                            if (offset.X >= 0)
                                destPtr += offset.X * p_nByte;

                            Marshal.Copy(aBuf, 0, destPtr, rect.Width * nByte);
                        }
                    }
                    else/* if (nByte == 3)*/
                    {
                        IntPtr ptrR = m_MemData.GetPtr(0);
                        IntPtr ptrG = m_MemData.GetPtr(1);
                        IntPtr ptrB = m_MemData.GetPtr(2);
                        if (ptrR == IntPtr.Zero || ptrB == IntPtr.Zero || ptrG == IntPtr.Zero)
                            return;

                        while (bFinish == false)
                        {
                            if (Worker_MemoryCopy.CancellationPending)
                                return;

                            Array.Clear(aBuf, 0, rect.Width * nByte);

                            lock (m_openThreadLock)
                            {
                                j = (int)(m_nThreadReadCount - ((Int64)bfOffbits + (height - rect.Bottom) * fileRowSize));
                                j = (rect.Bottom - 1) - rect.Top - (j / fileRowSize);

                                fs.Seek(m_nThreadReadCount, SeekOrigin.Begin);

                                // 이미지 좌측 영역 스킵
                                fs.Seek(rect.Left * nByte, SeekOrigin.Current);

                                // 파일에서 필요한 만큼 데이터를 읽어 메모리에 복사
                                fs.Read(aBuf, 0, rect.Width * nByte);
                                m_nThreadReadCount += rect.Width * nByte;

                                // 이미지 우측 영역 스킵
                                fs.Seek(fileRowSize - rect.Right * nByte, SeekOrigin.Current);

                                if (m_nThreadReadCount >= fileSize)
                                    bFinish = true;
                            }

                            // 진행상황 표시
                            p_nProgress = Convert.ToInt32(((double)rect.Height - (j - rect.Top)) / rect.Height * 100);

                            // 읽어온 데이터 메모리에 복사
                            Parallel.For(0, rect.Width, new ParallelOptions { MaxDegreeOfParallelism = 12 }, (i) =>
                            {
                                Int64 idx = ((Int64)j + offset.Y) * p_Size.X + i;
                                if (offset.X >= 0)
                                    idx += offset.X;

                                ((byte*)(ptrB))[idx] = aBuf[i * 3];
                                ((byte*)(ptrG))[idx] = aBuf[i * 3 + 1];
                                ((byte*)(ptrR))[idx] = aBuf[i * 3 + 2];
                            });
                        }
                    }
                }
                else
                {
                    p_Size = new CPoint(rect.Width + offset.X, rect.Height + offset.Y);
                    ReAllocate(p_Size, GetBytePerPixel());

                    byte[] pBuf = new byte[(long)rect.Width * nByte];

                    for (int y = p_Size.Y - 1; y >= 0; y--)
                    {
                        pBuf = br.ReadBytes((int)rect.Width * nByte);
                        Buffer.BlockCopy(pBuf, 0, m_aBuf, (int)(offset.X + (offset.Y + y) * p_Stride), (int)rect.Width * nByte);
                        p_nProgress = Convert.ToInt32(((double)(p_Size.Y - y) / p_Size.Y) * 100);
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                br.Close();
                fs.Close();
            }
        }

        public void ClearImage()
        {
            List<object> arguments = new List<object>();
            arguments.Add("test");
            Worker_MemoryClear.RunWorkerAsync(arguments);
        }

        public IntPtr GetPtr(int index)
        {
            IntPtr ip = (IntPtr)null;
            if (m_eMode == eMode.MemoryRead)
            {
                ip = (IntPtr)((long)m_MemData.GetPtr(index));
            }
            else if (m_eMode == eMode.ImageBuffer)
            {
            }
            return ip;
        }


        public IntPtr GetPtr(int y = 0, int x = 0)
        {
            IntPtr ip = (IntPtr)null;
            if (m_eMode == eMode.MemoryRead)
            {
                ip = (IntPtr)((long)m_ptrImg + p_nByte * (y * p_Stride + x));
            }
            else if (m_eMode == eMode.ImageBuffer)
            {
                if (m_aBuf == null)
                    return (IntPtr)null;
                try
                {
                    unsafe
                    {
                        fixed (byte* p = &m_aBuf[(long)p_nByte * (y * p_Stride + x)])
                        {
                            ip = (IntPtr)(p);
                        }
                    }
                }
                catch (Exception)
                {
                    return (IntPtr)null;
                }
            }
            return ip;
        }
        public void Clear()
        {
            m_aBuf = null;
            _nByte = 0;
            p_Size.X = 0;
            p_Size.Y = 0;
        }


        public void CopyToBuffer(out byte[] buffer, Rect rect = default(Rect))
        {
            int startX = (int)rect.Left;
            int startY = (int)rect.Top;
            int width = (int)rect.Width;
            int height = (int)rect.Height;
            int byteCount = GetBytePerPixel();

            if (rect == default(Rect))
            {
                buffer = new byte[m_aBuf.Length];
                Buffer.BlockCopy(m_aBuf, 0, buffer, 0, m_aBuf.Length);
            }
            else
            {
                buffer = new byte[(int)(rect.Width * rect.Height * byteCount)];

                for (int i = 0; i < height; i++)
                    Array.Copy(m_aBuf, (i + startY) * p_Stride + startX, buffer, i * width * byteCount, width * byteCount);
            }    
        }

        #region 주석
        //Bitmap bitmap = new Bitmap(sFile);
        //        if (bitmap == null) return "FileOpen Error"; 
        //        string sClone = Clone(bitmap);
        //        if (sClone == "OK") _bNew = true;
        //        return sClone; 

        //     public string Clone(Bitmap bitmap)
        //    {
        //        switch (bitmap.PixelFormat)
        //        {
        //            case PixelFormat.Format8bppIndexed:
        //                _nByte = 1;
        //                break;
        //            case PixelFormat.Format24bppRgb:
        //                _nByte = 3;
        //                break;
        //            case PixelFormat.Format32bppArgb:
        //                _nByte = 4;
        //                break;
        //            default:
        //                return "Invalid Pixel Format : " + bitmap.PixelFormat.ToString();
        //        }
        //        CPoint sz = new CPoint(bitmap.Width, bitmap.Height);
        //        p_sz = new CPoint((bitmap.Width / 4) * 4, bitmap.Height);
        //        BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
        //        Marshal.Copy(bitmapData.Scan0, m_aBuf, 0, p_nByte * p_sz.X * p_sz.Y);
        //        bitmap.UnlockBits(bitmapData);
        //        InvY();
        //        return "OK";
        //    }

        //    public Mat GetMat(CPoint cp, CPoint szROI)
        //    {
        //        if (cp.X < 0)
        //            cp.X = 0;
        //        if (cp.Y < 0)
        //            cp.Y = 0;
        //        if ((cp.X + szROI.X) > p_sz.X)
        //            szROI.X = p_sz.X - cp.X;
        //        if ((cp.Y + szROI.Y) > p_sz.Y)
        //            szROI.Y = p_sz.Y - cp.Y;
        //        if (szROI.X < 0)
        //            return null;
        //        if (szROI.Y < 0)
        //            return null;
        //        System.Drawing.Size sz = new System.Drawing.Size(szROI.X, szROI.Y);
        //        return new Mat(sz, DepthType.Cv8U, p_nByte, GetIntPtr(cp.Y, cp.X), (int)W);
        //    }
        //    public void InvY()
        //    {
        //        byte[] aTemp = new byte[W];
        //        long yp0 = 0;
        //        long yp1 = W * (p_sz.Y - 1); 
        //        for (int y = 0; y < p_sz.Y / 2; y++)
        //        {
        //            Array.Copy(m_aBuf, yp0, aTemp, 0, W);
        //            Array.Copy(m_aBuf, yp1, m_aBuf, yp0, W);
        //            Array.Copy(aTemp, 0, m_aBuf, yp1, W);
        //            yp0 += W;
        //            yp1 -= W; 
        //        }
        //    }

        //    public void ChangeGray()
        //    {
        //        if (p_nByte == 1) return; 
        //        byte[] aBuf = m_aBuf;
        //        int nByte = p_nByte;
        //        p_nByte = 1;
        //        Parallel.For(0, p_sz.Y, y => ChangeGray(y, nByte, aBuf)); 
        //    }

        //    void ChangeGray(long y, int nByte, byte[] aBuf)
        //    {
        //        long ySrc = y * nByte * p_sz.X;
        //        long yDst = y * W;
        //        for (int x = 0; x < p_sz.X; x++, yDst++, ySrc += nByte)
        //        {
        //            double fGV = 0.114 * aBuf[ySrc];
        //            fGV += 0.587 * aBuf[ySrc + 1];
        //            fGV += 0.299 * aBuf[ySrc + 2];
        //            m_aBuf[yDst] = (byte)Math.Round(fGV); 
        //        }
        //    }

        //    public Bitmap GetBitmap()
        //    {
        //        if ((p_sz.X < 1) || (p_sz.Y < 1)) return null;
        //        PixelFormat pixelFormat; 
        //        switch (p_nByte)
        //        {
        //            case 1: pixelFormat = PixelFormat.Format8bppIndexed; break;
        //            case 3: pixelFormat = PixelFormat.Format24bppRgb; break;
        //            case 4: pixelFormat = PixelFormat.Format32bppRgb; break;
        //            default: return null;
        //        }
        //        Bitmap bitmap = new Bitmap(p_sz.X, p_sz.Y, (int)W, pixelFormat, GetIntPtr(0, 0));
        //        if (p_nByte == 1) SetPalette(bitmap);
        //        bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY); 
        //        return bitmap; 
        //    }

        //    void SetPalette(Bitmap bitmap)
        //    {
        //        int n;
        //        ColorPalette palette = bitmap.Palette;
        //        for (n = 0; n < 256; n++) palette.Entries[n] = Color.FromArgb(n, n, n);
        //        bitmap.Palette = palette;
        //    }

        //    public IntPtr GetIntPtr(int y, int x) 
        //    {
        //        IntPtr ip;
        //        if (m_aBuf == null) return (IntPtr)null;
        //        try
        //        {
        //            unsafe
        //            {
        //                fixed (byte* p = &m_aBuf[y * W + x * p_nByte]) 
        //                { 
        //                    ip = (IntPtr)(p); 
        //                }
        //            }
        //        }
        //        catch (Exception)
        //        {
        //            return (IntPtr)null; 
        //        }
        //        return ip;
        //    }

        //    public string FileSave()
        //    {
        //        SaveFileDialog dlg = new SaveFileDialog();
        //        dlg.Filter = "Image Files(*.bmp;*.jpg)|*.bmp;*.jpg";
        //        if (dlg.ShowDialog() == false) return "Image Save File not Found !!";
        //        return FileSave(dlg.FileName);
        //    }

        //    public string FileSave(string sFile)
        //    {
        //        m_sFile = sFile; 
        //        string[] sFiles = sFile.Split(new char[] { '.' });
        //        int l = sFiles.Length;
        //        if (l < 2) return "Invalid File Name : " + sFile;
        //        string sExt = sFiles[l - 1].ToLower();
        //        Bitmap bitmap = GetBitmap();
        //        if (bitmap == null) return "No Image !!";
        //        try
        //        {
        //            if (sExt == "bmp") bitmap.Save(sFile, ImageFormat.Bmp);
        //            if (sExt == "jpg") bitmap.Save(sFile, ImageFormat.Jpeg);
        //        }
        //        catch (Exception ex)
        //        {
        //            return "Image File Save Exception : " + sFile + " : " + ex.Message;
        //        }
        //        return "OK";
        //    }

        //    public string m_sFile = ""; 
        //    public string FileOpen(string sFile)
        //    {
        //        m_sFile = sFile; 
        //        string[] sFiles = sFile.Split(new char[] { '.' });
        //        int l = sFiles.Length;
        //        if (l < 2) return "Invalid File Name : " + sFile; 
        //        string sExt = sFiles[l-1].ToLower();
        //        try
        //        {
        //            switch (sExt)
        //            {
        //                case "bmp": return FileOpenBMP(sFile);
        //                case "jpg": return FileOpenBitmap(sFile);
        //                default: return "Invalid File Ext : " + sFile;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            return "Image File Open Exception : " + sFile + " : " + ex.Message; 
        //        }
        //    }

        //    string FileOpenBMP(string sFile)
        //    {
        //        int nByte;
        //        CPoint szImg = new CPoint(0, 0);
        //        FileStream fs = null;
        //        try { fs = new FileStream(sFile, FileMode.Open, FileAccess.Read, FileShare.Read, 32768, true); }
        //        catch (Exception ex) { return ex.Message; }
        //        BinaryReader br = new BinaryReader(fs);
        //        ushort bfType = br.ReadUInt16();
        //        uint bfSize = br.ReadUInt32();
        //        br.ReadUInt16(); 
        //        br.ReadUInt16();
        //        uint bfOffBits = br.ReadUInt32();
        //        if (bfType != 0x4D42) return "File is not BMP !!";
        //        uint biSize = br.ReadUInt32();
        //        szImg.X = br.ReadInt32();
        //        szImg.Y = br.ReadInt32();
        //        br.ReadUInt16();
        //        nByte = br.ReadUInt16() / 8;
        //        br.ReadUInt32();
        //        br.ReadUInt32();
        //        br.ReadInt32(); 
        //        br.ReadInt32();
        //        br.ReadUInt32(); 
        //        br.ReadUInt32();
        //        if (nByte == 1)
        //        {
        //            byte[] hRGB = br.ReadBytes(256 * 4);
        //            _nByte = nByte; 
        //            p_sz = szImg;
        //            for (int y = 0; y < p_sz.Y; y++)
        //            {
        //                byte[] pBuf = br.ReadBytes((int)W);
        //                Buffer.BlockCopy(pBuf, 0, m_aBuf, (int)(y * W), (int)W); 
        //            }
        //            br.Close();
        //        }
        //        else
        //        {
        //            br.Close();
        //            return FileOpenBitmap(sFile); 
        //        }
        //        _bNew = true;
        //        return "OK";
        //    }

        //    string FileOpenBitmap(string sFile)
        //    {
        //        Bitmap bitmap = new Bitmap(sFile);
        //        if (bitmap == null) return "FileOpen Error"; 
        //        string sClone = Clone(bitmap);
        //        if (sClone == "OK") _bNew = true;
        //        return sClone; 
        //    }

        //    public string Copy(ImageD imgSrc, CPoint cpSrc, CPoint sz, CPoint cpDst)
        //    {
        //        if (imgSrc.p_sz.IsInside(cpSrc + sz) == false) return "Image Src Area not Valid";
        //        if (p_sz.IsInside(cpDst + sz) == false) return "Image Dst Area not Valid";
        //        if (imgSrc.p_nByte != p_nByte) return "Image Byte MissMatch"; 
        //        long pSrc = cpSrc.Y * imgSrc.W + cpSrc.X * imgSrc.p_nByte;
        //        long pDst = cpDst.Y * W + cpDst.X * p_nByte;
        //        int w = p_nByte * sz.X; 
        //        for (int y = 0; y < sz.Y; y++)
        //        {
        //            Array.Copy(imgSrc.m_aBuf, pSrc, m_aBuf, pDst, w);
        //            pSrc += imgSrc.W;
        //            pDst += W; 
        //        }
        //        return "OK"; 
        //    }

        //    public bool HasImage()
        //    {
        //        return (W * p_sz.Y != 0);
        //    }

        //    public string GetGVString(CPoint cpImg)
        //    {
        //        if ((cpImg.X < 0) || (cpImg.X >= p_sz.X)) return "0ut";
        //        if ((cpImg.Y < 0) || (cpImg.Y >= p_sz.Y)) return "0ut";
        //        switch (p_nByte)
        //        {
        //            case 1: return m_aBuf[cpImg.X + W * cpImg.Y].ToString();
        //            case 3:
        //                long lAdd = 3 * cpImg.X + W * cpImg.Y;
        //                return "(" + m_aBuf[lAdd + 2].ToString() + "," + m_aBuf[lAdd + 1].ToString() + "," + m_aBuf[lAdd].ToString() + ")";
        //            default: return "Unknown";
        //        }
        //    }
        //}
        #endregion
    }


    public static class ImageHelper
    {
        /// <summary>
        /// ImageSource to bytes
        /// </summary>
        /// <param name="encoder"></param>
        /// <param name="imageSource"></param>
        /// <returns></returns>
        public static byte[] ImageSourceToBytes(BitmapEncoder encoder, ImageSource imageSource)
        {
            byte[] bytes = null;
            var bitmapSource = imageSource as BitmapSource;

            if (bitmapSource != null)
            {
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                using (var stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    bytes = stream.ToArray();
                }
            }

            return bytes;
        }

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr value);

        public static Bitmap GetBitmap(BitmapSource source)
        {
            Bitmap bmp = new Bitmap
            (
              source.PixelWidth,
              source.PixelHeight,
              System.Drawing.Imaging.PixelFormat.Format32bppPArgb
            );

            BitmapData data = bmp.LockBits
            (
                new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size),
                ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppPArgb
            );

            source.CopyPixels
            (
              Int32Rect.Empty,
              data.Scan0,
              data.Height * data.Stride,
              data.Stride
            );

            bmp.UnlockBits(data);

            return bmp;
        }

        public static BitmapSource GetImageSource(Image myImage)
        {
            var bitmap = new Bitmap(myImage);
            IntPtr bmpPt = bitmap.GetHbitmap();
            BitmapSource bitmapSource =
             System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                   bmpPt,
                   IntPtr.Zero,
                   Int32Rect.Empty,
                   BitmapSizeOptions.FromEmptyOptions());

            //freeze bitmapSource and clear memory to avoid memory leaks
            bitmapSource.Freeze();
            DeleteObject(bmpPt);

            return bitmapSource;
        }

        /// <summary>
        /// Convert String to ImageFormat
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static System.Drawing.Imaging.ImageFormat ImageFormatFromString(string format)
        {
            if (format.Equals("Jpg"))
                format = "Jpeg";
            Type type = typeof(System.Drawing.Imaging.ImageFormat);
            BindingFlags flags = BindingFlags.GetProperty;
            object o = type.InvokeMember(format, flags, null, type, null);
            return (System.Drawing.Imaging.ImageFormat)o;
        }

        /// <summary>
        /// Read image from path
        /// </summary>
        /// <param name="imageFile"></param>
        /// <param name="imageFormat"></param>
        /// <returns></returns>
        public static byte[] BytesFromImage(String imageFile, System.Drawing.Imaging.ImageFormat imageFormat)
        {
            MemoryStream ms = new MemoryStream();
            Image img = Image.FromFile(imageFile);
            img.Save(ms, imageFormat);
            return ms.ToArray();
        }

        /// <summary>
        /// Convert image to byte array
        /// </summary>
        /// <param name="imageIn"></param>
        /// <param name="imageFormat"></param>
        /// <returns></returns>
        public static byte[] ImageToByteArray(System.Drawing.Image imageIn, System.Drawing.Imaging.ImageFormat imageFormat)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, imageFormat);
            return ms.ToArray();
        }

        /// <summary>
        /// Byte array to photo
        /// </summary>
        /// <param name="byteArrayIn"></param>
        /// <returns></returns>
        public static Image ByteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            //TypeConverter tc = TypeDescriptor.GetConverter(typeof(Bitmap));

            //Bitmap b = (Bitmap)tc.ConvertFrom(ms, true, false);
            Image returnImage = Image.FromStream(ms, true, false);
            return returnImage;
        }
        public static Bitmap ToBitmap(Image<Gray, byte> image)
        {
            using (System.Drawing.Bitmap source = image.Bitmap)
            {
                var bitmapData = source.LockBits(
                new System.Drawing.Rectangle(0, 0, source.Width, source.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, source.PixelFormat);

                BitmapSource bitmapSource = BitmapSource.Create(
                source.Width, source.Height,
                _dpi, _dpi,
                PixelFormats.Gray8, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);



                source.UnlockBits(bitmapData);

                return source;
            }


        }

        public static Mat ToMat(BitmapSource source)
        {
            if (source.Format == PixelFormats.Bgra32)
            {
                Mat result = new Mat();
                result.Create(source.PixelHeight, source.PixelWidth, DepthType.Cv8U, 4);
                source.CopyPixels(Int32Rect.Empty, result.DataPointer, result.Step * result.Rows, result.Step);
                return result;
            }
            else if (source.Format == PixelFormats.Bgr24)
            {
                Mat result = new Mat();
                result.Create(source.PixelHeight, source.PixelWidth, DepthType.Cv8U, 3);
                source.CopyPixels(Int32Rect.Empty, result.DataPointer, result.Step * result.Rows, result.Step);
                return result;
            }
            else
            {
                throw new Exception(String.Format("Convertion from BitmapSource of format {0} is not supported.", source.Format));
            }
        } 
        public static BitmapSource ToBitmapSource(Image<Bgra, byte> image)
        {
            using (System.Drawing.Bitmap source = image.Bitmap)
            {
                var bitmapData = source.LockBits(
                new System.Drawing.Rectangle(0, 0, source.Width, source.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, source.PixelFormat);

                BitmapSource bitmapSource = BitmapSource.Create(
                source.Width, source.Height,
                _dpi, _dpi,
                PixelFormats.Bgra32, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

                source.UnlockBits(bitmapData);

                //DeleteObject(ptr);
                return bitmapSource;
            }
        }
        public const int _dpi = 96;

        public static BitmapSource ToBitmapSource(Image<Gray, byte> image)
        {
            using (System.Drawing.Bitmap source = image.Bitmap)
            {
                var bitmapData = source.LockBits(
                new System.Drawing.Rectangle(0, 0, source.Width, source.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, source.PixelFormat);

                BitmapSource bitmapSource = BitmapSource.Create(
                source.Width, source.Height,
                _dpi, _dpi,
                PixelFormats.Gray8, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);


                source.UnlockBits(bitmapData);

                //DeleteObject(ptr);
                return bitmapSource;
            }
        }
        public static BitmapSource ToBitmapSource(Image<Rgb, byte> image)
        {
            using (System.Drawing.Bitmap source = image.Bitmap)
            {


                var bitmapData = source.LockBits(
                new System.Drawing.Rectangle(0, 0, source.Width, source.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, source.PixelFormat);

                BitmapSource bitmapSource = BitmapSource.Create(
                source.Width, source.Height,
                _dpi, _dpi,
                PixelFormats.Bgr24, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);


                source.UnlockBits(bitmapData);

                //DeleteObject(ptr);
                return bitmapSource;
            }


        }

        public static BitmapSource GetBitmapSourceFromBitmap(Bitmap bitmap)
        {
            BitmapSource bitmapSource;


            IntPtr hBitmap = bitmap.GetHbitmap();
            BitmapSizeOptions sizeOptions = BitmapSizeOptions.FromEmptyOptions();
            bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, sizeOptions);
            bitmapSource.Freeze();


            return bitmapSource;
        }


        public static byte[] FileLoadBitmap(string sFilePath, int nW, int nH, int nByteCnt = 1)
        {
            byte[] rawdata = new byte[(long)nW * nH];
            CLR_IP.Cpp_LoadBMP(sFilePath, rawdata, nW, nH, nByteCnt);
            return rawdata;
        }

        public static void FileSaveBitmap(string sFilePath, byte[] rawdata, int nW, int nH, int nByteCnt = 1)
        {
            CLR_IP.Cpp_SaveBMP(sFilePath, rawdata, nW, nH, nByteCnt);
        }
    }
}
