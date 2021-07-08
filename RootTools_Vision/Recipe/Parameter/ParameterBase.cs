using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using RootTools;
using RootTools_Vision;

namespace RootTools_Vision
{
    [Serializable]
    // ParameterBase를 상속받는 Class를 추가할 경우
    // XmlSerialize를 위해서 XmlInclude Attribute를 사용해서 class 타입을 추가해야함
    [XmlInclude(typeof(D2DParameter))]
    [XmlInclude(typeof(SurfaceParameter))]
    [XmlInclude(typeof(PositionParameter))]
    [XmlInclude(typeof(EdgeSurfaceParameter))]
    [XmlInclude(typeof(BacksideSurfaceParameter))]
    [XmlInclude(typeof(EBRParameter))]
    [XmlInclude(typeof(PBIParameter))]
    [XmlInclude(typeof(VerticalWireParameter))]
    [XmlInclude(typeof(ProcessDefectEdgeParameter))]
    [XmlInclude(typeof(ProcessDefectParameter))]
    [XmlInclude(typeof(ProcessDefectWaferParameter))]
    [XmlInclude(typeof(ProcessDefectBacksideParameter))]
    [XmlInclude(typeof(ProcessMeasurementParameter))]
    [XmlInclude(typeof(EUVPodSurfaceParameter))]

    //[XmlType(TypeName = "Parameter")]
    public abstract class ParameterBase : ObservableObject, IComparable<ParameterBase>, IRecipe
    {
        public ParameterBase() { }

        private readonly string name;
        private readonly  Type inspectionType;

        public Type InspectionType
        {
            get => this.inspectionType;
        }

        public string Name
        {
            get => this.name;
        }

        protected ParameterBase(Type _inspectionType)
        {
            this.inspectionType = _inspectionType;
            this.name = _inspectionType.Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns>class의 name 변수가 같으면 0, 다른면 -1을 리턴한다.</returns>
        public int CompareTo(ParameterBase other)
        {
            if(this.InspectionType == other.InspectionType)
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

        public static ObservableCollection<ParameterBase> GetFrontSideClass()
        {
            ObservableCollection<ParameterBase> objects = new ObservableCollection<ParameterBase>();
            foreach (Type type in
                Assembly.GetAssembly(typeof(ParameterBase)).GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(ParameterBase))
                && (myType.GetInterface("IFrontsideInspection") != null || myType.GetInterface("IFrontsideMeasurement") != null)))
            {
                objects.Add((ParameterBase)Activator.CreateInstance(type));
            }

            return objects;
        }

        public static ObservableCollection<ParameterBase> GetParameters(string inspection)
        {
            ObservableCollection<ParameterBase> objects = new ObservableCollection<ParameterBase>();
            foreach (Type type in
                Assembly.GetAssembly(typeof(ParameterBase)).GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(ParameterBase))
                && myType.GetInterface(inspection) != null))
            {
                objects.Add((ParameterBase)Activator.CreateInstance(type));
            }

            return objects;
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

        public void CopyTo(ParameterBase other)
        {
            other = (ParameterBase)this.MemberwiseClone();
        }

        // string과 같이 Value타입이 아니라 refence 타입인 경우(new로 생성되는 경우 clone 함수 작성시 따로 생성해야합니다)
        public virtual ParameterBase Clone()
        {
            return (ParameterBase)this.MemberwiseClone();
        }

    }
}
