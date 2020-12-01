using System;
using System.Collections;
using System.IO;
using System.Xml.Serialization;

namespace Root_CAMELLIA
{
    static class GeneralFunction
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
    }
}
