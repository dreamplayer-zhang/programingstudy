using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class Parameter : IParameterData
    {
        List<IParameterData> parameters;

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
    }
}
