using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_CAMELLIA.Data
{
    public class PresetData
    {
        public List<CCircle> DataCandidatePoint { get; set; } = new List<CCircle>();
        public List<CCircle> DataSelectedPoint { get; set; } = new List<CCircle>();
        public List<int> DataMeasurementRoute { get; set; } = new List<int>();
    }
}
