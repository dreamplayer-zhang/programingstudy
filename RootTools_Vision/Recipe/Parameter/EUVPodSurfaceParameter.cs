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
        EUVPodSurfaceParameterBase podSideLR;
        EUVPodSurfaceParameterBase podSideTB;
        EUVPodSurfaceParameterBase podTDI;
        EUVPodSurfaceParameterBase podStacking;

        public EUVPodSurfaceParameter() : base(typeof(EUVPodSurface))
        {
            /*
             Mask Number Info
            Stain 0,1
            TDI 2,3,4
            SideLR 5,6/ 
            SideTB 7,8/
            Stacking 9(Cover)
             */
            podStain = new EUVPodSurfaceParameterBase(0);
            podSideLR = new EUVPodSurfaceParameterBase(5);
            podSideTB = new EUVPodSurfaceParameterBase(7);
            podTDI = new EUVPodSurfaceParameterBase(2);
            podStacking = new EUVPodSurfaceParameterBase(9);
        }

        #region Getter/Setter

        public EUVPodSurfaceParameterBase PodStain
        {
            get => podStain;
            set => SetProperty(ref podStain, value);
        }

        public EUVPodSurfaceParameterBase PodSideLR
        {
            get => podSideLR;
            set => SetProperty(ref podSideLR, value);
        }

        public EUVPodSurfaceParameterBase PodSideTB
        {
            get => podSideTB;
            set => SetProperty(ref podSideTB, value);
        }

        public EUVPodSurfaceParameterBase PodTDI
        {
            get => podTDI;
            set => SetProperty(ref podSideLR, value);
        }
        public EUVPodSurfaceParameterBase PodStacking
        {
            get => podStacking;
            set => SetProperty(ref podStacking, value);
        }

        #endregion

    }

    [Serializable]
    public class SurfaceParam : ObservableObject
    {
        #region [Parameter]
        bool isEnable;
        string defectName;
        uint pitLevel, levelMin, levelMax, pitSize, sizeMax, sizeMin, defectCode;
        private DiffFilterMethod diffFilter = DiffFilterMethod.Average;

        #endregion

        public SurfaceParam()
        { }
        #region [Getter/Setter]
 
        public string DefectName
        {
            get => defectName;
            set => SetProperty(ref defectName, value);
        }
        public uint DefectCode
        {
            get => defectCode;
            set => SetProperty(ref defectCode, value);
        }
        public bool IsEnable
        {
            get => isEnable;
            set => SetProperty(ref isEnable, value);
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
            get => diffFilter;
            set
            {
                SetProperty(ref diffFilter, value);
            }
        }

        #endregion
    }

    [Serializable]
    public class EUVPodSurfaceParameterBase : ObservableObject, IMaskInspection //InspectionParameter
    {
        SurfaceParam brightParam, darkParam;
        bool isEnablebrignt,isEnabledark;

        #region Property
        public bool IsEnableBright
        {
            get => isEnablebrignt;
            set => SetProperty(ref isEnablebrignt, value);
        }
        public bool IsEnableDark
        {
            get => isEnabledark;
            set => SetProperty(ref isEnabledark, value);
        }
        public SurfaceParam BrightParam
        {
            get => brightParam;
            set => SetProperty(ref brightParam, value);
        }
        public SurfaceParam DarkParam
        {
            get => darkParam;
            set => SetProperty(ref darkParam, value);
        }

        public EUVPodSurfaceParameterBase(int MaskIndex)
        {
            DarkParam = new SurfaceParam();
            BrightParam = new SurfaceParam();
            this.MaskIndex = MaskIndex;
        }
        public EUVPodSurfaceParameterBase(){ }
        public EUVPodSurfaceParameterBase(EUVPodSurfaceParameterBase param)
        {
            isEnablebrignt = param.isEnablebrignt;
            isEnabledark = param.isEnabledark;
            brightParam = param.brightParam;
            darkParam = param.darkParam;
            MaskIndex = param.MaskIndex;
        }
        public int MaskIndex { get; set; }

        #endregion
    }
}
