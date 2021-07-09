using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools_Vision
{
    public class GlobalObjects
    {
        #region [Singleton]
        private GlobalObjects()
        {

        }

        private static Lazy<GlobalObjects> instance = new Lazy<GlobalObjects>(() => new GlobalObjects());

        public static GlobalObjects Instance
        {
            get
            {
                return instance.Value;
            }
        }
        #endregion


        #region [NamedObjects]
        Dictionary<string, object> objectDictionary = new Dictionary<string, object>();

        /// <summary>
        /// 같은 타입의 객체를 여러개 생성할 때 사용합니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_name"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public T RegisterNamed<T>(string _name, params object[] param)
        {
            foreach (string name in objectDictionary.Keys)
            {
                if (_name.ToLower() == name.ToLower())
                {
                    MessageBox.Show("이미 같은 이름의 객체가 있습니다");
                    return default(T);
                }
            }

            T obj = (T)Activator.CreateInstance(typeof(T), args: param);
            objectDictionary.Add(_name.ToLower(), obj);
            return obj;
        }

        public T GetNamed<T>(string name)
        {
            if (objectDictionary.ContainsKey(name.ToLower()) == true)
            {
                return (T)objectDictionary[name.ToLower()];
            }
            else
            {
                MessageBox.Show("객체가 등록되지 않았습니다.\nRegister 메서드를 통해 등록하세요.\n"+ name + "(type:"+ typeof(T).Name+")");
                return default(T);
            }
        }

        public void DelistNamed(string name)
        {
            if (objectDictionary.ContainsKey(name.ToLower()) == true)
            {
                objectDictionary.Remove(name.ToLower());
            }
            else
            {
                //MessageBox.Show("객체가 등록되지 않았습니다.");
            }
        }
        #endregion

        /// <summary>
        /// 중복되는 타입의 객체가 없는 경우 사용합니다.
        /// </summary>
        #region [SingleObject]
        List<object> objectList = new List<object>();

        public T Register<T>(params object[] param)
        {
            foreach (object obj in objectList)
            {
                if (obj.GetType() == typeof(T))
                {
                    return default(T);
                }
            }

            T t = (T)Activator.CreateInstance(typeof(T), args: param);
            objectList.Add(t);
            return t;
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

            //MessageBox.Show("객체가 존재하지 않습니다.");
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
            //throw new ArgumentException("객체가 등록되지 않았습니다.(Type :", typeof(T).ToString());
            //MessageBox.Show("객체가 등록되지 않았습니다.\n RegisterObject 메서드를 통해 등록하세요.");
            return default(T);
        }

        public void Clear()
        {
            this.objectDictionary.Clear();
            this.objectList.Clear();
        }
        #endregion
    }
}
