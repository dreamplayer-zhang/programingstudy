using RootTools;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public static class ReflectiveEnumerator
    {
        static ReflectiveEnumerator() { }
        public static IEnumerable<T> GetEnumerableOfType<T>(params object[] constructorArgs) where T : class, IComparable<T>
        {
            List<T> objects = new List<T>();
            foreach (Type type in
                Assembly.GetAssembly(typeof(T)).GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
            {
                objects.Add((T)Activator.CreateInstance(type, constructorArgs));
            }
            objects.Sort();
            return objects;
        }
    }

    public abstract class WorkBase : ObservableObject
    {

        public delegate void ChangeMode(object e);
        public event ChangeMode changeMode;

        protected string m_sName;
        public string p_sName 
        {
            get { return m_sName; }
            set { m_sName = value; }
        }

        private bool bPreworkDone;

        public bool IsPreworkDone
        {
            get { return bPreworkDone; }
            set { bPreworkDone = value; }
        }

        private bool bWorkDone;
        public bool IsWorkDone
        {
            get { return bWorkDone; }
            set { bWorkDone = value; }
        }


        public abstract WORK_TYPE Type { get; }



        public abstract void SetWorkplace(Workplace workplace);

        public abstract void SetWorkplaceBundle(WorkplaceBundle workplace);

        public abstract void SetData(IRecipeData _recipeData, IParameterData _parameterData);

        public abstract WorkBase Clone();

        // Virtual
        public virtual bool DoPrework()
        {
            // Prework 작업이 필요한 경우가 있을 때만 구현하고,
            // 없을시 pass
            this.IsPreworkDone = true;

            return true;
        }

        public virtual void DoWork()
        {
            this.IsWorkDone = true; // 이거 반드시 true로 바꿔줘야 다음 검사 진행
        }
    }
}
