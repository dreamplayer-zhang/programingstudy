using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools_Vision;


namespace Root_WIND2
{
    public class WIND2_InspectionManager : WorkFactory
    {

        public WIND2_InspectionManager(IntPtr _sharedBuffer, int _width, int _height)
        {
            this.m_ptrSharedBuffer = _sharedBuffer;
            this.m_SharedBufferWidth = _width;
            this.m_SharedBufferHeight = _height;
        }

        private IntPtr m_ptrSharedBuffer;
        private int m_SharedBufferWidth;
        private int m_SharedBufferHeight;

        private Recipe m_Recipe;

        public Recipe Recipe { get => m_Recipe; set => m_Recipe = value; }
        public IntPtr PtrSharedBuffer { get => m_ptrSharedBuffer; set => m_ptrSharedBuffer = value; }
        public int SharedBufferWidth { get => m_SharedBufferWidth; set => m_SharedBufferWidth = value; }
        public int SharedBufferHeight { get => m_SharedBufferHeight; set => m_SharedBufferHeight = value; }

        public void CreateInspecion(/*WaferMapInfo*/)
        {
            int InspectionNumber = 0;

            int nMapSize = 14;
            byte[] waferMapInfo = new byte[]
            {
                0,0,0,0,0,1,1,1,1,0,0,0,0,0,//1
                0,0,0,1,1,1,1,1,1,1,1,0,0,0,//2
                0,0,1,1,1,1,1,1,1,1,1,1,0,0,//3
                0,1,1,1,1,1,1,1,1,1,1,1,1,0,//4
                0,1,1,1,1,1,1,1,1,1,1,1,1,0,//5
                1,1,1,1,1,1,1,1,1,1,1,1,1,1,//6
                1,1,1,1,1,1,1,1,1,1,1,1,1,1,//7
                1,1,1,1,1,1,1,1,1,1,1,1,1,1,//8
                1,1,1,1,1,1,1,1,1,1,1,1,1,1,//9
                0,1,1,1,1,1,1,1,1,1,1,1,1,0,//10
                0,1,1,1,1,1,1,1,1,1,1,1,1,0,//11
                0,0,1,1,1,1,1,1,1,1,1,1,0,0,//12
                0,0,0,1,1,1,1,1,1,1,1,0,0,0,//13
                0,0,0,0,0,1,1,1,1,0,0,0,0,0,//14
            };

            WaferMapInfo mapInfo = new WaferMapInfo(nMapSize, nMapSize, waferMapInfo);



            WorkBundle works = new WorkBundle();

            Position position = new Position();

            position.SetData(m_Recipe.GetRecipeData(), m_Recipe.GetParameter());

            works.Add(position);

            WorkplaceBundle workplaces = WorkplaceBundle.CreateWaferMap(mapInfo);

            workplaces.SetSharedBuffer(this.PtrSharedBuffer, this.SharedBufferWidth, this.SharedBufferHeight);


            this.SetBundles(works, workplaces);
        }


        protected override void InitWorkManager()
        {
            this.Add(new WorkManager("Position", RootTools_Vision.UserTypes.WORK_TYPE.PREPARISON, WORKPLACE_STATE.READY, WORKPLACE_STATE.NONE));
            this.Add(new WorkManager("Inspection", RootTools_Vision.UserTypes.WORK_TYPE.MAINWORK, WORKPLACE_STATE.INSPECTION, WORKPLACE_STATE.READY, 8));
            this.Add(new WorkManager("ProcessDefect", RootTools_Vision.UserTypes.WORK_TYPE.FINISHINGWORK, WORKPLACE_STATE.DEFECTPROCESS, WORKPLACE_STATE.INSPECTION));
        }

        public new void Start()
        {
            if (this.Recipe == null) return;


            base.Start();
        }
    }
}
