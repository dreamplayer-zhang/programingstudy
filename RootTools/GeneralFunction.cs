using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace RootTools
{
    public static class GeneralFunction
    {
        public static void Save(object o, string strFile)
        {
            XmlSerializer xmlSerializer;
            StreamWriter xmlWriter;
            xmlSerializer = new XmlSerializer(o.GetType());
            xmlWriter = new StreamWriter(strFile);
            xmlSerializer.Serialize(xmlWriter, o);
            xmlWriter.Dispose();
        }

        public static object Read(object o, string strFile)
        {
            XmlSerializer xmlSerializer;
            StreamReader xmlReader;
            xmlSerializer = new XmlSerializer(o.GetType());
            FileInfo fi = new FileInfo(strFile);
            if (fi.Exists)
            {
                xmlReader = new StreamReader(strFile);
                o = xmlSerializer.Deserialize(xmlReader);
                xmlReader.Dispose();
            }
            return o;
        }

        public static TList GetSelectedRandom<TList>(this TList list, int count, int nMin, int nMax) where TList : IList, new()
        {
            var listCopy = new TList();

            for (int i = nMin; i <= nMax; i++)
            {
                listCopy.Add(list[i]);
            }

            var r = new Random();
            var rList = new TList();
            while (count > 0 && listCopy.Count > 0)
            {
                var n = r.Next(0, listCopy.Count);
                var e = listCopy[n];
                rList.Add(e);
                listCopy.RemoveAt(n);
                count--;
            }
            return rList;
        }

        #region .ini file : wafer 들어오면 .ini 파일만들어서 관리
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public static void WriteINIFile(string section, string key, string value, string path)
        {
            WritePrivateProfileString(section, key, value, path);
        }

        public static string ReadINIFile(string section, string key, string path)
        {
            StringBuilder sb = new StringBuilder(255);
            GetPrivateProfileString(section, key, "", sb, sb.Capacity, path);

            return sb.ToString();
        }
        #endregion
    }
}
