using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using RootTools;
using RootTools.Database;
using RootTools_CLR;

namespace RootTools_Vision
{
    public class PreCreateGolden
    {
        #region [Member variables]
        List<Cpp_Point> _chipLTPos = new List<Cpp_Point>();
        private PreCreateGoldenParameter _parameterGolden;
        private IntPtr _inspectionSharedBuffer;
        private int _sharedBufferW;
        private int _sharedBufferH;
        #endregion

        public PreCreateGolden()
        {
        }
        public void SetInspectionBuffer(IntPtr inspectionSharedBuffer, int sharedBufferW, int sharedBufferH)
        {
            this._inspectionSharedBuffer = inspectionSharedBuffer;
            this._sharedBufferW = sharedBufferW;
            this._sharedBufferH = sharedBufferH;
        }

        public void SetParameter(PreCreateGoldenParameter parameterGolden)
        {
            this._parameterGolden = parameterGolden;
        }

        public List<CPoint> SetChipPositionList(List<CPoint> chipLTPos, List<CPoint> validMap, int mapYSize)
        {
            List<CPoint> useChipMapData = new List<CPoint>();
            this._chipLTPos.Clear();

            PreCreateGoldenParameter.SelectYIndexMethod curMethod = _parameterGolden.SelectYIndex;

            if (chipLTPos.Count < 4)
                curMethod = PreCreateGoldenParameter.SelectYIndexMethod.Whole;

            for (int i = 0; i < chipLTPos.Count; i++)
            {
                switch((int)_parameterGolden.SelectYIndex)
                {
                    case (int)PreCreateGoldenParameter.SelectYIndexMethod.Whole:
                        this._chipLTPos.Add(new Cpp_Point(chipLTPos[i].X, chipLTPos[i].Y));
                        useChipMapData.Add(validMap[i]);
                        break;
                    case (int)PreCreateGoldenParameter.SelectYIndexMethod.Inner:
                        if((mapYSize / 4 < validMap [i].Y) && ((float)mapYSize / 4 * 3 > validMap[i].Y))
                        {
                            this._chipLTPos.Add(new Cpp_Point(chipLTPos[i].X, chipLTPos[i].Y));
                            useChipMapData.Add(validMap[i]);
                        }
                        break;
                    case (int)PreCreateGoldenParameter.SelectYIndexMethod.Outer:
                        if ((mapYSize / 4 > validMap[i].Y) || ((float)mapYSize / 4 * 3 < validMap[i].Y))
                        {
                            this._chipLTPos.Add(new Cpp_Point(chipLTPos[i].X, chipLTPos[i].Y));
                            useChipMapData.Add(validMap[i]);
                        }
                        break;
                }
            }

            return useChipMapData;
        }
        public byte[] CreateGoldenImage (int originW, int originH)
        {
            byte[] goldenImage = new byte[originW * originH];
            unsafe {

                switch (_parameterGolden.CreateRefImage)
                {
                    case CreateRefImageMethod.Average:
                        CLR_IP.Cpp_CreateGoldenImage_Avg((byte*)this._inspectionSharedBuffer.ToPointer(), goldenImage, _chipLTPos.Count,
                            _sharedBufferW, _sharedBufferH,
                            _chipLTPos, originW, originH);
                        break;
                    case CreateRefImageMethod.MedianAverage:
                        CLR_IP.Cpp_CreateGoldenImage_MedianAvg((byte*)this._inspectionSharedBuffer.ToPointer(), goldenImage, _chipLTPos.Count,
                           _sharedBufferW, _sharedBufferH,
                            _chipLTPos, originW, originH);

                        break;
                    case CreateRefImageMethod.Median:
                        CLR_IP.Cpp_CreateGoldenImage_Median((byte*)this._inspectionSharedBuffer.ToPointer(), goldenImage, _chipLTPos.Count,
                            _sharedBufferW, _sharedBufferH,
                            _chipLTPos, originW, originH);
                        break;
                    default:
                        CLR_IP.Cpp_CreateGoldenImage_Avg((byte*)this._inspectionSharedBuffer.ToPointer(), goldenImage, _chipLTPos.Count,
                            _sharedBufferW, _sharedBufferH,
                            _chipLTPos, originW, originH);
                        break;
                }
            }

            return goldenImage;
        }

        object lockObj = new object();
        
    }
}
