﻿using System;
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
        private SharedBufferInfo sharedBuffer;
        private RecipeBase recipe;

        private WorkPipeLine pipeLine;
        #endregion

        #region [Event]
        public event WorkManagerAllWorkDoneEvent AllWorkDone;

        public event EventHandler<PositionDoneEventArgs> PositionDone;

        public event EventHandler<InspectionStartArgs> InspectionStart;

        public event EventHandler<InspectionDoneEventArgs> InspectionDone;

        public event EventHandler<IntegratedProcessDefectDoneEventArgs> IntegratedProcessDefectDone;

        public event EventHandler<ProcessDefectWaferStartEventArgs> ProcessDefectWaferStart;

        public event EventHandler<ProcessDefectDoneEventArgs> ProcessDefectDone;

        public event EventHandler<ProcessMeasurementDoneEventArgs> ProcessMeasurementDone;

        public event EventHandler<WorkplaceStateChangedEventArgs> WorkplaceStateChanged;
        #endregion

        #region [Event Handler]
        private void PositionDone_Callback(object obj, PositionDoneEventArgs args)
        {
            this.PositionDone?.Invoke(obj, args);
        }

        private void InspectionStart_Callback(object obj, InspectionStartArgs args)
        {
            this.InspectionStart?.Invoke(obj, args);
        }

        private void InspectionDone_Callback(object obj, InspectionDoneEventArgs args)
        {
            this.InspectionDone?.Invoke(obj, args);
        }

        private void ProcessDefectDone_Callback(object obj, ProcessDefectDoneEventArgs args)
        {
            this.ProcessDefectDone?.Invoke(obj, args);
        }

        private void ProcessDefectWaferStart_Callback(object obj, ProcessDefectWaferStartEventArgs args)
        {
            this.ProcessDefectWaferStart?.Invoke(obj, args);
        }

        private void IntegratedProcessDefectDone_Callback(object obj, IntegratedProcessDefectDoneEventArgs args)
        {
            this.IntegratedProcessDefectDone?.Invoke(obj, args);
        }

        private void ProcessMeasurementDone_Callback(object obj, ProcessMeasurementDoneEventArgs args)
        {
            this.ProcessMeasurementDone?.Invoke(obj, args);
        }

        private void WorkplaceStateChanged_Callback(object obj, WorkplaceStateChangedEventArgs args)
        {
            this.WorkplaceStateChanged?.Invoke(obj, args);
        }
        #endregion

        public WorkManager(int inspectionThreadNum = 4)
        {
            pipeLine = new WorkPipeLine(inspectionThreadNum);

            WorkEventManager.PositionDone += PositionDone_Callback;

            WorkEventManager.InspectionStart += InspectionStart_Callback;
            WorkEventManager.InspectionDone += InspectionDone_Callback;

            WorkEventManager.ProcessDefectDone += ProcessDefectDone_Callback;

            WorkEventManager.ProcessDefectWaferStart += ProcessDefectWaferStart_Callback;
            WorkEventManager.IntegratedProcessDefectDone += IntegratedProcessDefectDone_Callback;

            WorkEventManager.ProcessMeasurementDone += ProcessMeasurementDone_Callback;

            WorkEventManager.WorkplaceStateChanged += WorkplaceStateChanged_Callback;
        }





        public void SetRecipe(RecipeBase recipe)
        {
            this.recipe = recipe;
        }

        public void SetSharedBuffer(SharedBufferInfo bufferInfo)
        {
            this.sharedBuffer = bufferInfo;
        }

        #region [Method]

        public bool OpenRecipe(string recipePath)
        {
            if (this.sharedBuffer.PtrR_GRAY == IntPtr.Zero)
            {
                throw new ArgumentException("SharedBuffer가 초기화되지 않았습니다.");
            }

            if (this.recipe == null)
            {
                throw new ArgumentException("Recipe가 초기화되지 않았습니다.");
            }

            this.recipe.Read(recipePath);

            return true;
        }

        public void Start(bool inspOnly = true)
        {
            if(this.sharedBuffer.PtrR_GRAY == IntPtr.Zero)
            {
                throw new ArgumentException("SharedBuffer가 초기화되지 않았습니다.");
            }

            WorkBundle works = new WorkBundle();
            // 레시피 기반으로 work/workplace 생성
            if(inspOnly == false)
            {
                RecipeToWorkConverter.Convert(this.recipe);
            }

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

        public bool WaitWorkDone(ref bool isCanceled, int timeoutSecond = 60)
        {
            int sec = 0;
            while(pipeLine.CheckPipeDone() == false && sec < timeoutSecond && isCanceled == false)
            {
                Thread.Sleep(1000);
                sec++;
            }

            return true;
        }

        #endregion
    }
}