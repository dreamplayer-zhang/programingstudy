using RootTools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class EUVPodSurfaceParameter:ParameterBase
    {
        EUVPodSurfaceParameterBase podStain;
        EUVPodSurfaceParameterBase podSide;
        EUVPodSurfaceParameterBase podTDI;
        EUVPodSurfaceParameterBase podStacking;

        public EUVPodSurfaceParameter() : base(typeof(EUVPodSurface))
        {
            podStain = new EUVPodSurfaceParameterBase();
            podSide = new EUVPodSurfaceParameterBase();
            podTDI = new EUVPodSurfaceParameterBase();
            podStacking = new EUVPodSurfaceParameterBase();
        }

        #region Getter/Setter
        [Category("Parameter")]
        public EUVPodSurfaceParameterBase PodStain
        {
            get => podStain;
            set => SetProperty(ref podStain, value);
        }
        [Category("Parameter")]
        public EUVPodSurfaceParameterBase PodSide
        {
            get => podSide;
            set => SetProperty(ref podSide, value);
        }
        [Category("Parameter")]
        public EUVPodSurfaceParameterBase PodTDI
        {
            get => podTDI;
            set => SetProperty(ref podSide, value);
        }
        [Category("Parameter")]
        public EUVPodSurfaceParameterBase PodStacking
        {
            get => podStacking;
            set => SetProperty(ref podStacking, value);
        }

        #endregion

    }

    public class EUVPodSurfaceParameterBase : ObservableObject, IMaskInspection
    {
        #region [Parameter]
        bool absolute;
        uint pitLevel,levelMin,levelMax,pitSize,sizeMax,sizeMin;
        int illumCondition;
        private DiffFilterMethod diffFilter = DiffFilterMethod.Average;

        #endregion

        #region [Getter/Setter]
        [Browsable(false)]
        public int IllumCondition
        {
            get => illumCondition;
            set => SetProperty(ref illumCondition, value);
        }
        public int MaskIndex { get; set; }

        public bool Absolute
        {
            get => absolute;
            set => SetProperty(ref absolute, value);
        }
        public uint PitLevel
        {
            get => pitLevel;
            set => SetProperty(ref pitLevel, value);
        }
        public uint LevelMin
        {
            get => levelMin;
            set => SetProperty(ref levelMin, value);
        }
        public uint LevelMax
        {
            get => levelMax;
            set => SetProperty(ref levelMax, value);
        }
        public uint PitSize
        {
            get => pitSize;
            set => SetProperty(ref pitSize, value);
        }
        public uint SizeMax
        {
            get => sizeMax;
            set => SetProperty(ref sizeMax, value);
        }
        public uint SizeMin
        {
            get => sizeMin;
            set => SetProperty(ref sizeMin, value);
        }
        public DiffFilterMethod DiffFilter
        {
            get => this.diffFilter;
            set
            {
                SetProperty(ref diffFilter, value);
            }
        }
        #endregion
    }
}
