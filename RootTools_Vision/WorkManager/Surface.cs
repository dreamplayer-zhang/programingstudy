
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using RootTools_CLR;


namespace RootTools_Vision
{
    public class Surface : Inspection, IInspection
    {
        #region IInspection 멤버

        public INSPECTION_TYPE TYPE
        {
            get { return INSPECTION_TYPE.Surface; }
        }

        #endregion

        private SurfaceParameter parameter;

        public Surface() { }

        public Surface(SurfaceParameter parameter, out object result)
        {
            result = new object();
        }

        public void SetParameter(IParameter _parameter)
        {
            this.parameter = (SurfaceParameter)_parameter;
        }

        public void DoInspection(Workplace workplace, out InspectionResultCollection inspectionResults) 
        {
            int nBufferWidth = (int)this.szInspectionBuffer.Width;
            int nBufferHeight = (int)this.szInspectionBuffer.Height;

            // Inspection Buffer는 검사 초기에 미리 할당해놓고 사용해야할수도 있음
            byte[] InspectionBuffer = new byte[nBufferWidth * nBufferHeight];

            for(int y = 0; y < nBufferHeight; y++)
            {
                Marshal.Copy(
                this.ptrImageBuffer + (int)workplace.ImagePosition.X + (y + (int)workplace.ImagePosition.Y) * (int)this.szImageBuffer.Width, // source
                InspectionBuffer,
                nBufferWidth * y,
                nBufferWidth
                );
            }

            CLR_IP.Cpp_Threshold(InspectionBuffer, InspectionBuffer, nBufferWidth, nBufferHeight, this.parameter.IsDark, this.parameter.Threshold);

            CLR_IP.Cpp_Morphology(InspectionBuffer, InspectionBuffer, nBufferWidth, nBufferHeight, 3, "Open", 3);

            var outLabels = CLR_IP.Cpp_Labeling(InspectionBuffer, InspectionBuffer, nBufferWidth, nBufferHeight, this.parameter.IsDark);

            inspectionResults = new InspectionResultCollection();

            foreach (var label in outLabels)
            {
                //inspectionResults.Add(new InspectionResult());
            }
            
        }
    }
}
