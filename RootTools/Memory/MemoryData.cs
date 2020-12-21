using Microsoft.Win32;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace RootTools.Memory
{
    public class MemoryData : NotifyProperty
    {
        const double c_fMB = 1024 * 1024;

        #region Property
        public string p_id { get; set; }

        int _nCount = 0;
        public int p_nCount 
        {
            get { return _nCount; }
            set
            {
                if (_nCount != value)
                {
                    _nCount = value;
                    OnPropertyChanged();
                    m_group.InitAddress();
                }
                while (m_aDraw.Count < value) m_aDraw.Add(new MemoryDraw(this)); 
            }
        }

        int _nByte;
        public int p_nByte
        {
            get { return _nByte; }
            set
            {
                if (_nByte != value)
                {
                    _nByte = value;
                    OnPropertyChanged();
                    OnPropertyChanged("p_sSize");
                    m_group.InitAddress();
                }
            }
        }

        CPoint _sz = new CPoint();
        public CPoint p_sz
        {
            get { return _sz; }
            set
            {
                if (_sz != value)
                {
                    _sz = value;
                    OnPropertyChanged();
                    OnPropertyChanged("p_sSize");
                    m_group.InitAddress();
                }
            }
        }

        public long W
        {
            get { return (long)p_nByte * p_sz.X; }
        }

        public string p_sSize
        {
            get
            {
                double fSize = p_lSize;
                fSize /= 1024;
                fSize *= p_nCount;
                fSize /= 1024;
                if (fSize < 1024) return fSize.ToString("0.0") + " MB";
                fSize /= 1024;
                return fSize.ToString("0.0") + " GB";
            }
        }

        int _mbOffset = 0;
        public int p_mbOffset
        {
            get { return _mbOffset; }
            set
            {
                if (_mbOffset == value) return;
                _mbOffset = value;
                OnPropertyChanged();
            }
        }

        public long p_lSize
        {
            get
            {
                long _lSize = p_nByte * p_sz.X;
                return _lSize * p_sz.Y;
            }
        }

        StopWatch m_swInfo = new StopWatch();
        string _sInfo = "OK";
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (_sInfo == value) return;
                if (m_swInfo.ElapsedMilliseconds < 10000) return;
                m_swInfo.Start();
                if (value == "OK") m_log.Info("memoryData " + p_id + " p_sInfo = " + value);
                else m_log.Error("memoryData " + p_id + " p_sInfo = " + value);
                _sInfo = value;
            }
        }
        #endregion

        #region Address
        public List<long> m_aAddress = new List<long>();
        public void InitAddress(ref int mbOffset)
        {
            p_mbOffset = mbOffset; 
            m_aAddress.Clear();
            p_sInfo = CheckParamOK();
            if (p_sInfo != "OK") return;
            try
            {
                long pAddress = m_group.m_pool.m_pAddress; 
                pAddress += (long)Math.Ceiling(p_mbOffset * c_fMB);
                for (int n = 0; n < p_nCount; n++)
                {
                    m_aAddress.Add(pAddress);
                    pAddress += p_lSize; 
                }
            }
            catch (Exception e) 
            { 
                m_log.Error(e, "Create Buffer Memory Error"); 
            }
            mbOffset += (int)Math.Ceiling(p_nCount * p_lSize / c_fMB);
        }
		public ulong GetMBOffset()
		{
            return (ulong)Math.Ceiling(p_mbOffset * c_fMB);
        }
        string CheckParamOK()
        {
            if (p_nCount < 1) return "p_nCount = " + p_nCount.ToString();
            switch (p_nByte)
            {
                case 1:
                case 3:
                case 4: break;
                default: return "p_nByte = " + p_nByte.ToString();
            }
            if ((p_sz.X <= 0) || (p_sz.Y <= 0)) return "p_szImage = " + p_sz.ToString();
            return "OK";
        }

        public IntPtr GetPtr()
        {
            if (m_aAddress.Count == 0) return IntPtr.Zero;
            return (IntPtr)m_aAddress[0];
        }

        public IntPtr GetPtr(int nIndex)
        {
            if ((nIndex < 0) || (nIndex >= m_aAddress.Count)) return IntPtr.Zero;
            return (IntPtr)m_aAddress[nIndex];
        }

        public IntPtr GetPtr(int nIndex, int x, int y)
        {
            IntPtr ip = GetPtr(nIndex);
            try
            {
                unsafe
                {
                    byte* p = (byte*)ip.ToPointer();
                    p += y * W + x * p_nByte;
                    return (IntPtr)(p);
                }
            }
            catch (Exception) { return IntPtr.Zero; }
        }
        #endregion

        #region File Raw
        public string SaveMemory(string sFile)
        {
            string sTitle = sFile.Replace(".raw", ""); 
            for (int nIndex = 0; nIndex < p_nCount; nIndex++)
            {
                FileStream fs = null;
                BinaryWriter bw = null;
                try
                {
                    fs = new FileStream(sTitle + nIndex.ToString("_000") + ".raw", FileMode.Create);
                    bw = new BinaryWriter(fs);
                    unsafe
                    {
                        bw.Write(p_nByte);
                        bw.Write(p_sz.X);
                        bw.Write(p_sz.Y);
                        int w = p_sz.X * p_nByte;
                        for (int y = 0; y < p_sz.Y; y++)
                        {
                            byte* p = (byte*)GetPtr(nIndex, 0, y);
                            for (int n = 0; n < w; n++, p++) bw.Write(*p);
                        }
                    }
                }
                catch (Exception e) { return "Save " + sFile + " Error : " + e.Message; }
                finally
                {
                    if (bw != null) bw.Close();
                    if (fs != null) fs.Close();
                }
            }
            p_sInfo = "Save Memory Done : " + p_id; 
            return "OK";
        }

        public string ReadMemory(string sFile)
        {
            string sTitle = sFile.Substring(0, sFile.Length - 7);
            for (int nIndex = 0; nIndex < p_nCount; nIndex++)
            {
                FileStream fs = null;
                BinaryReader br = null;
                try
                {
                    fs = new FileStream(sTitle + nIndex.ToString("000") + ".raw", FileMode.Open);
                    br = new BinaryReader(fs);
                    unsafe
                    {
                        int nByte = br.ReadInt32();
                        if (nByte != p_nByte) return "p_nByte not Correct";
                        int sX = Math.Min(br.ReadInt32(), p_sz.X);
                        int sY = Math.Min(br.ReadInt32(), p_sz.Y);
                        int wm = p_sz.X * p_nByte;
                        int wr = Math.Max(sX * p_nByte - wm, 0);
                        for (int y = 0; y < sY; y++)
                        {
                            byte* p = (byte*)GetPtr(nIndex, 0, y);
                            for (int n = 0; n < wm; n++, p++) *p = br.ReadByte();
                            for (int n = 0; n < wr; n++) br.ReadByte();
                        }
                    }
                }
                catch (Exception e) { return "Read " + sFile + " Error : " + e.Message; }
                finally
                {
                    if (br != null) br.Close();
                    if (fs != null) fs.Close();
                }
            }
            return "OK";
        }
        #endregion

        #region Bayer
        public string FileOpenBayer(string sFile, int nIndex)
        {
            if ((nIndex < 0) || (nIndex >= p_nCount)) return "Invalid Index";
            FileStream fs = null;
            try
            {
                fs = new FileStream(sFile, FileMode.Open, FileAccess.Read, FileShare.Read, 32768, true);
            }
            catch (Exception) { return "File Open Error"; }
            BinaryReader br = new BinaryReader(fs);
            CPoint szMemory = new CPoint(p_sz);
            for (int y = 0; y < szMemory.Y; y++)
            {
                byte[] pBuf = br.ReadBytes(p_nByte * szMemory.X);
                Marshal.Copy(pBuf, 0, GetPtr(nIndex, 0, y), p_nByte * szMemory.X);
            }
            br.Close();
            fs.Close();
            return "OK"; 
        }
        #endregion

        #region BMP File
        public string FileOpenBMP(string sFile, int nIndex)
        {
            if ((nIndex < 0) || (nIndex >= p_nCount)) return "Invalid Index"; 
            FileStream fs = null;
            try 
            { 
                fs = new FileStream(sFile, FileMode.Open, FileAccess.Read, FileShare.Read, 32768, true); 
            }
            catch (Exception) { return "File Open Error"; }
            BinaryReader br = new BinaryReader(fs);
            if (br.ReadUInt16() != 0x4D42) return "Invalid Bitmap Type";
            uint bfSize = br.ReadUInt32();
            br.ReadUInt16();
            br.ReadUInt16();
            uint bfOffBits = br.ReadUInt32();
            uint biSize = br.ReadUInt32();
            CPoint szBMP = new CPoint(); 
            szBMP.X = br.ReadInt32();
            szBMP.Y = br.ReadInt32();
            br.ReadUInt16();
            int nByte = br.ReadUInt16() / 8;
            if (nByte != p_nByte) return "Invalid Pixel Depth"; 
            br.ReadUInt32();
            br.ReadUInt32();
            br.ReadInt32();
            br.ReadInt32();
            br.ReadUInt32();
            br.ReadUInt32();
            if (p_sz.X < szBMP.X) p_sz.X = szBMP.X;
            if (p_sz.Y < szBMP.Y) p_sz.Y = szBMP.Y;
            if (p_nByte == 1) br.ReadBytes(256 * 4); 
            for (int y = 0; y < szBMP.Y; y++)
            {
                byte[] pBuf = br.ReadBytes(p_nByte * szBMP.X);
                Marshal.Copy(pBuf, 0, GetPtr(nIndex, 0, szBMP.Y - y - 1), p_nByte * szBMP.X); 
            }
            br.Close();
            fs.Close();
            return "OK"; 
        }

        public string FileSaveBMP(string sFile, int nIndex)
        {
            if ((nIndex < 0) || (nIndex >= p_nCount)) return "Invalid Index";
            FileStream fs = new FileStream(sFile, FileMode.Create, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write((ushort)0x4d42);
            bw.Write((uint)(54 + 1024 + p_sz.X * p_sz.Y));
            bw.Write((ushort)0);
            bw.Write((ushort)0);
            bw.Write((uint)1078);
            bw.Write((uint)40);
            bw.Write(p_sz.X);
            bw.Write(p_sz.Y);
            bw.Write((ushort)1);
            bw.Write((ushort)(8 * p_nByte));
            bw.Write((uint)0);
            bw.Write((uint)(p_sz.X * p_sz.Y));
            bw.Write(0);
            bw.Write(0);
            bw.Write((uint)256);
            bw.Write((uint)256);
            for (int n = 0; n < 256; n++)
            {
                byte i = (byte)n; 
                bw.Write(i);
                bw.Write(i);
                bw.Write(i);
                bw.Write((byte)255);
            }
            byte[] aBuf = new byte[p_nByte * p_sz.X]; 
            for (int y = 0; y < p_sz.Y; y++)
            {
                Marshal.Copy(GetPtr(nIndex, 0, p_sz.Y - y - 1), aBuf, 0, p_nByte * p_sz.X);
                bw.Write(aBuf); 
            }
            bw.Close();
            fs.Close();
            return "OK";
        }
        #endregion

        #region JPG File
        public string FileOpenJPG(string sFile, int nIndex)
        {
            if ((nIndex < 0) || (nIndex >= p_nCount)) return "Invalid Index";
            Bitmap bitmap = new Bitmap(sFile);
            if (bitmap == null) return "File Open Error";
            int nByte = 0; 
            switch (bitmap.PixelFormat)
            {
                case PixelFormat.Format8bppIndexed: nByte = 1; break;
                case PixelFormat.Format24bppRgb: nByte = 3; break;
                case PixelFormat.Format32bppArgb: nByte = 4; break;
                default: return "Invalid Pixel Format : " + bitmap.PixelFormat.ToString(); 
            }
            if (nByte != p_nByte) return "Invalid Pixel Depth";
            CPoint szJPG = new CPoint(bitmap.Width, bitmap.Height);
            p_sz.X = Math.Min(p_sz.X, szJPG.X) / 4 * 4;
            p_sz.Y = Math.Min(p_sz.Y, szJPG.Y);
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            byte[] aBuf = new byte[p_nByte * szJPG.X * szJPG.Y];
            Marshal.Copy(bitmapData.Scan0, aBuf, 0, p_nByte * szJPG.X * szJPG.Y);
            int wJpg = p_nByte * szJPG.X;
            int wMemory = p_nByte * p_sz.X; 
            for (int y = 0; y < p_sz.Y; y++)
            {
                Marshal.Copy(aBuf, y * wJpg, GetPtr(nIndex, 0, p_sz.Y - y - 1), wMemory); 
            }
            bitmap.UnlockBits(bitmapData);
            return "OK";
        }

        public string FileSaveJPG(string sFile, int nIndex)
        {
            if ((nIndex < 0) || (nIndex >= p_nCount)) return "Invalid Index";
            PixelFormat pixelFormat;
            switch (p_nByte)
            {
                case 1: pixelFormat = PixelFormat.Format8bppIndexed; break;
                case 3: pixelFormat = PixelFormat.Format24bppRgb; break;
                case 4: pixelFormat = PixelFormat.Format32bppRgb; break;
                default: return null;
            }
            Bitmap bitmap = new Bitmap(p_sz.X, p_sz.Y, (int)W, pixelFormat, GetPtr(nIndex));
            if (p_nByte == 1)
            {
                ColorPalette palette = bitmap.Palette;
                for (int n = 0; n < 256; n++) palette.Entries[n] = Color.FromArgb(n, n, n);
                bitmap.Palette = palette;
            }
            bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            bitmap.Save(sFile, ImageFormat.Jpeg);
            return "OK";
        }
        #endregion

        #region Tree
        public void RunTree(Tree tree, bool bVisible, bool bReadonly)
        {
            p_nCount = tree.Set(p_nCount, p_nCount, "Count", "Memory Count", bVisible, bReadonly);
            p_nByte = tree.Set(p_nByte, p_nByte, "Byte", "Memory Depth Byte (byte)", bVisible, bReadonly);
            p_sz = tree.Set(p_sz, p_sz, "Size", "Memory Size", bVisible, bReadonly);
            tree.Set(p_sSize, p_sSize, "Allocate", "Memory Allocate Size (MB)", bVisible, true);
        }
        #endregion

        public MemoryGroup m_group;
        public List<MemoryDraw> m_aDraw = new List<MemoryDraw>(); 
        Log m_log;
        public MemoryData(MemoryGroup group, string id, int nCount, int nByte, int xSize, int ySize, ref int gbOffset)
        {
            m_group = group;
            m_log = group.m_log;
            p_id = id;
            p_nCount = nCount;
            p_nByte = nByte;
            p_sz = new CPoint(xSize, ySize);
            InitAddress(ref gbOffset);
        }
    }
}
