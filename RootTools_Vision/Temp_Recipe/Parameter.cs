using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision.Temp_Recipe
{
    public class Parameter
    {
        List<IParameter> parameters;

        public Parameter()
        {
            parameters = new List<IParameter>();            

            AddParameter(new ParameterPosition());
        }



        private void AddParameter(IParameter parameter)
        {
            parameters.Add(parameter);
        }

        public IParameter GetParameter(Type type)
        {
            foreach (IParameter parameter in parameters)
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
            foreach (IParameter parameter in parameters)
            {
                parameter.Save();
            }
        }

        public void Load()
        {
            foreach (IParameter parameter in parameters)
            {
                parameter.Load();
            }
        }

    }
}
