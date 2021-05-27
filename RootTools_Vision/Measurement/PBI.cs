using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class PBI : WorkBase
    {
        #region [Member variables]
        public override WORK_TYPE Type => WORK_TYPE.MEASUREMENT;

        // PBI Recipe & Parameter
        private PBIParameter parameterPBI;
        private PBIRecipe recipePBI;

        // 필요한 이미지 버퍼 및 실행 관련 버퍼 추가 필요
        #endregion

        public PBI() : base()
        {
            m_sName = this.GetType().Name;
        }

        protected override bool Preparation()
        {
            if (this.parameterPBI == null || this.recipePBI == null)
            {
                this.parameterPBI = (PBIParameter)this.parameter;
                this.recipePBI = this.recipe.GetItem<PBIRecipe>();
            }
            return true;
        }

        protected override bool Execution()
        {
            DoMeasurement();

            return true;
        }

        public void DoMeasurement()
        {
            if (this.currentWorkplace.Index == 0)
                return;

            if (this.currentWorkplace.GetSubState(WORKPLACE_SUB_STATE.POSITION_SUCCESS) == false)
            {
                return;
            }

            WorkEventManager.OnProcessMeasurementDone(this.currentWorkplace, new ProcessMeasurementDoneEventArgs());
        }
    }
}
