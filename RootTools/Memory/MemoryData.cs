using Microsoft.Win32;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

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
        public long H
        {
            get { return (long)p_sz.Y; }
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
                case 2:
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
                UpdateOpenProgress?.Invoke(Convert.ToInt32(((double)y / p_sz.Y) * 100));
            }
            br.Close();
            fs.Close();
            return "OK"; 
        }
        #endregion

        private void Worker_MemoryCopy_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if(UpdateOpenProgress != null)
            UpdateOpenProgress(100);
        }

        private void Worker_MemoryCopy_DoWork(object sender, DoWorkEventArgs e)
        {
            List<object> arguments = (List<object>)(e.Argument);

            string sFile = arguments[0].ToString();
            int nMemIndex = Convert.ToInt32(arguments[1]);
            switch (GetUpperExt(sFile))
            {
                case "BAYER":
                    FileOpenBayer(sFile, nMemIndex);
                    break;
                case "BMP":
                    FileOpenBMP(sFile, nMemIndex);
                    break;
                case "JPG":
                    FileOpenJPG(sFile, nMemIndex);
                    break;
            }
        }
        string GetUpperExt(string sFile)
        {
            string[] sFiles = sFile.Split('.');
            return sFiles[sFiles.Length - 1].ToUpper();
        }

        public void FileOpen(string sFile, int nMemoryIndex)
        {

            FileInfo fileInfo = new FileInfo(sFile);
            if (fileInfo.Exists)
            {
                List<object> arguments = new List<object>();
                arguments.Add(sFile);
                arguments.Add(nMemoryIndex);
                Worker_MemoryCopy.RunWorkerAsync(arguments);
            }
            else
            {
                ImageData data = new ImageData(123, 123);
                MessageBox.Show("OpenFile() - 파일이 존재 하지 않거나 열기에 실패하였습니다. - " + sFile);
            }
        }

        #region BMP File
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
        public string FileOpenBMP(string sFile, int nIndex)
        {
            if ((nIndex < 0) || (nIndex >= p_nCount)) return "Invalid Index";

            FileStream fs = null;
            try
            {
                fs = new FileStream(sFile, FileMode.Open, FileAccess.Read, FileShare.Read, 32768, true);
            }
            catch (Exception)
            {
                return "Can't create FileStream";
            }

            BinaryReader br = null;
            try
            {
                br = new BinaryReader(fs);
            }
            catch (Exception)
            {
                fs.Close();
                return "Can't create BinaryWriter";
            }

            try
            {
                // Bitmap File Header
                uint bfOffbits = 0;
                if (ReadBitmapFileHeader(br, ref bfOffbits) == false)
                    return "Occured error reading bitmap file header";

                // Bitmap Info Header
                int width = 0;
                int height = 0;
                int nByte = 0;
                if (ReadBitmapInfoHeader(br, ref width, ref height, ref nByte) == false)
                    return "Occured error reading bitmap info header";

                // 이미지 파일의 채널 개수가 이미 생성된 메모리의 채널 개수보다 많을 경우 Open 과정 중단
                if (nByte > p_nByte * p_nCount)
                {
                    return "Not enough memory count to load image file";
                }

                // 읽어올 이미지 내 영역
                CRect rect = new CRect(0, 0, width, height);
                if (width > p_sz.X) rect.Right -= width - p_sz.X;
                if (height > p_sz.Y) rect.Bottom -= height - p_sz.Y;


                // 픽셀 데이터 존재하는 부분으로 Seek
                fs.Seek(bfOffbits, SeekOrigin.Begin);

                // 파일로부터 픽셀 데이터 읽어오기
                byte[] aBuf = new byte[(long)rect.Width * nByte];
                int fileRowSize = (width * nByte + 3) & ~3; // 파일 내 하나의 열당 너비 사이즈 (4의 배수)

                if (nByte == 1 || nByte == 2)
                {
                    IntPtr ptr = GetPtr(nIndex);
                    if (ptr == IntPtr.Zero)
                        return "Wrong memory nIndex"; ;

                    // 읽지 않아도 되는 하단부분은 읽지 않고 스킵
                    fs.Seek((height - rect.Bottom) * fileRowSize, SeekOrigin.Current);

                    for (int j = rect.Bottom - 1; j >= rect.Top; j--)
                    {
                        Array.Clear(aBuf, 0, rect.Width * nByte);

                        // 이미지 좌측 영역 스킵
                        fs.Seek(rect.Left * nByte, SeekOrigin.Current);

                        // 파일에서 필요한 만큼 데이터를 읽어 메모리에 복사
                        fs.Read(aBuf, 0, rect.Width * nByte);
                        //IntPtr destPtr = ptr + ((j + offset.Y) * p_Size.X) * p_nByte;
                        IntPtr destPtr = new IntPtr(ptr.ToInt64() + ((long)j * p_sz.X) * p_nByte);

                        Marshal.Copy(aBuf, 0, destPtr, rect.Width * nByte);

                        // 이미지 우측 영역 스킵
                        fs.Seek(fileRowSize - rect.Right * nByte, SeekOrigin.Current);

                        // 진행상황 표시
                        UpdateOpenProgress?.Invoke(Convert.ToInt32(((double)rect.Height - (j - rect.Top)) / rect.Height * 100));
                    }
                }
                else/* if (nByte == 3)*/
                {
                    IntPtr ptrR = GetPtr(0);
                    IntPtr ptrG = GetPtr(1);
                    IntPtr ptrB = GetPtr(2);
                    if (ptrR == IntPtr.Zero || ptrB == IntPtr.Zero || ptrG == IntPtr.Zero)
                        return "Not enough memory count";

                    // 읽지 않아도 되는 하단부분은 읽지 않고 스킵
                    fs.Seek((height - rect.Bottom) * fileRowSize, SeekOrigin.Current);

                    for (int j = rect.Bottom - 1; j >= rect.Top; j--)
                    {
                        Array.Clear(aBuf, 0, rect.Width * nByte);

                        // 이미지 좌측 영역 스킵
                        fs.Seek(rect.Left * nByte, SeekOrigin.Current);

                        // 파일에서 필요한 만큼 데이터를 읽어 메모리에 복사
                        fs.Read(aBuf, 0, rect.Width * nByte);

                        Parallel.For(0, rect.Width, new ParallelOptions { MaxDegreeOfParallelism = 12 }, (i) =>
                        {
                            Int64 idx = ((Int64)j) * p_sz.X + i;

                            unsafe
                            {
                                ((byte*)(ptrB))[idx] = aBuf[i * 3];
                                ((byte*)(ptrG))[idx] = aBuf[i * 3 + 1];
                                ((byte*)(ptrR))[idx] = aBuf[i * 3 + 2];
                            }
                        });

                        // 이미지 우측 영역 스킵
                        fs.Seek(fileRowSize - rect.Right * nByte, SeekOrigin.Current);

                        // 진행상황 표시
                        UpdateOpenProgress?.Invoke(Convert.ToInt32(((double)rect.Height - (j - rect.Top)) / rect.Height * 100));
                    }
                }
            }
            catch (Exception ex)
            {
                return "Occured error opening BMP file";
            }
            finally
            {
                br.Close();
                fs.Close();
            }




            //FileStream fs = null;
            //try 
            //{ 
            //    fs = new FileStream(sFile, FileMode.Open, FileAccess.Read, FileShare.Read, 32768, true); 
            //}
            //catch (Exception) { return "File Open Error"; }
            //BinaryReader br = new BinaryReader(fs);
            //if (br.ReadUInt16() != 0x4D42) return "Invalid Bitmap Type";
            //uint bfSize = br.ReadUInt32();
            //br.ReadUInt16();
            //br.ReadUInt16();
            //uint bfOffBits = br.ReadUInt32();
            //uint biSize = br.ReadUInt32();
            //CPoint szBMP = new CPoint(); 
            //szBMP.X = br.ReadInt32();
            //szBMP.Y = br.ReadInt32();
            //br.ReadUInt16();
            //int nByte = br.ReadUInt16() / 8;
            //if (nByte != p_nByte) return "Invalid Pixel Depth"; 
            //br.ReadUInt32();
            //br.ReadUInt32();
            //br.ReadInt32();
            //br.ReadInt32();
            //br.ReadUInt32();
            //br.ReadUInt32();
            //if (p_sz.X < szBMP.X) p_sz.X = szBMP.X;
            //if (p_sz.Y < szBMP.Y) p_sz.Y = szBMP.Y;
            //if (p_nByte == 1) br.ReadBytes(256 * 4); 
            //for (int y = 0; y < szBMP.Y; y++)
            //{
            //    byte[] pBuf = br.ReadBytes(p_nByte * szBMP.X);
            //    Marshal.Copy(pBuf, 0, GetPtr(nIndex, 0, szBMP.Y - y - 1), p_nByte * szBMP.X);
            //    UpdateOpenProgress?.Invoke(Convert.ToInt32(((double)y / p_sz.Y) * 100));
            //}
            //br.Close();
            //fs.Close();
            return "OK";
        }
        private void Worker_MemorySave_DoWork(object sender, DoWorkEventArgs e)
        {
            List<object> arguments = (List<object>)(e.Argument);

            string sFile = arguments[0].ToString();
            int nMemIndex = Convert.ToInt32(arguments[1]);
            int nByte = Convert.ToInt32(arguments[2]);

            switch (GetUpperExt(sFile))
            {
                case "BMP": p_sInfo = FileSaveBMP(sFile, nMemIndex, nByte); break;
                case "JPG": p_sInfo = FileSaveJPG(sFile, nMemIndex); break;
            }
        }
        public void FileSave(string sFile, int nMemoryIndex, int nByte)
        {
            List<object> arguments = new List<object>();
            arguments.Add(sFile);
            arguments.Add(nMemoryIndex);
            arguments.Add(nByte);

            BackgroundWorker Worker_MemorySave = new BackgroundWorker();
            Worker_MemorySave.DoWork += new DoWorkEventHandler(Worker_MemorySave_DoWork);
            Worker_MemorySave.RunWorkerAsync(arguments);
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
            bw.Write(Convert.ToUInt32((uint)size));                 // bfSize
            bw.Write(Convert.ToUInt16(0));                          // bfReserved1
            bw.Write(Convert.ToUInt16(0));                          // bfReserved2
            bw.Write(Convert.ToUInt32(offbit));                     // bfOffbits

            return true;
        }
        bool WriteBitmapInfoHeader(BinaryWriter bw, int width, int height, bool isGrayScale, int nByte)
        {
            if (bw == null)
                return false;

            int biBitCount;
            if(isGrayScale)
            {
                biBitCount = nByte * 8;
            }
            else
            {
                biBitCount = p_nByte * p_nCount * 8;
            }
            //int biBitCount = (isGrayScale ? nByte : p_nByte) * p_nCount * 8;

            bw.Write(Convert.ToUInt32(40));                         // biSize
            bw.Write(Convert.ToInt32(width));                       // biWidth
            bw.Write(Convert.ToInt32(height));                      // biHeight
            bw.Write(Convert.ToUInt16(1));                          // biPlanes
            bw.Write(Convert.ToUInt16(biBitCount));                  // biBitCount
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
        public string FileSaveBMP(string sFile, int nIndex, int nByte = 1)
        {
            if ((nIndex < 0) || (nIndex >= p_nCount)) return "Invalid Index";

            FileStream fs = null;
            try
            {
                fs = new FileStream(sFile, FileMode.Create, FileAccess.Write);
            }
            catch (Exception)
            {
                return "Can't create FileStream";
            }

            BinaryWriter bw = null;
            try
            {
                bw = new BinaryWriter(fs);
            }
            catch (Exception)
            {
                fs.Close();
                return "Can't create BinaryWriter";
            }

            try
            {
                // Bitmap File Header
                if (WriteBitmapFileHeader(bw, nByte, p_sz.X, p_sz.Y) == false)
                    return "Occured error writing bitmap file header";

                // Bitmap Info Header
                if (WriteBitmapInfoHeader(bw, p_sz.X, p_sz.Y, true, nByte) == false)
                    return "Occured error writing bitmap info header";

                // Palette (if it's 1byte gray image)
                if (nByte == 1)
                    WritePalette(bw);

                // Pixel data
                int rowSize = (p_sz.X * nByte + 3) & ~3;
                byte[] aBuf = new byte[(long)rowSize];
                IntPtr ptr = GetPtr(nIndex);

                if (ptr != IntPtr.Zero)
                {
                    if (p_nByte == nByte)
                    {
                        for (int j = p_sz.Y - 1; j >= 0; j--)
                        {
                            Array.Clear(aBuf, 0, rowSize);

                            long idx = (long)j * p_sz.X * nByte;
                            IntPtr srcPtr = new IntPtr(ptr.ToInt64() + idx);
                            Marshal.Copy(srcPtr, aBuf, 0, rowSize);

                            bw.Write(aBuf);

                            UpdateOpenProgress?.Invoke(Convert.ToInt32(((double)(p_sz.Y - j) / p_sz.Y) * 100));
                        }
                    }
                    else
                    {
                        for (int j = p_sz.Y - 1; j >= 0; j--)
                        {
                            Array.Clear(aBuf, 0, rowSize);

                            long idx = (long)j * p_sz.X * p_nByte;

                            Parallel.For(0, p_sz.X, new ParallelOptions { MaxDegreeOfParallelism = 12 }, (i) =>
                            {
                                unsafe
                                {
                                    byte* arrByte = (byte*)ptr.ToPointer();

                                    if (nByte == 1)         // 2byte -> 1byte
                                    {
                                        aBuf[i] = arrByte[idx + i * p_nByte + 0];
                                    }
                                    else if (nByte == 2)    // 1byte -> 2byte
                                    {
                                        int val = arrByte[idx + i * p_nByte];
                                        val = (int)((val / 255.0) * (Math.Pow(2, 8 * nByte) - 1));
                                        aBuf[i * nByte + 0] = (byte)((val & 0xFF00) >> 8);
                                        aBuf[i * nByte + 1] = (byte)((val & 0x00FF));
                                    }
                                }
                            });

                            bw.Write(aBuf);

                            UpdateOpenProgress?.Invoke(Convert.ToInt32(((double)(p_sz.Y - j) / p_sz.Y) * 100));
                        }
                    }
                }
                else
                    return "Cannot read addresss in MemoryData";
            }
            catch (Exception ex)
            {
                return "Occured error writing BMP file";
            }
            finally
            {
                bw.Close();
                fs.Close();
            }

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
            byte[] aBuf = new byte[(long)p_nByte * szJPG.X * szJPG.Y];
            Marshal.Copy(bitmapData.Scan0, aBuf, 0, p_nByte * szJPG.X * szJPG.Y);
            int wJpg = p_nByte * szJPG.X;
            int wMemory = p_nByte * p_sz.X; 
            for (int y = 0; y < p_sz.Y; y++)
            {
                Marshal.Copy(aBuf, y * wJpg, GetPtr(nIndex, 0, p_sz.Y - y - 1), wMemory);
                UpdateOpenProgress?.Invoke(Convert.ToInt32(((double)y / p_sz.Y) * 100));
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

        public delegate void DelegateOneInt(int nInt);
        public event DelegateOneInt UpdateOpenProgress;
        BackgroundWorker Worker_MemoryCopy = new BackgroundWorker();

        public MemoryData(MemoryGroup group, string id, int nCount, int nByte, int xSize, int ySize, ref int gbOffset)
        {
            m_group = group;
            m_log = group.m_log;
            p_id = id;
            p_nCount = nCount;
            p_nByte = nByte;
            p_sz = new CPoint(xSize, ySize);
            InitAddress(ref gbOffset);
            Worker_MemoryCopy.DoWork += Worker_MemoryCopy_DoWork;
            Worker_MemoryCopy.RunWorkerCompleted += Worker_MemoryCopy_RunWorkerCompleted;
            Worker_MemoryCopy.WorkerSupportsCancellation = true;
        }
    }
}
