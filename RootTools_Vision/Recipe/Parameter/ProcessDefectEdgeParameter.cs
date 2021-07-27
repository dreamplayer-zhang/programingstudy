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
            minAngles = new List<double>();
            maxAngles = new List<double>();
            defectCodes = new List<int>();
        }

        private bool useProcessDefectEdge = true;
        [Browsable(false)]
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
        private List<double> minAngles;
        [Browsable(false)]
        public List<double> MinAngles
        {
            get => this.minAngles;
            set
            {
                SetProperty(ref minAngles, value);
            }
        }

        [XmlArray("OptionDefectAngle")]
        private List<double> maxAngles;
        [Browsable(false)]
        public List<double> MaxAngles
        {
            get => this.maxAngles;
            set
            {
                SetProperty(ref maxAngles, value);
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
