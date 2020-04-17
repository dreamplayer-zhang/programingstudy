using RootTools_CLR;
using System;
using System.Threading;

namespace RootTools.Inspects
{
    public class Inspection
    {
        int threadIndex = -1;
        int inspectionID = -1;
        public InspectionState bState = InspectionState.None;
        Thread _thread;
        CLR_Demo clrDemo = new CLR_Demo();
        InspectionProperty m_InspProp;
        private volatile bool shouldStop = false;

        public int ThreadIndex { get { return threadIndex; } set { threadIndex = value; } }
        public int InspectionID { get { return inspectionID; } set { inspectionID = value; } }

        public void StartInspection(InspectionProperty prop, int threadIndex)
        {
            shouldStop = false;
            ThreadIndex = threadIndex;
            m_InspProp = prop;
            InspectionID = prop.p_index;
            bState = InspectionState.Run;
            _thread = new Thread(DoInspection);
            //_thread.IsBackground = true;
            _thread.Start();
        }
        public void EndInspection(int threadIndex)
        {
            clrDemo.EndInspection(threadIndex);

        }

        public void DoInspection(object threadId)
        {
            while (!shouldStop)
            {
                if (bState == InspectionState.Run)
                {
                    Console.WriteLine(string.Format("Inspection ID : {0} Thread Index {1}", InspectionID, ThreadIndex));
                    bState = InspectionState.Running;

                    clrDemo.Test_strip(ThreadIndex, m_InspProp.p_Rect.Left, m_InspProp.p_Rect.Top, m_InspProp.p_Rect.Right, m_InspProp.p_Rect.Bottom, 10000, 10000, m_InspProp.p_Sur_Param.p_GV, m_InspProp.p_Sur_Param.p_DefectSize, m_InspProp.p_Sur_Param.p_bDarkInspection);
                    //clrDemo.Test_strip(10000, 10000);
                }
                else if (bState == InspectionState.Running)
                {
                    shouldStop = true;
                    bState = InspectionState.Done;
                    _thread.Join();
                }
            }
        }

        public enum InspectionState
        {
            None,
            Ready,
            Run,
            Running,
            Done
        };
    }
}
