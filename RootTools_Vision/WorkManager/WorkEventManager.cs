using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    internal class WorkEventManager
    {
        #region [WorkDoneAll]
        public static event EventHandler<WorkDoneAllEventArgs> WorkDoneAll;

        public static void OnPositionDone(object obj, WorkDoneAllEventArgs args)
        {
            WorkDoneAll?.Invoke(obj, args);
        }
        #endregion

        #region [PositionDone]
        public static event EventHandler<PositionDoneEventArgs> PositionDone;

        public static void OnPositionDone(object obj, PositionDoneEventArgs args)
        {
            PositionDone?.Invoke(obj, args);
        }
        #endregion

        #region [InspectionDone]
        public static event EventHandler<InspectionDoneEventArgs> InspectionDone;

        public static void OnInspectionDone(object obj, InspectionDoneEventArgs args)
        {
            InspectionDone?.Invoke(obj, args);
        }
        #endregion

        #region [ProcessDefectDone]
        public static event EventHandler<ProcessDefectDoneEventArgs> ProcessDefectDone;

        public static void OnProcessDefectDone(object obj, ProcessDefectDoneEventArgs args)
        {
            ProcessDefectDone?.Invoke(obj, args);
        }
        #endregion

        

        #region [ProcessDefectWaferDone]
        public static event EventHandler<IntegratedProcessDefectDoneEventArgs> IntegratedProcessDefectDone;

        public static void OnIntegratedProcessDefectWaferDone(object obj, IntegratedProcessDefectDoneEventArgs args)
        {
            IntegratedProcessDefectDone?.Invoke(obj, args);
        }

        public static event EventHandler<ProcessDefectWaferStartEventArgs> ProcessDefectWaferStart;

        public static void OnProcessDefectWaferStart(object obj, ProcessDefectWaferStartEventArgs args)
        {
            ProcessDefectWaferStart?.Invoke(obj, args);
        }
        #endregion

        #region [ProcessMeasurementDone]
        public static event EventHandler<ProcessMeasurementDoneEventArgs> ProcessMeasurementDone;

        public static void OnProcessMeasurementDone(object obj, ProcessMeasurementDoneEventArgs args)
        {
            ProcessMeasurementDone?.Invoke(obj, args);
        }
        #endregion

        #region [WorkplaceStateChanged]
        public static event EventHandler<WorkplaceStateChangedEventArgs> WorkplaceStateChanged;

        public static void OnWorkplaceStateChanged(object obj, WorkplaceStateChangedEventArgs args)
        {
            WorkplaceStateChanged?.Invoke(obj, args);
        }
        #endregion

        #region [RequestStop]
        public static event EventHandler<RequestStopEventArgs> RequestStop;

        public static void OnRequestStop(object obj, RequestStopEventArgs args)
        {
            RequestStop?.Invoke(obj, args);
        }
        #endregion

        #region [InspectionStart]
        public static event EventHandler<InspectionStartArgs> InspectionStart;

        public static void OnInspectionStart(object obj, InspectionStartArgs args)
        {
            InspectionStart?.Invoke(obj, args);
        }
        #endregion
    }
}
