using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using RootTools;
using RootTools_Vision;

namespace RootTools_Vision
{
    // ParameterBase를 상속받는 Class를 추가할 경우
    // XmlSerialize를 위해서 XmlInclude Attribute를 사용해서 class 타입을 추가해야함
    [XmlInclude(typeof(D2DParameter))]
    [XmlInclude(typeof(SurfaceParameter))]
    [XmlInclude(typeof(PositionParameter))]
    //[XmlType(TypeName = "Parameter")]
    public class ParameterBase : ObservableObject, IComparable<ParameterBase>, IRecipe
    {
        public ParameterBase() { }


        private readonly string name;


        public string Name
        { 
            get => name;
        }

        protected ParameterBase(string displayName)
        {
            this.name = displayName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns>class의 name 변수가 같으면 0, 다른면 -1을 리턴한다.</returns>
        public int CompareTo(ParameterBase other)
        {
            if(this.name == other.Name)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }

        public static ObservableCollection<Type> GetChildClassType()
        {
            return Tools.GetInheritedClasses(typeof(ParameterBase));
        }

        public static ObservableCollection<ParameterBase> GetChildClass()
        {
            return Tools.GetEnumerableOfType<ParameterBase>();
        }

        public static T GetChildInstance<T>()
        {
            return Activator.CreateInstance<T>();
        }

        public static ParameterBase CreateChildInstance(ParameterBase param)
        {
            return Activator.CreateInstance(param.GetType()) as ParameterBase;
        }

        public bool Save(string recipePath)
        {
            return true;
        }

        public bool Read(string recipePath)
        {
            return true;
        }

        public int CompareTo(IRecipe other)
        {
            return 0;
        }
    }
}
