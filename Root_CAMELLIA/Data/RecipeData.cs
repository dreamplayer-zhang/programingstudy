using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_CAMELLIA.Data
{
    public class RecipeData
    {
        public List<CCircle> DataCandidatePoint { get; set; } = new List<CCircle>();
        public List<CCircle> DataCandidateSelectedPoint { get; set; } = new List<CCircle>();
        public List<CCircle> DataSelectedPoint { get; set; } = new List<CCircle>();
        public List<int> DataMeasurementRoute { get; set; } = new List<int>();
        public float LowerWaveLength { get; set; } = 350;
        public float UpperWaveLength { get; set; } = 950;
        public int LMIteration { get; set; } = 10;
        public float DampingFactor { get; set; } = 0.01f;
        public List<WavelengthItem> WaveLengthReflectance { get; set; } = new List<WavelengthItem>();
        public List<WavelengthItem> WaveLengthTransmittance { get; set; } = new List<WavelengthItem>();
        public int VISIntegrationTime { get; set; } = 20;
        public int NIRIntegrationTime { get; set; } = 150;
        public int MeasureRepeatCount { get; set; } = 1;
        public bool UseThickness { get; set; } = true;
        public bool UseTransmittance { get; set; } = true;
        //public List<string> MaterialList = new List<string>();
        public string ModelRecipePath = "";
        public void ClearPoint()
        {
            DataCandidatePoint.Clear();
            DataCandidateSelectedPoint.Clear();
            DataSelectedPoint.Clear();
            DataMeasurementRoute.Clear();
        }

        public void ClearCandidatePoint()
        {
            DataCandidatePoint.Clear();
        }


        public void CheckCircleSize()
        {
            int Index;
            List<CCircle> temp = new List<CCircle>();
            foreach (CCircle item in DataSelectedPoint)
            {
                temp.Add(item);
            }
            DataSelectedPoint.Clear();
            foreach (CCircle item in temp)
            {
                if (ContainsData(DataCandidatePoint, item, out Index))
                {
                    CCircle circle;
                    circle = new CCircle(item.x, item.y, DataCandidatePoint[Index].width, DataCandidatePoint[Index].height,
                        0, 0);

                    DataSelectedPoint.Add(circle);
                }
            }
        }

        public void Clone(RecipeData data)
        {
            data.DataCandidatePoint = new List<CCircle>(DataCandidatePoint.ToArray());
            data.DataSelectedPoint = new List<CCircle>(DataSelectedPoint.ToArray());
            data.DataMeasurementRoute = new List<int>(DataMeasurementRoute.ToArray());
            data.LowerWaveLength = LowerWaveLength;
            data.UpperWaveLength = UpperWaveLength;
            data.LMIteration = LMIteration;
            data.DampingFactor = DampingFactor;
            data.WaveLengthReflectance = new List<WavelengthItem>(WaveLengthReflectance.ToArray());
            data.WaveLengthTransmittance = new List<WavelengthItem>(WaveLengthTransmittance.ToArray());
            data.VISIntegrationTime = VISIntegrationTime;
            data.NIRIntegrationTime = NIRIntegrationTime;
            data.UseThickness = UseThickness;
            data.MeasureRepeatCount = MeasureRepeatCount;
            data.UseTransmittance = UseTransmittance;
            data.ModelRecipePath = ModelRecipePath;
            
        }

        

        public bool ContainsData(List<CCircle> list, CCircle circle, out int nIndex)
        {
            bool bRst = false;
            nIndex = -1;

            int nCount = 0;

            foreach (var item in list)
            {
                if (Math.Round(item.x, 3) == Math.Round(circle.x, 3) && Math.Round(item.y, 3) == Math.Round(circle.y, 3))
                {
                    bRst = true;
                    nIndex = nCount;
                }
                nCount++;
            }
            return bRst;
        }
    }

    #region Circle
    public struct CCircle
    {
        public double x, y, width, height;
        public double MeasurementOffsetX, MeasurementOffsetY;
        //public CamelliaState.CamelliaCenterEdge type;
        //public CamelliaState.CamelliaSubMode mode;
        //public int mode, type; /* mode : 0(Point), 1(Die) || type : 0(Blue), 1(Yellow), 2(Red)*/

        //public CCircle(double dX, double dY, double dWidth, double dHeight, CamelliaState.CamelliaCenterEdge cType, CamelliaState.CamelliaSubMode cMode, double dMeasurementOffsetX, double dMeasurementOffsetY)
        //{
        //    x = dX;
        //    y = dY;
        //    width = dWidth;
        //    height = dHeight;
        //    type = cType;
        //    mode = cMode;
        //    MeasurementOffsetX = dMeasurementOffsetX;
        //    MeasurementOffsetY = dMeasurementOffsetY;
        //}
        //public CCircle()
        //{

        //}

        public CCircle(double dX, double dY, double dWidth, double dHeight, double dMeasurementOffsetX, double dMeasurementOffsetY)
        {
            x = dX;
            y = dY;
            width = dWidth;
            height = dHeight;
            MeasurementOffsetX = dMeasurementOffsetX;
            MeasurementOffsetY = dMeasurementOffsetY;
        }

        public void Transform(double dScaleX, double dScaleY)
        {
            x *= dScaleX;
            y *= dScaleY;
            width *= dScaleX;
            height *= dScaleY;
            MeasurementOffsetX *= dScaleX;
            MeasurementOffsetY *= dScaleY;
        }

        public void ScaleOffset(int nScale, int nOffsetX, int nOffsetY)
        {
            x = x * nScale + nOffsetX;
            y = y * nScale + nOffsetY;
            width *= nScale;
            height *= nScale;
            MeasurementOffsetX *= nScale;
            MeasurementOffsetY *= nScale;
        }

        public void ScaleOffset(double dScale, int nOffsetX, int nOffsetY)
        {
            x = x * dScale + nOffsetX;
            y = y * dScale + nOffsetY;
            width *= dScale;
            height *= dScale;
            MeasurementOffsetX *= dScale;
            MeasurementOffsetY *= dScale;
        }
    }
    #endregion
    public struct WavelengthItem
    {
        public double p_waveLength { get; set; }
        public double p_scale { get; set; }
        public double p_offset { get; set; }
        public WavelengthItem(double waveLength, double scale, double offset)
        {
            p_waveLength = waveLength;
            p_scale = scale;
            p_offset = offset;
        }
    }

    
}
