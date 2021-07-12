using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using RootTools;
using RootTools_Vision;

namespace RootTools_Vision
{
    public abstract class FrontsideOthersParameterBase : ParameterBase
    {
        public FrontsideOthersParameterBase() { }

        #region [Parameter]

        private int _startLine = 0;
        private int _endLine = 1;
        private int _chipSearchRange = 100;
        private IMAGE_CHANNEL _indexChannel = IMAGE_CHANNEL.R_GRAY;

        #endregion

        #region [Getter Setter]

        [Category("Common")]
        [DisplayName("Start Line")]
        public int StartLine
        {
            get => this._startLine;
            set
            {
                SetProperty<int>(ref this._startLine, value);
            }
        }

        [Category("Common")]
        [DisplayName("End Line")]
        public int EndLine
        {
            get => _endLine;
            set
            {
                SetProperty<int>(ref _endLine, value);
            }
        }

        [Category("Common")]
        [DisplayName("Chip Search Range XY")]
        public int ChipSearchRange
        {
            get => _chipSearchRange;
            set
            {
                SetProperty<int>(ref _chipSearchRange, value);
            }
        }

        #endregion

    }
}
