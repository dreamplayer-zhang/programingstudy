using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools_Vision
{
    /// <summary>
    /// 일반 class를 싱글톤으로 관리하기 위하여 사용합니다.
    /// </summary>
    public class SingleObject
    {
        #region [Singleton]
        private SingleObject()
        {

        }

        private static Lazy<SingleObject> instance = new Lazy<SingleObject>(() => new SingleObject());

        public static SingleObject Instance
        {
            get
            {
                return instance.Value;
            }
        }
        #endregion


        List<object> objectList = new List<object>();

        #region [Method]
        public void Register<T>()
        {
            foreach (object obj in objectList)
            {
                if (obj.GetType() == typeof(T))
                {
                    MessageBox.Show("이미 같은 타입의 객체가 생성되었습니다.");
                    return;
                }
            }

            objectList.Add(Activator.CreateInstance<T>());
        }

        public void Register<T>(params object[] param)
        {
            foreach (object obj in objectList)
            {
                if (obj.GetType() == typeof(T))
                {
                    MessageBox.Show("이미 같은 타입의 객체가 생성되었습니다.");
                    return;
                }
            }

            objectList.Add(Activator.CreateInstance(typeof(T), args: param));
        }

        public void Delist<T>()
        {
            foreach (object obj in objectList)
            {
                if (obj.GetType() == typeof(T))
                {
                    objectList.Remove(obj);
                    return;
                }
            }

            MessageBox.Show("객체가 존재하지 않습니다.");
        }

        public T Get<T>()
        {
            foreach (object obj in objectList)
            {
                if (obj.GetType() == typeof(T))
                {
                    return (T)obj;
                }
            }

            MessageBox.Show("객체가 등록되지 않았습니다.\n RegisterObject 메서드를 통해 등록하세요.");
            return (T)new object();
        }

        #endregion
    }
}
