using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2
{
    public abstract class SettingItem : ISettingItem
    {
        #region Ini
        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        [DllImport("kernel32")]
        public static extern int WritePrivateProfileString(string section, string key, string val, string filePath);
        #endregion
        public SettingItem(string[] _treePath)
        {
            treePath = _treePath;
        }

        protected readonly string[] treePath;
        public string[] GetTreePath()
        {
            return this.treePath;
        }

        public void Load()
        {
            Type types = GetType();
            string sectionName = "";
            for (int i = 0; i < treePath.Length; i++)
            {
                if (i == treePath.Length - 1)
                {
                    sectionName += treePath[i];
                    break;
                }
                sectionName += treePath[i] + ".";
            }

            for (int j = 0; j < types.GetProperties().Length; j++)
            {
                PropertyInfo propertyInfo = types.GetProperty(types.GetProperties()[j].Name);
                object asf = propertyInfo.GetValue(this);
                StringBuilder temp = new StringBuilder(255);
                GetPrivateProfileString(sectionName, propertyInfo.Name, asf.ToString(), temp, 255, Constants.FilePath.SettingFilePath);
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
                propertyInfo.SetValue(this, val);
            }
        }

        public void Save()
        {
            Type types = this.GetType();
            string sectionName = "";
            for (int i = 0; i < treePath.Length; i++)
            {
                if(i == treePath.Length - 1)
                {
                    sectionName += treePath[i];
                    break;
                }
                sectionName += treePath[i] + ".";
            }


            for(int j = 0; j < types.GetProperties().Length; j++)
            {
                PropertyInfo propertyInfo = types.GetProperty(types.GetProperties()[j].Name);
                object val = propertyInfo.GetValue(this);
                Type type = propertyInfo.PropertyType;
                if (type.IsEnum){
                    val = (int)val;
                }

                WritePrivateProfileString(sectionName, propertyInfo.Name, val.ToString(), Constants.FilePath.SettingFilePath);
            }
        }
    }
}
