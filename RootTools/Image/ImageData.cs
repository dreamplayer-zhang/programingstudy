﻿using Emgu.CV;
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

namespace RootTools
{
    public class ImageData :ObservableObject
    {
        public enum eMode
        {
            MemoryRead,
            ImageBuffer,
        }
        public eMode m_eMode = eMode.MemoryRead;

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
                if (m_Size != value)
                    ReAllocate(value, p_nByte);
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

        public long p_Stride
        {
            get
            {
                return (long)p_nByte * p_Size.X;
            }
        }

        public IntPtr m_ptrImg;
        byte[] m_aBuf;
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
        public BackgroundWorker Worker_MemorySave = new BackgroundWorker();

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
                UpdateOpenProgress(m_nProgress);
            }
        }

        public ImageData(int Width, int Height, int nByte = 1)
        {
            m_eMode = eMode.ImageBuffer;
            p_Size = new CPoint(Width, Height);
            ReAllocate(p_Size, nByte);
            
        }
        public void SetData(IntPtr ptr, CRect rect, int stride)
        {
            for (int i = rect.Height - 1; i >= 0; i--)
            {
                Marshal.Copy((IntPtr)((long)ptr + rect.Left + ((long)i + (long)rect.Top) * stride), m_aBuf, 0, rect.Width);
            }

        }
        public ImageData(MemoryData data)
        {       
            m_eMode = eMode.MemoryRead;
            m_ptrImg = data.GetPtr();
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
            Worker_MemorySave.DoWork += Worker_MemorySave_DoWork;
            Worker_MemorySave.WorkerSupportsCancellation = true;
        }


        public void UpdateImage()
        {
            OnUpdateImage();
        }

        public void SaveRectImage(CRect memRect)
        {
            SaveFileDialog ofd = new SaveFileDialog();
            ofd.Filter = "BMP파일|*.bmp";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                List<object> arguments = new List<object>();
                arguments.Add(ofd.FileName);
                arguments.Add(memRect);
                
                Worker_MemorySave.RunWorkerAsync(arguments);
            }
        }

        public void SaveWholeImage()
        {
            SaveFileDialog ofd = new SaveFileDialog();
            ofd.Filter = "BMP파일|*.bmp";
            
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                List<object> arguments = new List<object>();
                arguments.Add(ofd.FileName);
                arguments.Add(new CRect(0, 0, p_Size.X, p_Size.Y));
                Worker_MemorySave.RunWorkerAsync(arguments);
            }
        }
        
        void Worker_MemorySave_DoWork(object sender, DoWorkEventArgs e)
        {
            List<object> arguments = (List<object>)(e.Argument);

            string sPath = arguments[0].ToString();
            CRect MemRect = (CRect)arguments[1];

            FileSaveBMP(sPath, m_ptrImg, MemRect);
        }

        unsafe void FileSaveBMP(string sFile, IntPtr ptr, CRect rect)
        {
            if(rect.Width %4 != 0)
            {
                rect.Right += 4 - rect.Width % 4;
            }
            if (rect.Height % 4 != 0)
            {
                rect.Bottom+= 4 - rect.Height % 4;
            }

            FileStream fs = new FileStream(sFile, FileMode.CreateNew, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fs);

            bw.Write(Convert.ToUInt16(0x4d42));
            bw.Write(Convert.ToUInt32(54 + 1024 + rect.Width * rect.Height));
            //image 크기 bw.Write();   bmfh.bfSize = sizeof(14byte) + nSizeHdr + rect.right * rect.bottom;
            bw.Write(Convert.ToUInt16(0));   //reserved
            bw.Write(Convert.ToUInt16(0));   //reserved
            bw.Write(Convert.ToUInt32(1078));

            bw.Write(Convert.ToUInt32(40));
            bw.Write(Convert.ToInt32(rect.Width));
            bw.Write(Convert.ToInt32(rect.Height));
            bw.Write(Convert.ToUInt16(1));
            bw.Write(Convert.ToUInt16(8));     //byte                      
            bw.Write(Convert.ToUInt32(0));      //compress
            bw.Write(Convert.ToUInt32(rect.Width * rect.Height));
            bw.Write(Convert.ToInt32(0));
            bw.Write(Convert.ToInt32(0));
            bw.Write(Convert.ToUInt32(256));      //color
            bw.Write(Convert.ToUInt32(256));      //import
            for (int i = 0; i < 256; i++)
            {
                bw.Write(Convert.ToByte(i));
                bw.Write(Convert.ToByte(i));
                bw.Write(Convert.ToByte(i));
                bw.Write(Convert.ToByte(255));
            }
            byte[] aBuf = new byte[rect.Width];
            for (int i = rect.Height-1; i >=0 ; i--)
            {
                Marshal.Copy((IntPtr)((long)ptr + rect.Left + ((long)i + (long)rect.Top) * p_Size.X), aBuf, 0, rect.Width);
                bw.Write(aBuf);
                 p_nProgress = Convert.ToInt32(((double)(rect.Height-i) / rect.Height) * 100);
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
        public void OpenFile(string sFileName, CPoint offset)
        {
            FileInfo fileInfo = new FileInfo(sFileName);
            if (fileInfo.Exists)
            {
                List<object> arguments = new List<object>();
                arguments.Add(sFileName);
                arguments.Add(offset);
                Worker_MemoryCopy.RunWorkerAsync(arguments);
            }
            else
            {
                //MessageBox.Show("OpenFile() - 파일이 존재 하지 않거나 열기에 실패하였습니다. - " + sFileName);
            }
        }

        void Worker_MemoryCopy_DoWork(object sender, DoWorkEventArgs e)
        {
            List<object> arguments = (List<object>)(e.Argument);

            string sPath = arguments[0].ToString();
            CPoint offset = (CPoint)(arguments[1]);
            if (sPath.ToLower().IndexOf(".bmp") >= 0)
            {
                OpenBMPFile(sPath, e, offset);
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


        void Worker_MemoryClear_DoWork(object sender, DoWorkEventArgs e)
        {  
            byte[] pBuf = new byte[p_Size.X];
            for (int y = 0; y < p_Size.Y; y++)
            {
                if (Worker_MemoryClear.CancellationPending)
                    return;
                Marshal.Copy(pBuf, 0, (IntPtr)((long)m_ptrImg + (long)p_Size.X * y), p_Size.X);
                p_nProgress = Convert.ToInt32(((double)y / p_Size.Y) * 100);
            }
        }

        void Worker_MemoryClear_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            p_nProgress = 100;
            OnCreateNewImage();
        }

        unsafe void OpenBMPFile(string sFile, DoWorkEventArgs e, CPoint offset)
        {  
            int nByte;
            int nWidth = 0, nHeight = 0;
            FileStream fs = null;
            try
            {
                fs = new FileStream(sFile, FileMode.Open, FileAccess.Read, FileShare.Read, 32768, true);
            }
            catch (Exception)
            {
                return;
            }

            int a = 0;
            UInt32 b = 0;
            BinaryReader br = new BinaryReader(fs);
            ushort bfType = br.ReadUInt16();
            uint bfSize = br.ReadUInt32();
            br.ReadUInt16();
            br.ReadUInt16();
            uint bfOffBits = br.ReadUInt32();
            if (bfType != 0x4D42)
                return;
            uint biSize = br.ReadUInt32();
            nWidth = br.ReadInt32();
            nHeight = br.ReadInt32();
            a = br.ReadUInt16();
            nByte = br.ReadUInt16() / 8;
            b = br.ReadUInt32();
            b = br.ReadUInt32();
            a = br.ReadInt32();
            a = br.ReadInt32();
            b = br.ReadUInt32();
            b = br.ReadUInt32();

            int lowwidth = 0, lowheight = 0;
            lowwidth = nWidth < p_Size.X - offset.X ? nWidth : p_Size.X - offset.X;
            lowheight = nHeight < p_Size.Y - offset.Y ? nHeight : p_Size.Y - offset.Y;

                if (m_eMode == eMode.MemoryRead)
                {
                    byte[] hRGB = br.ReadBytes(256 * 4);

                     for (int y = lowheight-1; y >=0 ; y--) 
                    {
                        if (Worker_MemoryCopy.CancellationPending)
                            return;
                        byte[] pBuf = br.ReadBytes(nWidth);
                        Marshal.Copy(pBuf, 0, (IntPtr)((long)m_ptrImg + offset.X + (long)p_Size.X * ((long)offset.Y + y)), lowwidth);
                        p_nProgress = Convert.ToInt32(((double)(lowheight -y) / lowheight) * 100);
                    }
                }
                else
                {
                    byte[] hRGB = br.ReadBytes(256 * 4);
                    p_nByte = nByte;
                    p_Size = new CPoint(nWidth + offset.X, nHeight + offset.Y);
                    ReAllocate(p_Size, _nByte);
                    for (int y = p_Size.Y - 1; y >= 0; y--)
                    {
                        byte[] pBuf = br.ReadBytes((int)nWidth);
                        Buffer.BlockCopy(pBuf, 0, m_aBuf, (int)(offset.X + (offset.Y + y) * p_Stride), (int)nWidth);
                        p_nProgress = Convert.ToInt32(((double)(p_Size.Y-y) / p_Size.Y) * 100);
                       
                    }
                }
                br.Close();
        }

        bool ReAllocate(CPoint sz, int nByte )
        {  
            p_Size.X = (p_Size.X  / 4) * 4;
            if (p_nByte <= 0)
                return false;
            if ((p_Size.X < 1) || (p_Size.Y < 1))
                return false;
            m_aBuf = new byte[(p_Size.X) * p_nByte * (p_Size.Y)];
            return false;
        }

        public void ClearImage()
        {
            List<object> arguments = new List<object>();
            arguments.Add("test");
            Worker_MemoryClear.RunWorkerAsync(arguments);
        }

        public IntPtr GetPtr(int y=0, int x=0)
        {
            IntPtr ip = (IntPtr)null;
            if (m_eMode == eMode.MemoryRead)
            {
                ip = (IntPtr)((long)m_ptrImg + y * p_Stride + x * p_nByte);
            }
            else if(m_eMode == eMode.ImageBuffer)
            {
                if (m_aBuf == null)
                    return (IntPtr)null;
                try
                {
                    unsafe
                    {
                        fixed (byte* p = &m_aBuf[y * p_Stride + x * p_nByte])
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

        public static BitmapSource GetImageStream(Image myImage)
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

        public static BitmapSource ToBitmapSource(Image<Gray, byte> image)
        {
            using (System.Drawing.Bitmap source = image.Bitmap)
            {


                var bitmapData = source.LockBits(
        new System.Drawing.Rectangle(0, 0, source.Width, source.Height),
        System.Drawing.Imaging.ImageLockMode.ReadOnly, source.PixelFormat);

                BitmapSource bitmapSource = BitmapSource.Create(
       source.Width, source.Height,
       source.HorizontalResolution, source.VerticalResolution,
       PixelFormats.Gray8, null,
       bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);


                source.UnlockBits(bitmapData);

                //DeleteObject(ptr);
                return bitmapSource;
            }
        }
    }
}
