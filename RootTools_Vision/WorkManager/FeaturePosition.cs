using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools_Vision
{
    public class FeaturePosition : IPosition
    {
        public POSITION_TYPE TYPE => POSITION_TYPE.Feature;

        public void DoPosition()
        {
            // Parameters
            int nWidth = 1000;
            int nHeight = 1000;
            byte[] pBuf = new byte[nWidth * nHeight];
            int nSearchLength = 100;

            List<byte[]> vtFeatures = new List<byte[]>();

            Point ptTrans = new Point();

            //Filter Option


            // TemplateMatching(pBuf, nWidth, nHeight, Feature, out ptTrans);
        }
    }
}
