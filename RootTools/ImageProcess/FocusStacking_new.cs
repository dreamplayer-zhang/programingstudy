using Emgu.CV;
using Emgu.CV.CvEnum;
using RootTools.Memory;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RootTools.ImageProcess
{
    class DataInfo_new
    {
        public DataInfo_new()
        { }

        public DataInfo_new(MemoryData mem)
        {
            memData = mem;
        }

        public MemoryData memData;
        public string sDirPath = @"C:\Recipe\VEGA_P\";
        public bool bTest = false;
        public int nTest_afterAvg = 1;
        public int nTest_beforeAvg = 1;
        public double dGammaValue = 0;

        //마스크 사이즈.
        //한 점을 만드는데 그 점을 기준으로 MaskSize by MaskSize 만큼의 영역을 연산.
        private int m_nMaskSize = 30;
        public int p_nMaskSize
        {
            get
            {
                return m_nMaskSize;
            }
            set
            {
                m_nMaskSize = value;
            }
        }

        //연산량 감소를 위해 1/p_nScale 만큼의 이미지로 (즉 p_nScale=2인경우 원본의 절반의 크기인 이미지 사용)
        //포커스스태킹 알고리즘 실행
        //최적화 이후 스케일은 1사용.
        private int m_nScale = 1;
        public int p_nScale
        {
            get
            {
                return m_nScale;
            }
            set
            {
                m_nScale = value;
            }
        }
        //폴더 선택하면 첫 이미지의 크기로 set
        private int m_nSrcHeight = 1080;
        public int p_nSrcHeight
        {
            get
            {
                return m_nSrcHeight;
            }
            set
            {
                m_nSrcHeight = value;
            }
        }
        //폴더 선택하면 첫 이미지의 크기로 set
        private int m_nSrcWidth = 2048;
        public int p_nSrcWidth
        {
            get
            {
                return m_nSrcWidth;
            }
            set
            {
                m_nSrcWidth = value;
            }
        }
        private int m_nHeight;
        public int p_nHeight
        {
            get
            {
                return m_nHeight;
            }
            set
            {
                m_nHeight = value;
            }
        }
        private int m_nWidth;
        public int p_nWidth
        {
            get
            {
                return m_nWidth;
            }
            set
            {
                m_nWidth = value;
            }
        }
        //하나의 정적인 피사체에 대한 포커스 다른 이미지들. 폴더선택하면 자동으로 set
        private int m_nFocusCount = 33;
        public int p_nFocusCount
        {
            get
            {
                return m_nFocusCount;
            }
            set
            {
                m_nFocusCount = value;
            }
        }
        public Stopwatch sw;
    }
    public class FocusStacking_new
    {
        DataInfo_new DI;

        public FocusStacking_new(MemoryData mem)
        {
            DI = new DataInfo_new(mem);
        }

        unsafe public void Run(int nWidth,int nHeight,int cntX, int cntY)
        {
            DI.sw = new Stopwatch();
            Stopwatch total = new Stopwatch();

            Mat[] ListOfSrc;

            DI.sw.Start();
            total.Start();

            DI.p_nFocusCount = DI.memData.p_nCount-1;
            DI.p_nHeight = nHeight;
            DI.p_nWidth = nWidth;
            ListOfSrc = new Mat[DI.p_nFocusCount];

            CreateSrcData(ref ListOfSrc,nHeight,nWidth,cntX*nWidth,cntY*nHeight);

            //ListofMat
            Mat[] ListOfLaplacianData = new Mat[DI.p_nFocusCount];
            Mat[] ListOfGradientData = new Mat[DI.p_nFocusCount];
            Mat[] blur = new Mat[DI.p_nFocusCount];

            //이미지 전처리. 원본 -> 가우시안 -> (라플라스+그라디언트) -> 가우시안.
            Mat lut = new Mat(1, 256, DepthType.Cv8U, 1); ;//= new InputArray[256];
            unsafe
            {
                byte* lutPtr = (byte*)lut.DataPointer;
                for (int i = 0; i < 256; i++)
                {
                    lutPtr[i] = (byte)(Math.Pow(i / 255.0, 1.5) * 255.0);
                }
            }

            Parallel.For(0, DI.p_nFocusCount, (i) =>
            {
                Mat Source = new Mat(DI.p_nSrcWidth, DI.p_nSrcWidth, DepthType.Cv8U, 1);
                Mat Weighted = new Mat(DI.p_nSrcWidth, DI.p_nSrcWidth, DepthType.Cv8U, 1);
                ListOfSrc[i].CopyTo(Source);
                CvInvoke.GaussianBlur(Source, Weighted, new System.Drawing.Size(0, 0), 1, 1, BorderType.Default);

                CvInvoke.AddWeighted(Source, 1.5, Weighted, -0.5, 0, Source);

                CvInvoke.GaussianBlur(Source, Weighted, new System.Drawing.Size(0, 0), 1, 1, BorderType.Default);

                CvInvoke.AddWeighted(Source, 1.5, Weighted, -0.5, 0, Source);

                blur[i] = new Mat();
                CvInvoke.GaussianBlur(Source, blur[i], new System.Drawing.Size(0, 0), 1, 1, BorderType.Default);

                ListOfLaplacianData[i] = new Mat();
                CvInvoke.Laplacian(blur[i], ListOfLaplacianData[i], DepthType.Cv32F, 3, 1, 0, BorderType.Default);
                ListOfLaplacianData[i].ConvertTo(ListOfLaplacianData[i], DepthType.Cv8U, 1);

                CvInvoke.GaussianBlur(ListOfLaplacianData[i], ListOfLaplacianData[i], new System.Drawing.Size(0, 0), 1, 1, BorderType.Default);

                CvInvoke.Resize(ListOfLaplacianData[i], ListOfLaplacianData[i], new System.Drawing.Size(0, 0), (double)1 / DI.p_nScale, (double)1 / DI.p_nScale);
            });

            Console.WriteLine("전처리과정 소요시간 : " + DI.sw.ElapsedMilliseconds + "ms");

            Mat res;
            Mat[] ListOfPartImg;
           
            //이미지 연산.
            res = ComparePixelData(ListOfSrc, ListOfLaplacianData, out ListOfPartImg);
            CvInvoke.Imwrite(DI.sDirPath + "Res.bmp",res);
            Parallel.For(0, res.Height, (j) =>
            {
                int memWidth = (int)DI.memData.W;
                IntPtr n_ptr = DI.memData.GetPtr(DI.memData.p_nCount - 1) + (j+cntY*nHeight)*memWidth+nWidth*cntX;
                IntPtr ptr = res.DataPointer + j*res.Width;

                Buffer.MemoryCopy((void*)ptr, (void*)n_ptr, res.Width, res.Width);
            });

            //save
            //res.Save(DI.sDirPath + "\\" + "_Result_" + ".bmp");
            //Console.WriteLine("총 소요시간 : " + total.ElapsedMilliseconds + "ms");
        }

        //실질적 연산 함수.
        // 

        unsafe public void CreateSrcData(ref Mat[] SrcMat, int nHeight,int nWidth,int startoffsetX, int startoffsetY)
        {
            MemoryData mem = DI.memData;
            int memcnt = DI.memData.p_nCount-1;
            int height = DI.p_nSrcHeight = nHeight;
            int width = DI.p_nSrcWidth = nWidth;
            for (int i = 0; i < memcnt; i++)
            {
                byte* n_ptr = (byte*)mem.GetPtr(i);
                SrcMat[i] = new Mat(height, width, DepthType.Cv8U, 1);
                byte* ptr = (byte*)SrcMat[i].DataPointer.ToPointer();
                Parallel.For(0, height, (j) =>
                {
                    Buffer.MemoryCopy(n_ptr + (j+startoffsetY) * mem.W+startoffsetX, ptr + j * width, width, width);
                });
            }
        }
        unsafe public Mat ComparePixelData(Mat[] _ListOfSrc, Mat[] _ListOfgData, out Mat[] ListOfPartImg)
        {
            DI.sw.Restart();

            Mat[] WeightMap = new Mat[DI.p_nFocusCount];
            byte*[] WeightMapPtr = new byte*[DI.p_nFocusCount];

            ListOfPartImg = new Mat[DI.p_nFocusCount];
            byte*[] ListPartPtr = new byte*[DI.p_nFocusCount];
            for (int n = 0; n < DI.p_nFocusCount; n++)
            {
                WeightMap[n] = new Mat(new System.Drawing.Size(DI.p_nSrcWidth, DI.p_nSrcHeight), DepthType.Cv8U, 1);
                WeightMapPtr[n] = (byte*)WeightMap[n].DataPointer;

                ListOfPartImg[n] = new Mat(new System.Drawing.Size(DI.p_nSrcWidth, DI.p_nSrcHeight), DepthType.Cv8U, 1);
                ListPartPtr[n] = (byte*)ListOfPartImg[n].DataPointer;
            }


            //사전에 평균을 뺀 데이터 _ListOfgData -> 평균빼기 -> FinalDataMap
            //또한 ListOfgData보다 FinalDataMap이 더 큼. 왜냐하면 CreateWeightData에서 사용될것을 고려해 Padding만큼 이미지 사이즈를 늘려놨기 때문.
            int[][] FinalDataMap;
            if (DI.nTest_beforeAvg >= 32)
                MakeFinalDataMap_Max(out FinalDataMap, ref _ListOfgData);
            else
                MakeFinalDataMap_LoopAvg(out FinalDataMap, ref _ListOfgData);

            //최종스코어 계산.
            long[][] WeightRes;
            CreateWeightData(out WeightRes, ref FinalDataMap);

            for (int _loop = 0; _loop < DI.nTest_afterAvg; _loop++)
            {
                //최종스코어에 평균 빼기
                SubtractAvg(ref WeightRes);
            }

            //최종스코어로부터 비율스코어를 산정해 결과이미지 도출.
            Mat Res = MakeRatioRes(ref WeightRes, ref _ListOfSrc);


            return Res;
        }
        //GradientData를 생성. 추후 라플라스이미지와 +연산에 사용.
        void SubtractAvg(ref long[][] WeightRes)
        {
            int nWidth = DI.p_nWidth;
            int nHeight = DI.p_nHeight;
            int nFocusCount = DI.p_nFocusCount;
            //전체의 평균
            long[] nSumScoreMap = new long[nHeight * nWidth];
            for (int y = 0; y < nHeight; y++)
            {
                for (int x = 0; x < nWidth; x++)
                {
                    nSumScoreMap[y * nWidth + x] = 0;
                    for (int n = 0; n < nFocusCount; n++)
                    {
                        nSumScoreMap[y * nWidth + x] += WeightRes[n][y * nWidth + x];
                    }
                }
            }

            long lTempValue;
            for (int y = 0; y < nHeight; y++)
            {
                for (int x = 0; x < nWidth; x++)
                {
                    for (int n = 0; n < nFocusCount; n++)
                    {
                        lTempValue = nFocusCount * WeightRes[n][y * nWidth + x] - nSumScoreMap[y * nWidth + x];
                        if (lTempValue > 0)
                            WeightRes[n][y * nWidth + x] = lTempValue / nFocusCount;
                        else
                            WeightRes[n][y * nWidth + x] = 0;
                    }
                }
            }
        }
        unsafe Mat MakeRatioRes(ref long[][] WeightRes, ref Mat[] _ListOfSrc)
        {
            Mat[] ratioMap = new Mat[DI.p_nFocusCount];
            byte*[] ratioMapPtr = new byte*[DI.p_nFocusCount];
            byte*[] ListSrcPtr = new byte*[DI.p_nFocusCount];
            for (int n = 0; n < DI.p_nFocusCount; n++)
            {
                ratioMap[n] = new Mat(new System.Drawing.Size(DI.p_nSrcWidth, DI.p_nSrcHeight), DepthType.Cv8U, 1);
                ratioMapPtr[n] = (byte*)ratioMap[n].DataPointer;
                ListSrcPtr[n] = (byte*)_ListOfSrc[n].DataPointer;
            }
            Mat Res = new Mat(new System.Drawing.Size(DI.p_nSrcWidth, DI.p_nSrcHeight), DepthType.Cv8U, 1);
            byte* ResPtr = (byte*)Res.DataPointer;



            long[] totalWeight = new long[DI.p_nWidth * DI.p_nHeight];
            for (int i = 0; i < DI.p_nWidth * DI.p_nHeight; i++)
            {
                totalWeight[i] = 0;
                for (int n = 0; n < DI.p_nFocusCount; n++)
                {
                    totalWeight[i] += WeightRes[n][i];
                }
            }

            long totalValue = 0;
            for (int y = 0; y < DI.p_nSrcHeight; y++)
                for (int x = 0; x < DI.p_nSrcWidth; x++)
                {
                    totalValue = 0;
                    for (int n = 0; n < DI.p_nFocusCount; n++)
                    {
                        totalValue += WeightRes[n][(y / DI.p_nScale) * DI.p_nWidth + (x / DI.p_nScale)] * ListSrcPtr[n][y * DI.p_nSrcWidth + x];
                    }
                    if (totalWeight[(y / DI.p_nScale) * DI.p_nWidth + (x / DI.p_nScale)] == 0)
                    {
                        totalValue = 0;
                        for (int n = 0; n < DI.p_nFocusCount; n++)
                            totalValue += ListSrcPtr[n][y * DI.p_nSrcWidth + x];
                        ResPtr[y * DI.p_nSrcWidth + x] = (byte)(totalValue / DI.p_nFocusCount);
                    }
                    else
                    {
                        ResPtr[y * DI.p_nSrcWidth + x] = (byte)(totalValue / totalWeight[(y / DI.p_nScale) * DI.p_nWidth + (x / DI.p_nScale)]);
                        //이부분 debug시 작동, release시 포인트인덱스 오류
                        for (int n = 0; n < DI.p_nFocusCount; n++)
                            ratioMapPtr[n][y * DI.p_nSrcWidth + x] = (byte)(255 * WeightRes[n][(y / DI.p_nScale) * DI.p_nWidth + (x / DI.p_nScale)] / totalWeight[(y / DI.p_nScale) * DI.p_nWidth + (x / DI.p_nScale)]);
                    }
                }
            for (int n = 0; n < DI.p_nFocusCount; n++)
            {
                //ratioMap[n].Save(DI.sDirPath + "\\" + "ratio" +n+ ".tif");
            }

            return Res;
        }
        unsafe void MakeFinalDataMap_Max(out int[][] FinalDataMap, ref Mat[] ListData)
        {
            int nHeight = DI.p_nHeight;
            int nWidth = DI.p_nWidth;
            int nFocusCount = DI.p_nFocusCount;

            byte*[] ListDataPtr = new byte*[nFocusCount];
            for (int n = 0; n < nFocusCount; n++)
            {
                ListData[n] = Padding(ref ListData[n], DI.p_nMaskSize);
                ListDataPtr[n] = (byte*)ListData[n].DataPointer;
            }
            int nPaddingImgHeight = nHeight + 2 * DI.p_nMaskSize;
            int nPaddingImgWidth = nWidth + 2 * DI.p_nMaskSize;

            FinalDataMap = new int[nFocusCount][];
            int _Value = 0;
            int _index = 0;
            for (int i = 0; i < nFocusCount; i++)
            {
                FinalDataMap[i] = new int[nPaddingImgHeight * nPaddingImgWidth];
            }
            for (int y = 0; y < nPaddingImgHeight; y++)
            {
                for (int x = 0; x < nPaddingImgWidth; x++)
                {
                    _Value = 0;
                    _index = 0;
                    for (int n = 0; n < nFocusCount; n++)
                    {
                        if (_Value < ListDataPtr[n][y * nPaddingImgWidth + x])
                        {
                            _Value = ListDataPtr[n][y * nPaddingImgWidth + x];
                            _index = n;
                        }
                    }
                    FinalDataMap[_index][y * nPaddingImgWidth + x] = _Value;
                }
            }

        }
        unsafe void MakeFinalDataMap_LoopAvg(out int[][] FinalDataMap, ref Mat[] ListData)
        {
            int nHeight = DI.p_nHeight;
            int nWidth = DI.p_nWidth;
            int nFocusCount = DI.p_nFocusCount;

            byte*[] ListDataPtr = new byte*[nFocusCount];
            for (int n = 0; n < nFocusCount; n++)
            {
                ListData[n] = Padding(ref ListData[n], DI.p_nMaskSize);
                ListDataPtr[n] = (byte*)ListData[n].DataPointer;
            }
            int nPaddingImgHeight = nHeight + 2 * DI.p_nMaskSize;
            int nPaddingImgWidth = nWidth + 2 * DI.p_nMaskSize;

            FinalDataMap = new int[nFocusCount][];
            for (int i = 0; i < nFocusCount; i++)
            {
                FinalDataMap[i] = new int[nPaddingImgHeight * nPaddingImgWidth];
            }
            int[] nSumMap = new int[nPaddingImgHeight * nPaddingImgWidth];
            for (int y = 0; y < nPaddingImgHeight; y++)
            {
                for (int x = 0; x < nPaddingImgWidth; x++)
                {
                    for (int n = 0; n < nFocusCount; n++)
                    {
                        FinalDataMap[n][y * nPaddingImgWidth + x] = ListDataPtr[n][y * nPaddingImgWidth + x];
                    }
                    for (int _loop = 0; _loop < DI.nTest_beforeAvg; _loop++)
                    {
                        nSumMap[y * nPaddingImgWidth + x] = 0;
                        for (int n = 0; n < nFocusCount; n++)
                        {
                            nSumMap[y * nPaddingImgWidth + x] += FinalDataMap[n][y * nPaddingImgWidth + x];
                        }
                        for (int n = 0; n < nFocusCount; n++)
                        {
                            FinalDataMap[n][y * nPaddingImgWidth + x] = nFocusCount * FinalDataMap[n][y * nPaddingImgWidth + x] - nSumMap[y * nPaddingImgWidth + x];
                            if (FinalDataMap[n][y * nPaddingImgWidth + x] < 0)
                            {
                                FinalDataMap[n][y * nPaddingImgWidth + x] = 0;
                            }
                            else
                            {
                                FinalDataMap[n][y * nPaddingImgWidth + x] /= nFocusCount;
                            }
                        }
                    }
                }
            }
        }
        //ref byte*[] ListDataPtr 는 Edge Score 이미지의 리스트.
        //
        //return int[][] 각각의 Weight ScoreMap
        unsafe void CreateWeightData(out long[][] res, ref int[][] FinalDataMap)
        {
            int nFocusCount = DI.p_nFocusCount;


            //for부분은 멀티스레드로 구현이 가능함.
            res = new long[nFocusCount][];
            for (int i = 0; i < nFocusCount; i++)
            {
                CompactAlgorithm(out res[i], ref FinalDataMap[i]);
            }
        }
        //연산최적화알고리즘 적용. ppt 참고.
        unsafe void CompactAlgorithm(out long[] res, ref int[] FinalDataMap)
        {
            int nHeight = DI.p_nHeight;
            int nWidth = DI.p_nWidth;
            int nMaskSize = DI.p_nMaskSize;
            int nPaddingImgWidth = nWidth + 2 * nMaskSize;

            res = new long[nHeight * nWidth];

            int[] nLengthTable;
            MakeLengthTable_half(out nLengthTable, nMaskSize);
            int nMultiScale = 1;
            //for (int nCurrentMaskSize = nMaskSize; nCurrentMaskSize > 0; nCurrentMaskSize--)
            int nCurrentMaskSize = nMaskSize;
            {

                int nCurrentPaddingImgWidth = nWidth + 2 * nCurrentMaskSize;

                int[] GSD = new int[nCurrentPaddingImgWidth];
                int[][] GD = new int[2][];
                GD[0] = new int[nCurrentPaddingImgWidth];
                GD[1] = new int[nCurrentPaddingImgWidth];

                int[] GSD_clone = new int[nCurrentPaddingImgWidth];
                int[] GD_clone = new int[nCurrentPaddingImgWidth];


                // 최초GSD ,GD 만들기
                for (int x = 0; x < nCurrentPaddingImgWidth; x++)
                {
                    for (int i = -nCurrentMaskSize; i <= 0; i++)
                    {
                        GSD[x] += FinalDataMap[(i + nMaskSize) * nPaddingImgWidth + x] * (nCurrentMaskSize - Math.Abs(i) + 1);
                        GD[0][x] += FinalDataMap[(i + nMaskSize) * nPaddingImgWidth + x];
                    }
                    for (int i = 1; i < nCurrentMaskSize + 1; i++)
                    {
                        GSD[x] += FinalDataMap[(i + nMaskSize) * nPaddingImgWidth + x] * (nCurrentMaskSize - Math.Abs(i) + 1);
                        GD[1][x] += FinalDataMap[(i + nMaskSize) * nPaddingImgWidth + x];
                    }
                }

                //GSD, GD정보를 넘겨주기
                for (int x = 0; x < nCurrentMaskSize; x++)
                {
                    GSD_clone[x] = GSD[x];
                    GD_clone[x] = GD[0][x] + GD[1][x];
                }

                //long TestResValue = 0;
                long ResValue = 0;
                long PreSum = 0;
                long PostSum = 0;
                //최초 Point정보 만들기.
                for (int i = -nCurrentMaskSize; i < nCurrentMaskSize + 1; i++)
                {
                    ResValue += GSD_clone[i + nCurrentMaskSize];
                    ResValue += GD_clone[i + nCurrentMaskSize] * (nCurrentMaskSize - Math.Abs(i));
                }
                for (int i = -nCurrentMaskSize + 1; i < 1; i++)
                {
                    PreSum += GD_clone[i + nCurrentMaskSize];
                }
                for (int i = 1; i < nCurrentMaskSize + 1; i++)
                {
                    PostSum += GD_clone[i + nCurrentMaskSize];
                }

                res[0] += nMultiScale * ResValue;

                //넘겨받은 정보로 res 한줄 만들기
                for (int x = 1; x < nWidth; x++)
                {
                    ResValue -= GSD_clone[(x + nCurrentMaskSize) - nCurrentMaskSize - 1];
                    ResValue -= PreSum;
                    ResValue += GSD_clone[(x + nCurrentMaskSize) + nCurrentMaskSize];
                    ResValue += PostSum;

                    PreSum -= GD_clone[(x + nCurrentMaskSize) - nCurrentMaskSize];
                    PreSum += GD_clone[(x + nCurrentMaskSize)];
                    PostSum -= GD_clone[(x + nCurrentMaskSize)];
                    PostSum += GD_clone[(x + nCurrentMaskSize) + nCurrentMaskSize];

                    res[x] += nMultiScale * ResValue;
                }


                for (int y = 1; y < nHeight; y++)
                {
                    //다음줄 GSD, GD 만들기.
                    for (int x = 0; x < nCurrentPaddingImgWidth; x++)
                    {
                        GSD[x] -= GD[0][x];
                        GSD[x] += GD[1][x];
                        GSD[x] += FinalDataMap[((y + nMaskSize) + nCurrentMaskSize) * nPaddingImgWidth + x];

                        GD[0][x] -= FinalDataMap[(y + nMaskSize - nCurrentMaskSize - 1) * nPaddingImgWidth + x];
                        GD[0][x] += FinalDataMap[(y + nMaskSize) * nPaddingImgWidth + x];
                        GD[1][x] -= FinalDataMap[(y + nMaskSize) * nPaddingImgWidth + x];
                        GD[1][x] += FinalDataMap[(y + nMaskSize + nCurrentMaskSize) * nPaddingImgWidth + x];
                    }



                    //정보 넘겨주기
                    for (int x = 0; x < nCurrentPaddingImgWidth; x++)
                    {
                        GSD_clone[x] = GSD[x];
                        GD_clone[x] = GD[0][x] + GD[1][x];
                    }

                    ResValue = 0;
                    PreSum = 0;
                    PostSum = 0;
                    //최초 Point정보 만들기.
                    for (int i = -nCurrentMaskSize; i < nCurrentMaskSize + 1; i++)
                    {
                        ResValue += GSD_clone[i + nCurrentMaskSize];
                        ResValue += GD_clone[i + nCurrentMaskSize] * (nCurrentMaskSize - Math.Abs(i));
                    }
                    for (int i = -nCurrentMaskSize + 1; i < 1; i++)
                    {
                        PreSum += GD_clone[i + nCurrentMaskSize];
                    }
                    for (int i = 1; i < nCurrentMaskSize + 1; i++)
                    {
                        PostSum += GD_clone[i + nCurrentMaskSize];
                    }
                    res[y * nWidth] += nMultiScale * ResValue;


                    //넘겨받은 정보로 res 한줄 만들기
                    for (int x = 1; x < nWidth; x++)
                    {
                        ResValue -= GSD_clone[(x + nCurrentMaskSize) - nCurrentMaskSize - 1];
                        ResValue -= PreSum;
                        ResValue += GSD_clone[(x + nCurrentMaskSize) + nCurrentMaskSize];
                        ResValue += PostSum;

                        PreSum -= GD_clone[(x + nCurrentMaskSize) - nCurrentMaskSize];
                        PreSum += GD_clone[(x + nCurrentMaskSize)];
                        PostSum -= GD_clone[(x + nCurrentMaskSize)];
                        PostSum += GD_clone[(x + nCurrentMaskSize) + nCurrentMaskSize];

                        res[y * nWidth + x] += nMultiScale * ResValue;

                    }
                }
            }
        }
        void MakeLengthTable_half(out int[] LengthTable, int nMaskSize)
        {
            int LengthScale = 100;

            while (true)
            {
                if ((int)(LengthScale / (nMaskSize * nMaskSize)) == 0)
                    LengthScale *= 10;
                else
                    break;
            }

            LengthTable = new int[nMaskSize + 1];
            for (int _i = 0; _i < nMaskSize + 1; _i++)
            {
                if (_i == 0)
                    LengthTable[_i] = 0;
                else
                    LengthTable[_i] = (int)(LengthScale / (_i * _i));
            }
        }
        unsafe Mat Padding(ref Mat _input, int MaskSize)
        {
            int _h = _input.Height;
            int _w = _input.Width;

            Mat Output = new Mat(_input.Height + 2 * MaskSize, _input.Width + 2 * MaskSize, DepthType.Cv8U, 1);
            byte* OutputPtr = (byte*)Output.DataPointer;
            byte* InputPtr = (byte*)_input.DataPointer;

            for (int y = 0; y < _h; y++)
                for (int x = 0; x < _w; x++)
                {
                    OutputPtr[(y + MaskSize) * (_w + MaskSize * 2) + (x + MaskSize)] = InputPtr[y * _w + x];
                }

            //Output.Save(@"C:\Users\ATI\Desktop\FocusStackingTest\test\" + "SizeTest.tif");
            return Output;
        }
    }
}
