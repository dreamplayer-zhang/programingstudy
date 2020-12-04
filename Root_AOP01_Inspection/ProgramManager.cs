using Microsoft.Win32;
using RootTools;
using RootTools.Memory;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace Root_AOP01_Inspection
{
    public class ProgramManager
    {
        //Single ton
        private ProgramManager() 
        {

        }

        private static readonly Lazy<ProgramManager> instance = new Lazy<ProgramManager>(() => new ProgramManager());

        public static ProgramManager Instance 
        { 
            get 
            { 
                return instance.Value;
            } 
        }
        #region [Setting Parameters]
        private bool IsInitilized = false;
        #endregion

        #region [Members]
        private IDialogService dialogService;

        //private WIND2_Engineer engineer = new WIND2_Engineer();
        //private MemoryTool memoryTool;
        //private Recipe recipe;

        //private ImageData image;
        //private ImageData roiLayer;

        //private ImageData imageEdge;

        //InspectionManager_Vision inspectionVision;
        //InspectionManager_EFEM inspectionEFEM;
        #endregion

        #region [Getter Setter]
        //public WIND2_Engineer Engineer { get => engineer; private set => engineer = value; }
        //public MemoryTool MemoryTool { get => memoryTool; private set => memoryTool = value; }
        //public Recipe Recipe { get => recipe; private set => recipe = value; }
        //public ImageData Image { get => image; private set => image = value; }
        //public ImageData ROILayer { get => roiLayer; private set => roiLayer = value; }
        //public InspectionManager_Vision InspectionVision { get => inspectionVision; private set => inspectionVision = value; }
        //public InspectionManager_EFEM InspectionEFEM { get => inspectionEFEM; private set => inspectionEFEM = value; }
        public IDialogService DialogService { get => dialogService; set => dialogService = value; }
        //public ImageData ImageEdge { get => imageEdge; private set => imageEdge = value; }
        #endregion

        public bool Initialize()
        {
            bool result = true;

            try
            {
                if(IsInitilized == false)
                {
                    IsInitilized = true;
                }
            }
            catch(Exception ex)
            {
                result = false;
            }

            return result;
        }
    }
}
