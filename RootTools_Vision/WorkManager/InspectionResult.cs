using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools_Vision
{
    public class InspectionResultCollection : Collection<InspectionResult>
    {
        
    }

    public class InspectionResult
    {
        public int index;
        public int chipPositionX;
        public int chipPositionY;
        public int defectSizeX;
        public int defectSizeY;
        public long imagePositionX;
        public long imagePositionY;
        public double value;

        public InspectionResult(int _index, int _chipPositionX, int _chipPositionY, int _defectSizeX, int _defectSizeY, long _imagePositionX, long _imagePositionY, double _value)
        {

        }


    }
}
