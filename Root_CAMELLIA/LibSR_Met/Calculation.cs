using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Windows.Forms;
using System.Diagnostics;
using MathNet.Numerics.LinearAlgebra;

namespace Root_CAMELLIA.LibSR_Met
{
    public class Calculation
    {
        // 2020.12.23 Met.DS
        Complex C01 = new Complex(0, 1);
        //DataManager m_DM;

        public Calculation()
        {
            // m_DM = new DataManager();
        }

        DataManager m_DM = DataManager.GetInstance();


        //계산된 Reflectance구하기
        public double CalcReflectance(int nPointIndex, int[] arrNKDataIdx, double dWavelength, double[] Thickness) //계산된 R
        {
            try
            {
                if (m_DM.m_LayerData.Count == 0)
                {
                    MessageBox.Show("CalcReflectance() - Please open a recipe first!");
                    return -1;
                }
                //int nDataNum = Wavelength.Count(i => dLowerWavelength < i && i < dUpperWavelength);

                Complex[][] mM = MatrixCreate(2, 2);
                mM[0][0] = 1;
                mM[0][1] = 0;
                mM[1][0] = 0;
                mM[1][1] = 1;

                Complex cN;
                Complex cD;
                Complex cDelta;
                Complex[][] mMj = MatrixCreate(2, 2);

                for (int n = 0; n < m_DM.m_LayerData.Count - 1; n++)
                {
                    cN = new Complex(m_DM.m_LayerData[n].n[arrNKDataIdx[n]], -m_DM.m_LayerData[n].k[arrNKDataIdx[n]]);
                    cD = new Complex(Thickness[n], 0); //초기 두께
                    cDelta = (2 * Math.PI * cN * cD) / (dWavelength * 10.0); //d 두께/ 단위땜에 10곱해줌 

                    //Transfer matrix method
                    mMj[0][0] = Complex.Cos(cDelta);
                    mMj[0][1] = C01 * Complex.Sin(cDelta) / cN;
                    mMj[1][0] = C01 * Complex.Sin(cDelta) * cN;
                    mMj[1][1] = Complex.Cos(cDelta);

                    mM = MatrixProduct(mM, mMj);
                }
                //다층 박막에서의 admittance
                Complex cY = 0.0, cB = 0.0, cC = 0.0;

                Complex[][] mN = MatrixCreate(2, 1);
                mN[0][0] = 1;
                mN[1][0] = new Complex(m_DM.m_LayerData[m_DM.m_LayerData.Count - 1].n[arrNKDataIdx[m_DM.m_LayerData.Count - 1]], -m_DM.m_LayerData[m_DM.m_LayerData.Count - 1].k[arrNKDataIdx[m_DM.m_LayerData.Count - 1]]);
                Complex[][] A = MatrixProduct(mM, mN);
                cY = A[0][0] / A[1][0];
                cB = A[1][0];
                cC = A[0][0];

                Complex RCoef = (1 - cY) / (1 + cY);
                Complex cR = RCoef * Complex.Conjugate(RCoef);
                //                Complex RCoef = ((A[0, 0] - A[1, 0]) / (A[0, 0] + A[1, 0])) * ((A[0, 0] - A[1, 0]) / (A[0, 0] + A[1, 0]));

                //n sub는 맨 밑바닥물질의 N
                //다층 박막에서의 반사율
                return cR.Real * 100.0;
            }
            catch (Exception ex)
            {
                return -1.0;
            }
        }

        public double CalcTransmittance(int nPointIndex, int[] arrNKDataIdx, double dWavelength, double[] Thickness) //계산된 T
        {
            try
            {
                if (m_DM.m_LayerData.Count - 1 == 0)
                {
                    MessageBox.Show("CalcTransmittance() - Please open a recipe first!");
                    return -1;
                }
                //int nDataNum = Wavelength.Count(i => dLowerWavelength < i && i < dUpperWavelength);

                double dN_Air = 1.0;
                double dK_Air = 0.0;

                Complex[][] mM = MatrixCreate(2, 2);
                mM[0][0] = 1;
                mM[0][1] = 0;
                mM[1][0] = 0;
                mM[1][1] = 1;

                Complex cN;
                Complex cD; //초기 두께
                Complex cDelta; //d 두께 단위땜에 10 곱해줌 //여기수정

                Complex[][] mMj = MatrixCreate(2, 2);
                int nCalLayer = m_DM.m_LayerData.Count - 2;
                for (int n = 0; n < m_DM.m_LayerData.Count - 1; n++)
                {
                    cN = new Complex(m_DM.m_LayerData[nCalLayer].n[arrNKDataIdx[nCalLayer]], -m_DM.m_LayerData[nCalLayer].k[arrNKDataIdx[nCalLayer]]);
                    cD = new Complex(Thickness[n], 0); //초기 두께
                    cDelta = (2 * Math.PI * cN * cD) / (dWavelength * 10.0); //d 두께 단위땜에 10 곱해줌 //여기수정

                    mMj[0][0] = Complex.Cos(cDelta);
                    mMj[0][1] = C01 * Complex.Sin(cDelta) / cN;
                    mMj[1][0] = C01 * Complex.Sin(cDelta) * cN;
                    mMj[1][1] = Complex.Cos(cDelta);

                    mM = MatrixProduct(mM, mMj);
                    nCalLayer--;
                }
                //다층 박막에서의 admittance
                Complex cY = 0.0, cB = 0.0, cC = 0.0;

                Complex[][] mN = MatrixCreate(2, 1);
                mN[0][0] = 1;
                mN[1][0] = new Complex(dN_Air, -dK_Air);
                Complex[][] A = MatrixProduct(mM, mN);
                cY = A[0][0] / A[1][0];
                cB = A[1][0];
                cC = A[0][0];

                Complex cT_cal = 2 / (cB + cC);//2 * cY / ((cY * cB) + cC);
                Complex cT = cT_cal * Complex.Conjugate(cT_cal);

                return cT.Real * 100.0;
            }
            catch (Exception ex)
            {
                return -1.0;
            }
        }

        public void CalcTransmittance_OptimizingSi(int nPointIdx, int nSiAvgOffsetRange, int nSiAvgOffsetStep, int nDNum, double[] mPn, double[] CalWL)
        {
            int nDataMin = 9999;
            int nDataMinLayerIdx = 0;
            int nLayerCount = m_DM.m_LayerData.Count - 1;
            for (int n = 0; n < nLayerCount - 1; n++)
            {
                if (m_DM.m_LayerData[n].wavelength.Count < nDataMin)
                {
                    nDataMin = m_DM.m_LayerData[n].wavelength.Count;
                    nDataMinLayerIdx = n;
                }
            }

            m_DM.m_RawData[nPointIdx].Transmittance = new double[nDataMin];

            int[][] arrNKIndexes = new int[nDataMin][];
            for (int i = 0; i < nDataMin; i++)
            {
                arrNKIndexes[i] = new int[nLayerCount];
            }

            //투과율계산
            //Task task1 = new Task(() =>
            //{
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var MaxWorkingCount = nDataMin;
            int nWorkingRangeSize = MaxWorkingCount / ConstValue.MULTI_THREAD_COUNT;

            //var part = System.Collections.Concurrent.Partitioner.Create(0, MaxWorkingCount, nWorkingRangeSize);
            var part = System.Collections.Concurrent.Partitioner.Create(0, MaxWorkingCount);

            Parallel.ForEach(part, (num, state) =>
            {
                for (int i = num.Item1; i < num.Item2; i++)
                {
                    double dWavelength = m_DM.m_LayerData[nDataMinLayerIdx].wavelength[i];

                    for (int n = 0; n < nLayerCount; n++)
                    {
                        int nNKIdx = m_DM.m_LayerData[n].wavelength.FindIndex(wl => Math.Abs(dWavelength - wl) < 0.1);
                        if (nNKIdx == -1)
                        {
                            m_DM.m_Log.WriteLog(LogType.Datas, "CalcThickness() - Please check nk data file and wavelength range. Wavelength : " + dWavelength.ToString());
                            return;
                        }
                        arrNKIndexes[i][n] = nNKIdx;
                    }
                }
            });

            sw.Stop();
            Debug.WriteLine("task1 >> " + sw.ElapsedMilliseconds.ToString());
            //});
            //task1.Start();
            //task1.Wait();

            var mPPTemp = new double[nDNum];//Matrix<double>.Build.Dense(nDNum, 1, 0.0);
            mPn.CopyTo(mPPTemp, 0);
            var PP = new double[nLayerCount];
            for (int n = 0; n < nDNum; n++)
            {
                PP[n] = mPPTemp[n];
            }

            //Task task2 = new Task(() =>
            //{
            //var MaxWorkingCount = nDataMin;
            //int nWorkingRangeSize = MaxWorkingCount / ConstValue.MULTI_THREAD_COUNT;

            //var part = System.Collections.Concurrent.Partitioner.Create(0, MaxWorkingCount, nWorkingRangeSize);
            sw.Start();
            Parallel.ForEach(part, (numm, state) =>
            {
                for (int i = numm.Item1; i < numm.Item2; i++)
                {
                    //for (int i = 0; i < MaxWorkingCount; i++)
                    //{
                    double dTSum = 0;
                    int nRange = -nSiAvgOffsetRange;
                    int nStep = nSiAvgOffsetStep;
                    int nNum = (int)Math.Abs((((double)nRange * 2.0) / (double)nStep));

                    double dWavelength = m_DM.m_LayerData[nDataMinLayerIdx].wavelength[i];
                    var PPP = new double[nLayerCount];
                    int nLastRowIndex = nLayerCount - 1;

                    PP.CopyTo(PPP, 0);

                    double dSiThickness = ConstValue.SI_INIT_THICKNESS;
                    var parts = System.Collections.Concurrent.Partitioner.Create(0, nNum);
                    //Parallel.ForEach(parts, (num, state) =>
                    for (int n = 0; n < nNum; n++)
                    {
                        double dCalcT = 0.0;
                        PPP[nLastRowIndex] = dSiThickness + (double)nRange;

                        dCalcT = CalcTransmittance(nPointIdx, arrNKIndexes[i], dWavelength, PPP);
                        dTSum += dCalcT;

                        nRange += nStep;
                    }//);
                    double dTAvg = dTSum / (double)nNum;

                    if (dTAvg < 0.0 || double.IsNaN(dTAvg))
                    {
                        dTAvg = 0.0;
                    }

                    m_DM.m_RawData[nPointIdx].Transmittance[i] = dTAvg;
                }
            });
            sw.Stop();
            Debug.WriteLine("task2 >> " + sw.ElapsedMilliseconds.ToString());
            //});
            //task2.Start();
            //task2.Wait();
            PointTransmittanceData(CalWL, nPointIdx);
        }
        private void PointTransmittanceData(double[] CalWL, int nPointidx)
        {
            int nDCOLTransDataNum = 0;
            m_DM.m_RawData[nPointidx].DCOLTransmittance.Clear();
            for (int indexT = 0; indexT < m_DM.m_ContourMapDataT.Count; indexT++)
            {
                for (int n = 0; n < m_DM.m_RawData[0].nNIRDataNum; n++)
                {
                    if (nDCOLTransDataNum < CalWL.Length && m_DM.m_RawData[nPointidx].Wavelength[n] == m_DM.m_ContourMapDataT[nDCOLTransDataNum].Wavelength)
                    {
                        DCOLTransmittanceData DCOLData = new DCOLTransmittanceData();
                        DCOLData.Wavelength = m_DM.m_ScalesListT[nDCOLTransDataNum].p_waveLength;
                        DCOLData.RawTransmittance = m_DM.m_RawData[nPointidx].Transmittance[n];
                        m_DM.m_RawData[nPointidx].DCOLTransmittance.Add(DCOLData);
                        nDCOLTransDataNum++;

                        break;
                    }
                }
            }
        }
        public void PointCalcTransmittance_OptimizingSi(int nPointIdx, int nSiAvgOffsetRange, int nSiAvgOffsetStep, int nDNum, double[] mPn, double[] CalWL)
        {
            int nDataMin = 9999;
            int nDataMinLayerIdx = 0;
            int nLayerCount = m_DM.m_LayerData.Count - 1;
            for (int n = 0; n < nLayerCount - 1; n++)
            {
                if (m_DM.m_LayerData[n].wavelength.Count < nDataMin)
                {
                    nDataMin = CalWL.Length;
                    nDataMinLayerIdx = n;
                }
            }
            int[][] arrNKIndexes = new int[nDataMin][];
            for (int i = 0; i < nDataMin; i++)
            {
                arrNKIndexes[i] = new int[nLayerCount];
            }
            var MaxWorkingCount = nDataMin;
            int nWorkingRangeSize = MaxWorkingCount / ConstValue.MULTI_THREAD_COUNT;
            for (int i = 0; i < CalWL.Length; i++)
            {
                double dWavelength = CalWL[i];

                for (int n = 0; n < nLayerCount; n++)
                {
                    int nNKIdx = m_DM.m_LayerData[n].wavelength.FindIndex(wl => Math.Abs(dWavelength - wl) < 0.1);
                    if (nNKIdx == -1)
                    {
                        m_DM.m_Log.WriteLog(LogType.Datas, "CalcThickness() - Please check nk data file and wavelength range. Wavelength : " + dWavelength.ToString());
                        return;
                    }
                    arrNKIndexes[i][n] = nNKIdx;
                }
            }

            var mPPTemp = new double[nDNum];//Matrix<double>.Build.Dense(nDNum, 1, 0.0);
            mPn.CopyTo(mPPTemp, 0);
            var PP = new double[nLayerCount];
            for (int n = 0; n < nDNum; n++)
            {
                PP[n] = mPPTemp[n];
            }

            for (int i = 0; i < CalWL.Length; i++)
            {
                double dTSum = 0;
                int nRange = -nSiAvgOffsetRange;
                int nStep = nSiAvgOffsetStep;
                int nNum = (int)Math.Abs((((double)nRange * 2.0) / (double)nStep));

                double dWavelength = CalWL[i];
                var PPP = new double[nLayerCount];
                int nLastRowIndex = nLayerCount - 1;

                PP.CopyTo(PPP, 0);

                double dSiThickness = ConstValue.SI_INIT_THICKNESS;
                var parts = System.Collections.Concurrent.Partitioner.Create(0, nNum);
                //Parallel.ForEach(parts, (num, state) =>
                for (int n = 0; n < nNum; n++)
                {
                    double dCalcT = 0.0;
                    PPP[nLastRowIndex] = dSiThickness + (double)nRange;

                    dCalcT = CalcTransmittance(nPointIdx, arrNKIndexes[i], dWavelength, PPP);
                    dTSum += dCalcT;

                    nRange += nStep;
                }//);
                double dTAvg = dTSum / (double)nNum;

                if (dTAvg < 0.0 || double.IsNaN(dTAvg))
                {
                    dTAvg = 0.0;
                }
                DCOLTransmittanceData DCOLData = new DCOLTransmittanceData();
                DCOLData.Wavelength = CalWL[i];
                DCOLData.RawTransmittance = dTAvg;
                m_DM.m_RawData[nPointIdx].DCOLTransmittance.Add(DCOLData);
            }
        }
        public bool CalcThickness(int nPointIndex, double dWLStart, double dWLEnd, int nNum_Iteration, double dEigenValue, ref List<double> GoFs, ref List<double> Thickness, ref long lCalcTime, ref int nMaxGoFInit)
        {
            //int nDataMin = 9999;
            //int nDataMinLayerIdx = 0;
            //for (int n = 0; n < m_DM.m_LayerData.Count; n++)
            //{
            //    if (m_DM.m_LayerData[n].wavelength.Count < nDataMin)
            //    {
            //        nDataMin = m_DM.m_LayerData[n].wavelength.Count;
            //        nDataMinLayerIdx = n;
            //    }
            //}

            RawData data = m_DM.m_RawData[nPointIndex];

            //계산영역 시작과 끝의 인덱스 찾기
            int nStartNum = 0, nEndNum = 0;
            bool bFound = false;
            double dAvgR = 0.0;
            double dGoF = 0.0;

            for (int n = 0; n < data.eV.Count(); n++)
            {
                if (dWLStart < data.eV[n] && data.eV[n] < dWLEnd)
                {
                    if (bFound == false)
                    {
                        nStartNum = n;
                        bFound = true;
                    }
                    dAvgR += data.Reflectance[n];
                }
                else if (bFound == true)
                {
                    nEndNum = n - 1;
                    break;
                }
            }

            int nDNum = m_DM.m_LayerData.Count - 1; // Base c-Si 층 빼기
            int nWLCount = nEndNum - nStartNum + 1;// 분석 Wavelength Range 갯수
            dAvgR = dAvgR / nWLCount;

            #region NK Index 구하기

            int[][] arrNKIndexes = new int[nWLCount][];
            bool bFailNK = false;
            string sErrorNKLayer = "";

            for (int i = 0; i < nWLCount; i++)
            {
                arrNKIndexes[i] = new int[m_DM.m_LayerData.Count];
            }
            //m_DM.m_CalcReflectance = new double[nWLCount];

            Task task1 = new Task(() =>
            {
                var MaxWorkingCount = nEndNum + 1;
                int nWorkingRangeSize = MaxWorkingCount / ConstValue.MULTI_THREAD_COUNT;

                var part = System.Collections.Concurrent.Partitioner.Create(nStartNum, MaxWorkingCount, nWorkingRangeSize);
                Parallel.ForEach(part, (num, state) =>
                {
                    for (int i = num.Item1; i < num.Item2; i++)
                    {
                        bool bFail = false;
                        double dWavelength = data.eV[i];

                        for (int n = 0; n < m_DM.m_LayerData.Count; n++)
                        {
                            int nRow = i - nStartNum;
                            int nNKIdx = m_DM.m_LayerData[n].wavelength.FindIndex(wl => Math.Abs(dWavelength - wl) < 0.1);

                            if (nNKIdx == -1)
                            {
                                sErrorNKLayer = m_DM.m_LayerData[n].sRefName;
                                bFail = true;
                                break;
                            }
                            arrNKIndexes[nRow][n] = nNKIdx;
                        }
                        if (bFail)
                        {
                            bFailNK = true;
                            return;
                        }
                    }
                });
            });
            task1.Start();
            task1.Wait();

            if (bFailNK)
            {
                System.Windows.Forms.MessageBox.Show("NK data and reflectance data wavelength do not match - [" + sErrorNKLayer + "]", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
            #endregion

            #region All FIX 

            var fix_mr = Matrix<double>.Build.Dense(nWLCount, 1, 0.0);  // Matrix r : r is residual = f(x) = || r(x) ||^2 = || Gaussian(x;a,b,c) - y ||^2
            var mPn = new double[nDNum]; // Matrix P

            int nDNumLM = 0;//LM fitting할 layer 개수
            for (int i = 0; i < nDNum; i++)
            {
                if (m_DM.m_LayerData[i].bFix == false)
                {
                    nDNumLM++;
                }
                mPn[i] = m_DM.m_LayerData[i].dInitThickness;// 초기 두께값 넣기
            }

            if (nDNumLM == 0)    //다 고정일 경우
            {
                for (int i = nStartNum; i < nEndNum + 1; i++)
                {
                    int nRow = i - nStartNum;
                    double dWavelength = data.eV[i];

                    double dCalcR = CalcReflectance(nPointIndex, arrNKIndexes[nRow], dWavelength, mPn);
                    data.CalcReflectance[nRow] = dCalcR;

                    fix_mr[nRow, 0] = dCalcR - data.Reflectance[i];
                }

                if (m_DM.m_bCalcTransmittance)
                {
                    // 투과율 계산 안함
                    //CalcTransmittance_OptimizingSi(nPointIndex, ConstValue.SI_AVG_OFFSET_RANGE, ConstValue.SI_AVG_OFFSET_STEP, nDNum, mPn);
                }
                else
                {
                    for (int n = 0; n < nWLCount; n++)
                    {
                        //투과율 계산 안함
                        //data.CalcTransmittance[n] = 99.9;
                    }
                }
                //추후 제거
                //추가
                //int nWLCount_CalR = 0;

                //for (int i = 0; i < data.CalcReflectance.Length; i++)
                //{
                //    if (data.CalcReflectance[i] > 0)
                //    {
                //        nWLCount_CalR++;
                //    }
                //}


                dGoF = CalcGoF(data.Reflectance.ToArray(), data.CalcReflectance, nStartNum, nEndNum, 0, nEndNum, dAvgR);

                if (dGoF < 0.0)
                {
                    dGoF = 0.0;
                }
                //Review Station 과 Calassistant 차이
                //data.dGoF = dGoF;
                //data.Thickness.Clear();
                GoFs.Add(dGoF);
                Thickness.Clear();
                //추후 수정 : Calassistant의 두께 출력 List 에 연관되어 있는데 CAMELLIA 2에 연동시키려면 다시한번 고민해야함
                for (int n = 0; n < nDNum; n++)
                {
                    Thickness.Add(m_DM.m_LayerData[n].dInitThickness);
                }
                return true;
            }

            #endregion

            #region Fitting ( output Matrix 계산) __ ((J^T*J) + lamda*diag(J^T*J))^-1 * J^T*r 

            var mTemp = Matrix<double>.Build.Dense(nDNum, 1, 0.0);  //Matrix Temp
            var mTempDiag = Matrix<double>.Build.DenseDiagonal(nDNum, 0.0);   //Matrix Temp Diagonal
            var mr = Matrix<double>.Build.Dense(nWLCount, 1, 0.0);                                                                  //var mJ = Matrix<double>.Build.Dense(nWLCount, nDNum, 0.0); //Jacobian Matrix J
            var mJ = Matrix<double>.Build.Dense(nWLCount, nDNumLM, 0.0); //Jacobian Matrix J  

            for (int k = 0; k < nNum_Iteration; k++)
            {
                Task task2 = new Task(() =>
                {
                    var MaxWorkingCount = nEndNum + 1;
                    int nWorkingRangeSize = MaxWorkingCount / ConstValue.MULTI_THREAD_COUNT;

                    var part = System.Collections.Concurrent.Partitioner.Create(nStartNum, MaxWorkingCount, nWorkingRangeSize);
                    Parallel.ForEach(part, (numm, state) =>
                    {
                        var mThickness = new double[nDNum];
                        for (int i = numm.Item1; i < numm.Item2; i++)
                        {
                            int nRow = i - nStartNum;
                            double dWavelength = data.eV[i];

                            double dCalcR = CalcReflectance(nPointIndex, arrNKIndexes[nRow], dWavelength, mPn);
                            data.CalcReflectance[nRow] = dCalcR;

                            mr[nRow, 0] = dCalcR - data.Reflectance[i];

                            int nJCol = 0;
                            for (int j = 0; j < nDNum; j++)
                            {
                                for (int n = 0; n < nDNum; n++) //초기화
                                {
                                    mThickness[n] = mPn[n];
                                }

                                mThickness[j] = mPn[j] + ConstValue.DELTA;

                                if (m_DM.m_LayerData[j].bFix == false)
                                {
                                    double dR = CalcReflectance(nPointIndex, arrNKIndexes[nRow], dWavelength, mThickness);
                                    double dR1 = CalcReflectance(nPointIndex, arrNKIndexes[nRow], dWavelength, mPn);

                                    mJ[nRow, nJCol] = (dR - dR1) / ConstValue.DELTA;
                                    nJCol++;
                                }
                            }
                        }
                    });
                });
                task2.Start();
                task2.Wait();

                //m_DM.m_CalcReflectance = new double[nWLCount];

                // output Matrix 계산
                // ((J^T*J) + lamda*diag(J^T*J))^-1 * J^T*r 

                Vector<double> diag = mJ.TransposeThisAndMultiply(mJ).Diagonal();
                mTempDiag = Matrix<double>.Build.Dense(nDNumLM, nDNumLM, 0.0);
                for (int i = 0; i < nDNumLM; i++)
                {
                    for (int j = 0; j < nDNumLM; j++)
                    {
                        if (i == j)
                        {
                            mTempDiag[i, j] = diag[i];
                        }
                        else
                        {
                            mTempDiag[i, j] = 0.0;
                        }
                    }
                }
                mTemp = (mJ.TransposeThisAndMultiply(mJ) + (dEigenValue * mTempDiag)).Inverse().Multiply(mJ.TransposeThisAndMultiply(mr));

                int nLMRow = 0;
                for (int i = 0; i < nDNum; i++)
                {
                    if (m_DM.m_LayerData[i].bFix == true)
                    {
                        continue;
                    }
                    else
                    {
                        double dNewThickness = mPn[i] - mTemp[nLMRow, 0];
                        if (0.0 < m_DM.m_LayerData[i].dTHKRangeRate && m_DM.m_LayerData[i].dTHKRangeRate <= 100.0)
                        {
                            double dTHKRangeOffset = m_DM.m_LayerData[i].dInitThickness * (m_DM.m_LayerData[i].dTHKRangeRate / 100.0);
                            double dTHKRangeMin = m_DM.m_LayerData[i].dInitThickness - dTHKRangeOffset;
                            double dTHKRangeMax = m_DM.m_LayerData[i].dInitThickness + dTHKRangeOffset;

                            if (dTHKRangeMin > dNewThickness || dNewThickness > dTHKRangeMax)
                            {
                                mPn[i] = mPn[i];
                            }
                            else
                            {
                                mPn[i] = dNewThickness;
                            }
                        }
                        else
                        {
                            mPn[i] = dNewThickness;
                        }
                        nLMRow++;
                    }
                }


            }
            #endregion 

            #region CalcR 업데이트
            Task task3 = new Task(() =>
            {
                var MaxWorkingCount = nEndNum + 1;
                int nWorkingRangeSize = MaxWorkingCount / ConstValue.MULTI_THREAD_COUNT;

                var part = System.Collections.Concurrent.Partitioner.Create(nStartNum, MaxWorkingCount, nWorkingRangeSize);
                Parallel.ForEach(part, (numm, state) =>
                {
                    for (int i = numm.Item1; i < numm.Item2; i++)
                    {
                        int nRow = i - nStartNum;
                        double dWavelength = data.eV[i];

                        double dCalcR = CalcReflectance(nPointIndex, arrNKIndexes[nRow], dWavelength, mPn);
                        data.CalcReflectance[nRow] = dCalcR;
                    }
                });
            });
            task3.Start();
            task3.Wait();

            if (m_DM.m_bCalcTransmittance)
            {
                //CalcTransmittance_OptimizingSi(nPointIndex, ConstValue.SI_AVG_OFFSET_RANGE, ConstValue.SI_AVG_OFFSET_STEP, nDNum, mPn);
            }
            else
            {
                for (int n = nStartNum; n < nEndNum + 1; n++)
                {
                    int nRow = n - nStartNum;
                    //data.CalcTransmittance[nRow] = 99.9;
                }
            }

            #endregion

            #region GoF 계산 & Thickness output
            //추후 제거
            //int nWLCount_CalR = 0;

            //for (int i = 0; i < data.CalcReflectance.Length; i++)
            //{
            //    if (data.CalcReflectance[i] > 0)
            //    {
            //        nWLCount_CalR++;
            //    }
            //}
            dGoF = CalcGoF(data.Reflectance.ToArray(), data.CalcReflectance, nStartNum, nEndNum, 0, nEndNum, dAvgR);
            //dGoF = CalcGoF(data.Reflectance.ToArray(), data,CalcReflectance, nStartNum, nEndNum, 0, nEndNum, dAvgR);

            if (dGoF < 0.0 || double.IsNaN(dGoF) || double.IsNegativeInfinity(dGoF) || double.IsPositiveInfinity(dGoF))
            {
                dGoF = 0.0;
            }
            //Review Station 과 Calassistant 차이
            //data.dGoF = dGoF;
            //data.Thickness.Clear();
            GoFs.Add(dGoF);
            Thickness.Clear();

            for (int n = 0; n < nDNum; n++)
            {
                double dTHK = mPn[n];
                //dTHK *= m_DM.m_LayerData[n].scales.dScale;
                // dTHK += m_DM.m_LayerData[n].scales.dOffset;

                if (double.IsNaN(dTHK) || double.IsNegativeInfinity(dTHK) || double.IsPositiveInfinity(dTHK))
                {
                    dTHK = 0.0;
                }
                //Review Station 과 Calassistant 차이
                //data.Thickness.Add(dTHK);
                Thickness.Add(dTHK);
            }

            #endregion



            return true;
        }

        public double CalcGoF(double[] MeasuredData, double[] CalcData, int nMeasureStartIdx, int nMeasureLastIdx, int nCalcStartIdx, int nCalcLastIdx, double dAvgValue)
        {
            double GoF = 0.0;
            double SST = 0.0;
            double SSE = 0.0;

            if (MeasuredData.Length <= nMeasureStartIdx || MeasuredData.Length <= nMeasureLastIdx
                || CalcData.Length <= nCalcStartIdx || CalcData.Length <= nCalcLastIdx)
                return ConstValue.OUT_OF_RANGE;

            if (0 > nMeasureStartIdx || 0 > nMeasureLastIdx
                || 0 > nCalcStartIdx || 0 > nCalcLastIdx)
                return ConstValue.OUT_OF_RANGE;

            int nCalcIdx = nCalcStartIdx;
            for (int i = nMeasureStartIdx; i < nMeasureLastIdx + 1; i++)
            {
                if (nCalcIdx <= nCalcLastIdx)
                {
                    SST += Math.Pow(MeasuredData[i] - dAvgValue, 2);
                    SSE += Math.Pow(CalcData[nCalcIdx] - MeasuredData[i], 2);
                    nCalcIdx++;
                }
            }
            GoF = 1 - (SSE / SST);

            return GoF;
        }




        public void Calc_PM()
        {
        }

        static Complex[][] MatrixCreate(int rows, int cols)
        {
            // do error checking here
            Complex[][] result = new Complex[rows][];
            for (int i = 0; i < rows; ++i)
                result[i] = new Complex[cols];
            return result;
        }

        static Complex[][] MatrixProduct(Complex[][] matrixA, Complex[][] matrixB)
        {
            int aRows = matrixA.Length; int aCols = matrixA[0].Length;
            int bRows = matrixB.Length; int bCols = matrixB[0].Length;
            if (aCols != bRows)
                throw new Exception("xxx");

            Complex[][] result = MatrixCreate(aRows, bCols);

            for (int i = 0; i < aRows; ++i) // each row of A
                for (int j = 0; j < bCols; ++j) // each col of B
                    for (int k = 0; k < aCols; ++k) // could use k < bRows
                        result[i][j] += matrixA[i][k] * matrixB[k][j];
            return result;
        }
    }
}
