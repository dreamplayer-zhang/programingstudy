using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{

    // ICloneable 인터페이스는 깊은 복사를 위하여 사용
    // 얕은 복사를 구분해주기 위하여 Copyable 인터페이스 생성
    interface ICopyable
    {
        object Copy();
    }

    public class InspectionParameter : ICopyable
    {
        public object Copy()
        {
            return this.MemberwiseClone();
        }
    }

    public class PositionParameter : ICopyable
    {
        public object Copy()
        {
            return this.MemberwiseClone();
        }
    }

    public class PreInspectionParameter : ICopyable
    {
        public object Copy()
        {
            return this.MemberwiseClone();
        }
    }

    public class MeasurementParameter : ICopyable
    {
        public object Copy()
        {
            return this.MemberwiseClone();
        }
    }

    public class Parameter : ICloneable
    {
        #region [Member Variables]
        private PositionParameter posParam;
        private PreInspectionParameter preInspParam;
        private InspectionParameter inspParam;
        private MeasurementParameter measureParam;

        public PositionParameter PosParam
        {
            get { return this.posParam; }
        }
        public PreInspectionParameter PreInspParam
        {
            get { return this.preInspParam; }
        }
        public InspectionParameter InspParam
        {
            get { return this.inspParam; }
        }
        public MeasurementParameter MeasureParam
        {
            get { return this.measureParam; }
        }
        #endregion


        public Parameter()
        {
            this.posParam = new PositionParameter();
            this.preInspParam = new PreInspectionParameter();
            this.inspParam = new InspectionParameter();
            this.measureParam = new MeasurementParameter();
        }

        public Parameter(PositionParameter _posParam, PreInspectionParameter _preInspParam, InspectionParameter _inspParam, MeasurementParameter _measureParam)
        {
            this.posParam = new PositionParameter();
            this.preInspParam = new PreInspectionParameter();
            this.inspParam = new InspectionParameter();
            this.measureParam = new MeasurementParameter();

            this.posParam = (PositionParameter)_posParam.Copy();
            this.preInspParam = (PreInspectionParameter)_preInspParam.Copy();
            this.inspParam = (InspectionParameter)_inspParam.Copy();
            this.measureParam = (MeasurementParameter)_measureParam.Copy();
        }

        public void SetParameter(PositionParameter _posParam, PreInspectionParameter _preInspParam, InspectionParameter _inspParam, MeasurementParameter _measureParam)
        {
            this.posParam = (PositionParameter)_posParam.Copy();
            this.preInspParam = (PreInspectionParameter)_preInspParam.Copy();
            this.inspParam = (InspectionParameter)_inspParam.Copy();
            this.measureParam = (MeasurementParameter)_measureParam.Copy();
        }

        public object Clone()
        {
            Parameter param = new Parameter(this.PosParam, this.PreInspParam, this.InspParam, this.MeasureParam);
            return param;
        }
    }
}
