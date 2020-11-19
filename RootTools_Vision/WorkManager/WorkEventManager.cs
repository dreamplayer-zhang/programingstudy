using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class WorkEventManager
    {
        ///
        ///     PositionChanged 
        ///
        public static event EventHandler<PositionDoneEventArgs> PositionDone;
        
        public static void OnPositionDone(object obj, PositionDoneEventArgs args)
        {
            PositionDone?.Invoke(obj, args);
        }


        //
        //      InspectionDone
        //
        public static event EventHandler<InspectionDoneEventArgs> InspectionDone;

        public static void OnInspectionDone(object obj, InspectionDoneEventArgs args)
        {
            InspectionDone?.Invoke(obj, args);
        }


        ///
        ///     PositionChanged 
        ///
        public static event EventHandler<PocessDefectDoneEventArgs> ProcessDefectDone;

        public static void OnProcessDefectDone(object obj, PocessDefectDoneEventArgs args)
        {
            ProcessDefectDone?.Invoke(obj, args);
        }



        public static event EventHandler<UIRedrawEventArgs> UIRedraw;

        public static void OnUIRedraw(object obj, UIRedrawEventArgs args)
        {
            UIRedraw?.Invoke(obj, args);
        }
    }
}
