using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    #region [Parameter]
    #endregion
    #region [Getter Setter]
    #endregion
    public class EUVPodSurfaceRecipe : RecipeItemBase
    {
        EUVPodSurfaceRecipeBase podStain;
        EUVPodSurfaceRecipeBase podSide;
        EUVPodSurfaceRecipeBase podTDI;
        EUVPodSurfaceRecipeBase podStacking;

        public EUVPodSurfaceRecipe()
        {
            podStain = new EUVPodSurfaceRecipeBase();
            podSide = new EUVPodSurfaceRecipeBase();
            podTDI = new EUVPodSurfaceRecipeBase();
            podStacking = new EUVPodSurfaceRecipeBase();
        }

        #region [Getter/Setter]
        public EUVPodSurfaceRecipeBase PodStain
        {
            get => podStain;
            set => SetProperty(ref podStain, value);
        }
        public EUVPodSurfaceRecipeBase PodSide
        {
            get => podSide;
            set => SetProperty(ref podSide, value);
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

    public class EUVPodSurfaceRecipeBase:ObservableObject
    {
    }
}
