using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RootTools_Vision
{
	public abstract class KlarfSettingItem /*: ISettingItem*/
	{
		//public void Load()
		//{
  //          Type types = GetType();
  //          PropertyInfo propertyInfo = types.GetProperty(types.GetProperties()[0].Name);
  //          object asf = propertyInfo.GetValue(this);
  //          asf = Load(asf);
  //          propertyInfo.SetValue(asf, this);
  //          }
  //      }

		public object Load()
        {
            // TO-DO 수정 필요
			object o = null;
			Type types = GetType();

            XmlSerializer xmlSerializer;
            StreamReader xmlReader;
            xmlSerializer = new XmlSerializer(types);
            FileInfo fi = new FileInfo(Constants.FilePath.KlarfSettingFilePath);
            if (fi.Exists)
            {
                xmlReader = new StreamReader(Constants.FilePath.KlarfSettingFilePath);
                o = xmlSerializer.Deserialize(xmlReader);
                xmlReader.Dispose();
            }

            return o;
        }

        public object Load(object o, string strFile)
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

        public object Load<T>(object o, Expression<Func<T>> memberExpression)
        {
            XmlSerializer xmlSerializer;
            StreamReader xmlReader;
            MemberExpression expressionBody = (MemberExpression)memberExpression.Body;
            xmlSerializer = new XmlSerializer(typeof(T));
            String Name = expressionBody.Member.Name;
            FileInfo fi = new FileInfo(Name);
            if (fi.Exists)
            {
                xmlReader = new StreamReader(Name);
                o = xmlSerializer.Deserialize(xmlReader);
                xmlReader.Dispose();
            }

            return o;
        }

        public void Save()
        {
            Type types = GetType();
            try
            {
                XmlSerializer xmlSerializer;
                StreamWriter xmlWriter;
                xmlSerializer = new XmlSerializer(types);
                xmlWriter = new StreamWriter(Constants.FilePath.KlarfSettingFilePath);
                xmlSerializer.Serialize(xmlWriter, this);
                xmlWriter.Dispose();
            }
            catch (Exception ex)
            {
            }
        }

        public void Save(object o, string strFile)
        {
            try
            {
                XmlSerializer xmlSerializer;
                StreamWriter xmlWriter;
                xmlSerializer = new XmlSerializer(o.GetType());
                xmlWriter = new StreamWriter(strFile);
                xmlSerializer.Serialize(xmlWriter, o);
                xmlWriter.Dispose();
            }
            catch (Exception ex)
            {
            }
        }

        public void Save<T>(object o, Expression<Func<T>> memberExpression)
        {
            XmlSerializer xmlSerializer;
            StreamWriter xmlWriter;
            MemberExpression expressionBody = (MemberExpression)memberExpression.Body;
            xmlSerializer = new XmlSerializer(typeof(T));

            String Name = expressionBody.Member.Name;
            xmlWriter = new StreamWriter(Name);
            xmlSerializer.Serialize(xmlWriter, o);
            xmlWriter.Dispose();
        }
    }
}
