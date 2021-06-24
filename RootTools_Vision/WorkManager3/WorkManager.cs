﻿using RootTools;
using RootTools.Database;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools_Vision.WorkManager3
{
    /// <summary>
    /// 이 코드는 수정할 일이 없어야함(버그 제외)
    /// </summary>
    public class WorkManager
    {
        #region [Attributes]
        private SharedBufferInfo sharedBuffer = new SharedBufferInfo();
        private CameraInfo cameraInfo = new CameraInfo();
        private RecipeBase recipe;

        private WorkPipeLine pipeLine;

        private int threadNum;

        private ConcurrentQueue<Workplace> currentWorkplaceQueue;
        #endregion


        #region [Properties]
        public SharedBufferInfo SharedBuffer
        {
            get => this.sharedBuffer;
        }
        #endregion

        #region [Event]
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


        public WorkManager()
        {
            pipeLine = new WorkPipeLine(1);

            this.threadNum = 1;
            WorkEventManager.PositionDone += PositionDone_Callback;

            WorkEventManager.InspectionStart += InspectionStart_Callback;
            WorkEventManager.InspectionDone += InspectionDone_Callback;

            WorkEventManager.ProcessDefectDone += ProcessDefectDone_Callback;

            WorkEventManager.ProcessDefectWaferStart += ProcessDefectWaferStart_Callback;
            WorkEventManager.IntegratedProcessDefectDone += IntegratedProcessDefectDone_Callback;

            WorkEventManager.ProcessMeasurementDone += ProcessMeasurementDone_Callback;

            WorkEventManager.WorkplaceStateChanged += WorkplaceStateChanged_Callback;
        }

        public WorkManager(int inspectionThreadNum)
        {
            pipeLine = new WorkPipeLine(inspectionThreadNum);

            this.threadNum = inspectionThreadNum;

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

        public void SetCameraInfo(CameraInfo cameraInfo)
        {
            this.cameraInfo = cameraInfo;
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


        private bool block = false;

        public async void Start(bool inspOnly = true, Lotinfo lotInfo = null)
        {
            if (block) return;

            block = true;

            if (this.sharedBuffer.PtrR_GRAY == IntPtr.Zero)
            {
                throw new ArgumentException("SharedBuffer가 초기화되지 않았습니다.");
            }

            WorkBundle works = new WorkBundle();
            // 레시피 기반으로 work/workplace 생성
            if(inspOnly == false)
            {
                works.Add(new Snap());
                
            }
            WorkBundle temp = RecipeToWorkConverter.Convert(this.recipe);
            foreach(WorkBase work in temp)
            {
                works.Add(work);
            }

            this.currentWorkplaceQueue = 
                RecipeToWorkplaceConverter.ConvertToQueue(this.recipe, this.sharedBuffer, this.cameraInfo);

            if(pipeLine.Stop() == false)
            {
                //this.pipeLine = new WorkPipeLine(this.threadNum);

                TempLogger.Write("Worker", "PipeLine Initialize");
            }

            this.pipeLine = new WorkPipeLine(this.threadNum);

            if (lotInfo == null)
                DatabaseManager.Instance.SetLotinfo(DateTime.Now, DateTime.Now, Path.GetFileName(this.recipe.RecipePath));
            else
                DatabaseManager.Instance.SetLotinfo(lotInfo);

            pipeLine.Start(
                this.currentWorkplaceQueue,
                works
                );

            WorkEventManager.OnInspectionStart(new object(), new InspectionStartArgs());

            await Task.Delay(1000);

            block = false;
        }


        public void CheckSnapDone(Rect snapArea)
        {
            if (this.currentWorkplaceQueue == null || this.currentWorkplaceQueue.Count == 0) return;

            foreach (Workplace wp in this.currentWorkplaceQueue)
            {
                if (wp.WorkState >= WORK_TYPE.SNAP) continue;


                wp.CheckSnapDone_Line(new CRect(0, 0, (int)snapArea.Right, (int)snapArea.Bottom));
            }
        }

        public void Stop()
        {
            pipeLine.Stop();

            GC.Collect();
        }

        public void Exit()
        {

        }

        public bool WaitWorkDone(ref bool isCanceled, int timeoutSecond = 60)
        {
            int sec = 0;
            while (pipeLine.CheckPipeDone() == false && sec < timeoutSecond && isCanceled == false)
            {
                Thread.Sleep(1000);
                sec++;
            }

            return true;
        }

        #endregion
    }
}
