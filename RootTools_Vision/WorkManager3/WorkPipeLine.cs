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
    public class WorkPipeLine
    {
        List<WorkPipe> pipes;

        WorkPipe pipeSnap;
        WorkPipe pipeAlignment;
        WorkPipe pipeInspection;
        WorkPipe pipeDefectProcess;
        WorkPipe pipeDefectProcessAll;

        CancellationTokenSource cts;

        public WorkPipeLine(int inspectionThreadNum = 4)
        {
            CreatePIpeLines(inspectionThreadNum);
        }

        private void CreatePIpeLines(int inspectionThreadNum)
        {
            pipes = new List<WorkPipe>();

            // PipeLine 생성
            pipeSnap = new WorkPipe(WORK_TYPE.SNAP);
            pipeAlignment = new WorkPipe(WORK_TYPE.ALIGNMENT);
            pipeInspection = new WorkPipe(WORK_TYPE.INSPECTION, inspectionThreadNum);
            pipeDefectProcess = new WorkPipe(WORK_TYPE.DEFECTPROCESS, inspectionThreadNum);
            pipeDefectProcessAll = new WorkPipe(WORK_TYPE.DEFECTPROCESS_ALL, 1, true);

            pipeSnap.SetNextPipe(pipeAlignment);
            pipeAlignment.SetNextPipe(pipeInspection);
            pipeInspection.SetNextPipe(pipeDefectProcess);
            pipeDefectProcess.SetNextPipe(pipeDefectProcessAll);

            pipes.Add(pipeSnap);
            pipes.Add(pipeAlignment);
            pipes.Add(pipeInspection);
            pipes.Add(pipeDefectProcess);
            pipes.Add(pipeDefectProcessAll);
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
                if(pipe.Start(cts.Token) == false)
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
                cts.Cancel();
                
                foreach (WorkPipe pipe in pipes)
                {
                    if(!pipe.TryStop()) result = false;
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
