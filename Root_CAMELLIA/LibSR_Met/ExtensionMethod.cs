using NanoView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace Root_CAMELLIA.LibSR_Met
{
    public static class ExtensionMethod
    {
        public static List<double> Times(this List<double> dData, double dTimes)
        {
            for (int n = 0; n < dData.Count; n++)
            {
                dData[n] = dData[n] * dTimes;
            }
            return dData;
        }

        public static double Stdev(this List<double> dData)
        {
            double dStdev = 0.0;
            if (dData.Count() > 0)
            {
                //Compute the Average
                double avg = dData.Average();
                //Perform the Sum of (value-avg)_2_2
                double sum = dData.Sum(d => Math.Pow(d - avg, 2));
                //Put it all together
                dStdev = Math.Sqrt((sum) / (dData.Count() - 1));
            }
            return dStdev;
        }

        public static _layer[] To_layer(this List<LayerData> layerDataList)
        {
            _layer[] layer = new _layer[layerDataList.Count];

            int i = 0;
            string str = string.Empty;
            foreach (LayerData l in layerDataList)
            {
                layer[i].hostname = new char[128];
                layer[i].hostpath = new char[512];
                layer[i].guest1name = new char[128];
                layer[i].guest1path = new char[512];
                layer[i].guest2name = new char[128];
                layer[i].guest2path = new char[512];

                layer[i].hostname = l.hostname;
                layer[i].hostpath = l.hostpath;

                if (l.guest1name != null)
                {
                    layer[i].guest1name = l.guest1name;
                    layer[i].guest1path = l.guest1path;
                }
                else
                {
                    str = ConstValue.NONE;
                    layer[i].guest1name = str.ToCharArray();
                    layer[i].guest1path = str.ToCharArray();
                }
                if (l.guest2name != null)
                {
                    layer[i].guest2name = l.guest2name;
                    layer[i].guest2path = l.guest2path;
                }
                else
                {
                    str = ConstValue.NONE;
                    layer[i].guest2name = str.ToCharArray();
                    layer[i].guest2path = str.ToCharArray();
                }
                layer[i].fv1 = l.dFv1;
                layer[i].fv2 = l.dFv2;
                layer[i].thickness = l.dThickness;
                layer[i].fv1fit = l.bFv1fit;
                layer[i].fv2fit = l.bFv2fit;
                layer[i].thfit = l.bThicknessFit;
                layer[i].emm = l.nEmm;

                i++;
            }
            return layer;
        }

        public static List<LayerData> ToLayerData(this LayerList layerList)
        {
            List<LayerData> layerDatas = new List<LayerData>();
            foreach(Layer layer in layerList)
            {
                LayerData layerData = new LayerData();

                if (layer.m_Host != null)
                {
                    layerData.hostname = layer.m_Host.m_Name.ToCharArray();
                    layerData.hostpath = layer.m_Host.m_Path.ToCharArray();
                }

                if (layer.m_Guest1 != null)
                {
                    layerData.guest1name = layer.m_Guest1.m_Name.ToCharArray();
                    layerData.guest1path = layer.m_Guest1.m_Path.ToCharArray();
                }
                if (layer.m_Guest2 != null)
                {
                    layerData.guest2name = layer.m_Guest2.m_Name.ToCharArray();
                    layerData.guest2path = layer.m_Guest2.m_Path.ToCharArray();
                }

                layerData.dFv1 = layer.m_fv1;
                layerData.dFv2 = layer.m_fv2;
                layerData.dThickness = layer.m_Thickness;
                layerData.bFv1fit = layer.m_bFitfv1;
                layerData.bFv2fit = layer.m_bFitfv2;
                layerData.bThicknessFit = layer.m_bFitThickness;
                layerData.nEmm = layer.m_Emm;

                layerDatas.Add(layerData);
            }

            return layerDatas;
        }
    }
}
