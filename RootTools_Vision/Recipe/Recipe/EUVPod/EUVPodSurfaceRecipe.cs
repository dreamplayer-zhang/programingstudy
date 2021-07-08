using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools_Vision
{

    public class EUVPodSurfaceRecipe : RecipeItemBase
    {
        EUVPodSurfaceRecipeBase podStain;
        EUVPodSurfaceRecipeBase podSideLR;
        EUVPodSurfaceRecipeBase podSideTB;
        EUVPodSurfaceRecipeBase podTDI;
        EUVPodSurfaceRecipeBase podStacking;

        public EUVPodSurfaceRecipe()
        {
            podStain = new EUVPodSurfaceRecipeBase();
            podSideLR = new EUVPodSurfaceRecipeBase();
            podSideTB = new EUVPodSurfaceRecipeBase();
            podTDI = new EUVPodSurfaceRecipeBase();
            podStacking = new EUVPodSurfaceRecipeBase();
        }

        #region [Getter/Setter]
        public EUVPodSurfaceRecipeBase PodStain
        {
            get => podStain;
            set => SetProperty(ref podStain, value);
        }
        public EUVPodSurfaceRecipeBase PodSideLR
        {
            get => podSideLR;
            set => SetProperty(ref podSideLR, value);
        }
        public EUVPodSurfaceRecipeBase PodSideTB
        {
            get => podSideTB;
            set => SetProperty(ref podSideTB, value);
        }
        public EUVPodSurfaceRecipeBase PodTDI
        {
            get => podTDI;
            set => SetProperty(ref podTDI, value);
        }
        public EUVPodSurfaceRecipeBase PodStacking
        {
            get => podStacking;
            set => SetProperty(ref podStacking, value);
        }
        #endregion
        public override void Clear()
        {

        }

        public override bool Read(string recipePath)
        {
            return true;
        }

        public override bool Save(string recipePath)
        {
            return true;
        }
    }
    [Serializable]
    public class EUVPodSurfaceRecipeBase:ObservableObject
    {
        public EUVPodSurfaceRecipeBase() { }
    }
}
