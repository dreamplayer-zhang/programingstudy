﻿using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision.delete
{
    public class WorkEventManager
    {
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
        public static event EventHandler<ProcessDefectWaferDoneEventArgs> ProcessDefectWaferDone;

        public static void OnProcessDefectWaferDone(object obj, ProcessDefectWaferDoneEventArgs args)
        {
            ProcessDefectWaferDone?.Invoke(obj, args);
        }
        #endregion

        #region [ProcessDefectEdgeDone]
        public static event EventHandler<ProcessDefectEdgeDoneEventArgs> ProcessDefectEdgeDone;

        public static void OnProcessDefectEdgeDone(object obj, ProcessDefectEdgeDoneEventArgs args)
        {
            ProcessDefectEdgeDone?.Invoke(obj, args);
        }
        #endregion

        #region [ProcessMeasurementDone]
        public static event EventHandler<ProcessMeasurementDoneEventArgs> ProcessMeasurementDone;

        public static void OnProcessMeasurementDone(object obj, ProcessMeasurementDoneEventArgs args)
        {
            ProcessMeasurementDone?.Invoke(obj, args);
        }
        #endregion

        #region [UIRedraw]
        public static event EventHandler<UIRedrawEventArgs> UIRedraw;

        public static void OnUIRedraw(object obj, UIRedrawEventArgs args)
        {
            UIRedraw?.Invoke(obj, args);
        }
        #endregion

        #region [WorkplaceStateChanged]
        public static event EventHandler<WorkplaceStateChangedEventArgs> WorkplaceStateChanged;

        public static void OnWorkplaceStateChanged(object obj, WorkplaceStateChangedEventArgs args)
        {
            WorkplaceStateChanged?.Invoke(obj, args);
        }
        #endregion
    }
}
