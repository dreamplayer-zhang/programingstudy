
using RootTools;
using RootTools_Vision;
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

        XMLCONTENTS _XMLData;
        public XMLCONTENTS XMLData
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
        }

        public void OriginBoxReset_Callback()
        {
            XMLData = new XMLCONTENTS();
            try
            {
                GeneralFunction.Read(XMLData, @"D:\RACXMLTEST\ProductB.XML");
            }
            catch (Exception e)
            {
                string t = e.ToString();
            }
            //GeneralFunction.Save(XMLData, @"D:\RACXMLTEST\test.xml");

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
    }
}
