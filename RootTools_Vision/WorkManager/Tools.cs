using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
    }
}
