using RootTools.Database;
using RootTools_Vision.WorkManager3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision.WorkManager4
{
    public class WorkManager : IWorkManager
    {
        #region [Fields]
        private SharedBufferInfo sharedBuffer = new SharedBufferInfo();
        private CameraInfo cameraInfo = new CameraInfo();
        private RecipeBase recipe;

        private int threadNum;
        private bool useCopyBuffer;

        private bool startLock = false;
        #endregion

        #region [Properties]

        public SharedBufferInfo SharedBuffer
        {
            get => this.sharedBuffer;
        }

        public RecipeBase Recipe
        {
            get => this.recipe;
        }
        #endregion

        public void SetCameraInfo(CameraInfo cameraInfo)
        {
            this.cameraInfo = cameraInfo;
        }

        public void SetRecipe(RecipeBase recipe)
        {
            this.recipe = recipe;
        }

        public void SetSharedBuffer(SharedBufferInfo bufferInfo)
        {
            this.sharedBuffer = bufferInfo;
        }

        public void Start(Lotinfo lotInfo = null)
        {
            if (this.startLock == true)
                return;

            this.startLock = true;


            WorkBundle works = CreateWorkBundle(false);

            // WorkplaceBundle


            if (lotInfo == null)
                DatabaseManager.Instance.SetLotinfo(DateTime.Now, DateTime.Now, Path.GetFileName(this.recipe.RecipePath));
            else
                DatabaseManager.Instance.SetLotinfo(lotInfo);

            this.startLock = false;
        }

        private WorkBundle CreateWorkBundle(bool withSnap)
        {
            WorkBundle works = new WorkBundle();
            // 레시피 기반으로 work/workplace 생성
            if (withSnap == false)
            {
                works.Add(new Snap());

            }
            WorkBundle temp = RecipeToWorkConverter.Convert(this.recipe);
            foreach (WorkBase work in temp)
            {
                works.Add(work);
            }

            return works;
        }

        public void StartWithSnap(Lotinfo lotInfo = null)
        {
            
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public bool WaitWorkDone(ref bool isCanceled, int timeoutSecound = 60)
        {
            throw new NotImplementedException();
        }

        public bool WaitWorkDone(int timeoutSecound = 60)
        {
            throw new NotImplementedException();
        }
    }
}
