using System;
using Microsoft.Win32;

namespace RootTools
{
    public class Registry
    {
        RegistryKey m_reg = null;

        public Registry(string sGroup, string sModel = "")
        {
            if (sModel == "") sModel = EQ.m_sModel;
            m_reg = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software").CreateSubKey(sModel).CreateSubKey(sGroup);
        }

        public void Write(string sSub, object obj)
        {
            if (m_reg == null) return;
            m_reg.SetValue(sSub, obj);
        }

        public dynamic Read(string sSub, dynamic valueDefault)
        {
            if (m_reg == null) return valueDefault;
            dynamic value = m_reg.GetValue(sSub);
            if (value == null) return valueDefault; 
            Type type = valueDefault.GetType();
            if (type == typeof(bool)) return (value.ToString() == true.ToString());
            if (type == typeof(int)) return Convert.ToInt32(value.ToString());
            if (type == typeof(long)) return Convert.ToInt64(value.ToString());
            if (type == typeof(double)) return Convert.ToDouble(value.ToString());
            if (type == typeof(string)) return value.ToString();
            if (type == typeof(CPoint)) return new CPoint(value.ToString(), null);
            if (type == typeof(RPoint)) return new RPoint(value.ToString(), null);
            return valueDefault; 
        }
    }

}
