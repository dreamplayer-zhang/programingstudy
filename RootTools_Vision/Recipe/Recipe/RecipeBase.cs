using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RootTools_Vision
{
    // ParameterBase를 상속받는 Class를 추가할 경우
    // XmlSerialize를 위해서 XmlInclude Attribute를 사용해서 class 타입을 추가해야함
    [XmlInclude(typeof(OriginRecipe))]
    [XmlInclude(typeof(PositionRecipe))]
    [XmlInclude(typeof(SurfaceRecipe))]
    [XmlInclude(typeof(D2DRecipe))]
    [XmlInclude(typeof(BacksideRecipe))]
    [XmlInclude(typeof(MaskRecipe))]
    [XmlInclude(typeof(EdgeSurfaceRecipe))]
    public abstract class RecipeBase : ObservableObject, IComparable<RecipeBase>, IRecipe
    {
        public int CompareTo(RecipeBase other)
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
