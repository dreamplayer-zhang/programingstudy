using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RootTools_Vision
{
    public class ProcessDefectEdgeParameter : ParameterBase
    {
        public ProcessDefectEdgeParameter() : base(typeof(ProcessDefect_Edge))
        {
            angles = new List<double>();
            defectCodes = new List<int>();
        }

        private bool useProcessDefectEdge = true;
        public bool Use
        {
            get => this.useProcessDefectEdge;
            set
            {
                SetProperty(ref useProcessDefectEdge, value);
            }
        }

        private bool useMergeDefect = true;
        public bool UseMergeDefect
        {
            get => this.useMergeDefect;
            set
            {
                SetProperty(ref this.useMergeDefect, value);
            }
        }

        private int mergeDefectDistance = 2;
        public int MergeDefectDistnace
        {
            get => this.mergeDefectDistance;
            set
            {
                SetProperty(ref this.mergeDefectDistance, value);
            }
        }

        private bool useOptionDefect = false;
        public bool UseOptionDefect
        {
            get => this.useOptionDefect;
            set
            {
                SetProperty(ref this.useOptionDefect, value);
            }
        }

        [XmlArray("OptionDefectAngle")]
        private List<double> angles;
        [Browsable(false)]
        public List<double> Angles
        {
            get => this.angles;
            set
            {
                SetProperty(ref angles, value);
            }
        }
        [XmlArray("OptionDefectCode")]
        private List<int> defectCodes;
        [Browsable(false)]
        public List<int> DefectCodes
        {
            get => this.defectCodes;
            set
            {
                SetProperty(ref defectCodes, value);
            }
        }
    }
}
