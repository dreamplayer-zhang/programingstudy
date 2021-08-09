using RootTools.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision.WorkManager4
{
    interface IWorkManager
    {
        void Start(Lotinfo lotInfo = null);
        void StartWithSnap(Lotinfo lotInfo = null);

        void Stop();

        void SetRecipe(RecipeBase recipe);

        void SetSharedBuffer(SharedBufferInfo bufferInfo);

        void SetCameraInfo(CameraInfo cameraInfo);

        bool WaitWorkDone(ref bool isCanceled, int timeoutSecound = 60);

        bool WaitWorkDone(int timeoutSecound = 60);
    }
}
