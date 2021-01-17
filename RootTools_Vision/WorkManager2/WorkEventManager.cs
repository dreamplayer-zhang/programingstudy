using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class WorkEventManager
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
        public static event EventHandler<PocessDefectDoneEventArgs> ProcessDefectDone;

        public static void OnProcessDefectDone(object obj, PocessDefectDoneEventArgs args)
        {
            ProcessDefectDone?.Invoke(obj, args);
        }
        #endregion


        #region [ProcessDefectWaferDone]
        public static event EventHandler<PocessDefectWaferDoneEventArgs> ProcessDefectWaferDone;

        public static void OnProcessDefectWaferDone(object obj, PocessDefectWaferDoneEventArgs args)
        {
            ProcessDefectWaferDone?.Invoke(obj, args);
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
