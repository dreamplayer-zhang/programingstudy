using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RootTools_Vision
{
    [Serializable]
    // ParameterBase를 상속받는 Class를 추가할 경우
    // XmlSerialize를 위해서 XmlInclude Attribute를 사용해서 class 타입을 추가해야함
    [XmlInclude(typeof(OriginRecipe))]
    [XmlInclude(typeof(PositionRecipe))]
    [XmlInclude(typeof(SurfaceRecipe))]
    [XmlInclude(typeof(D2DRecipe))]
    [XmlInclude(typeof(BacksideRecipe))]
    [XmlInclude(typeof(MaskRecipe))]
    [XmlInclude(typeof(EdgeSurfaceRecipe))]
    [XmlInclude(typeof(EBRRecipe))]
    [XmlInclude(typeof(PBIRecipe))]
    [XmlInclude(typeof(VerticalWireRecipe))]
    [XmlInclude(typeof(FrontAlignRecipe))]
    [XmlInclude(typeof(FrontVRSAlignRecipe))]
    #region[EUVPod]
    [XmlInclude(typeof(EUVOriginRecipe))]
    [XmlInclude(typeof(EUVPodSurfaceRecipe))]
    [XmlInclude(typeof(EUVPositionRecipe))]
    [XmlInclude(typeof(StainRecipe))]
    [XmlInclude(typeof(SideRecipe))]
    [XmlInclude(typeof(LowResRecipe))]
    [XmlInclude(typeof(HighResRecipe))]
    #endregion
    public abstract class RecipeItemBase : ObservableObject, IComparable<RecipeItemBase>, IRecipe
    {
        public int CompareTo(RecipeItemBase other)
        {
            return 0;
        }

        public int CompareTo(IRecipe other)
        {
            return 0;
        }

        public abstract bool Read(string recipePath);

        public abstract bool Save(string recipePath);

        public abstract void Clear();
    }
}
