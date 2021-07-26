
using RootTools;
using RootTools_Vision;
using Root_WindII.Engineer;
using Root_EFEM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Root_WindII
{
    public class RACSetup_ViewModel : ObservableObject
    {
        RACSetup_ImageViewer_ViewModel _RACSetupImageViewer_VM;
        public RACSetup_ImageViewer_ViewModel RACSetupImageViewer_VM
        {
            get
            {
                return _RACSetupImageViewer_VM;
            }
            set
            {
                SetProperty(ref _RACSetupImageViewer_VM, value);
            }
        }

        XMLData _XMLData;
        public XMLData XMLData
        {
            get
            {
                return _XMLData;
            }
            set
            {
                SetProperty(ref _XMLData, value);
            }
        }
        public RACSetup_ViewModel()
        {
            RACSetupImageViewer_VM = new RACSetup_ImageViewer_ViewModel();
            RACSetupImageViewer_VM.init(GlobalObjects.Instance.GetNamed<ImageData>("FrontImage"), GlobalObjects.Instance.Get<DialogService>());

            RACSetupImageViewer_VM.OriginBoxReset += OriginBoxReset_Callback;
            RACSetupImageViewer_VM.OriginPointDone += OriginPointDone_Callback;
            RACSetupImageViewer_VM.OriginBoxDone += OriginBoxDone_Callback;
            RACSetupImageViewer_VM.PitchPointDone += PitchPointDone_Callback;
            RACSetupImageViewer_VM.CenteringPointDone += CenteringPointDone_Callback;

            XMLData = GlobalObjects.Instance.Get<XMLData>();
            XMLParser.Parsing += DataUpdate;
            WindII_Handler windII_Handler = (WindII_Handler)GlobalObjects.Instance.Get<WindII_Engineer>().ClassHandler();

            GrabModeFront grabModeFront = windII_Handler.p_VisionFront.GetGrabMode(0);
            //GlobalObjects.Instance.Get<>
        }

        void DataUpdate()
        {
            RACSetupImageViewer_VM.DeviceID = XMLData.Device;
            RACSetupImageViewer_VM.UnitX = XMLData.UnitX;
            RACSetupImageViewer_VM.UnitY = XMLData.UnitY;
            RACSetupImageViewer_VM.DiePitchX = XMLData.DiePitchX;
            RACSetupImageViewer_VM.DiePitchY = XMLData.DiePitchY;
            RACSetupImageViewer_VM.ScribeLaneX = XMLData.ScribeLineX;
            RACSetupImageViewer_VM.ScribeLaneY = XMLData.ScribeLineY;
            RACSetupImageViewer_VM.ShotOffsetX = XMLData.ShotOffsetX;
            RACSetupImageViewer_VM.ShotOffsetY = XMLData.ShotOffsetY;
            RACSetupImageViewer_VM.MapOffsetX = XMLData.MapOffsetX;
            RACSetupImageViewer_VM.MapOffsetY = XMLData.MapOffsetY;
            RACSetupImageViewer_VM.SmiOffsetX = XMLData.SMIOffsetX;
            RACSetupImageViewer_VM.SmiOffsetY = XMLData.SMIOffsetY;
            RACSetupImageViewer_VM.OriginDieX = XMLData.OriginDieY;
            RACSetupImageViewer_VM.ShotSizeX = XMLData.ShotX;
            RACSetupImageViewer_VM.ShotSizeY = XMLData.ShotY;

            RACSetupImageViewer_VM.DataLoadDone();
            RACSetupImageViewer_VM.IsLoad = true;
        }

        public void OriginBoxReset_Callback()
        {
            //XMLData = new XMLData();
            //try
            //{
            //    GeneralFunction.Read(XMLData, @"D:\RACXMLTEST\ProductB.XML");
            //}
            //catch (Exception e)
            //{
            //    string t = e.ToString();
            //}
            ////GeneralFunction.Save(XMLData, @"D:\RACXMLTEST\test.xml");

            Clear();
        }

        public void OriginPointDone_Callback()
        {
            //OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();

            //XMLData.p_OriginDie = RACSetupImageViewer_VM.p_
            //XMLData.p_OriginDie.X = 
            //this.OriginY = originRecipe.OriginY;
        }

        public void PitchPointDone_Callback()
        {
            //OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();

            //this.PitchX = originRecipe.DiePitchX;
            //this.PitchY = originRecipe.DiePitchY;

            //WIND2EventManager.OnRecipeUpdated(this, new RecipeEventArgs());

        }

        public void CenteringPointDone_Callback()
        {

        }

        public void OriginBoxDone_Callback()
        {
            //this.OriginX = originRecipe.OriginX;
            //this.OriginY = originRecipe.OriginY;

            //this.OriginWidth = originRecipe.OriginWidth;
            //this.OriginHeight = originRecipe.OriginHeight;

            //this.PitchX = originRecipe.DiePitchX;
            //this.PitchY = originRecipe.DiePitchY;

        }

        public void Clear()
        {
            RACSetupImageViewer_VM.ClearObjects(true);

            //this.OriginX = 0;
            //this.OriginY = 0;
            //this.OriginWidth = 0;
            //this.OriginHeight = 0;
            //this.PitchX = 0;
            //this.PitchY = 0;
        }

        public RelayCommand CmdCreate
        {
            get
            {
                return new RelayCommand(() =>
                {
                    int i = 10;
                });
            }
        }
    }
}