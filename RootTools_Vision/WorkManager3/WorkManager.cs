using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RootTools_Vision.WorkManager3
{
    /// <summary>
    /// 이 코드는 수정할 일이 없어야함(버그 제외)
    /// </summary>
    public class WorkManager
    {
        #region [Attributes]
        private readonly SharedBufferInfo sharedBuffer;
        private RecipeBase recipe;

        private WorkPipeLine pipeLine;

        #endregion

        public WorkManager(SharedBufferInfo _sharedBuffer)
        {
            this.sharedBuffer = _sharedBuffer;

            ThreadPool.SetMinThreads(50, 10);

            pipeLine = new WorkPipeLine();
        }

        #region [Method]

        public bool OpenRecipe(string recipePath)
        {
            this.recipe.Read(recipePath);

            return true;
        }
        
        public void SetRecipe(RecipeBase recipe)
        {
            this.recipe = recipe;
        }



        public void Start()
        {
            // 레시피 기반으로 work/workplace 생성

            pipeLine.Start(
                RecipeToWorkplaceConverter.ConvertToQueue(this.recipe.WaferMap, this.recipe.GetItem<OriginRecipe>(), this.sharedBuffer), 
                RecipeToWorkConverter.Convert(this.recipe)
                );
        }

        public void Stop()
        {
            pipeLine.Stop();
        }

        public void Exit()
        {

        }
        #endregion
    }
}
