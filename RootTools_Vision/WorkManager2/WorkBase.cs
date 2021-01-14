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
        protected Recipe recipe;
        protected Workplace currentWorkplace;
        protected WorkplaceBundle workplaceBundle;

        protected byte[] workplaceBufferR_GRAY;
        protected byte[] workplaceBufferG;
        protected byte[] workplaceBufferB;


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


        public void SetRecipe(Recipe recipe)
        {
            this.recipe = recipe;
        }

        public void SetWorkplace(Workplace workplace)
        {
            this.currentWorkplace = workplace;
        }

        public void SetWorkplaceBundle(WorkplaceBundle workplaceBundle)
        {
            this.workplaceBundle = workplaceBundle;
        }

        public void SetWorkplaceBuffer(byte[] bufferR_GRAY, byte[] bufferG, byte[] bufferB)
        {
            this.workplaceBufferR_GRAY = bufferR_GRAY;
            this.workplaceBufferG = bufferG;
            this.workplaceBufferB = bufferB;
        }

        public byte[] GetWorkplaceBuffer(IMAGE_CHANNEL channel)
        {
            switch (channel)
            {
                case IMAGE_CHANNEL.R_GRAY:
                    return this.workplaceBufferR_GRAY;
                case IMAGE_CHANNEL.G:
                    return this.workplaceBufferG;
                case IMAGE_CHANNEL.B:
                    return this.workplaceBufferB;

            }
            return this.workplaceBufferR_GRAY;
        }


        /// <summary>
        /// 멤버 중에 값이 아닌 참조 타입에 객체가 있으면 직접 구현해줘야함
        /// </summary>
        /// <returns></returns>
        public virtual WorkBase Clone()
        {
            return (WorkBase)this.MemberwiseClone();
        }

        // Virtual
        public bool DoPrework()
        {
            // Prework 작업이 필요한 경우가 있을 때만 구현하고,
            // 없을시 pass
            this.IsPreworkDone = Preparation();

            return this.IsPreworkDone;
        }

        /// <summary>
        /// 메인작업(execution에서 실행되는 작업)을 수행하기 전에 준비해야할 작업을 구현합니다.
        /// </summary>
        /// <returns>완료 true, 미완료 false</returns>

        protected abstract bool Preparation();

        public bool DoWork()
        {
            this.IsWorkDone = this.Execution(); // 이거 반드시 true로 바꿔줘야 다음 검사 진행
            return this.IsWorkDone;
        }

        /// <summary>
        /// 실제로 수행할 메인작업을 구현합니다.
        /// </summary>
        /// <returns>완료 true, 미완료 false</returns>
        protected abstract bool Execution();
        

        public WorkBase CreateInstnace(WorkBase work)
        {
            return Activator.CreateInstance(work.GetType()) as WorkBase;
        }

        public void Reset()
        {
            this.IsPreworkDone = false;
            this.IsWorkDone = false;
        }
    }
}
