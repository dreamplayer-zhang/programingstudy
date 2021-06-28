using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools_Vision.WorkManager3
{

    public delegate void WorkPipeLineDoneHandler();

    public class WorkPipeLine
    {
        public event WorkPipeLineDoneHandler WorkPipeLineDone;

        List<WorkPipe> pipes;

        WorkPipe pipeSnap;
        WorkPipe pipeAlignment;
        WorkPipe pipeInspection;
        WorkPipe pipeDefectProcess;
        WorkPipe pipeDefectProcessAll;

        CancellationTokenSource cts;

        private int inspectionThreadNum = 1;
        private bool bCopyBuffer = false;

        public WorkPipeLine(int inspectionThreadNum = 4, bool bCopyBuffer = false)
        {
            this.inspectionThreadNum = inspectionThreadNum;
            this.bCopyBuffer = bCopyBuffer;

            CreatePipeLines(inspectionThreadNum, bCopyBuffer);
        }


        public void Reset()
        {
            CreatePipeLines(this.inspectionThreadNum, this.bCopyBuffer);
        }

        private void CreatePipeLines(int inspectionThreadNum, bool bCopyBuffer)
        {
            pipes = new List<WorkPipe>();

            // PipeLine 생성
            
            pipeSnap = new WorkPipe(WORK_TYPE.SNAP, 1, false, false);
            pipeAlignment = new WorkPipe(WORK_TYPE.ALIGNMENT, 1, false, false);
            pipeInspection = new WorkPipe(WORK_TYPE.INSPECTION, inspectionThreadNum, false, bCopyBuffer);
            pipeDefectProcess = new WorkPipe(WORK_TYPE.DEFECTPROCESS, inspectionThreadNum, false, false);
            pipeDefectProcessAll = new WorkPipe(WORK_TYPE.DEFECTPROCESS_ALL, 1, true);

            pipeSnap.SetNextPipe(pipeAlignment);
            pipeAlignment.SetNextPipe(pipeInspection);
            pipeInspection.SetNextPipe(pipeDefectProcess);
            pipeDefectProcess.SetNextPipe(pipeDefectProcessAll);

            pipeSnap.WorkPipeDone += WorkPipeLineDone_Callback;
            pipeAlignment.WorkPipeDone += WorkPipeLineDone_Callback;
            pipeInspection.WorkPipeDone += WorkPipeLineDone_Callback;
            pipeDefectProcess.WorkPipeDone += WorkPipeLineDone_Callback;
            pipeDefectProcessAll.WorkPipeDone += WorkPipeLineDone_Callback;

            pipes.Add(pipeSnap);
            pipes.Add(pipeAlignment);
            pipes.Add(pipeInspection);
            pipes.Add(pipeDefectProcess);
            pipes.Add(pipeDefectProcessAll);
        }


        private void  WorkPipeLineDone_Callback()
        {
            if(CheckPipesReady() == true)
            {
                if(this.WorkPipeLineDone != null)
                {
                    WorkPipeLineDone();
                }
            }
        }

        private bool CheckPipesReady()
        {
            bool result = true;
            foreach (WorkPipe pipe in pipes)
            {
                if (!pipe.IsReady)
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        public bool CheckPipeDone()
        {
            return CheckPipesReady();
        }


        public bool Start(ConcurrentQueue<Workplace> queue, WorkBundle works)
        {
            // Thread 안전성을 위해서 반드시 Stop을 호출하고 시작하도록해야함

            if (!CheckPipesReady()) return false;

            //Stop(); //Cancellation Token 때문에 이걸로 Stop해야함

            foreach (WorkPipe pipe in pipes)
            {
                pipe.Initialize(works, queue.Count);
            }

            pipes[0].SetQueue(queue);

            cts = new CancellationTokenSource();
            bool result = true;
            foreach (WorkPipe pipe in pipes)
            {
                if(pipe.Start(cts) == false)
                {
                    result = false;
                }
            }

            if(result == false)
            {
                Stop();
                return true;
            }

            return true;
        }

        public bool Stop()
        {
            bool result = true;
            if (cts != null)
            {
                //cts.Cancel();
                
                foreach (WorkPipe pipe in pipes)
                {
                    if(!pipe.TryStop()) result = false;

                    //pipe.Abort();
                }
            }

            if (result == false) // 간헐적으로 Stop Fail하는 경우 있음... 
            {
                foreach (WorkPipe pipe in pipes)
                {
                    pipe.Exit();
                }
                //MessageBox.Show("Fail Stop");
            }
                

            return result;
        }
    }
}
