using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RootTools_Vision
{
    public class Tools
    {
        public static byte[] CovertImageToArray(Image img)
        {
            byte[] data;

            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

                data = new byte[ms.Length];

                data = ms.ToArray();
            }

            return data;
        }

        public static ObservableCollection<Type> GetInheritedClasses(Type MyType)
        {
            IEnumerable<Type> type = Assembly.GetAssembly(MyType).GetTypes().Where(TheType => TheType.IsClass && !TheType.IsAbstract && TheType.IsSubclassOf(MyType));
            //if you want the abstract classes drop the !TheType.IsAbstract but it is probably to instance so its a good idea to keep it.

            ObservableCollection<Type> typeList = new ObservableCollection<Type>();
            foreach(Type t in type)
            {
                typeList.Add(t);
            }

            return typeList;
        }

        public static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null) return null;

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }

        public static object ByteArrayToObject(byte[] byteArr)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(byteArr))
                {
                    stream.Position = 0;
                    BinaryFormatter bf = new BinaryFormatter();
                    return bf.Deserialize(stream);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return null;
            //if (byteArr == null) return null;

            //MemoryStream ms = new MemoryStream();
            //BinaryFormatter bf = new BinaryFormatter();
            //ms.Write(byteArr, 0, byteArr.Length);
            //ms.Seek(0, SeekOrigin.Begin);
            //object obj = (object)bf.Deserialize(ms);

            //return obj;
        }
    }
}
