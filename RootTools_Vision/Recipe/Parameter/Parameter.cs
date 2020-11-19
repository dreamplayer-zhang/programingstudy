using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace RootTools_Vision
{
    public class Parameter : IParameterData
    {
        [XmlArray("Parameter")]
        [XmlArrayItem("Origin", typeof(ParamData_Origin))]
        [XmlArrayItem("Position", typeof(ParamData_Position))]
        public List<IParameterData> parameters;

        public Parameter()
        {
            parameters = new List<IParameterData>();

            AddParameter(new ParamData_Origin());
            AddParameter(new ParamData_Position());
        }

        private void AddParameter(IParameterData parameter)
        {
            parameters.Add(parameter);
        }

        public IParameterData GetParameter(Type type)
        {
            foreach (IParameterData parameter in parameters)
            {
                if (parameter.GetType() == type)
                {
                    return parameter;
                }
            }

            return null;
        }

        public void Save()
        {
            using (TextWriter tw = new StreamWriter(@"C:\Wind2\Wind2.xml"))
            {
                XmlSerializer xml = new XmlSerializer(this.GetType());
                xml.Serialize(tw, this);
            }
        }
    }
}
