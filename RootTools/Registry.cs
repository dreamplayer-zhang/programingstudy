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

        public Registry(bool bIncludeModel, params string[] strGroups)
        {
            // bIncludeModel : strGroups[0]에 Model Name 포함 유무.
            if (strGroups.Length < 1)
                return;

            string sModel = (bIncludeModel == true) ? strGroups[0] : EQ.m_sModel;

            m_reg = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software").CreateSubKey(sModel);
            foreach(string strGroup in strGroups)
            {
                m_reg = m_reg.CreateSubKey(strGroup);
            }
        }

        public void Write(string sSub, object obj)
        {
            if (m_reg == null || obj == null) return;
            m_reg.SetValue(sSub, obj);
        }

        public dynamic Read(string sSub, dynamic valueDefault)
        {
            if (m_reg == null) return valueDefault;
            dynamic value = m_reg.GetValue(sSub);
            if (value == null) return valueDefault; 
            Type type = valueDefault.GetType();
            try
            {
                if (type == typeof(bool)) return (value.ToString() == true.ToString());
                if (type == typeof(int)) return Convert.ToInt32(value.ToString());
                if (type == typeof(long)) return Convert.ToInt64(value.ToString());
                if (type == typeof(double)) return Convert.ToDouble(value.ToString());
                if (type == typeof(string)) return value.ToString();
                if (type == typeof(CPoint)) return new CPoint(value.ToString(), null);
                if (type == typeof(RPoint)) return new RPoint(value.ToString(), null);
                return valueDefault;
            }
            catch (Exception) { return valueDefault; }
        }
    }

}
