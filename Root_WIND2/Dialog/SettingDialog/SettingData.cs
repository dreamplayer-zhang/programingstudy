using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2
{
    public abstract class SettingData : ISettingData
    {
        #region Ini
        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        [DllImport("kernel32")]
        public static extern int WritePrivateProfileString(string section, string key, string val, string filePath);
        #endregion
        public SettingData(string[] _treeViewPath)
        {
            treeViewPath = _treeViewPath;
        }

        protected readonly string[] treeViewPath;
        public string[] GetTreeViewPath()
        {
            return this.treeViewPath;
        }

        public void Load(object type)
        {
            Type types = type.GetType();
            string sectionName = "";
            for (int i = 0; i < treeViewPath.Length; i++)
            {
                if (i == treeViewPath.Length - 1)
                {
                    sectionName += treeViewPath[i];
                    break;
                }
                sectionName += treeViewPath[i] + ".";
            }

            for (int j = 0; j < types.GetProperties().Length; j++)
            {
                PropertyInfo propertyInfo = types.GetProperty(types.GetProperties()[j].Name);
                object asf = propertyInfo.GetValue(type);
                StringBuilder temp = new StringBuilder(255);
                GetPrivateProfileString(sectionName, propertyInfo.Name, asf.ToString(), temp, 255, @"D:\\test.ini");
                Type propType = propertyInfo.PropertyType;

                string tempVal = temp.ToString();

                dynamic val = null;
                if (propType == typeof(string))
                {
                    val = tempVal;
                }
                else if (propType == typeof(bool))
                {
                    val = bool.Parse(tempVal);
                }
                else if (propType == typeof(int))
                {
                    val = int.Parse(tempVal);
                }
                else if (propType == typeof(float))
                {
                    val = float.Parse(tempVal);
                }
                else if (propType == typeof(double))
                {
                    val = double.Parse(tempVal);
                }
                else if (propType.IsEnum)
                {
                    val = Enum.Parse(propType, tempVal);
                }
                propertyInfo.SetValue(type, val);
            }
        }

        public void Save()
        {
            Type types = this.GetType();
            string sectionName = "";
            for (int i = 0; i < treeViewPath.Length; i++)
            {
                if(i == treeViewPath.Length - 1)
                {
                    sectionName += treeViewPath[i];
                    break;
                }
                sectionName += treeViewPath[i] + ".";
            }

            { 
                for(int j = 0; j < types.GetProperties().Length; j++)
                {
                    PropertyInfo propertyInfo = types.GetProperty(types.GetProperties()[j].Name);
                    object val = propertyInfo.GetValue(this);
                    Type type = propertyInfo.PropertyType;
                    if (type.IsEnum){
                        val = (int)val;
                    }
                    WritePrivateProfileString(sectionName, propertyInfo.Name, val.ToString(), @"D:\\test.ini");
                }
            }
        }

    }
}
